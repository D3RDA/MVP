using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVP.Data;
using MVP.DTOs.Auth;
using MVP.Extensions;
using MVP.Models;
using MVP.Services;

namespace MVP.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    // Fiókzárolás. A meglévő rate limit IP szerint korlátoz; ez FIÓK szerint,
    // tehát elosztott (több IP-s) jelszó-találgatás ellen is fog.
    private const int MaxFailedAttempts = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan FailedAttemptWindow = TimeSpan.FromMinutes(15);
    private const string InvalidCredentials = "Hibás email vagy jelszó.";

    // Ha a felhasználó nem létezik, nincs mit ellenőrizni - viszont a válasz így
    // észrevehetően gyorsabb lenne, mint létező fiók rossz jelszavánál, és abból
    // kiderülne, mely email címek vannak regisztrálva. Ezért ismeretlen email
    // esetén is lefuttatunk egy hasonlóan drága ellenőrzést egy álhash-en.
    private static readonly string TimingEqualiserHash =
        new PasswordHasher<User>().HashPassword(new User(), "timing-equaliser");

    private readonly AppDbContext _db;
    private readonly PasswordHasher<User> _passwordHasher;
    private readonly JwtTokenService _jwtTokenService;

    public AuthController(AppDbContext db, PasswordHasher<User> passwordHasher, JwtTokenService jwtTokenService)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    [EnableRateLimiting("auth")]
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        if (!request.AcceptTerms || !request.AcceptPrivacy)
        {
            return BadRequest("A regisztrációhoz el kell fogadni a felhasználási feltételeket és az adatkezelési tájékoztatót.");
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var emailExists = await _db.Users.AnyAsync(u => u.Email == normalizedEmail);
        if (emailExists)
        {
            return Conflict("Ez az email cím már regisztrálva van.");
        }

        var now = DateTime.UtcNow;
        var user = new User
        {
            Name = request.Name.Trim(),
            Email = normalizedEmail,
            CreatedAt = now,
            UpdatedAt = now,
            AcceptedTermsVersion = "1.0",
            AcceptedPrivacyVersion = "1.0",
            TermsAcceptedAt = now,
            PrivacyAcceptedAt = now
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var token = _jwtTokenService.CreateToken(user);
        return Ok(new AuthResponse(user.Id, user.Name, user.Email, token));
    }

    [EnableRateLimiting("auth")]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var now = DateTime.UtcNow;
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail);

        if (user is null)
        {
            _passwordHasher.VerifyHashedPassword(new User(), TimingEqualiserHash, request.Password);
            return Unauthorized(InvalidCredentials);
        }

        if (user.LockoutEndsAt is { } lockedUntil && lockedUntil > now)
        {
            var minutesLeft = Math.Max(1, (int)Math.Ceiling((lockedUntil - now).TotalMinutes));
            return StatusCode(
                StatusCodes.Status423Locked,
                $"A fiók a sok sikertelen bejelentkezés miatt átmenetileg zárolva. Próbáld újra {minutesLeft} perc múlva.");
        }

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

        if (result == PasswordVerificationResult.Failed)
        {
            // Ha az utolsó hibás próbálkozás régen volt, nem halmozzuk tovább:
            // aki hetente egyszer elgépeli a jelszavát, ne záródjon ki hónapok múlva.
            if (user.LastFailedLoginAt is null || now - user.LastFailedLoginAt.Value > FailedAttemptWindow)
            {
                user.FailedLoginAttempts = 0;
            }

            user.FailedLoginAttempts++;
            user.LastFailedLoginAt = now;

            if (user.FailedLoginAttempts >= MaxFailedAttempts)
            {
                user.LockoutEndsAt = now.Add(LockoutDuration);
                user.FailedLoginAttempts = 0;
            }

            await _db.SaveChangesAsync();
            return Unauthorized(InvalidCredentials);
        }

        // Sikeres belépés: a számláló és a zárolás törlődik.
        var needsSave = user.FailedLoginAttempts != 0 || user.LastFailedLoginAt is not null || user.LockoutEndsAt is not null;

        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
            needsSave = true;
        }

        if (needsSave)
        {
            user.FailedLoginAttempts = 0;
            user.LastFailedLoginAt = null;
            user.LockoutEndsAt = null;
            await _db.SaveChangesAsync();
        }

        var token = _jwtTokenService.CreateToken(user);
        return Ok(new AuthResponse(user.Id, user.Name, user.Email, token));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<object>> Me()
    {
        var userId = User.GetUserId();
        var user = await _db.Users
            .Where(u => u.Id == userId)
            .Select(u => new { u.Id, u.Name, u.Email, u.CreatedAt })
            .FirstOrDefaultAsync();

        return user is null ? Unauthorized() : Ok(user);
    }
}

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
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail);

        if (user is null)
        {
            return Unauthorized("Hibás email vagy jelszó.");
        }

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            return Unauthorized("Hibás email vagy jelszó.");
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

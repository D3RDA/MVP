using System.ComponentModel.DataAnnotations;

namespace MVP.DTOs.Auth;

public record RegisterRequest(
    [Required, MaxLength(100)] string Name,
    [Required, EmailAddress, MaxLength(255)] string Email,
    [Required, MinLength(6), MaxLength(100)] string Password,
    bool AcceptTerms,
    bool AcceptPrivacy
);

public record LoginRequest(
    [Required, EmailAddress, MaxLength(255)] string Email,
    [Required] string Password
);

public record AuthResponse(
    int UserId,
    string Name,
    string Email,
    string Token
);

using System.ComponentModel.DataAnnotations;

namespace MVP.DTOs.Companies;

public record CompanyRequest(
    [Required, MaxLength(150)] string Name,
    string? Website,
    string? Location,
    string? ContactName,
    string? ContactEmail,
    string? ContactPhone,
    string? Notes
);

public record CompanyResponse(
    int Id,
    string Name,
    string? Website,
    string? Location,
    string? ContactName,
    string? ContactEmail,
    string? ContactPhone,
    string? Notes,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

using System.ComponentModel.DataAnnotations;

namespace MVP.DTOs.JobApplications;

public record JobApplicationRequest(
    int CompanyId,
    [Required, MaxLength(150)] string PositionTitle,
    DateOnly? ApplicationDate,
    string Status = "applied",
    string? JobAdUrl = null,
    string? SalaryRange = null,
    DateTime? InterviewDateTime = null,
    string? ExperienceNotes = null,
    string? NextStep = null
);

public record JobApplicationWithCompanyRequest(
    [Required, MaxLength(150)] string CompanyName,
    string? Website,
    string? Location,
    string? ContactName,
    string? ContactEmail,
    string? ContactPhone,
    string? CompanyNotes,
    [Required, MaxLength(150)] string PositionTitle,
    DateOnly? ApplicationDate,
    string Status = "applied",
    string? JobAdUrl = null,
    string? SalaryRange = null,
    DateTime? InterviewDateTime = null,
    string? ExperienceNotes = null,
    string? NextStep = null
);

public record JobApplicationResponse(
    int Id,
    int CompanyId,
    string? CompanyName,
    string? Website,
    string? Location,
    string? ContactName,
    string? ContactEmail,
    string? ContactPhone,
    string? CompanyNotes,
    string PositionTitle,
    DateOnly? ApplicationDate,
    string Status,
    string? JobAdUrl,
    string? SalaryRange,
    DateTime? InterviewDateTime,
    string? ExperienceNotes,
    string? NextStep,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

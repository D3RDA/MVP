using System.ComponentModel.DataAnnotations;

namespace MVP.DTOs.Projects;

public record ProjectRequest(
    [Required, MaxLength(150)] string Title,
    string? Description,
    DateOnly? StartDate,
    DateOnly? EndDate,
    decimal? DailyHours,
    string Status = "planned",
    string Priority = "medium"
);

public record ProjectResponse(
    int Id,
    string Title,
    string? Description,
    DateOnly? StartDate,
    DateOnly? EndDate,
    decimal? DailyHours,
    string Status,
    string Priority,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

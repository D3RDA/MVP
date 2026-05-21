using System.ComponentModel.DataAnnotations;

namespace MVP.DTOs.Tasks;

public record TaskRequest(
    [Required, MaxLength(150)] string Title,
    string? Description,
    DateOnly? DueDate,
    decimal? EstimatedHours,
    string Status = "todo"
);

public record TaskResponse(
    int Id,
    int ProjectId,
    string Title,
    string? Description,
    DateOnly? DueDate,
    decimal? EstimatedHours,
    string Status,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

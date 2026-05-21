using System.ComponentModel.DataAnnotations;

namespace MVP.DTOs.Notes;

public record NoteRequest(
    int? ProjectId,
    int? TaskId,
    int? CompanyId,
    int? JobApplicationId,
    [MaxLength(150)] string? Title,
    [Required] string Content
);

public record NoteResponse(
    int Id,
    int? ProjectId,
    int? TaskId,
    int? CompanyId,
    int? JobApplicationId,
    string? Title,
    string Content,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

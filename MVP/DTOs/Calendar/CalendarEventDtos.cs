using System.ComponentModel.DataAnnotations;

namespace MVP.DTOs.Calendar;

public record CalendarEventRequest(
    int? ProjectId,
    int? TaskId,
    [Required, MaxLength(150)] string Title,
    string? Description,
    DateTime StartDateTime,
    DateTime EndDateTime,
    string EventType = "other"
);

public record CalendarEventResponse(
    int Id,
    int? ProjectId,
    int? TaskId,
    string Title,
    string? Description,
    DateTime StartDateTime,
    DateTime EndDateTime,
    string EventType,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

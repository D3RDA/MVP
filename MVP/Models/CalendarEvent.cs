namespace MVP.Models;

public class CalendarEvent
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int? ProjectId { get; set; }
    public int? TaskId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public string EventType { get; set; } = "other";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public User? User { get; set; }
    public Project? Project { get; set; }
    public TaskItem? Task { get; set; }
}

namespace MVP.Models;

public class Project
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public decimal? DailyHours { get; set; }
    public string Status { get; set; } = "planned";
    public string Priority { get; set; } = "medium";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public User? User { get; set; }
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}

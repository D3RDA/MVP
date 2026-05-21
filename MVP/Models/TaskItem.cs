namespace MVP.Models;

public class TaskItem
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateOnly? DueDate { get; set; }
    public decimal? EstimatedHours { get; set; }
    public string Status { get; set; } = "todo";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Project? Project { get; set; }
}

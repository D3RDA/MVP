namespace MVP.Models;

public class Note
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int? ProjectId { get; set; }
    public int? TaskId { get; set; }
    public int? CompanyId { get; set; }
    public int? JobApplicationId { get; set; }
    public string? Title { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public User? User { get; set; }
    public Project? Project { get; set; }
    public TaskItem? Task { get; set; }
    public Company? Company { get; set; }
    public JobApplication? JobApplication { get; set; }
}

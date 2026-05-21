namespace MVP.Models;

public class JobApplication
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int CompanyId { get; set; }
    public string PositionTitle { get; set; } = string.Empty;
    public DateOnly? ApplicationDate { get; set; }
    public string Status { get; set; } = "applied";
    public string? JobAdUrl { get; set; }
    public string? SalaryRange { get; set; }
    public DateTime? InterviewDateTime { get; set; }
    public string? ExperienceNotes { get; set; }
    public string? NextStep { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public User? User { get; set; }
    public Company? Company { get; set; }
}

namespace MVP.Models;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? AcceptedTermsVersion { get; set; }
    public string? AcceptedPrivacyVersion { get; set; }
    public DateTime? TermsAcceptedAt { get; set; }
    public DateTime? PrivacyAcceptedAt { get; set; }

    public ICollection<Project> Projects { get; set; } = new List<Project>();
    public ICollection<Company> Companies { get; set; } = new List<Company>();
    public ICollection<CalendarEvent> CalendarEvents { get; set; } = new List<CalendarEvent>();
    public ICollection<JobApplication> JobApplications { get; set; } = new List<JobApplication>();
    public ICollection<Note> Notes { get; set; } = new List<Note>();
}

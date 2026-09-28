using Microsoft.EntityFrameworkCore;
using MVP.Models;

namespace MVP.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<CalendarEvent> CalendarEvents => Set<CalendarEvent>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<JobApplication> JobApplications => Set<JobApplication>();
    public DbSet<Note> Notes => Set<Note>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash").HasMaxLength(255).IsRequired();
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.AcceptedTermsVersion).HasColumnName("accepted_terms_version").HasMaxLength(20);
            entity.Property(e => e.AcceptedPrivacyVersion).HasColumnName("accepted_privacy_version").HasMaxLength(20);
            entity.Property(e => e.TermsAcceptedAt).HasColumnName("terms_accepted_at");
            entity.Property(e => e.PrivacyAcceptedAt).HasColumnName("privacy_accepted_at");
            entity.Property(e => e.FailedLoginAttempts).HasColumnName("failed_login_attempts").HasDefaultValue(0);
            entity.Property(e => e.LastFailedLoginAt).HasColumnName("last_failed_login_at");
            entity.Property(e => e.LockoutEndsAt).HasColumnName("lockout_ends_at");
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.ToTable("projects");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Title).HasColumnName("title").HasMaxLength(150).IsRequired();
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.DailyHours).HasColumnName("daily_hours").HasPrecision(4, 2);
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(50).HasDefaultValue("planned");
            entity.Property(e => e.Priority).HasColumnName("priority").HasMaxLength(50).HasDefaultValue("medium");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(e => e.User)
                .WithMany(e => e.Projects)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.ToTable("tasks");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.Title).HasColumnName("title").HasMaxLength(150).IsRequired();
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.DueDate).HasColumnName("due_date");
            entity.Property(e => e.EstimatedHours).HasColumnName("estimated_hours").HasPrecision(4, 2);
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(50).HasDefaultValue("todo");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(e => e.Project)
                .WithMany(e => e.Tasks)
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CalendarEvent>(entity =>
        {
            entity.ToTable("calendar_events");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.TaskId).HasColumnName("task_id");
            entity.Property(e => e.Title).HasColumnName("title").HasMaxLength(150).IsRequired();
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.StartDateTime).HasColumnName("start_datetime");
            entity.Property(e => e.EndDateTime).HasColumnName("end_datetime");
            entity.Property(e => e.EventType).HasColumnName("event_type").HasMaxLength(50).HasDefaultValue("other");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(e => e.User)
                .WithMany(e => e.CalendarEvents)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Project)
                .WithMany()
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Task)
                .WithMany()
                .HasForeignKey(e => e.TaskId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Company>(entity =>
        {
            entity.ToTable("companies");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(150).IsRequired();
            entity.Property(e => e.Website).HasColumnName("website").HasMaxLength(255);
            entity.Property(e => e.Location).HasColumnName("location").HasMaxLength(150);
            entity.Property(e => e.ContactName).HasColumnName("contact_name").HasMaxLength(100);
            entity.Property(e => e.ContactEmail).HasColumnName("contact_email").HasMaxLength(255);
            entity.Property(e => e.ContactPhone).HasColumnName("contact_phone").HasMaxLength(50);
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(e => e.User)
                .WithMany(e => e.Companies)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<JobApplication>(entity =>
        {
            entity.ToTable("job_applications");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.CompanyId).HasColumnName("company_id");
            entity.Property(e => e.PositionTitle).HasColumnName("position_title").HasMaxLength(150).IsRequired();
            entity.Property(e => e.ApplicationDate).HasColumnName("application_date");
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(50).HasDefaultValue("applied");
            entity.Property(e => e.JobAdUrl).HasColumnName("job_ad_url").HasMaxLength(255);
            entity.Property(e => e.SalaryRange).HasColumnName("salary_range").HasMaxLength(100);
            entity.Property(e => e.InterviewDateTime).HasColumnName("interview_datetime");
            entity.Property(e => e.ExperienceNotes).HasColumnName("experience_notes");
            entity.Property(e => e.NextStep).HasColumnName("next_step");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(e => e.User)
                .WithMany(e => e.JobApplications)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Company)
                .WithMany(e => e.JobApplications)
                .HasForeignKey(e => e.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Note>(entity =>
        {
            entity.ToTable("notes");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.TaskId).HasColumnName("task_id");
            entity.Property(e => e.CompanyId).HasColumnName("company_id");
            entity.Property(e => e.JobApplicationId).HasColumnName("job_application_id");
            entity.Property(e => e.Title).HasColumnName("title").HasMaxLength(150);
            entity.Property(e => e.Content).HasColumnName("content").IsRequired();
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(e => e.User).WithMany(e => e.Notes).HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Project).WithMany().HasForeignKey(e => e.ProjectId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Task).WithMany().HasForeignKey(e => e.TaskId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Company).WithMany().HasForeignKey(e => e.CompanyId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.JobApplication).WithMany().HasForeignKey(e => e.JobApplicationId).OnDelete(DeleteBehavior.Cascade);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is User or Project or TaskItem or CalendarEvent or Company or JobApplication or Note)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Property("CreatedAt").CurrentValue = now;
                    entry.Property("UpdatedAt").CurrentValue = now;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Property("UpdatedAt").CurrentValue = now;
                }
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}

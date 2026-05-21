using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVP.Data;
using MVP.DTOs.Dashboard;
using MVP.Extensions;

namespace MVP.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _db;

    public DashboardController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<DashboardResponse>> Get()
    {
        var userId = User.GetUserId();
        var now = DateTime.UtcNow;
        var nextWeek = now.AddDays(7);

        var activeProjects = await _db.Projects
            .CountAsync(p => p.UserId == userId && (p.Status == "planned" || p.Status == "in_progress" || p.Status == "paused"));

        var completedProjects = await _db.Projects
            .CountAsync(p => p.UserId == userId && p.Status == "completed");

        var openJobApplications = await _db.JobApplications
            .CountAsync(j => j.UserId == userId && j.Status != "rejected" && j.Status != "declined" && j.Status != "closed");

        var waitingJobApplications = await _db.JobApplications
            .CountAsync(j => j.UserId == userId && j.Status == "waiting_response");

        var upcomingCalendarEvents = await _db.CalendarEvents
            .CountAsync(e => e.UserId == userId && e.StartDateTime >= now && e.StartDateTime <= nextWeek);

        var upcomingEvents = await _db.CalendarEvents
            .Where(e => e.UserId == userId && e.StartDateTime >= now)
            .OrderBy(e => e.StartDateTime)
            .Take(5)
            .Select(e => new UpcomingEventResponse(e.Id, e.Title, e.StartDateTime, e.EndDateTime, e.EventType))
            .ToListAsync();

        var recentProjects = await _db.Projects
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .Take(5)
            .Select(p => new RecentProjectResponse(p.Id, p.Title, p.Status, p.Priority, p.EndDate))
            .ToListAsync();

        var recentJobApplications = await _db.JobApplications
            .Include(j => j.Company)
            .Where(j => j.UserId == userId)
            .OrderByDescending(j => j.CreatedAt)
            .Take(5)
            .Select(j => new RecentJobApplicationResponse(
                j.Id,
                j.Company != null ? j.Company.Name : "Ismeretlen cég",
                j.PositionTitle,
                j.Status,
                j.ApplicationDate
            ))
            .ToListAsync();

        return Ok(new DashboardResponse(
            activeProjects,
            completedProjects,
            openJobApplications,
            waitingJobApplications,
            upcomingCalendarEvents,
            upcomingEvents,
            recentProjects,
            recentJobApplications
        ));
    }
}

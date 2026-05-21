using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVP.Data;
using MVP.DTOs.Calendar;
using MVP.Extensions;
using MVP.Models;

namespace MVP.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CalendarEventsController : ControllerBase
{
    private readonly AppDbContext _db;

    public CalendarEventsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CalendarEventResponse>>> GetAll([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var userId = User.GetUserId();
        var query = _db.CalendarEvents.Where(e => e.UserId == userId);

        if (from.HasValue) query = query.Where(e => e.StartDateTime >= from.Value);
        if (to.HasValue) query = query.Where(e => e.StartDateTime <= to.Value);

        var events = await query
            .OrderBy(e => e.StartDateTime)
            .ToListAsync();

        return Ok(events.Select(ToResponse));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CalendarEventResponse>> GetById(int id)
    {
        var userId = User.GetUserId();
        var calendarEvent = await _db.CalendarEvents.FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);
        return calendarEvent is null ? NotFound() : Ok(ToResponse(calendarEvent));
    }

    [HttpPost]
    public async Task<ActionResult<CalendarEventResponse>> Create(CalendarEventRequest request)
    {
        var userId = User.GetUserId();
        var validation = await ValidateReferences(userId, request.ProjectId, request.TaskId);
        if (validation is not null) return BadRequest(validation);

        if (request.EndDateTime <= request.StartDateTime)
        {
            return BadRequest("A befejezési időnek későbbinek kell lennie, mint a kezdési időnek.");
        }

        var calendarEvent = new CalendarEvent
        {
            UserId = userId,
            ProjectId = request.ProjectId,
            TaskId = request.TaskId,
            Title = request.Title.Trim(),
            Description = request.Description,
            StartDateTime = request.StartDateTime,
            EndDateTime = request.EndDateTime,
            EventType = request.EventType
        };

        _db.CalendarEvents.Add(calendarEvent);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = calendarEvent.Id }, ToResponse(calendarEvent));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<CalendarEventResponse>> Update(int id, CalendarEventRequest request)
    {
        var userId = User.GetUserId();
        var calendarEvent = await _db.CalendarEvents.FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);
        if (calendarEvent is null) return NotFound();

        var validation = await ValidateReferences(userId, request.ProjectId, request.TaskId);
        if (validation is not null) return BadRequest(validation);

        if (request.EndDateTime <= request.StartDateTime)
        {
            return BadRequest("A befejezési időnek későbbinek kell lennie, mint a kezdési időnek.");
        }

        calendarEvent.ProjectId = request.ProjectId;
        calendarEvent.TaskId = request.TaskId;
        calendarEvent.Title = request.Title.Trim();
        calendarEvent.Description = request.Description;
        calendarEvent.StartDateTime = request.StartDateTime;
        calendarEvent.EndDateTime = request.EndDateTime;
        calendarEvent.EventType = request.EventType;

        await _db.SaveChangesAsync();
        return Ok(ToResponse(calendarEvent));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = User.GetUserId();
        var calendarEvent = await _db.CalendarEvents.FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);
        if (calendarEvent is null) return NotFound();

        _db.CalendarEvents.Remove(calendarEvent);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private async Task<string?> ValidateReferences(int userId, int? projectId, int? taskId)
    {
        if (projectId.HasValue)
        {
            var projectExists = await _db.Projects.AnyAsync(p => p.Id == projectId && p.UserId == userId);
            if (!projectExists) return "A megadott projekt nem található ennél a felhasználónál.";
        }

        if (taskId.HasValue)
        {
            var taskExists = await _db.Tasks.Include(t => t.Project).AnyAsync(t => t.Id == taskId && t.Project!.UserId == userId);
            if (!taskExists) return "A megadott feladat nem található ennél a felhasználónál.";
        }

        return null;
    }

    private static CalendarEventResponse ToResponse(CalendarEvent e) => new(
        e.Id, e.ProjectId, e.TaskId, e.Title, e.Description,
        e.StartDateTime, e.EndDateTime, e.EventType, e.CreatedAt, e.UpdatedAt
    );
}

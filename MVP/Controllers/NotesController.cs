using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVP.Data;
using MVP.DTOs.Notes;
using MVP.Extensions;
using MVP.Models;

namespace MVP.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class NotesController : ControllerBase
{
    private readonly AppDbContext _db;

    public NotesController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<NoteResponse>>> GetAll(
        [FromQuery] int? projectId,
        [FromQuery] int? taskId,
        [FromQuery] int? companyId,
        [FromQuery] int? jobApplicationId)
    {
        var userId = User.GetUserId();
        var query = _db.Notes.Where(n => n.UserId == userId);

        if (projectId.HasValue) query = query.Where(n => n.ProjectId == projectId);
        if (taskId.HasValue) query = query.Where(n => n.TaskId == taskId);
        if (companyId.HasValue) query = query.Where(n => n.CompanyId == companyId);
        if (jobApplicationId.HasValue) query = query.Where(n => n.JobApplicationId == jobApplicationId);

        var notes = await query
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

        return Ok(notes.Select(ToResponse));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<NoteResponse>> GetById(int id)
    {
        var userId = User.GetUserId();
        var note = await _db.Notes.FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);
        return note is null ? NotFound() : Ok(ToResponse(note));
    }

    [HttpPost]
    public async Task<ActionResult<NoteResponse>> Create(NoteRequest request)
    {
        var userId = User.GetUserId();
        var validation = await ValidateReferences(userId, request);
        if (validation is not null) return BadRequest(validation);

        var note = new Note
        {
            UserId = userId,
            ProjectId = request.ProjectId,
            TaskId = request.TaskId,
            CompanyId = request.CompanyId,
            JobApplicationId = request.JobApplicationId,
            Title = request.Title,
            Content = request.Content
        };

        _db.Notes.Add(note);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = note.Id }, ToResponse(note));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<NoteResponse>> Update(int id, NoteRequest request)
    {
        var userId = User.GetUserId();
        var note = await _db.Notes.FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);
        if (note is null) return NotFound();

        var validation = await ValidateReferences(userId, request);
        if (validation is not null) return BadRequest(validation);

        note.ProjectId = request.ProjectId;
        note.TaskId = request.TaskId;
        note.CompanyId = request.CompanyId;
        note.JobApplicationId = request.JobApplicationId;
        note.Title = request.Title;
        note.Content = request.Content;

        await _db.SaveChangesAsync();
        return Ok(ToResponse(note));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = User.GetUserId();
        var note = await _db.Notes.FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);
        if (note is null) return NotFound();

        _db.Notes.Remove(note);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private async Task<string?> ValidateReferences(int userId, NoteRequest request)
    {
        if (request.ProjectId.HasValue && !await _db.Projects.AnyAsync(p => p.Id == request.ProjectId && p.UserId == userId))
            return "A megadott projekt nem található ennél a felhasználónál.";

        if (request.TaskId.HasValue && !await _db.Tasks.Include(t => t.Project).AnyAsync(t => t.Id == request.TaskId && t.Project!.UserId == userId))
            return "A megadott feladat nem található ennél a felhasználónál.";

        if (request.CompanyId.HasValue && !await _db.Companies.AnyAsync(c => c.Id == request.CompanyId && c.UserId == userId))
            return "A megadott cég nem található ennél a felhasználónál.";

        if (request.JobApplicationId.HasValue && !await _db.JobApplications.AnyAsync(j => j.Id == request.JobApplicationId && j.UserId == userId))
            return "A megadott állásjelentkezés nem található ennél a felhasználónál.";

        return null;
    }

    private static NoteResponse ToResponse(Note n) => new(
        n.Id, n.ProjectId, n.TaskId, n.CompanyId, n.JobApplicationId,
        n.Title, n.Content, n.CreatedAt, n.UpdatedAt
    );
}

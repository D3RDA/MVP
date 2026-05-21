using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVP.Data;
using MVP.DTOs.Projects;
using MVP.Extensions;
using MVP.Models;

namespace MVP.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly AppDbContext _db;

    public ProjectsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProjectResponse>>> GetAll()
    {
        var userId = User.GetUserId();
        var projects = await _db.Projects
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return Ok(projects.Select(ToResponse));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProjectResponse>> GetById(int id)
    {
        var userId = User.GetUserId();
        var project = await _db.Projects.FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);
        return project is null ? NotFound() : Ok(ToResponse(project));
    }

    [HttpPost]
    public async Task<ActionResult<ProjectResponse>> Create(ProjectRequest request)
    {
        var userId = User.GetUserId();
        var project = new Project
        {
            UserId = userId,
            Title = request.Title.Trim(),
            Description = request.Description,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            DailyHours = request.DailyHours,
            Status = request.Status,
            Priority = request.Priority
        };

        _db.Projects.Add(project);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = project.Id }, ToResponse(project));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProjectResponse>> Update(int id, ProjectRequest request)
    {
        var userId = User.GetUserId();
        var project = await _db.Projects.FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);
        if (project is null) return NotFound();

        project.Title = request.Title.Trim();
        project.Description = request.Description;
        project.StartDate = request.StartDate;
        project.EndDate = request.EndDate;
        project.DailyHours = request.DailyHours;
        project.Status = request.Status;
        project.Priority = request.Priority;

        await _db.SaveChangesAsync();
        return Ok(ToResponse(project));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = User.GetUserId();
        var project = await _db.Projects.FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);
        if (project is null) return NotFound();

        _db.Projects.Remove(project);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private static ProjectResponse ToResponse(Project p) => new(
        p.Id, p.Title, p.Description, p.StartDate, p.EndDate, p.DailyHours,
        p.Status, p.Priority, p.CreatedAt, p.UpdatedAt
    );
}

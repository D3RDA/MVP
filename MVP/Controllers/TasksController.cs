using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVP.Data;
using MVP.DTOs.Tasks;
using MVP.Extensions;
using MVP.Models;

namespace MVP.Controllers;

[Authorize]
[ApiController]
[Route("api/projects/{projectId:int}/tasks")]
public class TasksController : ControllerBase
{
    private readonly AppDbContext _db;

    public TasksController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskResponse>>> GetAll(int projectId)
    {
        var userId = User.GetUserId();
        var projectExists = await _db.Projects.AnyAsync(p => p.Id == projectId && p.UserId == userId);
        if (!projectExists) return NotFound("A projekt nem található.");

        var tasks = await _db.Tasks
            .Where(t => t.ProjectId == projectId)
            .OrderBy(t => t.DueDate)
            .ToListAsync();

        return Ok(tasks.Select(ToResponse));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TaskResponse>> GetById(int projectId, int id)
    {
        var userId = User.GetUserId();
        var task = await _db.Tasks
            .Include(t => t.Project)
            .FirstOrDefaultAsync(t => t.Id == id && t.ProjectId == projectId && t.Project!.UserId == userId);

        return task is null ? NotFound() : Ok(ToResponse(task));
    }

    [HttpPost]
    public async Task<ActionResult<TaskResponse>> Create(int projectId, TaskRequest request)
    {
        var userId = User.GetUserId();
        var projectExists = await _db.Projects.AnyAsync(p => p.Id == projectId && p.UserId == userId);
        if (!projectExists) return NotFound("A projekt nem található.");

        var task = new TaskItem
        {
            ProjectId = projectId,
            Title = request.Title.Trim(),
            Description = request.Description,
            DueDate = request.DueDate,
            EstimatedHours = request.EstimatedHours,
            Status = request.Status
        };

        _db.Tasks.Add(task);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { projectId, id = task.Id }, ToResponse(task));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<TaskResponse>> Update(int projectId, int id, TaskRequest request)
    {
        var userId = User.GetUserId();
        var task = await _db.Tasks
            .Include(t => t.Project)
            .FirstOrDefaultAsync(t => t.Id == id && t.ProjectId == projectId && t.Project!.UserId == userId);

        if (task is null) return NotFound();

        task.Title = request.Title.Trim();
        task.Description = request.Description;
        task.DueDate = request.DueDate;
        task.EstimatedHours = request.EstimatedHours;
        task.Status = request.Status;

        await _db.SaveChangesAsync();
        return Ok(ToResponse(task));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int projectId, int id)
    {
        var userId = User.GetUserId();
        var task = await _db.Tasks
            .Include(t => t.Project)
            .FirstOrDefaultAsync(t => t.Id == id && t.ProjectId == projectId && t.Project!.UserId == userId);

        if (task is null) return NotFound();

        _db.Tasks.Remove(task);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private static TaskResponse ToResponse(TaskItem t) => new(
        t.Id, t.ProjectId, t.Title, t.Description, t.DueDate, t.EstimatedHours,
        t.Status, t.CreatedAt, t.UpdatedAt
    );
}

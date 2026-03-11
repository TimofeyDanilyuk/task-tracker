using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using System.Security.Claims;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Microsoft.AspNetCore.Authorization.Authorize]
public class TasksController : ControllerBase
{
    private readonly AppDbContext _db;
    public TasksController(AppDbContext db) => _db = db;
    private int UserId => int.Parse(User.FindFirst("userId")!.Value);
    public class LinkRequest { public int LinkedTaskId { get; set; } }
    public class StageRequest { public int? StageId { get; set; } }


    [HttpPatch("{id}/done")]
    public async Task<IActionResult> ToggleDone(int id)
    {
        var task = await _db.Tasks.FindAsync(id);
        if (task is null) return NotFound();
        task.IsDone = !task.IsDone;
        await _db.SaveChangesAsync();
        return Ok(task);
    }
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool? isTodo = null) =>
        Ok(await _db.Tasks
            .Include(t => t.Stage)
            .Include(t => t.SubTasks)
            .Include(t => t.Comments)
            .Where(t => t.ParentTaskId == null && t.UserId == UserId &&
                    (isTodo == null || t.IsTodo == isTodo))
            .Select(t => new
            {
                t.Id,
                t.Title,
                t.Description,
                t.Priority,
                t.CreatedAt,
                t.DueDate,
                t.StageId,
                t.IsTodo,
                Stage = t.Stage == null ? null : new { t.Stage.Id, t.Stage.Name, t.Stage.Color },
                SubTasks = t.SubTasks.Select(s => new { s.Id, s.Title, s.Priority, s.IsDone }),
                Comments = t.Comments.Select(c => new { c.Id, c.Text })
            })
            .ToListAsync());


    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var task = await _db.Tasks
            .Include(t => t.Stage)
            .Include(t => t.Comments)
            .Include(t => t.SubTasks)
                .ThenInclude(s => s.Comments)
            .Where(t => t.Id == id)
            .Select(t => new
            {
                t.Id,
                t.Title,
                t.Description,
                t.Priority,
                t.CreatedAt,
                t.DueDate,
                t.StageId,
                Stage = t.Stage == null ? null : new { t.Stage.Id, t.Stage.Name, t.Stage.Color },
                SubTasks = t.SubTasks.Select(s => new
                {
                    s.Id,
                    s.Title,
                    s.Description,
                    s.Priority,
                    s.IsDone,
                    Comments = s.Comments.Select(c => new { c.Id, c.Text, c.CreatedAt })
                }),
                Comments = t.Comments.Select(c => new { c.Id, c.Text, c.CreatedAt })
            })
            .FirstOrDefaultAsync();

        if (task is null) return NotFound();
        return Ok(task);
    }

    [HttpPost]
    public async Task<IActionResult> Create(TaskItem task)
    {
        task.CreatedAt = DateTime.UtcNow;
        if (task.ParentTaskId.HasValue)
        {
            var parent = await _db.Tasks.FindAsync(task.ParentTaskId);
            task.UserId = parent?.UserId;
        }
        else
        {
            task.UserId = UserId;
        }
        _db.Tasks.Add(task);
        await _db.SaveChangesAsync();
        return Ok(task);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, TaskItem updated)
    {
        var task = await _db.Tasks.FindAsync(id);
        if (task is null) return NotFound();

        task.Title = updated.Title;
        task.Description = updated.Description;
        task.Priority = updated.Priority;
        task.DueDate = updated.DueDate;
        task.StageId = updated.StageId;

        await _db.SaveChangesAsync();
        return Ok(task);
    }

    [HttpPatch("{id}/stage")]
    public async Task<IActionResult> ChangeStage(int id, [FromBody] StageRequest req)
    {
        var task = await _db.Tasks.FindAsync(id);
        if (task is null) return NotFound();
        task.StageId = req.StageId;
        await _db.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var task = await _db.Tasks.FindAsync(id);
        if (task is null) return NotFound();
        _db.Tasks.Remove(task);
        await _db.SaveChangesAsync();
        return Ok();
    }

    [HttpGet("{id}/links")]
    public async Task<IActionResult> GetLinks(int id)
    {
        var links = await _db.TaskLinks
            .Where(l => l.TaskId == id || l.LinkedTaskId == id)
            .Include(l => l.Task).ThenInclude(t => t!.Stage)
            .Include(l => l.LinkedTask).ThenInclude(t => t!.Stage)
            .Select(l => new
            {
                id = l.Id,
                task = l.TaskId == id
                    ? new
                    {
                        l.LinkedTask!.Id,
                        l.LinkedTask.Title,
                        l.LinkedTask.Priority,
                        Stage = l.LinkedTask.Stage == null ? null : new { l.LinkedTask.Stage.Id, l.LinkedTask.Stage.Name, l.LinkedTask.Stage.Color }
                    }
                    : new
                    {
                        l.Task!.Id,
                        l.Task.Title,
                        l.Task.Priority,
                        Stage = l.Task.Stage == null ? null : new { l.Task.Stage.Id, l.Task.Stage.Name, l.Task.Stage.Color }
                    }
            })
            .ToListAsync();

        return Ok(links);
    }

    [HttpPost("{id}/links")]
    public async Task<IActionResult> AddLink(int id, [FromBody] LinkRequest req)
    {
        var exists = await _db.TaskLinks.AnyAsync(l =>
            (l.TaskId == id && l.LinkedTaskId == req.LinkedTaskId) ||
            (l.TaskId == req.LinkedTaskId && l.LinkedTaskId == id));

        if (exists) return BadRequest("Связь уже существует");

        var link = new TaskLink { TaskId = id, LinkedTaskId = req.LinkedTaskId };
        _db.TaskLinks.Add(link);
        await _db.SaveChangesAsync();
        return Ok(link);
    }

    [HttpDelete("{id}/links/{linkedTaskId}")]
    public async Task<IActionResult> RemoveLink(int id, int linkedTaskId)
    {
        var link = await _db.TaskLinks.FirstOrDefaultAsync(l =>
            (l.TaskId == id && l.LinkedTaskId == linkedTaskId) ||
            (l.TaskId == linkedTaskId && l.LinkedTaskId == id));

        if (link is null) return NotFound();
        _db.TaskLinks.Remove(link);
        await _db.SaveChangesAsync();
        return Ok();
    }
}
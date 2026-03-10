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
            .Select(t => new {
                t.Id, t.Title, t.Description, t.Priority,
                t.CreatedAt, t.DueDate, t.StageId, t.IsTodo,
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
    public async Task<IActionResult> ChangeStage(int id, [FromBody] int stageId)
    {
        var task = await _db.Tasks.FindAsync(id);
        if (task is null) return NotFound();
        task.StageId = stageId;
        await _db.SaveChangesAsync();
        return Ok(task);
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
}
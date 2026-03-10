using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Microsoft.AspNetCore.Authorization.Authorize]
public class CommentsController : ControllerBase
{
    private readonly AppDbContext _db;
    public CommentsController(AppDbContext db) => _db = db;

    [HttpGet("task/{taskId}")]
    public async Task<IActionResult> GetByTask(int taskId) =>
        Ok(await _db.Comments
            .Where(c => c.TaskItemId == taskId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync());

    [HttpPost]
    public async Task<IActionResult> Create(Comment comment)
    {
        comment.CreatedAt = DateTime.UtcNow;
        _db.Comments.Add(comment);
        await _db.SaveChangesAsync();
        return Ok(comment);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var comment = await _db.Comments.FindAsync(id);
        if (comment is null) return NotFound();
        _db.Comments.Remove(comment);
        await _db.SaveChangesAsync();
        return Ok();
    }
}
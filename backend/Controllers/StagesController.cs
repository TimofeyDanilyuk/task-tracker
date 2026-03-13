using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using System.Security.Claims;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Microsoft.AspNetCore.Authorization.Authorize]
public class StagesController : ControllerBase
{
    private readonly AppDbContext _db;
    public StagesController(AppDbContext db) => _db = db;

    private int UserId => int.Parse(User.FindFirstValue("userId")!);

    // Личные этапы
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _db.Stages.Where(s => s.UserId == UserId && s.BoardId == null).ToListAsync());

    // Этапы доски
    [HttpGet("board/{boardId}")]
    public async Task<IActionResult> GetBoardStages(int boardId)
    {
        var hasAccess = await _db.Boards.AnyAsync(b =>
            b.Id == boardId && (b.OwnerId == UserId || b.Members.Any(m => m.UserId == UserId)));
        if (!hasAccess) return Forbid();
        return Ok(await _db.Stages.Where(s => s.BoardId == boardId).ToListAsync());
    }

    // Создать личный этап
    [HttpPost]
    public async Task<IActionResult> Create(Stage stage)
    {
        stage.UserId = UserId;
        stage.BoardId = null;
        _db.Stages.Add(stage);
        await _db.SaveChangesAsync();
        return Ok(stage);
    }

    // Создать этап для доски (только админ)
    [HttpPost("board/{boardId}")]
    public async Task<IActionResult> CreateBoardStage(int boardId, Stage stage)
    {
        if (!await IsAdmin(boardId)) return Forbid();
        stage.BoardId = boardId;
        stage.UserId = null;
        _db.Stages.Add(stage);
        await _db.SaveChangesAsync();
        return Ok(stage);
    }

    // Обновить этап
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Stage updated)
    {
        var stage = await _db.Stages.FirstOrDefaultAsync(s => s.Id == id);
        if (stage is null) return NotFound();

        // Личный этап
        if (stage.BoardId == null && stage.UserId != UserId) return Forbid();
        // Этап доски — только админ
        if (stage.BoardId != null && !await IsAdmin(stage.BoardId.Value)) return Forbid();

        stage.Name = updated.Name;
        stage.Color = updated.Color;
        stage.IsFinal = updated.IsFinal;
        await _db.SaveChangesAsync();
        return Ok(stage);
    }

    // Удалить этап
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var stage = await _db.Stages.FirstOrDefaultAsync(s => s.Id == id);
        if (stage is null) return NotFound();

        if (stage.BoardId == null && stage.UserId != UserId) return Forbid();
        if (stage.BoardId != null && !await IsAdmin(stage.BoardId.Value)) return Forbid();

        _db.Stages.Remove(stage);
        await _db.SaveChangesAsync();
        return Ok();
    }

    private async Task<bool> IsAdmin(int boardId)
    {
        var board = await _db.Boards.Include(b => b.Members)
            .FirstOrDefaultAsync(b => b.Id == boardId);
        if (board is null) return false;
        return board.OwnerId == UserId ||
            board.Members.Any(m => m.UserId == UserId && m.Role == BoardRole.Admin);
    }
}
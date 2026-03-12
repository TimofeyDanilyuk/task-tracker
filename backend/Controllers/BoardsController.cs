using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using System.Security.Claims;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Microsoft.AspNetCore.Authorization.Authorize]
public class BoardsController : ControllerBase
{
    private readonly AppDbContext _db;
    public BoardsController(AppDbContext db) => _db = db;

    private int UserId => int.Parse(User.FindFirstValue("userId")!);

    // Все доски где я участник или владелец
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var boards = await _db.Boards
            .Where(b => b.OwnerId == UserId || b.Members.Any(m => m.UserId == UserId))
            .Select(b => new
            {
                b.Id,
                b.Name,
                b.CreatedAt,
                owner = new { b.Owner.Id, b.Owner.Username },
                isOwner = b.OwnerId == UserId,
                myRole = b.OwnerId == UserId ? "Admin" :
                    b.Members.Where(m => m.UserId == UserId).Select(m => m.Role.ToString()).FirstOrDefault(),
                membersCount = b.Members.Count + 1
            })
            .ToListAsync();
        return Ok(boards);
    }

    // Одна доска с участниками и этапами
    [HttpGet("{id}")]
    public async Task<IActionResult> GetOne(int id)
    {
        var board = await _db.Boards
            .Where(b => b.Id == id && (b.OwnerId == UserId || b.Members.Any(m => m.UserId == UserId)))
            .Select(b => new
            {
                b.Id,
                b.Name,
                b.CreatedAt,
                owner = new { b.Owner.Id, b.Owner.Username },
                isOwner = b.OwnerId == UserId,
                myRole = b.OwnerId == UserId ? "Admin" :
                    b.Members.Where(m => m.UserId == UserId).Select(m => m.Role.ToString()).FirstOrDefault(),
                members = b.Members.Select(m => new
                {
                    m.Id,
                    m.UserId,
                    m.Role,
                    username = m.User.Username
                }),
                stages = b.Stages.Select(s => new { s.Id, s.Name, s.Color })
            })
            .FirstOrDefaultAsync();
        if (board is null) return NotFound();
        return Ok(board);
    }

    // Создать доску
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBoardDto dto)
    {
        var board = new Board { Name = dto.Name, OwnerId = UserId };
        _db.Boards.Add(board);
        await _db.SaveChangesAsync();
        return Ok(new { board.Id, board.Name });
    }

    // Переименовать доску
    [HttpPut("{id}")]
    public async Task<IActionResult> Rename(int id, [FromBody] CreateBoardDto dto)
    {
        var board = await GetBoardIfAdmin(id);
        if (board is null) return Forbid();
        board.Name = dto.Name;
        await _db.SaveChangesAsync();
        return Ok();
    }

    // Удалить доску (любой админ)
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var board = await GetBoardIfAdmin(id);
        if (board is null) return Forbid();
        _db.Boards.Remove(board);
        await _db.SaveChangesAsync();
        return Ok();
    }

    // Добавить участника (только из друзей)
    [HttpPost("{id}/members")]
    public async Task<IActionResult> AddMember(int id, [FromBody] AddMemberDto dto)
    {
        var board = await GetBoardIfAdmin(id);
        if (board is null) return Forbid();

        var isFriend = await _db.Friendships.AnyAsync(f =>
            f.Status == FriendshipStatus.Accepted &&
            ((f.FromUserId == UserId && f.ToUserId == dto.UserId) ||
             (f.ToUserId == UserId && f.FromUserId == dto.UserId)));
        if (!isFriend) return BadRequest("Пользователь не в списке друзей");

        var already = await _db.BoardMembers.AnyAsync(m => m.BoardId == id && m.UserId == dto.UserId);
        if (already) return BadRequest("Пользователь уже участник");

        _db.BoardMembers.Add(new BoardMember { BoardId = id, UserId = dto.UserId, Role = BoardRole.Member });
        await _db.SaveChangesAsync();
        return Ok();
    }

    // Удалить участника
    [HttpDelete("{id}/members/{userId}")]
    public async Task<IActionResult> RemoveMember(int id, int userId)
    {
        var board = await GetBoardIfAdmin(id);
        if (board is null) return Forbid();
        var member = await _db.BoardMembers.FirstOrDefaultAsync(m => m.BoardId == id && m.UserId == userId);
        if (member is null) return NotFound();
        _db.BoardMembers.Remove(member);
        await _db.SaveChangesAsync();
        return Ok();
    }

    // Изменить роль участника
    [HttpPatch("{id}/members/{userId}/role")]
    public async Task<IActionResult> SetRole(int id, int userId, [FromBody] SetRoleDto dto)
    {
        var board = await GetBoardIfAdmin(id);
        if (board is null) return Forbid();
        var member = await _db.BoardMembers.FirstOrDefaultAsync(m => m.BoardId == id && m.UserId == userId);
        if (member is null) return NotFound();
        member.Role = dto.Role;
        await _db.SaveChangesAsync();
        return Ok();
    }

    // Назначить ответственного за задачу
    [HttpPatch("{id}/tasks/{taskId}/assign")]
    public async Task<IActionResult> AssignTask(int id, int taskId, [FromBody] AssignDto dto)
    {
        var board = await GetBoardIfAdmin(id);
        if (board is null) return Forbid();
        var task = await _db.Tasks.FirstOrDefaultAsync(t => t.Id == taskId && t.BoardId == id);
        if (task is null) return NotFound();
        task.AssignedUserId = dto.UserId;
        await _db.SaveChangesAsync();
        return Ok();
    }

    private async Task<Board?> GetBoardIfAdmin(int boardId)
    {
        var board = await _db.Boards
            .Include(b => b.Members)
            .FirstOrDefaultAsync(b => b.Id == boardId);
        if (board is null) return null;
        var isAdmin = board.OwnerId == UserId ||
            board.Members.Any(m => m.UserId == UserId && m.Role == BoardRole.Admin);
        return isAdmin ? board : null;
    }
}

public record CreateBoardDto(string Name);
public record AddMemberDto(int UserId);
public record SetRoleDto(BoardRole Role);
public record AssignDto(int? UserId);
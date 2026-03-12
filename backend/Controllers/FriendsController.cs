using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using System.Security.Claims;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Microsoft.AspNetCore.Authorization.Authorize]
public class FriendsController : ControllerBase
{
    private readonly AppDbContext _db;
    public FriendsController(AppDbContext db) => _db = db;

    private int UserId => int.Parse(User.FindFirstValue("userId")!);

    // Список друзей
    [HttpGet]
    public async Task<IActionResult> GetFriends()
    {
        var friends = await _db.Friendships
            .Where(f => (f.FromUserId == UserId || f.ToUserId == UserId) && f.Status == FriendshipStatus.Accepted)
            .Select(f => new
            {
                id = f.Id,
                user = f.FromUserId == UserId
                    ? new { f.ToUser.Id, f.ToUser.Username }
                    : new { f.FromUser.Id, f.FromUser.Username }
            })
            .ToListAsync();
        return Ok(friends);
    }

    // Входящие заявки
    [HttpGet("requests")]
    public async Task<IActionResult> GetRequests()
    {
        var requests = await _db.Friendships
            .Where(f => f.ToUserId == UserId && f.Status == FriendshipStatus.Pending)
            .Select(f => new { f.Id, f.FromUser.Username, f.CreatedAt })
            .ToListAsync();
        return Ok(requests);
    }

    // Количество входящих заявок (для индикатора)
    [HttpGet("requests/count")]
    public async Task<IActionResult> GetRequestsCount()
    {
        var count = await _db.Friendships
            .CountAsync(f => f.ToUserId == UserId && f.Status == FriendshipStatus.Pending);
        return Ok(new { count });
    }

    // Отправить заявку по username
    [HttpPost("request")]
    public async Task<IActionResult> SendRequest([FromBody] SendRequestDto dto)
    {
        var toUser = await _db.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);
        if (toUser is null) return NotFound("Пользователь не найден");
        if (toUser.Id == UserId) return BadRequest("Нельзя добавить себя");

        var exists = await _db.Friendships.AnyAsync(f =>
            (f.FromUserId == UserId && f.ToUserId == toUser.Id) ||
            (f.FromUserId == toUser.Id && f.ToUserId == UserId));
        if (exists) return BadRequest("Заявка уже существует или вы уже друзья");

        var friendship = new Friendship { FromUserId = UserId, ToUserId = toUser.Id };
        _db.Friendships.Add(friendship);
        await _db.SaveChangesAsync();
        return Ok();
    }

    // Принять заявку
    [HttpPost("requests/{id}/accept")]
    public async Task<IActionResult> Accept(int id)
    {
        var f = await _db.Friendships.FirstOrDefaultAsync(f => f.Id == id && f.ToUserId == UserId);
        if (f is null) return NotFound();
        f.Status = FriendshipStatus.Accepted;
        await _db.SaveChangesAsync();
        return Ok();
    }

    // Отклонить заявку
    [HttpPost("requests/{id}/decline")]
    public async Task<IActionResult> Decline(int id)
    {
        var f = await _db.Friendships.FirstOrDefaultAsync(f => f.Id == id && f.ToUserId == UserId);
        if (f is null) return NotFound();
        _db.Friendships.Remove(f);
        await _db.SaveChangesAsync();
        return Ok();
    }

    // Удалить из друзей
    [HttpDelete("{id}")]
    public async Task<IActionResult> Remove(int id)
    {
        var f = await _db.Friendships.FirstOrDefaultAsync(f =>
            f.Id == id && (f.FromUserId == UserId || f.ToUserId == UserId));
        if (f is null) return NotFound();
        _db.Friendships.Remove(f);
        await _db.SaveChangesAsync();
        return Ok();
    }
}

public record SendRequestDto(string Username);
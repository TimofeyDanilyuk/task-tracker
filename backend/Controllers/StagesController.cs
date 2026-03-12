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

    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _db.Stages.Where(s => s.UserId == UserId).ToListAsync());

    [HttpPost]
    public async Task<IActionResult> Create(Stage stage)
    {
        stage.UserId = UserId;
        _db.Stages.Add(stage);
        await _db.SaveChangesAsync();
        return Ok(stage);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Stage updated)
    {
        var stage = await _db.Stages.FirstOrDefaultAsync(s => s.Id == id && s.UserId == UserId);
        if (stage is null) return NotFound();
        stage.Name = updated.Name;
        stage.Color = updated.Color;
        await _db.SaveChangesAsync();
        return Ok(stage);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var stage = await _db.Stages.FirstOrDefaultAsync(s => s.Id == id && s.UserId == UserId);
        if (stage is null) return NotFound();
        _db.Stages.Remove(stage);
        await _db.SaveChangesAsync();
        return Ok();
    }
}
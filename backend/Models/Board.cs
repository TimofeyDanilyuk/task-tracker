namespace backend.Models;

public class Board
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int OwnerId { get; set; }
    public User Owner { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<BoardMember> Members { get; set; } = new();
    public List<Stage> Stages { get; set; } = new();
    public List<TaskItem> Tasks { get; set; } = new();
}
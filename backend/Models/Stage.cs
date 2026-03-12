namespace backend.Models;

public class Stage
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#3498db";
    public int? UserId { get; set; }
    public int? BoardId { get; set; }
    public Board? Board { get; set; }
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}
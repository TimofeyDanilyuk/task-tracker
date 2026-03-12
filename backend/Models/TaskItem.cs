namespace backend.Models;

public class TaskItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Priority { get; set; } // 1-5
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DueDate { get; set; }

    public int? StageId { get; set; }
    public Stage? Stage { get; set; }

    public bool IsDone { get; set; } = false;
    public bool IsTodo { get; set; } = false;

    public int? ParentTaskId { get; set; }
    public TaskItem? ParentTask { get; set; }
    public ICollection<TaskItem> SubTasks { get; set; } = new List<TaskItem>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public int? UserId { get; set; }
    public User? User { get; set; }

    public int? BoardId { get; set; }
    public Board? Board { get; set; }

    public int? AssignedUserId { get; set; }
    public User? AssignedUser { get; set; }
}
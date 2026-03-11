namespace backend.Models;

public class TaskLink
{
    public int Id { get; set; }
    public int TaskId { get; set; }
    public TaskItem? Task { get; set; }
    public int LinkedTaskId { get; set; }
    public TaskItem? LinkedTask { get; set; }
}
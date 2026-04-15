namespace StaffTaskManagement.Models;

public class TaskLog
{
    public int Id { get; set; }

    public int TaskItemId { get; set; }
    public TaskItem? TaskItem { get; set; }

    public TaskStatus FromStatus { get; set; }
    public TaskStatus ToStatus { get; set; }

    public int? ChangedById { get; set; }
    public ApplicationUser? ChangedBy { get; set; }

    public string? Notes { get; set; }

    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
}


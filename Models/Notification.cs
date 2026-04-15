namespace StaffTaskManagement.Models;

public class Notification
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public ApplicationUser? User { get; set; }

    public NotificationType Type { get; set; } = NotificationType.General;

    public string Message { get; set; } = string.Empty;

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int? TaskItemId { get; set; }
    public TaskItem? TaskItem { get; set; }
}


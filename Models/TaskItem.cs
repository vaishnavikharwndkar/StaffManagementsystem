using System.ComponentModel.DataAnnotations;

namespace StaffTaskManagement.Models;

public class TaskItem
{
    public int Id { get; set; }

    [Required, StringLength(150)]
    public string Title { get; set; } = string.Empty;

    [DataType(DataType.MultilineText)]
    public string? Description { get; set; }

    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    public TaskStatus Status { get; set; } = TaskStatus.NotStarted;

    [DataType(DataType.DateTime)]
    public DateTime? DueDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public int? DepartmentId { get; set; }
    public Department? Department { get; set; }

    public int? CreatedById { get; set; }
    public ApplicationUser? CreatedBy { get; set; }

    public int? AssignedToId { get; set; }
    public ApplicationUser? AssignedTo { get; set; }

    public ICollection<TaskComment> Comments { get; set; } = new List<TaskComment>();
    public ICollection<TaskLog> Logs { get; set; } = new List<TaskLog>();
    public ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();
    public ICollection<TaskAttachment> Attachments { get; set; } = new List<TaskAttachment>();
}


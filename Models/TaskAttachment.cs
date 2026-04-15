using System.ComponentModel.DataAnnotations;

namespace StaffTaskManagement.Models;

public class TaskAttachment
{
    public int Id { get; set; }

    [Required]
    public int TaskItemId { get; set; }
    public TaskItem? TaskItem { get; set; }

    [Required, StringLength(255)]
    public string FileName { get; set; } = string.Empty;

    [Required, StringLength(260)]
    public string StoredFileName { get; set; } = string.Empty;

    [StringLength(100)]
    public string? ContentType { get; set; }

    public int UploadedById { get; set; }
    public ApplicationUser? UploadedBy { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}


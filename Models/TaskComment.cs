using System.ComponentModel.DataAnnotations;

namespace StaffTaskManagement.Models;

public class TaskComment
{
    public int Id { get; set; }

    [Required]
    public int TaskItemId { get; set; }
    public TaskItem? TaskItem { get; set; }

    [Required]
    public int AuthorId { get; set; }
    public ApplicationUser? Author { get; set; }

    [Required, StringLength(1000)]
    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}


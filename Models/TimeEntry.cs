using System.ComponentModel.DataAnnotations;

namespace StaffTaskManagement.Models;

public class TimeEntry
{
    public int Id { get; set; }

    [Required]
    public int TaskItemId { get; set; }
    public TaskItem? TaskItem { get; set; }

    [Required]
    public int UserId { get; set; }
    public ApplicationUser? User { get; set; }

    [DataType(DataType.Date)]
    public DateTime Date { get; set; } = DateTime.UtcNow.Date;

    [Range(0, 24)]
    public decimal Hours { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }
}


using System.ComponentModel.DataAnnotations;

namespace StaffTaskManagement.Models;

public class Department
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(250)]
    public string? Description { get; set; }

    public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}


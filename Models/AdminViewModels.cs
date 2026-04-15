using System.ComponentModel.DataAnnotations;

namespace StaffTaskManagement.Models;

public class CreateUserViewModel
{
    [Required, StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public UserRole Role { get; set; } = UserRole.Employee;

    public int? DepartmentId { get; set; }
}

public class CreateDepartmentViewModel
{
    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(250)]
    public string? Description { get; set; }
}


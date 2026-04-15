using StaffTaskManagement.Models;
using StaffTaskManagement.Utilities;

namespace StaffTaskManagement.Data;

public static class DbSeeder
{
    public static void Seed(ApplicationDbContext context)
    {
        if (context.Departments.Any())
        {
            return;
        }

        var defaultDept = new Department
        {
            Name = "General",
            Description = "Default department"
        };

        var adminUser = new ApplicationUser
        {
            FullName = "System Administrator",
            Email = "admin@company.com",
            PasswordHash = PasswordHelper.HashPassword("Admin@123"),
            Role = UserRole.Admin,
            Department = defaultDept,
            IsActive = true
        };

        context.Departments.Add(defaultDept);
        context.Users.Add(adminUser);
        context.SaveChanges();
    }
}


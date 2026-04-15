using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StaffTaskManagement.Data;
using StaffTaskManagement.Models;

namespace StaffTaskManagement.Controllers;

[Authorize(Roles = "Employee,Manager,Admin")]
public class EmployeeController(ApplicationDbContext context) : Controller
{
    public IActionResult Index()
    {
        return RedirectToAction(nameof(MyTasks));
    }

    public async Task<IActionResult> MyTasks()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        int.TryParse(userIdClaim, out var userId);

        var tasks = await context.Tasks
            .Include(t => t.CreatedBy)
            .Include(t => t.Department)
            .Where(t => t.AssignedToId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return View(tasks);
    }

    public async Task<IActionResult> MyPerformance()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        int.TryParse(userIdClaim, out var userId);

        var total = await context.Tasks.CountAsync(t => t.AssignedToId == userId);
        var completed = await context.Tasks.CountAsync(t => t.AssignedToId == userId && t.Status == StaffTaskManagement.Models.TaskStatus.Completed);
        var pending = await context.Tasks.CountAsync(t => t.AssignedToId == userId && t.Status != StaffTaskManagement.Models.TaskStatus.Completed);
        var overdue = await context.Tasks.CountAsync(t =>
            t.AssignedToId == userId &&
            t.DueDate < DateTime.UtcNow &&
            t.Status != StaffTaskManagement.Models.TaskStatus.Completed);

        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);

        var model = new EmployeeDashboardViewModel
        {
            EmployeeName = user?.FullName,
            MyTasks = total,
            PendingTasks = pending,
            CompletedTasks = completed,
            OverdueTasks = overdue
        };

        return View(model);
    }
}


using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StaffTaskManagement.Data;
using StaffTaskManagement.Models;

namespace StaffTaskManagement.Controllers;

[Authorize]
public class HomeController(ApplicationDbContext context) : Controller
{
    public async Task<IActionResult> Index()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        int.TryParse(userIdClaim, out var userId);
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

        var totalTasks = await context.Tasks.CountAsync();
        var myTasks = await context.Tasks.CountAsync(t => t.AssignedToId == userId);
        var pendingTasks = await context.Tasks.CountAsync(t => t.AssignedToId == userId && t.Status != StaffTaskManagement.Models.TaskStatus.Completed);
        var completedTasks = await context.Tasks.CountAsync(t => t.AssignedToId == userId && t.Status == StaffTaskManagement.Models.TaskStatus.Completed);
        var dueSoon = await context.Tasks
            .CountAsync(t => t.AssignedToId == userId && t.DueDate >= DateTime.UtcNow && t.DueDate <= DateTime.UtcNow.AddDays(3));

        var model = new DashboardViewModel
        {
            TotalTasks = totalTasks,
            MyTasks = myTasks,
            PendingTasks = pendingTasks,
            CompletedTasks = completedTasks,
            DueSoonTasks = dueSoon
        };

        if (role == UserRole.Admin.ToString())
        {
            model.IsAdminOverview = true;
            model.TotalUsers = await context.Users.CountAsync();
            model.TotalDepartments = await context.Departments.CountAsync();
        }

        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StaffTaskManagement.Data;
using StaffTaskManagement.Models;

namespace StaffTaskManagement.Controllers;

[Authorize]
public class ReportsController(ApplicationDbContext context) : Controller
{
    public async Task<IActionResult> Index()
    {
        var totalTasks = await context.Tasks.CountAsync();
        var completed = await context.Tasks.CountAsync(t => t.Status == StaffTaskManagement.Models.TaskStatus.Completed);
        var inProgress = await context.Tasks.CountAsync(t => t.Status == StaffTaskManagement.Models.TaskStatus.InProgress);
        var onHold = await context.Tasks.CountAsync(t => t.Status == StaffTaskManagement.Models.TaskStatus.OnHold);
        var notStarted = await context.Tasks.CountAsync(t => t.Status == StaffTaskManagement.Models.TaskStatus.NotStarted);

        ViewBag.TotalTasks = totalTasks;
        ViewBag.Completed = completed;
        ViewBag.InProgress = inProgress;
        ViewBag.OnHold = onHold;
        ViewBag.NotStarted = notStarted;

        return View();
    }
}


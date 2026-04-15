using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StaffTaskManagement.Data;
using StaffTaskManagement.Models;

namespace StaffTaskManagement.Controllers;

[Authorize(Roles = "Manager,Admin")]
public class ManagerController(ApplicationDbContext context) : Controller
{
    public async Task<IActionResult> Index()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        int.TryParse(userIdClaim, out var managerId);

        var manager = await context.Users
            .Include(u => u.Department)
            .FirstOrDefaultAsync(u => u.Id == managerId);

        var departmentId = manager?.DepartmentId;

        var teamMembersQuery = context.Users.AsQueryable();
        if (departmentId.HasValue)
        {
            teamMembersQuery = teamMembersQuery.Where(u => u.DepartmentId == departmentId && u.Id != managerId);
        }

        var departmentTasksQuery = context.Tasks.AsQueryable();
        if (departmentId.HasValue)
        {
            departmentTasksQuery = departmentTasksQuery.Where(t => t.DepartmentId == departmentId);
        }

        var model = new ManagerDashboardViewModel
        {
            ManagerName = manager?.FullName,
            DepartmentName = manager?.Department?.Name,
            TeamMembers = await teamMembersQuery.CountAsync(),
            DepartmentTasks = await departmentTasksQuery.CountAsync(),
            OpenTeamTasks = await departmentTasksQuery.CountAsync(t =>
                t.Status == StaffTaskManagement.Models.TaskStatus.NotStarted ||
                t.Status == StaffTaskManagement.Models.TaskStatus.InProgress ||
                t.Status == StaffTaskManagement.Models.TaskStatus.OnHold),
            CompletedTeamTasks = await departmentTasksQuery.CountAsync(t => t.Status == StaffTaskManagement.Models.TaskStatus.Completed)
        };

        return View(model);
    }

    public async Task<IActionResult> TeamTasks()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        int.TryParse(userIdClaim, out var managerId);

        var manager = await context.Users.FirstOrDefaultAsync(u => u.Id == managerId);
        var departmentId = manager?.DepartmentId;

        var query = context.Tasks
            .Include(t => t.AssignedTo)
            .Include(t => t.Department)
            .AsQueryable();

        if (departmentId.HasValue)
        {
            query = query.Where(t => t.DepartmentId == departmentId || t.CreatedById == managerId);
        }
        else
        {
            query = query.Where(t => t.CreatedById == managerId);
        }

        var tasks = await query
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return View(tasks);
    }
}


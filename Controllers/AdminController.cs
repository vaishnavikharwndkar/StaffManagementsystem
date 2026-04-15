using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StaffTaskManagement.Data;
using StaffTaskManagement.Models;
using StaffTaskManagement.Utilities;

namespace StaffTaskManagement.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController(ApplicationDbContext context) : Controller
{
    public async Task<IActionResult> Index()
    {
        var totalUsers = await context.Users.CountAsync();
        var totalManagers = await context.Users.CountAsync(u => u.Role == UserRole.Manager);
        var totalEmployees = await context.Users.CountAsync(u => u.Role == UserRole.Employee);
        var totalDepartments = await context.Departments.CountAsync();

        var totalTasks = await context.Tasks.CountAsync();
        var openTasks = await context.Tasks.CountAsync(t => t.Status != StaffTaskManagement.Models.TaskStatus.Completed);

        ViewBag.TotalUsers = totalUsers;
        ViewBag.TotalManagers = totalManagers; 
        ViewBag.TotalEmployees = totalEmployees;
        ViewBag.TotalDepartments = totalDepartments;
        ViewBag.TotalTasks = totalTasks;
        ViewBag.OpenTasks = openTasks;

        return View();
    }

    public async Task<IActionResult> Users()
    {
        var users = await context.Users
            .Include(u => u.Department)
            .OrderBy(u => u.FullName)
            .ToListAsync();
        return View(users);
    }

    public IActionResult CreateUser()
    {
        ViewBag.Departments = new SelectList(context.Departments.ToList(), "Id", "Name");
        return View(new CreateUserViewModel());
    }

    public IActionResult CreateEmployee()
    {
        ViewData["Title"] = "Add Employee";
        ViewBag.Departments = new SelectList(context.Departments.ToList(), "Id", "Name");
        return View(nameof(CreateUser), new CreateUserViewModel { Role = UserRole.Employee });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateUser(CreateUserViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Departments = new SelectList(context.Departments.ToList(), "Id", "Name");
            return View(model);
        }

        if (await context.Users.AnyAsync(u => u.Email == model.Email))
        {
            ModelState.AddModelError(nameof(model.Email), "A user with this email already exists.");
            ViewBag.Departments = new SelectList(context.Departments.ToList(), "Id", "Name");
            return View(model);
        }

        var user = new ApplicationUser
        {
            FullName = model.FullName,
            Email = model.Email,
            PasswordHash = PasswordHelper.HashPassword(model.Password),
            Role = model.Role,
            DepartmentId = model.DepartmentId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        return RedirectToAction(nameof(Users));
    }

    public async Task<IActionResult> Departments()
    {
        var departments = await context.Departments
            .Include(d => d.Users)
            .OrderBy(d => d.Name)
            .ToListAsync();
        return View(departments);
    }

    public IActionResult CreateDepartment()
    {
        return View(new CreateDepartmentViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateDepartment(CreateDepartmentViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var department = new Department
        {
            Name = model.Name,
            Description = model.Description
        };

        context.Departments.Add(department);
        await context.SaveChangesAsync();

        return RedirectToAction(nameof(Departments));
    }
    public async Task<IActionResult> Details(int id)
    {
        var user = await context.Users
            .Include(u => u.Department)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            return NotFound();
        }

        return View(user);
    }
    public async Task<IActionResult> Edit(int id)
    {
        var user = await context.Users.FindAsync(id);

        if (user == null)
        {
            return NotFound();
        }

        ViewBag.Departments = new SelectList(context.Departments, "Id", "Name", user.DepartmentId);

        return View(user);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ApplicationUser model)
    {
        if (id != model.Id)
            return NotFound();

        // 🔥 Fix validation issue
        ModelState.Remove("PasswordHash");

        if (!ModelState.IsValid)
        {
            ViewBag.Departments = new SelectList(context.Departments, "Id", "Name", model.DepartmentId);
            return View(model);
        }

        var user = await context.Users.FindAsync(id);

        if (user == null)
            return NotFound();

        user.FullName = model.FullName;
        user.Email = model.Email;
        user.Role = model.Role;
        user.DepartmentId = model.DepartmentId;
        user.IsActive = model.IsActive;

        await context.SaveChangesAsync();

        return RedirectToAction(nameof(Users));
    }
    public async Task<IActionResult> Delete(int id)
    {
        var user = await context.Users
            .Include(u => u.Department)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            return NotFound();
        }

        return View(user);
    }
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var user = await context.Users.FindAsync(id);

        if (user != null)
        {
            context.Users.Remove(user);
            await context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Users));
    }
}



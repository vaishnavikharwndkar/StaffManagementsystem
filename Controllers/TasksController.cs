using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StaffTaskManagement.Data;
using StaffTaskManagement.Models;

namespace StaffTaskManagement.Controllers;

[Authorize]
public class TasksController(ApplicationDbContext context, IWebHostEnvironment environment) : Controller
{
    public async Task<IActionResult> Index(StaffTaskManagement.Models.TaskStatus? status, TaskPriority? priority)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        int.TryParse(userIdClaim, out var userId);
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

        var query = context.Tasks
            .Include(t => t.AssignedTo)
            .Include(t => t.CreatedBy)
            .AsQueryable();

        if (role == UserRole.Employee.ToString())
        {
            query = query.Where(t => t.AssignedToId == userId);
        }
        else if (role == UserRole.Manager.ToString())
        {
            query = query.Where(t => t.CreatedById == userId || t.AssignedToId == userId);
        }

        if (status.HasValue)
        {
            query = query.Where(t => t.Status == status);
        }

        if (priority.HasValue)
        {
            query = query.Where(t => t.Priority == priority);
        }

        var tasks = await query
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        ViewBag.Status = status;
        ViewBag.Priority = priority;
        return View(tasks);
    }

    [Authorize(Roles = "Admin,Manager")]
    public IActionResult Create()
    {
        PopulateUsersAndDepartments();
        return View(new TaskItem { DueDate = DateTime.UtcNow.AddDays(3) });
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TaskItem task)
    {
        if (!ModelState.IsValid)
        {
            PopulateUsersAndDepartments();
            return View(task);
        }

        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        int.TryParse(userIdClaim, out var userId);
        task.CreatedById = userId;
        task.CreatedAt = DateTime.UtcNow;

        context.Tasks.Add(task);
        await context.SaveChangesAsync();

        if (task.AssignedToId.HasValue)
        {
            context.Notifications.Add(new Notification
            {
                UserId = task.AssignedToId.Value,
                TaskItemId = task.Id,
                Type = NotificationType.TaskAssigned,
                Message = $"You have been assigned a new task: {task.Title}",
                CreatedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id)
    {
        var task = await context.Tasks
            .Include(t => t.AssignedTo)
            .Include(t => t.CreatedBy)
            .Include(t => t.Comments).ThenInclude(c => c.Author)
            .Include(t => t.Logs).ThenInclude(l => l.ChangedBy)
            .Include(t => t.Attachments).ThenInclude(a => a.UploadedBy)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (task == null)
        {
            return NotFound();
        }

        return View(task);
    }

    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Edit(int id)
    {
        var task = await context.Tasks.FindAsync(id);
        if (task == null)
        {
            return NotFound();
        }

        PopulateUsersAndDepartments();
        return View(task);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, TaskItem task)
    {
        if (id != task.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            PopulateUsersAndDepartments();
            return View(task);
        }

        var existing = await context.Tasks.FindAsync(id);
        if (existing == null)
        {
            return NotFound();
        }

        existing.Title = task.Title;
        existing.Description = task.Description;
        existing.Priority = task.Priority;
        existing.Status = task.Status;
        existing.DueDate = task.DueDate;
        existing.AssignedToId = task.AssignedToId;
        existing.DepartmentId = task.DepartmentId;
        existing.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, StaffTaskManagement.Models.TaskStatus status)
    {
        var task = await context.Tasks.FindAsync(id);
        if (task == null)
        {
            return NotFound();
        }

        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        int.TryParse(userIdClaim, out var userId);

        var previousStatus = task.Status;
        if (previousStatus == status)
        {
            return RedirectToAction(nameof(Details), new { id });
        }

        task.Status = status;
        task.UpdatedAt = DateTime.UtcNow;

        context.TaskLogs.Add(new TaskLog
        {
            TaskItemId = task.Id,
            FromStatus = previousStatus,
            ToStatus = status,
            ChangedById = userId,
            ChangedAt = DateTime.UtcNow,
            Notes = "Status updated from task details."
        });
        
        if (task.AssignedToId.HasValue && task.AssignedToId != userId)
        {
            context.Notifications.Add(new Notification
            {
                UserId = task.AssignedToId.Value,
                TaskItemId = task.Id,
                Type = NotificationType.TaskUpdated,
                Message = $"Status changed to {status} for task: {task.Title}",
                CreatedAt = DateTime.UtcNow
            });
        }

        if (task.CreatedById.HasValue && task.CreatedById != userId)
        {
            context.Notifications.Add(new Notification
            {
                UserId = task.CreatedById.Value,
                TaskItemId = task.Id,
                Type = NotificationType.TaskUpdated,
                Message = $"Status changed to {status} for task: {task.Title}",
                CreatedAt = DateTime.UtcNow
            });
        }

        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadAttachment(int id, IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return RedirectToAction(nameof(Details), new { id });
        }

        var task = await context.Tasks.FindAsync(id);
        if (task == null)
        {
            return NotFound();
        }

        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        int.TryParse(userIdClaim, out var userId);

        var uploadsRoot = Path.Combine(environment.WebRootPath ?? "wwwroot", "uploads");
        Directory.CreateDirectory(uploadsRoot);

        var safeFileName = Path.GetFileName(file.FileName);
        var storedFileName = $"{Guid.NewGuid()}{Path.GetExtension(safeFileName)}";
        var filePath = Path.Combine(uploadsRoot, storedFileName);

        await using (var stream = System.IO.File.Create(filePath))
        {
            await file.CopyToAsync(stream);
        }

        context.TaskAttachments.Add(new TaskAttachment
        {
            TaskItemId = task.Id,
            FileName = safeFileName,
            StoredFileName = storedFileName,
            ContentType = file.ContentType,
            UploadedById = userId,
            UploadedAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> DownloadAttachment(int attachmentId)
    {
        var attachment = await context.TaskAttachments
            .Include(a => a.TaskItem)
            .FirstOrDefaultAsync(a => a.Id == attachmentId);

        if (attachment == null)
        {
            return NotFound();
        }

        var uploadsRoot = Path.Combine(environment.WebRootPath ?? "wwwroot", "uploads");
        var filePath = Path.Combine(uploadsRoot, attachment.StoredFileName);

        if (!System.IO.File.Exists(filePath))
        {
            return NotFound();
        }

        var contentType = attachment.ContentType ?? "application/octet-stream";
        return PhysicalFile(filePath, contentType, attachment.FileName);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddComment(int id, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return RedirectToAction(nameof(Details), new { id });
        }

        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        int.TryParse(userIdClaim, out var userId);

        var task = await context.Tasks.FindAsync(id);
        if (task == null)
        {
            return NotFound();
        }

        context.TaskComments.Add(new TaskComment
        {
            TaskItemId = id,
            AuthorId = userId,
            Content = content,
            CreatedAt = DateTime.UtcNow
        });
        
        if (task.AssignedToId.HasValue && task.AssignedToId != userId)
        {
            context.Notifications.Add(new Notification
            {
                UserId = task.AssignedToId.Value,
                TaskItemId = task.Id,
                Type = NotificationType.CommentAdded,
                Message = $"New comment on task: {task.Title}",
                CreatedAt = DateTime.UtcNow
            });
        }

        if (task.CreatedById.HasValue && task.CreatedById != userId)
        {
            context.Notifications.Add(new Notification
            {
                UserId = task.CreatedById.Value,
                TaskItemId = task.Id,
                Type = NotificationType.CommentAdded,
                Message = $"New comment on task: {task.Title}",
                CreatedAt = DateTime.UtcNow
            });
        }

        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id });
    }

    private void PopulateUsersAndDepartments()
    {
        ViewBag.Users = new SelectList(
            context.Users
                .Where(u => u.IsActive && u.Role != UserRole.Admin)
                .OrderBy(u => u.FullName)
                .ToList(),
            "Id",
            "FullName");
        ViewBag.Departments = new SelectList(context.Departments.ToList(), "Id", "Name");
    }
}


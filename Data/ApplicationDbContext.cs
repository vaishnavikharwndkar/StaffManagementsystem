using Microsoft.EntityFrameworkCore;
using StaffTaskManagement.Models;

namespace StaffTaskManagement.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<ApplicationUser> Users { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<TaskItem> Tasks { get; set; }
    public DbSet<TaskComment> TaskComments { get; set; }
    public DbSet<TaskLog> TaskLogs { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<TimeEntry> TimeEntries { get; set; }
    public DbSet<TaskAttachment> TaskAttachments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ApplicationUser>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<ApplicationUser>()
            .HasMany(u => u.CreatedTasks)
            .WithOne(t => t.CreatedBy)
            .HasForeignKey(t => t.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ApplicationUser>()
            .HasMany(u => u.AssignedTasks)
            .WithOne(t => t.AssignedTo)
            .HasForeignKey(t => t.AssignedToId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TaskItem>()
            .HasMany(t => t.Logs)
            .WithOne(l => l.TaskItem!)
            .HasForeignKey(l => l.TaskItemId);

        modelBuilder.Entity<TaskItem>()
            .HasMany(t => t.Comments)
            .WithOne(c => c.TaskItem!)
            .HasForeignKey(c => c.TaskItemId);

        modelBuilder.Entity<TaskItem>()
            .HasMany(t => t.TimeEntries)
            .WithOne(e => e.TaskItem!)
            .HasForeignKey(e => e.TaskItemId);

        modelBuilder.Entity<TaskItem>()
            .HasMany(t => t.Attachments)
            .WithOne(a => a.TaskItem!)
            .HasForeignKey(a => a.TaskItemId);
    }
}


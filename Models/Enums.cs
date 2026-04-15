namespace StaffTaskManagement.Models;

public enum UserRole
{
    Admin = 1,
    Manager = 2,
    Employee = 3
}

public enum TaskPriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

public enum TaskStatus
{
    NotStarted = 1,
    InProgress = 2,
    OnHold = 3,
    Completed = 4,
    Cancelled = 5
}

public enum NotificationType
{
    TaskAssigned = 1,
    TaskUpdated = 2,
    DeadlineApproaching = 3,
    TaskOverdue = 4,
    CommentAdded = 5,
    General = 6
}


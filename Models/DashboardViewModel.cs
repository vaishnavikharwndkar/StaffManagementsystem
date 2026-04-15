namespace StaffTaskManagement.Models;

public class DashboardViewModel
{
    public int TotalTasks { get; set; }
    public int MyTasks { get; set; }
    public int PendingTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int DueSoonTasks { get; set; }

    public bool IsAdminOverview { get; set; }
    public int TotalUsers { get; set; }
    public int TotalDepartments { get; set; }
}


namespace StaffTaskManagement.Models;

public class ManagerDashboardViewModel
{
    public string? ManagerName { get; set; }
    public string? DepartmentName { get; set; }
    public int TeamMembers { get; set; }
    public int DepartmentTasks { get; set; }
    public int OpenTeamTasks { get; set; }
    public int CompletedTeamTasks { get; set; }
}

public class EmployeeDashboardViewModel
{
    public string? EmployeeName { get; set; }
    public int MyTasks { get; set; }
    public int PendingTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int OverdueTasks { get; set; }
}


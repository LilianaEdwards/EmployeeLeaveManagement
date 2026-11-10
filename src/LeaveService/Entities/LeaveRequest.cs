
namespace LeaveService.Entities;

public enum LeaveStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
}

public enum LeaveType
{
    Annual = 0,
    Sick = 1,
    Paid = 2,
    Unpaid = 3
}

public class LeaveRequest
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // References an employee managed by EmployeeService.
    public Guid EmployeeId { get; set; }

    public LeaveType Type { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public string Reason { get; set; } = string.Empty;

    public LeaveStatus Status { get; set; } = LeaveStatus.Pending;

    public DateTime CreatedAtUtc { get; set; } =
        DateTime.UtcNow;

    public DateTime? ReviewedAtUtc { get; set; }

    public Guid? ReviewedByEmployeeId { get; set; }

    public string? ReviewComment { get; set; }
}

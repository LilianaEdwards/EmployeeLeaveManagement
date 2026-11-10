
using LeaveService.Entities;

namespace LeaveService.DTOs;

public class LeaveBalanceDto
{
    public LeaveType Type { get; set; }

    public int AllowanceDays { get; set; }

    public int UsedDays { get; set; }

    public int RemainingDays { get; set; }

    public string Period { get; set; } = string.Empty;
}

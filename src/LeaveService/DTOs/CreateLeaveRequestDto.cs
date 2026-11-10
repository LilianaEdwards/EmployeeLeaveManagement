
using System.ComponentModel.DataAnnotations;
using LeaveService.Entities;

namespace LeaveService.DTOs;

public class CreateLeaveRequestDto
{
    [Required]
    public Guid EmployeeId { get; set; }

    [EnumDataType(typeof(LeaveType))]
    public LeaveType Type { get; set; }

    [Required]
    public DateOnly StartDate { get; set; }

    [Required]
    public DateOnly EndDate { get; set; }

    [Required]
    [StringLength(500, MinimumLength = 5)]
    public string Reason { get; set; } = string.Empty;
}

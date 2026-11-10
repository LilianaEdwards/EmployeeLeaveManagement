
using System.ComponentModel.DataAnnotations;
using LeaveService.Entities;

namespace LeaveService.DTOs;

public class ReviewLeaveRequestDto
{
    [EnumDataType(typeof(LeaveStatus))]
    public LeaveStatus Decision { get; set; }

    [Required]
    public Guid ReviewerEmployeeId { get; set; }

    [StringLength(500)]
    public string? Comment { get; set; }
}

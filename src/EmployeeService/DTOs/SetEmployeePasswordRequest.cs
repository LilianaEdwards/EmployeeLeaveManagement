using System.ComponentModel.DataAnnotations;

namespace EmployeeService.DTOs;

public class SetEmployeePasswordRequest
{
    [Required]
    [MinLength(12)]
    [MaxLength(128)]
    public string Password { get; set; } = string.Empty;
}
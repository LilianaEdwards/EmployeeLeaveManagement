namespace EmployeeService.DTOs;

public record EmployeeResponse(
    Guid Id,
    string FullName,
    string Email,
    string Department,
    string Role,
    DateTime CreatedAtUtc
);
namespace EmployeeService.DTOs;

public record LoginResponse(
    string AccessToken,
    DateTime ExpiresAtUtc,
    Guid EmployeeId,
    string FullName,
    string Email,
    string Role
);
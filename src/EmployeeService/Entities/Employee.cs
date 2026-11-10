
namespace EmployeeService.Entities;

public class Employee
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Department { get; set; } = string.Empty;

    // Stores the securely hashed password, never the plain-text password.
    public string? PasswordHash { get; set; }

    // New employees receive the Employee role by default.
    public string Role { get; set; } = "Employee";

    public DateTime CreatedAtUtc { get; set; } =
        DateTime.UtcNow;
}

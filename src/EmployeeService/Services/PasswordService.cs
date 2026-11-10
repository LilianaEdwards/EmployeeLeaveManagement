
using EmployeeService.Entities;
using Microsoft.AspNetCore.Identity;

namespace EmployeeService.Services;

public class PasswordService
{
    private readonly PasswordHasher<Employee> _hasher = new();

    public string HashPassword(Employee employee, string password)
    {
        return _hasher.HashPassword(employee, password);
    }

    public bool VerifyPassword(
        Employee employee,
        string password)
    {
        if (string.IsNullOrWhiteSpace(employee.PasswordHash))
        {
            return false;
        }

        var result = _hasher.VerifyHashedPassword(
            employee,
            employee.PasswordHash,
            password);

        return result != PasswordVerificationResult.Failed;
    }
}

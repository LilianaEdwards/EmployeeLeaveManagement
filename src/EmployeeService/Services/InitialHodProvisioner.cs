using EmployeeService.Data;
using EmployeeService.Entities;
using Microsoft.EntityFrameworkCore;

namespace EmployeeService.Services;

public class InitialHodProvisioner
{
    private readonly EmployeeDbContext _db;
    private readonly PasswordService _passwordService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<InitialHodProvisioner> _logger;

    public InitialHodProvisioner(
        EmployeeDbContext db,
        PasswordService passwordService,
        IConfiguration configuration,
        ILogger<InitialHodProvisioner> logger)
    {
        _db = db;
        _passwordService = passwordService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task ProvisionAsync(
        CancellationToken cancellationToken = default)
    {
        var fullName = _configuration["BootstrapHod:FullName"];
        var email = _configuration["BootstrapHod:Email"];
        var department = _configuration["BootstrapHod:Department"];
        var password = _configuration["BootstrapHod:Password"];

        if (string.IsNullOrWhiteSpace(fullName) &&
            string.IsNullOrWhiteSpace(email) &&
            string.IsNullOrWhiteSpace(department) &&
            string.IsNullOrWhiteSpace(password))
        {
            _logger.LogWarning(
                "Initial HOD bootstrap is not configured. No HOD account was created.");

            return;
        }

        if (string.IsNullOrWhiteSpace(fullName) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(department) ||
            string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                "All BootstrapHod settings must be configured.");
        }

        if (fullName.Trim().Length < 2 ||
            fullName.Trim().Length > 100)
        {
            throw new InvalidOperationException(
                "Bootstrap HOD full name must contain 2 to 100 characters.");
        }

        if (email.Trim().Length > 254 ||
            !new System.ComponentModel.DataAnnotations.EmailAddressAttribute()
                .IsValid(email.Trim()))
        {
            throw new InvalidOperationException(
                "Bootstrap HOD email address is invalid.");
        }

        if (department.Trim().Length > 100)
        {
            throw new InvalidOperationException(
                "Bootstrap HOD department cannot exceed 100 characters.");
        }

        if (password.Length < 12 || password.Length > 128)
        {
            throw new InvalidOperationException(
                "Bootstrap HOD password must contain 12 to 128 characters.");
        }

        var normalizedEmail = email.Trim().ToLowerInvariant();

        var existingEmployee = await _db.Employees
            .FirstOrDefaultAsync(
                e => e.Email == normalizedEmail,
                cancellationToken);

        if (existingEmployee is not null)
        {
            if (!string.Equals(
                existingEmployee.Role,
                "HOD",
                StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "The configured bootstrap email already belongs to an account that is not an HOD. No role changes were made.");
            }

            _logger.LogInformation(
                "Initial HOD account already exists. No changes were made.");

            return;
        }

        var hod = new Employee
        {
            FullName = fullName.Trim(),
            Email = normalizedEmail,
            Department = department.Trim(),
            Role = "HOD"
        };

        hod.PasswordHash = _passwordService.HashPassword(
            hod,
            password);

        _db.Employees.Add(hod);

        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Initial HOD account was created successfully.");
    }
}
using EmployeeService.Data;
using EmployeeService.DTOs;
using EmployeeService.Entities;
using Microsoft.EntityFrameworkCore;

namespace EmployeeService.Services;

public class EmployeeManager
{
    private readonly EmployeeDbContext _db;
    private readonly PasswordService _passwordService;

    public EmployeeManager(
        EmployeeDbContext db,
        PasswordService passwordService)
    {
        _db = db;
        _passwordService = passwordService;
    }

    public async Task<EmployeeResponse?> CreateAsync(
        CreateEmployeeRequest request,
        CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var emailExists = await _db.Employees
            .AnyAsync(
                e => e.Email == email,
                cancellationToken);

        if (emailExists)
        {
            return null;
        }

        var employee = new Employee
        {
            FullName = request.FullName.Trim(),
            Email = email,
            Department = request.Department.Trim()
        };

        _db.Employees.Add(employee);

        await _db.SaveChangesAsync(cancellationToken);

        return ToResponse(employee);
    }

    public async Task<EmployeeResponse?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var employee = await _db.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(
                e => e.Id == id,
                cancellationToken);

        return employee is null
            ? null
            : ToResponse(employee);
    }

    public async Task<List<EmployeeResponse>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _db.Employees
            .AsNoTracking()
            .OrderBy(e => e.FullName)
            .Select(e => new EmployeeResponse(
                e.Id,
                e.FullName,
                e.Email,
                e.Department,
                e.Role,
                e.CreatedAtUtc))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> SetPasswordAsync(
        Guid employeeId,
        string password,
        CancellationToken cancellationToken = default)
    {
        var employee = await _db.Employees
            .FirstOrDefaultAsync(
                e => e.Id == employeeId,
                cancellationToken);

        if (employee is null)
        {
            return false;
        }

        employee.PasswordHash = _passwordService.HashPassword(
            employee,
            password);

        await _db.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static EmployeeResponse ToResponse(
    Employee employee)
{
    return new EmployeeResponse(
        employee.Id,
        employee.FullName,
        employee.Email,
        employee.Department,
        employee.Role,
        employee.CreatedAtUtc);
}
}
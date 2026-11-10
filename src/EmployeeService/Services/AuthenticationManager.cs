
using EmployeeService.Data;
using EmployeeService.DTOs;
using Microsoft.EntityFrameworkCore;

namespace EmployeeService.Services;

public class AuthenticationManager
{
    private readonly EmployeeDbContext _db;
    private readonly PasswordService _passwordService;
    private readonly JwtTokenService _jwtTokenService;

    public AuthenticationManager(
        EmployeeDbContext db,
        PasswordService passwordService,
        JwtTokenService jwtTokenService)
    {
        _db = db;
        _passwordService = passwordService;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<LoginResponse?> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var employee = await _db.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(
                e => e.Email == email,
                cancellationToken);

        if (employee is null)
        {
            return null;
        }

        var passwordIsValid =
            _passwordService.VerifyPassword(
                employee,
                request.Password);

        if (!passwordIsValid)
        {
            return null;
        }

        var (accessToken, expiresAtUtc) =
            _jwtTokenService.GenerateToken(employee);

        return new LoginResponse(
            accessToken,
            expiresAtUtc,
            employee.Id,
            employee.FullName,
            employee.Email,
            employee.Role
        );
    }
}

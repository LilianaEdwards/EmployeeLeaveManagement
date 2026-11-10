using EmployeeService.DTOs;
using EmployeeService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly EmployeeManager _employeeManager;

    public EmployeesController(EmployeeManager employeeManager)
    {
        _employeeManager = employeeManager;
    }

    [HttpPost]
    [Authorize(Roles = "HOD")]
    public async Task<IActionResult> Create(
        [FromBody] CreateEmployeeRequest request,
        CancellationToken cancellationToken)
    {
        var employee = await _employeeManager.CreateAsync(
            request,
            cancellationToken);

        if (employee is null)
        {
            return Conflict(new
            {
                message = "An employee with this email already exists."
            });
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = employee.Id },
            employee);
    }

    [HttpPost("{id:guid}/set-password")]
    [Authorize(Roles = "HOD")]
    public async Task<IActionResult> SetPassword(
        Guid id,
        [FromBody] SetEmployeePasswordRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await _employeeManager.SetPasswordAsync(
            id,
            request.Password,
            cancellationToken);

        if (!updated)
        {
            return NotFound(new
            {
                message = "Employee not found."
            });
        }

        return Ok(new
        {
            message = "Employee password has been set successfully."
        });
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "EmployeeServiceOnly")]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var employee = await _employeeManager.GetByIdAsync(
            id,
            cancellationToken);

        if (employee is null)
        {
            return NotFound();
        }

        return Ok(employee);
    }

    [HttpGet]
    [Authorize(Roles = "HOD")]
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken)
    {
        var employees = await _employeeManager.GetAllAsync(
            cancellationToken);

        return Ok(employees);
    }
}

using System.Net.Http;
using System.Security.Claims;
using LeaveService.DTOs;
using LeaveService.Entities;
using LeaveService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeaveService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LeaveRequestsController : ControllerBase
{
    private readonly LeaveManager _leaveManager;
    private readonly ILogger<LeaveRequestsController> _logger;

    public LeaveRequestsController(
        LeaveManager leaveManager,
        ILogger<LeaveRequestsController> logger)
    {
        _leaveManager = leaveManager;
        _logger = logger;
    }

    private bool TryGetEmployeeId(out Guid employeeId)
    {
        return Guid.TryParse(
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            out employeeId);
    }

    private bool IsManagement()
    {
        return User.IsInRole("HOD") ||
            User.IsInRole("HR");
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateLeaveRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!TryGetEmployeeId(out var employeeId))
        {
            return Unauthorized();
        }

        // Never trust the employee ID supplied by the client.
        request.EmployeeId = employeeId;

        try
        {
            var leaveRequest = await _leaveManager.CreateAsync(
                request,
                cancellationToken);

            return CreatedAtAction(
                nameof(GetById),
                new { id = leaveRequest.Id },
                leaveRequest);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(
                exception,
                "EmployeeService request failed while creating a leave request.");

            return StatusCode(503, new
            {
                message = "Employee validation service is unavailable. Please try again later."
            });
        }
        catch (TaskCanceledException exception)
        {
            _logger.LogError(
                exception,
                "EmployeeService request timed out while creating a leave request.");

            return StatusCode(503, new
            {
                message = "Employee validation service timed out. Please try again later."
            });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? employeeId,
        [FromQuery] LeaveStatus? status,
        CancellationToken cancellationToken)
    {
        if (status.HasValue && !Enum.IsDefined(status.Value))
        {
            return BadRequest(new { message = "Invalid leave status." });
        }

        if (!IsManagement())
        {
            if (!TryGetEmployeeId(out var currentEmployeeId))
            {
                return Unauthorized();
            }

            // Employees can only retrieve their own requests.
            employeeId = currentEmployeeId;
        }

        var requests = await _leaveManager.GetAllAsync(
            employeeId,
            status,
            cancellationToken);

        return Ok(requests);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        if (!TryGetEmployeeId(out var currentEmployeeId))
        {
            return Unauthorized();
        }

        var request = await _leaveManager.GetByIdAsync(
            id,
            cancellationToken);

        if (request is null)
        {
            return NotFound(new
            {
                message = "Leave request not found."
            });
        }

        // Hide requests belonging to other employees.
        if (!IsManagement() && request.EmployeeId != currentEmployeeId)
        {
            return NotFound(new
            {
                message = "Leave request not found."
            });
        }

        return Ok(request);
    }

    [HttpPatch("{id:guid}/review")]
    [Authorize(Roles = "HOD,HR")]
    public async Task<IActionResult> Review(
        Guid id,
        [FromBody] ReviewLeaveRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!TryGetEmployeeId(out var reviewerEmployeeId))
        {
            return Unauthorized();
        }

        try
        {
            // Reviewer identity comes from the authenticated JWT.
            var leaveRequest = await _leaveManager.ReviewAsync(
                id,
                request.Decision,
                reviewerEmployeeId,
                request.Comment,
                cancellationToken);

            if (leaveRequest is null)
            {
                return NotFound(new
                {
                    message = "Leave request not found."
                });
            }

            return Ok(leaveRequest);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(
                exception,
                "EmployeeService request failed while reviewing a leave request.");

            return StatusCode(503, new
            {
                message = "Employee validation service is unavailable. Please try again later."
            });
        }
        catch (TaskCanceledException exception)
        {
            _logger.LogError(
                exception,
                "EmployeeService request timed out while reviewing a leave request.");

            return StatusCode(503, new
            {
                message = "Employee validation service timed out. Please try again later."
            });
        }
    }

    [HttpGet("{employeeId:guid}/balance")]
    public async Task<IActionResult> GetEmployeeBalance(
        Guid employeeId,
        CancellationToken cancellationToken)
    {
        if (!TryGetEmployeeId(out var currentEmployeeId))
        {
            return Unauthorized();
        }

        if (!IsManagement() && employeeId != currentEmployeeId)
        {
            return NotFound(new
            {
                message = "Employee balance not found."
            });
        }

        try
        {
            var balances = await _leaveManager.GetEmployeeBalanceAsync(
                employeeId,
                cancellationToken);

            return Ok(balances);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }
}

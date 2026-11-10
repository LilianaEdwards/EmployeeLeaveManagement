
using LeaveService.Data;
using LeaveService.DTOs;
using LeaveService.Entities;
using Microsoft.EntityFrameworkCore;

namespace LeaveService.Services;

public class LeaveManager
{
    private readonly LeaveDbContext _db;
    private readonly EmployeeApiClient _employeeApiClient;

    public LeaveManager(
        LeaveDbContext db,
        EmployeeApiClient employeeApiClient)
    {
        _db = db;
        _employeeApiClient = employeeApiClient;
    }

    public async Task<LeaveRequest> CreateAsync(
        CreateLeaveRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (request.EmployeeId == Guid.Empty)
        {
            throw new ArgumentException(
                "A valid employee ID is required.");
        }

        var employeeExists = await _employeeApiClient.ExistsAsync(
            request.EmployeeId,
            cancellationToken);

        if (!employeeExists)
        {
            throw new ArgumentException(
                "The specified employee does not exist.");
        }

        if (!Enum.IsDefined(request.Type))
        {
            throw new ArgumentException(
                "The selected leave type is invalid.");
        }

        if (request.EndDate < request.StartDate)
        {
            throw new ArgumentException(
                "End date cannot be earlier than start date.");
        }

        var reason = request.Reason?.Trim();

        if (string.IsNullOrWhiteSpace(reason) ||
            reason.Length < 5 ||
            reason.Length > 500)
        {
            throw new ArgumentException(
                "Reason must contain between 5 and 500 characters.");
        }

    var existingRequests = await _db.LeaveRequests
            .AsNoTracking()
            .Where(existing =>
            existing.EmployeeId == request.EmployeeId &&
            existing.Status != LeaveStatus.Rejected)
            .ToListAsync(cancellationToken);

    var candidateRequest = new LeaveRequest
    {
            EmployeeId = request.EmployeeId,
            Type = request.Type,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Reason = reason
    };

    LeaveAllowanceValidator.Validate(
            candidateRequest,
            existingRequests);

        var leaveRequest = new LeaveRequest
        {
            EmployeeId = request.EmployeeId,
            Type = request.Type,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Reason = reason,
            Status = LeaveStatus.Pending,
            CreatedAtUtc = DateTime.UtcNow
        };

        _db.LeaveRequests.Add(leaveRequest);

        await _db.SaveChangesAsync(cancellationToken);

        return leaveRequest;
    }

    public async Task<LeaveRequest?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _db.LeaveRequests
            .AsNoTracking()
            .FirstOrDefaultAsync(
                request => request.Id == id,
                cancellationToken);
    }

    public async Task<List<LeaveRequest>> GetAllAsync(
        Guid? employeeId = null,
        LeaveStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = _db.LeaveRequests.AsNoTracking();

        if (employeeId.HasValue)
        {
            query = query.Where(
                request => request.EmployeeId == employeeId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(
                request => request.Status == status.Value);
        }

        return await query
            .OrderByDescending(request => request.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<LeaveRequest?> ReviewAsync(
    Guid id,
    LeaveStatus decision,
    Guid reviewerEmployeeId,
    string? comment = null,
    CancellationToken cancellationToken = default)
{
    if (decision is not LeaveStatus.Approved
        and not LeaveStatus.Rejected)
    {
        throw new ArgumentException(
            "A review decision must be Approved or Rejected.");
    }

    if (reviewerEmployeeId == Guid.Empty)
    {
        throw new ArgumentException(
            "A valid reviewer employee ID is required.");
    }

    if (comment?.Length > 500)
    {
        throw new ArgumentException(
            "Review comment cannot exceed 500 characters.");
    }

    var leaveRequest = await _db.LeaveRequests
        .FirstOrDefaultAsync(
            request => request.Id == id,
            cancellationToken);

    if (leaveRequest is null)
    {
        return null;
    }

    if (leaveRequest.Status != LeaveStatus.Pending)
    {
        throw new InvalidOperationException(
            "Only pending leave requests can be reviewed.");
    }

    // Prevent employees from reviewing their own requests.
    if (leaveRequest.EmployeeId == reviewerEmployeeId)
    {
        throw new InvalidOperationException(
            "You cannot review your own leave request.");
    }

    // Retrieve the request owner's information.
    var requestOwner = await _employeeApiClient.GetByIdAsync(
        leaveRequest.EmployeeId,
        cancellationToken);

    if (requestOwner is null)
    {
        throw new InvalidOperationException(
            "The employee who submitted this request no longer exists.");
    }

    // Retrieve the reviewer's information.
    var reviewer = await _employeeApiClient.GetByIdAsync(
        reviewerEmployeeId,
        cancellationToken);

    if (reviewer is null)
    {
        throw new ArgumentException(
            "The specified reviewer employee does not exist.");
    }

    var ownerRole = requestOwner.Role.Trim();
    var reviewerRole = reviewer.Role.Trim();

    // Enforce the approval hierarchy.
    var canReview = ownerRole switch
    {
        "Employee" =>
            reviewerRole is "HOD" or "HR",

        "HOD" =>
            reviewerRole == "HR",

        "HR" =>
            reviewerRole == "HR",

        _ => false
    };

    if (!canReview)
    {
        throw new InvalidOperationException(
            "You are not authorized to review this employee's leave request.");
    }

    if (decision == LeaveStatus.Approved)
    {
        var approvedRequests = await _db.LeaveRequests
            .AsNoTracking()
            .Where(request =>
                request.EmployeeId == leaveRequest.EmployeeId &&
                request.Status == LeaveStatus.Approved)
            .ToListAsync(cancellationToken);

        var hasOverlap = approvedRequests.Any(request =>
            leaveRequest.StartDate <= request.EndDate &&
            leaveRequest.EndDate >= request.StartDate);

        if (hasOverlap)
        {
            throw new InvalidOperationException(
                "The requested dates overlap with existing approved leave.");
        }

        LeaveAllowanceValidator.Validate(
            leaveRequest,
            approvedRequests);
    }

    leaveRequest.Status = decision;
    leaveRequest.ReviewedByEmployeeId = reviewerEmployeeId;
    leaveRequest.ReviewedAtUtc = DateTime.UtcNow;
    leaveRequest.ReviewComment = comment?.Trim();

    await _db.SaveChangesAsync(cancellationToken);

    return leaveRequest;
}

public async Task<List<LeaveBalanceDto>> GetEmployeeBalanceAsync(
    Guid employeeId,
    CancellationToken cancellationToken = default)
{
    if (employeeId == Guid.Empty)
    {
        throw new ArgumentException(
            "A valid employee ID is required.");
    }

    var today = DateOnly.FromDateTime(DateTime.Today);
    var currentYear = today.Year;
    var currentMonth = today.Month;

    var approvedRequests = await _db.LeaveRequests
        .AsNoTracking()
        .Where(request =>
            request.EmployeeId == employeeId &&
            request.Status == LeaveStatus.Approved)
        .ToListAsync(cancellationToken);

    var allowances = new Dictionary<LeaveType, int>
    {
        { LeaveType.Annual, 14 },
        { LeaveType.Sick, 5 },
        { LeaveType.Paid, 5 },
        { LeaveType.Unpaid, 5 }
    };

    var balances = new List<LeaveBalanceDto>();

    foreach (var allowance in allowances)
    {
        var isUnpaid = allowance.Key == LeaveType.Unpaid;

        var usedDays = 0;

        foreach (var request in approvedRequests)
        {
            if (request.Type != allowance.Key)
            {
                continue;
            }

            var start = request.StartDate;
            var end = request.EndDate;

            if (isUnpaid)
            {
                var periodStart = new DateOnly(
                    currentYear, currentMonth, 1);

                var periodEnd = periodStart.AddMonths(1).AddDays(-1);

                if (start < periodStart)
                {
                    start = periodStart;
                }

                if (end > periodEnd)
                {
                    end = periodEnd;
                }

                if (start > end)
                {
                    continue;
                }
            }
            else
            {
                var periodStart = new DateOnly(currentYear, 1, 1);
                var periodEnd = new DateOnly(currentYear, 12, 31);

                if (start < periodStart)
                {
                    start = periodStart;
                }

                if (end > periodEnd)
                {
                    end = periodEnd;
                }

                if (start > end)
                {
                    continue;
                }
            }

            usedDays += LeaveDayCalculator.CountWorkingDays(
                start,
                end);
        }

        balances.Add(new LeaveBalanceDto
        {
            Type = allowance.Key,
            AllowanceDays = allowance.Value,
            UsedDays = usedDays,
            RemainingDays = Math.Max(
                0,
                allowance.Value - usedDays),
            Period = isUnpaid
                ? $"{currentYear}-{currentMonth:D2}"
                : currentYear.ToString()
        });
    }

    return balances;
}

}

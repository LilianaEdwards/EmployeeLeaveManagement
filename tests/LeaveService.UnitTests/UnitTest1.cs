using System.Net;
using System.Net.Http.Json;
using LeaveService.Data;
using LeaveService.DTOs;
using LeaveService.Entities;
using LeaveService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace LeaveService.UnitTests;

public class LeaveManagerTests
{
    private static LeaveManager CreateManager(
        Guid? nonexistentEmployeeId = null,
        string ownerRole = "Employee",
        string reviewerRole = "HOD")
    {
        var options = new DbContextOptionsBuilder<LeaveDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var db = new LeaveDbContext(options);

        var handler = new FakeEmployeeHandler(
            nonexistentEmployeeId,
            ownerRole,
            reviewerRole);

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost")
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["ServiceAuthentication:ApiKey"] = "test-api-key"
                })
            .Build();

        var employeeApiClient = new EmployeeApiClient(
            httpClient,
            configuration);

        return new LeaveManager(db, employeeApiClient);
    }

    private sealed class FakeEmployeeHandler : HttpMessageHandler
    {
        private readonly Guid? _nonexistentEmployeeId;
        private readonly string _ownerRole;
        private readonly string _reviewerRole;
        private Guid? _ownerEmployeeId;

        public FakeEmployeeHandler(
            Guid? nonexistentEmployeeId = null,
            string ownerRole = "Employee",
            string reviewerRole = "HOD")
        {
            _nonexistentEmployeeId = nonexistentEmployeeId;
            _ownerRole = ownerRole;
            _reviewerRole = reviewerRole;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var requestedEmployeeIdText = request.RequestUri?
                .AbsolutePath
                .Split('/', StringSplitOptions.RemoveEmptyEntries)
                .LastOrDefault();

            if (!Guid.TryParse(
                requestedEmployeeIdText,
                out var requestedEmployeeId))
            {
                return Task.FromResult(
                    new HttpResponseMessage(HttpStatusCode.BadRequest));
            }

            if (_nonexistentEmployeeId == requestedEmployeeId)
            {
                return Task.FromResult(
                    new HttpResponseMessage(HttpStatusCode.NotFound));
            }

            _ownerEmployeeId ??= requestedEmployeeId;

            var role = requestedEmployeeId == _ownerEmployeeId
                ? _ownerRole
                : _reviewerRole;

            var employee = new EmployeeInfo(
                requestedEmployeeId,
                "Test Employee",
                "test.employee@example.com",
                "Engineering",
                role,
                DateTime.UtcNow);

            return Task.FromResult(
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(employee)
                });
        }
    }

    private static CreateLeaveRequestDto ValidRequest()
    {
        return new CreateLeaveRequestDto
        {
            EmployeeId = Guid.NewGuid(),
            Type = LeaveType.Annual,
            StartDate = new DateOnly(2026, 11, 2),
            EndDate = new DateOnly(2026, 11, 4),
            Reason = "Annual leave for personal matters"
        };
    }

    // --------------------------------------------------
    // CREATE REQUEST TESTS
    // --------------------------------------------------

    [Fact]
    public async Task CreateAsync_ValidRequest_CreatesPendingRequest()
    {
        var manager = CreateManager();

        var result = await manager.CreateAsync(ValidRequest());

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(LeaveStatus.Pending, result.Status);
        Assert.Equal(LeaveType.Annual, result.Type);
        Assert.Equal("Annual leave for personal matters", result.Reason);
        Assert.Null(result.ReviewedAtUtc);
    }

    [Fact]
    public async Task CreateAsync_EmptyEmployeeId_ThrowsArgumentException()
    {
        var manager = CreateManager();
        var request = ValidRequest();
        request.EmployeeId = Guid.Empty;

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => manager.CreateAsync(request));

        Assert.Contains("employee ID", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_InvalidLeaveType_ThrowsArgumentException()
    {
        var manager = CreateManager();
        var request = ValidRequest();
        request.Type = (LeaveType)999;

        await Assert.ThrowsAsync<ArgumentException>(
            () => manager.CreateAsync(request));
    }

    [Fact]
    public async Task CreateAsync_EndDateBeforeStartDate_ThrowsArgumentException()
    {
        var manager = CreateManager();
        var request = ValidRequest();
        request.StartDate = new DateOnly(2026, 11, 5);
        request.EndDate = new DateOnly(2026, 11, 4);

        await Assert.ThrowsAsync<ArgumentException>(
            () => manager.CreateAsync(request));
    }

    [Fact]
    public async Task CreateAsync_ReasonTooShort_ThrowsArgumentException()
    {
        var manager = CreateManager();
        var request = ValidRequest();
        request.Reason = "No";

        await Assert.ThrowsAsync<ArgumentException>(
            () => manager.CreateAsync(request));
    }

    // --------------------------------------------------
    // REVIEW REQUEST TESTS
    // --------------------------------------------------

    [Fact]
    public async Task ReviewAsync_ApprovedPendingRequest_UpdatesReviewDetails()
    {
        var manager = CreateManager();
        var created = await manager.CreateAsync(ValidRequest());
        var reviewerId = Guid.NewGuid();

        var result = await manager.ReviewAsync(
            created.Id,
            LeaveStatus.Approved,
            reviewerId,
            "Approved by manager");

        Assert.NotNull(result);
        Assert.Equal(LeaveStatus.Approved, result.Status);
        Assert.Equal(reviewerId, result.ReviewedByEmployeeId);
        Assert.NotNull(result.ReviewedAtUtc);
        Assert.Equal("Approved by manager", result.ReviewComment);
    }

    [Fact]
    public async Task ReviewAsync_RejectsPendingRequest()
    {
        var manager = CreateManager();
        var created = await manager.CreateAsync(ValidRequest());

        var result = await manager.ReviewAsync(
            created.Id,
            LeaveStatus.Rejected,
            Guid.NewGuid(),
            "Insufficient leave balance");

        Assert.NotNull(result);
        Assert.Equal(LeaveStatus.Rejected, result.Status);
    }

    [Fact]
    public async Task ReviewAsync_AlreadyReviewedRequest_ThrowsInvalidOperationException()
    {
        var manager = CreateManager();
        var created = await manager.CreateAsync(ValidRequest());

        await manager.ReviewAsync(
            created.Id,
            LeaveStatus.Approved,
            Guid.NewGuid());

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => manager.ReviewAsync(
                created.Id,
                LeaveStatus.Rejected,
                Guid.NewGuid()));
    }

    [Fact]
    public async Task ReviewAsync_UnknownRequest_ReturnsNull()
    {
        var manager = CreateManager();

        var result = await manager.ReviewAsync(
            Guid.NewGuid(),
            LeaveStatus.Approved,
            Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task ReviewAsync_NonexistentReviewer_ThrowsArgumentException()
    {
        var nonexistentReviewerId = Guid.NewGuid();
        var manager = CreateManager(nonexistentReviewerId);
        var created = await manager.CreateAsync(ValidRequest());

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => manager.ReviewAsync(
                created.Id,
                LeaveStatus.Approved,
                nonexistentReviewerId,
                "Testing reviewer validation"));

        Assert.Contains(
            "reviewer employee does not exist",
            exception.Message,
            StringComparison.OrdinalIgnoreCase);
    }

    // --------------------------------------------------
    // WORKING DAY CALCULATOR TESTS
    // --------------------------------------------------

    [Fact]
    public void CountWorkingDays_OneWeekday_ReturnsOne()
    {
        var result = LeaveDayCalculator.CountWorkingDays(
            new DateOnly(2026, 10, 26),
            new DateOnly(2026, 10, 26));

        Assert.Equal(1, result);
    }

    [Fact]
    public void CountWorkingDays_WeekendOnly_ReturnsZero()
    {
        var result = LeaveDayCalculator.CountWorkingDays(
            new DateOnly(2026, 10, 24),
            new DateOnly(2026, 10, 25));

        Assert.Equal(0, result);
    }

    [Fact]
    public void CountWorkingDays_FridayToMonday_ReturnsTwo()
    {
        var result = LeaveDayCalculator.CountWorkingDays(
            new DateOnly(2026, 10, 23),
            new DateOnly(2026, 10, 26));

        Assert.Equal(2, result);
    }

    // --------------------------------------------------
    // LEAVE ALLOWANCE VALIDATION TESTS
    // --------------------------------------------------

    [Fact]
    public async Task CreateAsync_AnnualLeaveExceedsAllowance_ThrowsInvalidOperationException()
    {
        var manager = CreateManager();
        var employeeId = Guid.NewGuid();

        // First request: 10 working days of annual leave.
        var firstRequest = new CreateLeaveRequestDto
        {
            EmployeeId = employeeId,
            Type = LeaveType.Annual,
            StartDate = new DateOnly(2026, 11, 2),
            EndDate = new DateOnly(2026, 11, 13),
            Reason = "Annual leave for a planned vacation"
        };

        var first = await manager.CreateAsync(firstRequest);

        await manager.ReviewAsync(
            first.Id,
            LeaveStatus.Approved,
            Guid.NewGuid());

        // Second request: another 5 working days.
        // Total would be 15, exceeding the annual allowance of 14.
        var secondRequest = new CreateLeaveRequestDto
        {
            EmployeeId = employeeId,
            Type = LeaveType.Annual,
            StartDate = new DateOnly(2026, 11, 16),
            EndDate = new DateOnly(2026, 11, 20),
            Reason = "Additional annual leave request"
        };

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => manager.CreateAsync(secondRequest));

        Assert.Contains("14 working days", exception.Message);

        var requests = await manager.GetAllAsync(employeeId);
        Assert.Single(requests);
    }

    [Fact]
    public async Task CreateAsync_UnpaidLeaveExceedsMonthlyAllowance_ThrowsInvalidOperationException()
    {
        var manager = CreateManager();
        var employeeId = Guid.NewGuid();

        // First request: 3 working days in November.
        var firstRequest = new CreateLeaveRequestDto
        {
            EmployeeId = employeeId,
            Type = LeaveType.Unpaid,
            StartDate = new DateOnly(2026, 11, 2),
            EndDate = new DateOnly(2026, 11, 4),
            Reason = "Unpaid leave for personal matters"
        };

        var first = await manager.CreateAsync(firstRequest);

        await manager.ReviewAsync(
            first.Id,
            LeaveStatus.Approved,
            Guid.NewGuid());

        // Second request: another 3 working days in November.
        // Total would be 6, exceeding the monthly allowance of 5.
        var secondRequest = new CreateLeaveRequestDto
        {
            EmployeeId = employeeId,
            Type = LeaveType.Unpaid,
            StartDate = new DateOnly(2026, 11, 5),
            EndDate = new DateOnly(2026, 11, 9),
            Reason = "Additional unpaid leave request"
        };

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => manager.CreateAsync(secondRequest));

        Assert.Contains("5 working days", exception.Message);

        var requests = await manager.GetAllAsync(employeeId);
        Assert.Single(requests);
    }

    [Fact]
    public async Task CreateAsync_SickLeaveExceedsAnnualAllowance_ThrowsInvalidOperationException()
    {
        var manager = CreateManager();
        var employeeId = Guid.NewGuid();

        // First request: 4 working days of sick leave.
        var firstRequest = new CreateLeaveRequestDto
        {
            EmployeeId = employeeId,
            Type = LeaveType.Sick,
            StartDate = new DateOnly(2026, 11, 2),
            EndDate = new DateOnly(2026, 11, 5),
            Reason = "Sick leave for recovery"
        };

        var first = await manager.CreateAsync(firstRequest);

        await manager.ReviewAsync(
            first.Id,
            LeaveStatus.Approved,
            Guid.NewGuid());

        // Second request: another 2 working days.
        // Total would be 6, exceeding the annual allowance of 5.
        var secondRequest = new CreateLeaveRequestDto
        {
            EmployeeId = employeeId,
            Type = LeaveType.Sick,
            StartDate = new DateOnly(2026, 11, 6),
            EndDate = new DateOnly(2026, 11, 9),
            Reason = "Additional sick leave"
        };

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => manager.CreateAsync(secondRequest));

        Assert.Contains("5 working days", exception.Message);

        var requests = await manager.GetAllAsync(employeeId);
        Assert.Single(requests);
    }

    [Fact]
    public async Task CreateAsync_PaidLeaveExceedsAnnualAllowance_ThrowsInvalidOperationException()
    {
        var manager = CreateManager();
        var employeeId = Guid.NewGuid();

        // First request: 5 working days of paid leave.
        var firstRequest = new CreateLeaveRequestDto
        {
            EmployeeId = employeeId,
            Type = LeaveType.Paid,
            StartDate = new DateOnly(2026, 11, 2),
            EndDate = new DateOnly(2026, 11, 6),
            Reason = "Paid leave for personal matters"
        };

        var first = await manager.CreateAsync(firstRequest);

        await manager.ReviewAsync(
            first.Id,
            LeaveStatus.Approved,
            Guid.NewGuid());

        // Second request: another working day.
        // Total would be 6, exceeding the annual allowance of 5.
        var secondRequest = new CreateLeaveRequestDto
        {
            EmployeeId = employeeId,
            Type = LeaveType.Paid,
            StartDate = new DateOnly(2026, 11, 9),
            EndDate = new DateOnly(2026, 11, 9),
            Reason = "Additional paid leave request"
        };

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => manager.CreateAsync(secondRequest));

        Assert.Contains("5 working days", exception.Message);

        var requests = await manager.GetAllAsync(employeeId);
        Assert.Single(requests);
    }

    [Fact]
    public async Task CreateAsync_PendingLeaveReservesAllowance_ThrowsWhenLimitExceeded()
    {
        var manager = CreateManager();
        var employeeId = Guid.NewGuid();

        // First request is still Pending: 3 working days.
        var firstRequest = new CreateLeaveRequestDto
        {
            EmployeeId = employeeId,
            Type = LeaveType.Unpaid,
            StartDate = new DateOnly(2026, 11, 2),
            EndDate = new DateOnly(2026, 11, 4),
            Reason = "First pending unpaid leave request"
        };

        await manager.CreateAsync(firstRequest);

        // Another 3 working days would exceed the monthly limit.
        // Pending requests must reserve allowance too.
        var secondRequest = new CreateLeaveRequestDto
        {
            EmployeeId = employeeId,
            Type = LeaveType.Unpaid,
            StartDate = new DateOnly(2026, 11, 5),
            EndDate = new DateOnly(2026, 11, 9),
            Reason = "Second pending unpaid leave request"
        };

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => manager.CreateAsync(secondRequest));

        Assert.Contains("5 working days", exception.Message);

        var requests = await manager.GetAllAsync(employeeId);
        Assert.Single(requests);
        Assert.Equal(LeaveStatus.Pending, requests[0].Status);
    }

    [Fact]
    public async Task ReviewAsync_UnpaidLeaveInDifferentMonths_UsesSeparateAllowances()
    {
        var manager = CreateManager();
        var employeeId = Guid.NewGuid();

        var novemberRequest = new CreateLeaveRequestDto
        {
            EmployeeId = employeeId,
            Type = LeaveType.Unpaid,
            StartDate = new DateOnly(2026, 11, 2),
            EndDate = new DateOnly(2026, 11, 6),
            Reason = "Unpaid leave in November"
        };

        var november = await manager.CreateAsync(novemberRequest);

        await manager.ReviewAsync(
            november.Id,
            LeaveStatus.Approved,
            Guid.NewGuid());

        var decemberRequest = new CreateLeaveRequestDto
        {
            EmployeeId = employeeId,
            Type = LeaveType.Unpaid,
            StartDate = new DateOnly(2026, 12, 1),
            EndDate = new DateOnly(2026, 12, 5),
            Reason = "Unpaid leave in December"
        };

        var december = await manager.CreateAsync(decemberRequest);

        var result = await manager.ReviewAsync(
            december.Id,
            LeaveStatus.Approved,
            Guid.NewGuid());

        Assert.NotNull(result);
        Assert.Equal(LeaveStatus.Approved, result.Status);
    }

    // --------------------------------------------------
    // OVERLAPPING LEAVE TEST
    // --------------------------------------------------

    [Fact]
    public async Task ReviewAsync_OverlappingApprovedLeave_ThrowsInvalidOperationException()
    {
        var manager = CreateManager();
        var employeeId = Guid.NewGuid();

        var firstRequest = new CreateLeaveRequestDto
        {
            EmployeeId = employeeId,
            Type = LeaveType.Annual,
            StartDate = new DateOnly(2026, 11, 2),
            EndDate = new DateOnly(2026, 11, 4),
            Reason = "First annual leave request"
        };

        var first = await manager.CreateAsync(firstRequest);

        await manager.ReviewAsync(
            first.Id,
            LeaveStatus.Approved,
            Guid.NewGuid());

        var secondRequest = new CreateLeaveRequestDto
        {
            EmployeeId = employeeId,
            Type = LeaveType.Annual,
            StartDate = new DateOnly(2026, 11, 4),
            EndDate = new DateOnly(2026, 11, 6),
            Reason = "Overlapping annual leave"
        };

        var second = await manager.CreateAsync(secondRequest);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => manager.ReviewAsync(
                second.Id,
                LeaveStatus.Approved,
                Guid.NewGuid()));

        Assert.Contains(
            "overlap",
            exception.Message,
            StringComparison.OrdinalIgnoreCase);
    }

    // --------------------------------------------------
    // EMPLOYEE BALANCE TESTS
    // --------------------------------------------------

    [Fact]
    public async Task GetEmployeeBalanceAsync_NoApprovedLeave_ReturnsFullAllowance()
    {
        var manager = CreateManager();
        var employeeId = Guid.NewGuid();

        var balances = await manager.GetEmployeeBalanceAsync(employeeId);

        Assert.Equal(4, balances.Count);

        var annual = balances.Single(
            balance => balance.Type == LeaveType.Annual);

        Assert.Equal(14, annual.AllowanceDays);
        Assert.Equal(0, annual.UsedDays);
        Assert.Equal(14, annual.RemainingDays);

        var sick = balances.Single(
            balance => balance.Type == LeaveType.Sick);

        Assert.Equal(5, sick.AllowanceDays);
        Assert.Equal(0, sick.UsedDays);
        Assert.Equal(5, sick.RemainingDays);
    }

    [Fact]
    public async Task GetEmployeeBalanceAsync_ApprovedAnnualLeave_ReducesRemainingBalance()
    {
        var manager = CreateManager();
        var employeeId = Guid.NewGuid();
        var today = DateOnly.FromDateTime(DateTime.Today);

        var startDate = today.AddDays(1);

        while (startDate.DayOfWeek is DayOfWeek.Saturday
            or DayOfWeek.Sunday)
        {
            startDate = startDate.AddDays(1);
        }

        // Find an end date that gives exactly three working days.
        var endDate = startDate;

        while (LeaveDayCalculator.CountWorkingDays(
            startDate, endDate) < 3)
        {
            endDate = endDate.AddDays(1);
        }

        var request = new CreateLeaveRequestDto
        {
            EmployeeId = employeeId,
            Type = LeaveType.Annual,
            StartDate = startDate,
            EndDate = endDate,
            Reason = "Annual leave balance test"
        };

        var created = await manager.CreateAsync(request);

        await manager.ReviewAsync(
            created.Id,
            LeaveStatus.Approved,
            Guid.NewGuid());

        var balances = await manager.GetEmployeeBalanceAsync(employeeId);

        var annual = balances.Single(
            balance => balance.Type == LeaveType.Annual);

        Assert.Equal(3, annual.UsedDays);
        Assert.Equal(11, annual.RemainingDays);
    }

    [Fact]
    public async Task GetEmployeeBalanceAsync_PendingAndRejectedLeave_DoNotReduceBalance()
    {
        var manager = CreateManager();
        var employeeId = Guid.NewGuid();
        var today = DateOnly.FromDateTime(DateTime.Today);

        var startDate = today.AddDays(1);

        while (startDate.DayOfWeek is DayOfWeek.Saturday
            or DayOfWeek.Sunday)
        {
            startDate = startDate.AddDays(1);
        }

        var endDate = startDate.AddDays(1);

        var pendingRequest = new CreateLeaveRequestDto
        {
            EmployeeId = employeeId,
            Type = LeaveType.Annual,
            StartDate = startDate,
            EndDate = endDate,
            Reason = "Pending annual leave test"
        };

        await manager.CreateAsync(pendingRequest);

        var rejectedRequest = new CreateLeaveRequestDto
        {
            EmployeeId = employeeId,
            Type = LeaveType.Sick,
            StartDate = startDate,
            EndDate = endDate,
            Reason = "Rejected sick leave test"
        };

        var rejected = await manager.CreateAsync(rejectedRequest);

        await manager.ReviewAsync(
            rejected.Id,
            LeaveStatus.Rejected,
            Guid.NewGuid());

        var balances = await manager.GetEmployeeBalanceAsync(employeeId);

        var annual = balances.Single(
            balance => balance.Type == LeaveType.Annual);

        var sick = balances.Single(
            balance => balance.Type == LeaveType.Sick);

        Assert.Equal(0, annual.UsedDays);
        Assert.Equal(14, annual.RemainingDays);

        Assert.Equal(0, sick.UsedDays);
        Assert.Equal(5, sick.RemainingDays);
    }

    // --------------------------------------------------
    // ROLE-BASED REVIEW AUTHORIZATION TESTS
    // --------------------------------------------------

    [Theory]
    [InlineData("Employee", "HOD")]
    [InlineData("Employee", "HR")]
    [InlineData("HOD", "HR")]
    [InlineData("HR", "HR")]
    public async Task ReviewAsync_AuthorizedRoleCombination_ApprovesRequest(
        string ownerRole,
        string reviewerRole)
    {
        var manager = CreateManager(
            ownerRole: ownerRole,
            reviewerRole: reviewerRole);

        var created = await manager.CreateAsync(ValidRequest());
        var reviewerId = Guid.NewGuid();

        var result = await manager.ReviewAsync(
            created.Id,
            LeaveStatus.Approved,
            reviewerId,
            "Authorized review");

        Assert.NotNull(result);
        Assert.Equal(LeaveStatus.Approved, result.Status);
        Assert.Equal(reviewerId, result.ReviewedByEmployeeId);
    }

    [Theory]
    [InlineData("Employee", "Employee")]
    [InlineData("HOD", "Employee")]
    [InlineData("HOD", "HOD")]
    [InlineData("HR", "Employee")]
    [InlineData("HR", "HOD")]
    public async Task ReviewAsync_UnauthorizedRoleCombination_ThrowsInvalidOperationException(
        string ownerRole,
        string reviewerRole)
    {
        var manager = CreateManager(
            ownerRole: ownerRole,
            reviewerRole: reviewerRole);

        var created = await manager.CreateAsync(ValidRequest());

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => manager.ReviewAsync(
                created.Id,
                LeaveStatus.Approved,
                Guid.NewGuid()));
    }

    [Fact]
    public async Task ReviewAsync_EmployeeCannotReviewOwnRequest()
    {
        var manager = CreateManager();
        var created = await manager.CreateAsync(ValidRequest());

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => manager.ReviewAsync(
                created.Id,
                LeaveStatus.Approved,
                created.EmployeeId));

        Assert.Contains(
            "cannot review your own",
            exception.Message,
            StringComparison.OrdinalIgnoreCase);
    }
}

using LeaveService.Entities;

namespace LeaveService.Services;

public static class LeaveAllowanceValidator
{
    public static void Validate(
        LeaveRequest request,
        IReadOnlyCollection<LeaveRequest> approvedRequests)
    {
        var limit = request.Type switch
        {
            LeaveType.Annual => 14,
            LeaveType.Sick => 5,
            LeaveType.Paid => 5,
            LeaveType.Unpaid => 5,
            _ => throw new ArgumentException(
                "The selected leave type is invalid.")
        };

        var isMonthlyLimit = request.Type == LeaveType.Unpaid;

        var candidateDays = CountDaysByPeriod(
            request.StartDate,
            request.EndDate,
            isMonthlyLimit);

        var approvedDays = new Dictionary<string, int>();

        foreach (var approved in approvedRequests)
        {
            if (approved.EmployeeId != request.EmployeeId ||
            approved.Type != request.Type ||
            (approved.Status != LeaveStatus.Approved &&
             approved.Status != LeaveStatus.Pending))
            {
                continue;
            }

            var days = CountDaysByPeriod(
                approved.StartDate,
                approved.EndDate,
                isMonthlyLimit);

            foreach (var period in days)
            {
                approvedDays[period.Key] =
                    approvedDays.GetValueOrDefault(period.Key) +
                    period.Value;
            }
        }

        foreach (var period in candidateDays)
        {
            var alreadyUsed = approvedDays.GetValueOrDefault(period.Key);

            if (alreadyUsed + period.Value > limit)
            {
                var periodDescription = isMonthlyLimit
                    ? "calendar month"
                    : "calendar year";

                throw new InvalidOperationException(
                    $"{request.Type} leave exceeds the limit of " +
                    $"{limit} working days per {periodDescription}.");
            }
        }
    }

    private static Dictionary<string, int> CountDaysByPeriod(
        DateOnly startDate,
        DateOnly endDate,
        bool monthly)
    {
        var result = new Dictionary<string, int>();

        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            if (date.DayOfWeek is DayOfWeek.Saturday
                or DayOfWeek.Sunday)
            {
                continue;
            }

            var period = monthly
                ? $"{date.Year}-{date.Month:D2}"
                : $"{date.Year}";

            result[period] = result.GetValueOrDefault(period) + 1;
        }

        return result;
    }
}

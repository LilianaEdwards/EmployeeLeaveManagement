
namespace LeaveService.Services;

public static class LeaveDayCalculator
{
    public static int CountWorkingDays(
        DateOnly startDate,
        DateOnly endDate)
    {
        if (endDate < startDate)
        {
            throw new ArgumentException(
                "End date cannot be earlier than start date.");
        }

        var workingDays = 0;

        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            if (date.DayOfWeek is not DayOfWeek.Saturday
                and not DayOfWeek.Sunday)
            {
                workingDays++;
            }
        }

        return workingDays;
    }
}


using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LeaveService.Data;

public class LeaveDbContextFactory
    : IDesignTimeDbContextFactory<LeaveDbContext>
{
    public LeaveDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable(
                "ConnectionStrings__LeaveDatabase")
            ?? "Host=localhost;Database=LeaveDb;Username=postgres;Password=postgres";

        var optionsBuilder =
            new DbContextOptionsBuilder<LeaveDbContext>();

        optionsBuilder.UseNpgsql(connectionString);

        return new LeaveDbContext(optionsBuilder.Options);
    }
}


using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EmployeeService.Data;

public class EmployeeDbContextFactory
    : IDesignTimeDbContextFactory<EmployeeDbContext>
{
    public EmployeeDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable(
                "ConnectionStrings__EmployeeDatabase")
            ?? "Host=localhost;Database=EmployeeDb;Username=postgres;Password=postgres";

        var optionsBuilder =
            new DbContextOptionsBuilder<EmployeeDbContext>();

        optionsBuilder.UseNpgsql(connectionString);

        return new EmployeeDbContext(optionsBuilder.Options);
    }
}

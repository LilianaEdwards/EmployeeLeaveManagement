
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EmployeeService.Data;

public class EmployeeDbContextFactory
    : IDesignTimeDbContextFactory<EmployeeDbContext>
{
    public EmployeeDbContext CreateDbContext(string[] args)
    {
        var databaseProvider =
            Environment.GetEnvironmentVariable("DatabaseProvider")
            ?? "SqlServer";

        var connectionString =
            Environment.GetEnvironmentVariable(
                "ConnectionStrings__EmployeeDatabase");

        var optionsBuilder =
            new DbContextOptionsBuilder<EmployeeDbContext>();

        if (databaseProvider.Equals(
            "PostgreSQL",
            StringComparison.OrdinalIgnoreCase))
        {
            connectionString ??=
                "Host=localhost;Database=EmployeeDb;Username=postgres;Password=postgres";

            optionsBuilder.UseNpgsql(connectionString);
        }
        else if (databaseProvider.Equals(
            "SqlServer",
            StringComparison.OrdinalIgnoreCase))
        {
            connectionString ??=
                "Server=localhost,1434;Database=EmployeeDb;User Id=sa;Password=DesignTimeOnly_Placeholder_123!;Encrypt=True;TrustServerCertificate=True";

            optionsBuilder.UseSqlServer(connectionString);
        }
        else
        {
            throw new InvalidOperationException(
                $"Unsupported database provider: {databaseProvider}");
        }

        return new EmployeeDbContext(optionsBuilder.Options);
    }
}

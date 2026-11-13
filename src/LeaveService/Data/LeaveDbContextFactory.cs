
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LeaveService.Data;

public class LeaveDbContextFactory
    : IDesignTimeDbContextFactory<LeaveDbContext>
{
    public LeaveDbContext CreateDbContext(string[] args)
    {
        var databaseProvider =
            Environment.GetEnvironmentVariable("DatabaseProvider")
            ?? "SqlServer";

        var connectionString =
            Environment.GetEnvironmentVariable(
                "ConnectionStrings__LeaveDatabase");

        var optionsBuilder =
            new DbContextOptionsBuilder<LeaveDbContext>();

        if (databaseProvider.Equals(
            "PostgreSQL",
            StringComparison.OrdinalIgnoreCase))
        {
            connectionString ??=
                "Host=localhost;Database=LeaveDb;Username=postgres;Password=postgres";

            optionsBuilder.UseNpgsql(connectionString);
        }
        else if (databaseProvider.Equals(
            "SqlServer",
            StringComparison.OrdinalIgnoreCase))
        {
            connectionString ??=
                "Server=localhost,1434;Database=LeaveDb;User Id=sa;Password=DesignTimeOnly_Placeholder_123!;Encrypt=True;TrustServerCertificate=True";

            optionsBuilder.UseSqlServer(connectionString);
        }
        else
        {
            throw new InvalidOperationException(
                $"Unsupported database provider: {databaseProvider}");
        }

        return new LeaveDbContext(optionsBuilder.Options);
    }
}

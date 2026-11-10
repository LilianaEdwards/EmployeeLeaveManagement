
using LeaveService.Entities;
using Microsoft.EntityFrameworkCore;

namespace LeaveService.Data;

public class LeaveDbContext : DbContext
{
    public LeaveDbContext(
        DbContextOptions<LeaveDbContext> options)
        : base(options)
    {
    }

    public DbSet<LeaveRequest> LeaveRequests =>
        Set<LeaveRequest>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<LeaveRequest>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Reason)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(e => e.Type)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(e => e.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(e => e.StartDate)
                .HasColumnType("date");

            entity.Property(e => e.EndDate)
                .HasColumnType("date");

            entity.Property(e => e.CreatedAtUtc)
                .IsRequired();

            entity.HasIndex(e => e.EmployeeId);

            entity.HasIndex(e => e.Status);
        });
    }
}

using Microsoft.EntityFrameworkCore;
using RentaFacil.Shared.Models;
using RentaFacil.Shared.Enums;

namespace WorkerService.Data;

public class ReportingDbContext : DbContext
{
    public ReportingDbContext(DbContextOptions<ReportingDbContext> options) : base(options)
    {
    }

    // Read-only access to data from both services
    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Reservation> Reservations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Vehicle configuration (matches VehicleService)
        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Model).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Type).IsRequired().HasConversion<string>();
            entity.Property(e => e.PricePerDay).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(e => e.Year).IsRequired();
            entity.Property(e => e.Available).IsRequired().HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).IsRequired().HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.Available);
        });

        // Customer configuration (matches BookingService)
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Phone).IsRequired().HasMaxLength(20);
            entity.Property(e => e.DocumentNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).IsRequired().HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.DocumentNumber).IsUnique();
        });

        // Reservation configuration (matches BookingService)
        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ConfirmationNumber).IsRequired().HasMaxLength(20);
            entity.Property(e => e.TotalPrice).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(e => e.Status).IsRequired().HasConversion<string>().HasDefaultValue(ReservationStatus.Pending);
            entity.Property(e => e.StartDate).IsRequired();
            entity.Property(e => e.EndDate).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).IsRequired().HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(r => r.Customer)
                .WithMany(c => c.Reservations)
                .HasForeignKey(r => r.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.Vehicle)
                .WithMany(v => v.Reservations)
                .HasForeignKey(r => r.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.ConfirmationNumber).IsUnique();
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => new { e.VehicleId, e.StartDate, e.EndDate });
            entity.HasIndex(e => e.CustomerId);
        });
    }
}
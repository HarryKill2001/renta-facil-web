using Microsoft.EntityFrameworkCore;
using RentaFacil.Shared.Models;
using RentaFacil.Shared.Enums;

namespace VehicleService.Data;

public class VehicleDbContext : DbContext
{
    public VehicleDbContext(DbContextOptions<VehicleDbContext> options) : base(options)
    {
    }

    public DbSet<Vehicle> Vehicles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Vehicle entity configuration
        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Model)
                .IsRequired()
                .HasMaxLength(100);
                
            entity.Property(e => e.Type)
                .IsRequired()
                .HasConversion<string>();
                
            entity.Property(e => e.PricePerDay)
                .HasColumnType("decimal(18,2)")
                .IsRequired();
                
            entity.Property(e => e.Year)
                .IsRequired();
                
            entity.Property(e => e.Available)
                .IsRequired()
                .HasDefaultValue(true);
                
            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");
                
            entity.Property(e => e.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // Indexes for performance
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.Available);
            entity.HasIndex(e => new { e.Type, e.Available });
        });

        // Seed data
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Vehicle>().HasData(
            new Vehicle
            {
                Id = 1,
                Type = VehicleType.Sedan,
                Model = "Toyota Camry 2023",
                Year = 2023,
                PricePerDay = 45.00m,
                Available = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Vehicle
            {
                Id = 2,
                Type = VehicleType.SUV,
                Model = "Honda CR-V 2023",
                Year = 2023,
                PricePerDay = 65.00m,
                Available = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Vehicle
            {
                Id = 3,
                Type = VehicleType.Compact,
                Model = "Nissan Versa 2023",
                Year = 2023,
                PricePerDay = 35.00m,
                Available = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Vehicle
            {
                Id = 4,
                Type = VehicleType.SUV,
                Model = "Ford Explorer 2023",
                Year = 2023,
                PricePerDay = 75.00m,
                Available = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Vehicle
            {
                Id = 5,
                Type = VehicleType.Sedan,
                Model = "Chevrolet Malibu 2023",
                Year = 2023,
                PricePerDay = 40.00m,
                Available = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        );
    }
}
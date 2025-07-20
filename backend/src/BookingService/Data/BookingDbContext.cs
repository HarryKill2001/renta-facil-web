using Microsoft.EntityFrameworkCore;
using RentaFacil.Shared.Models;
using RentaFacil.Shared.Enums;

namespace BookingService.Data;

public class BookingDbContext : DbContext
{
    public BookingDbContext(DbContextOptions<BookingDbContext> options) : base(options)
    {
    }

    public DbSet<Customer> Customers { get; set; }
    public DbSet<Reservation> Reservations { get; set; }
    public DbSet<Vehicle> Vehicles { get; set; } // Read-only for reservation validation

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Customer entity configuration
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);
                
            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(100);
                
            entity.Property(e => e.Phone)
                .IsRequired()
                .HasMaxLength(20);
                
            entity.Property(e => e.DocumentNumber)
                .IsRequired()
                .HasMaxLength(50);
                
            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");
                
            entity.Property(e => e.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // Unique constraints
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.DocumentNumber).IsUnique();
        });

        // Reservation entity configuration
        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.ConfirmationNumber)
                .IsRequired()
                .HasMaxLength(20);
                
            entity.Property(e => e.TotalPrice)
                .HasColumnType("decimal(18,2)")
                .IsRequired();
                
            entity.Property(e => e.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasDefaultValue(ReservationStatus.Pending);
                
            entity.Property(e => e.StartDate)
                .IsRequired();
                
            entity.Property(e => e.EndDate)
                .IsRequired();
                
            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");
                
            entity.Property(e => e.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // Foreign key relationships
            entity.HasOne(r => r.Customer)
                .WithMany(c => c.Reservations)
                .HasForeignKey(r => r.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.Vehicle)
                .WithMany(v => v.Reservations)
                .HasForeignKey(r => r.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes for performance
            entity.HasIndex(e => e.ConfirmationNumber).IsUnique();
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => new { e.VehicleId, e.StartDate, e.EndDate });
            entity.HasIndex(e => e.CustomerId);
        });

        // Vehicle entity configuration (read-only for validation)
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

            // Indexes
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.Available);
        });

        // Add check constraints
        modelBuilder.Entity<Reservation>()
            .HasCheckConstraint("CK_Reservation_EndDate", "[EndDate] > [StartDate]");
            
        modelBuilder.Entity<Reservation>()
            .HasCheckConstraint("CK_Reservation_TotalPrice", "[TotalPrice] > 0");
    }
}
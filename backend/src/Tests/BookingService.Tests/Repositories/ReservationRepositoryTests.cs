using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RentaFacil.Shared.Enums;
using RentaFacil.Shared.Models;
using BookingService.Data;
using BookingService.Repositories;

namespace BookingService.Tests.Repositories;

public class ReservationRepositoryTests : IDisposable
{
    private readonly BookingDbContext _context;
    private readonly ReservationRepository _repository;

    public ReservationRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<BookingDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new BookingDbContext(options);
        _repository = new ReservationRepository(_context);

        SeedTestData();
    }

    private void SeedTestData()
    {
        var customers = new List<Customer>
        {
            new Customer
            {
                Id = 1,
                Name = "Juan Pérez",
                Email = "juan.perez@email.com",
                Phone = "+57 300 123 4567",
                DocumentNumber = "12345678",
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            },
            new Customer
            {
                Id = 2,
                Name = "María García",
                Email = "maria.garcia@email.com",
                Phone = "+57 300 987 6543",
                DocumentNumber = "87654321",
                CreatedAt = DateTime.UtcNow.AddDays(-15)
            }
        };

        // Note: Vehicles are now managed by VehicleService - no longer seeded in BookingService database
        // Vehicle references in reservations use VehicleId only

        var reservations = new List<Reservation>
        {
            new Reservation
            {
                Id = 1,
                CustomerId = 1,
                VehicleId = 1,
                StartDate = DateTime.Today.AddDays(5),
                EndDate = DateTime.Today.AddDays(10),
                TotalPrice = 425.00m,
                Status = ReservationStatus.Confirmed,
                ConfirmationNumber = "RF-20241201-001",
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new Reservation
            {
                Id = 2,
                CustomerId = 1,
                VehicleId = 2,
                StartDate = DateTime.Today.AddDays(15),
                EndDate = DateTime.Today.AddDays(20),
                TotalPrice = 275.00m,
                Status = ReservationStatus.Pending,
                ConfirmationNumber = "RF-20241201-002",
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            },
            new Reservation
            {
                Id = 3,
                CustomerId = 2,
                VehicleId = 1,
                StartDate = DateTime.Today.AddDays(-5),
                EndDate = DateTime.Today.AddDays(-1),
                TotalPrice = 340.00m,
                Status = ReservationStatus.Completed,
                ConfirmationNumber = "RF-20241120-001",
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            },
            new Reservation
            {
                Id = 4,
                CustomerId = 2,
                VehicleId = 2,
                StartDate = DateTime.Today.AddDays(1),
                EndDate = DateTime.Today.AddDays(4),
                TotalPrice = 165.00m,
                Status = ReservationStatus.Cancelled,
                ConfirmationNumber = "RF-20241201-003",
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            }
        };

        _context.Customers.AddRange(customers);
        _context.Reservations.AddRange(reservations);
        _context.SaveChanges();
    }

    [Fact]
    public async Task AddAsync_WithValidReservation_ShouldAddReservation()
    {
        // Arrange
        var reservation = new Reservation
        {
            CustomerId = 1,
            VehicleId = 1,
            StartDate = DateTime.Today.AddDays(25),
            EndDate = DateTime.Today.AddDays(30),
            TotalPrice = 425.00m,
            Status = ReservationStatus.Pending,
            ConfirmationNumber = "RF-20241201-004",
            CreatedAt = DateTime.UtcNow
        };

        // Act
        await _repository.AddAsync(reservation);
        await _repository.SaveChangesAsync();

        // Assert
        reservation.Id.Should().BeGreaterThan(0);
        var savedReservation = await _context.Reservations.FindAsync(reservation.Id);
        savedReservation.Should().NotBeNull();
        savedReservation!.ConfirmationNumber.Should().Be("RF-20241201-004");
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnReservation()
    {
        // Act
        var result = await _repository.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.ConfirmationNumber.Should().Be("RF-20241201-001");
        result.Status.Should().Be(ReservationStatus.Confirmed);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllReservations()
    {
        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(4);
        result.Should().Contain(r => r.ConfirmationNumber == "RF-20241201-001");
        result.Should().Contain(r => r.ConfirmationNumber == "RF-20241201-002");
        result.Should().Contain(r => r.ConfirmationNumber == "RF-20241120-001");
        result.Should().Contain(r => r.ConfirmationNumber == "RF-20241201-003");
    }

    [Fact]
    public async Task Update_WithValidReservation_ShouldUpdateReservation()
    {
        // Arrange
        var reservation = await _repository.GetByIdAsync(2);
        reservation!.Status = ReservationStatus.Confirmed;
        reservation.TotalPrice = 300.00m;

        // Act
        _repository.Update(reservation);
        await _repository.SaveChangesAsync();

        // Assert
        var updatedReservation = await _context.Reservations.FindAsync(2);
        updatedReservation!.Status.Should().Be(ReservationStatus.Confirmed);
        updatedReservation.TotalPrice.Should().Be(300.00m);
    }

    [Fact]
    public async Task Remove_WithValidReservation_ShouldDeleteReservation()
    {
        // Arrange
        var reservation = await _repository.GetByIdAsync(4);

        // Act
        _repository.Remove(reservation!);
        await _repository.SaveChangesAsync();

        // Assert
        var deletedReservation = await _context.Reservations.FindAsync(4);
        deletedReservation.Should().BeNull();
    }

    [Fact]
    public async Task GetByConfirmationNumberAsync_WithValidConfirmation_ShouldReturnReservationWithDetails()
    {
        // Act
        var result = await _repository.GetByConfirmationNumberAsync("RF-20241201-001");

        // Assert
        result.Should().NotBeNull();
        result!.ConfirmationNumber.Should().Be("RF-20241201-001");
        result.Id.Should().Be(1);
        result.Customer.Should().NotBeNull();
        result.Customer!.Name.Should().Be("Juan Pérez");
        // Note: Vehicle information is no longer included in reservations from BookingService
        // Vehicle details must be fetched separately from VehicleService using VehicleId
        result.VehicleId.Should().Be(1);
    }

    [Fact]
    public async Task GetByConfirmationNumberAsync_WithInvalidConfirmation_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetByConfirmationNumberAsync("INVALID-CONFIRMATION");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByCustomerIdAsync_WithValidCustomerId_ShouldReturnCustomerReservations()
    {
        // Act
        var result = await _repository.GetByCustomerIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.All(r => r.CustomerId == 1).Should().BeTrue();
        result.Should().Contain(r => r.ConfirmationNumber == "RF-20241201-001");
        result.Should().Contain(r => r.ConfirmationNumber == "RF-20241201-002");
        
        // Should be ordered by CreatedAt descending
        var reservationsList = result.ToList();
        reservationsList[0].CreatedAt.Should().BeAfter(reservationsList[1].CreatedAt);
        
        // Note: Vehicle details are no longer included in reservations from BookingService
        // Vehicle information must be fetched separately from VehicleService using VehicleId
        reservationsList.All(r => r.VehicleId > 0).Should().BeTrue();
    }

    [Fact]
    public async Task GetByCustomerIdAsync_WithCustomerWithoutReservations_ShouldReturnEmpty()
    {
        // Arrange - Create a customer without reservations
        var customer = new Customer
        {
            Id = 3,
            Name = "Carlos López",
            Email = "carlos.lopez@email.com",
            Phone = "+57 300 555 1234",
            DocumentNumber = "55511234",
            CreatedAt = DateTime.UtcNow
        };
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByCustomerIdAsync(3);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByVehicleIdAsync_WithValidVehicleId_ShouldReturnVehicleReservations()
    {
        // Act
        var result = await _repository.GetByVehicleIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.All(r => r.VehicleId == 1).Should().BeTrue();
        
        // Should be ordered by StartDate descending
        var reservationsList = result.ToList();
        reservationsList[0].StartDate.Should().BeAfter(reservationsList[1].StartDate);
        
        // Should include customer details
        reservationsList.All(r => r.Customer != null).Should().BeTrue();
    }

    [Fact]
    public async Task GetActiveReservationsAsync_ShouldReturnOnlyActiveReservations()
    {
        // Act
        var result = await _repository.GetActiveReservationsAsync();

        // Assert
        result.Should().NotBeNull();
        // Note: Test data has one confirmed reservation but it's in the future (days 5-10)
        // Active reservations need to be confirmed AND currently in progress
        result.Should().HaveCount(0); // No reservations are currently active (in progress)
        
        // If we had active reservations, they would have these properties:
        // - Status should be Confirmed
        // - StartDate should be <= now
        // - EndDate should be > now
        // - Should include customer and vehicle details
    }

    [Fact]
    public async Task GetReservationsByDateRangeAsync_WithValidRange_ShouldReturnReservationsInRange()
    {
        // Arrange
        var startDate = DateTime.Today.AddDays(1);
        var endDate = DateTime.Today.AddDays(15);

        // Act
        var result = await _repository.GetReservationsByDateRangeAsync(startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        // Test data reservations:
        // RF-20241201-001: days 5-10 (overlaps with range 1-15)
        // RF-20241201-002: days 15-20 (overlaps with range 1-15)
        // RF-20241120-001: days -5 to -1 (doesn't overlap)
        // RF-20241201-003: days 1-4 (overlaps with range 1-15)
        result.Should().HaveCount(3); // Should include reservations that overlap with the range
        result.Should().Contain(r => r.ConfirmationNumber == "RF-20241201-001");
        result.Should().Contain(r => r.ConfirmationNumber == "RF-20241201-002");
        result.Should().Contain(r => r.ConfirmationNumber == "RF-20241201-003");
        
        // Should be ordered by StartDate
        var reservationsList = result.ToList();
        for (int i = 0; i < reservationsList.Count - 1; i++)
        {
            reservationsList[i].StartDate.Should().BeOnOrBefore(reservationsList[i + 1].StartDate);
        }
        
        // Should include customer details (vehicle details must be fetched from VehicleService)
        reservationsList.All(r => r.Customer != null && r.VehicleId > 0).Should().BeTrue();
    }

    [Fact]
    public async Task GetReservationsByStatusAsync_WithValidStatus_ShouldReturnReservationsWithStatus()
    {
        // Act
        var result = await _repository.GetReservationsByStatusAsync(ReservationStatus.Confirmed);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.All(r => r.Status == ReservationStatus.Confirmed).Should().BeTrue();
        result.First().ConfirmationNumber.Should().Be("RF-20241201-001");
        
        // Should be ordered by CreatedAt descending
        var reservation = result.First();
        reservation.Customer.Should().NotBeNull();
        // Note: Vehicle details are no longer included in reservations from BookingService
        reservation.VehicleId.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetReservationWithDetailsAsync_WithValidId_ShouldReturnReservationWithDetails()
    {
        // Act
        var result = await _repository.GetReservationWithDetailsAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.ConfirmationNumber.Should().Be("RF-20241201-001");
        result.Customer.Should().NotBeNull();
        result.Customer!.Name.Should().Be("Juan Pérez");
        // Note: Vehicle information is no longer included in reservations from BookingService
        // Vehicle details must be fetched separately from VehicleService using VehicleId
        result.VehicleId.Should().Be(1);
    }

    [Fact]
    public async Task GetReservationWithDetailsAsync_WithInvalidId_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetReservationWithDetailsAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task HasConflictingReservationsAsync_WithConflictingDates_ShouldReturnTrue()
    {
        // Arrange - Check for conflicts with the confirmed reservation (days 5-10)
        var startDate = DateTime.Today.AddDays(7);
        var endDate = DateTime.Today.AddDays(12);

        // Act
        var result = await _repository.HasConflictingReservationsAsync(1, startDate, endDate);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasConflictingReservationsAsync_WithNonConflictingDates_ShouldReturnFalse()
    {
        // Arrange - Check for conflicts with dates that don't overlap
        var startDate = DateTime.Today.AddDays(25);
        var endDate = DateTime.Today.AddDays(30);

        // Act
        var result = await _repository.HasConflictingReservationsAsync(1, startDate, endDate);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasConflictingReservationsAsync_WithExcludedReservation_ShouldIgnoreExcludedReservation()
    {
        // Arrange - Check for conflicts excluding the existing confirmed reservation
        var startDate = DateTime.Today.AddDays(7);
        var endDate = DateTime.Today.AddDays(12);
        var excludeReservationId = 1; // Exclude the conflicting reservation

        // Act
        var result = await _repository.HasConflictingReservationsAsync(1, startDate, endDate, excludeReservationId);

        // Assert
        result.Should().BeFalse(); // Should not find conflicts when excluding the overlapping reservation
    }

    [Fact]
    public async Task HasConflictingReservationsAsync_OnlyChecksConfirmedReservations_ShouldIgnoreOtherStatuses()
    {
        // Arrange - Check for conflicts with pending/cancelled reservations dates
        var startDate = DateTime.Today.AddDays(16);
        var endDate = DateTime.Today.AddDays(19);

        // Act
        var result = await _repository.HasConflictingReservationsAsync(2, startDate, endDate);

        // Assert
        result.Should().BeFalse(); // Should not find conflicts with non-confirmed reservations
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
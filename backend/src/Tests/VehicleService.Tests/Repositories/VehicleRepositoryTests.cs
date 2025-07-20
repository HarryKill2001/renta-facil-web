using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RentaFacil.Shared.Enums;
using RentaFacil.Shared.Models;
using VehicleService.Data;
using VehicleService.Repositories;

namespace VehicleService.Tests.Repositories;

public class VehicleRepositoryTests : IDisposable
{
    private readonly VehicleDbContext _context;
    private readonly VehicleRepository _repository;

    public VehicleRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<VehicleDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new VehicleDbContext(options);
        _repository = new VehicleRepository(_context);

        SeedTestData();
    }

    private void SeedTestData()
    {
        var vehicles = new List<Vehicle>
        {
            new Vehicle
            {
                Id = 1,
                Type = VehicleType.Sedan,
                Model = "Toyota Corolla",
                Year = 2022,
                PricePerDay = 45.00m,
                Available = true,
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            },
            new Vehicle
            {
                Id = 2,
                Type = VehicleType.SUV,
                Model = "Honda CR-V",
                Year = 2023,
                PricePerDay = 65.00m,
                Available = true,
                CreatedAt = DateTime.UtcNow.AddDays(-25)
            },
            new Vehicle
            {
                Id = 3,
                Type = VehicleType.Compact,
                Model = "Nissan Versa",
                Year = 2021,
                PricePerDay = 35.00m,
                Available = false,
                CreatedAt = DateTime.UtcNow.AddDays(-20)
            },
            new Vehicle
            {
                Id = 4,
                Type = VehicleType.SUV,
                Model = "Ford Explorer",
                Year = 2023,
                PricePerDay = 75.00m,
                Available = true,
                CreatedAt = DateTime.UtcNow.AddDays(-15)
            }
        };

        _context.Vehicles.AddRange(vehicles);
        _context.SaveChanges();
    }

    [Fact]
    public async Task AddAsync_WithValidVehicle_ShouldAddVehicle()
    {
        // Arrange
        var vehicle = new Vehicle
        {
            Type = VehicleType.Sedan,
            Model = "Chevrolet Malibu",
            Year = 2024,
            PricePerDay = 50.00m,
            Available = true,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        await _repository.AddAsync(vehicle);
        await _repository.SaveChangesAsync();

        // Assert
        vehicle.Id.Should().BeGreaterThan(0);
        var savedVehicle = await _context.Vehicles.FindAsync(vehicle.Id);
        savedVehicle.Should().NotBeNull();
        savedVehicle!.Model.Should().Be("Chevrolet Malibu");
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnVehicle()
    {
        // Act
        var result = await _repository.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Model.Should().Be("Toyota Corolla");
        result.Type.Should().Be(VehicleType.Sedan);
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
    public async Task GetAllAsync_ShouldReturnAllVehicles()
    {
        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(4);
        result.Should().Contain(v => v.Model == "Toyota Corolla");
        result.Should().Contain(v => v.Model == "Honda CR-V");
        result.Should().Contain(v => v.Model == "Nissan Versa");
        result.Should().Contain(v => v.Model == "Ford Explorer");
    }

    [Fact]
    public async Task Update_WithValidVehicle_ShouldUpdateVehicle()
    {
        // Arrange
        var vehicle = await _repository.GetByIdAsync(1);
        vehicle!.PricePerDay = 55.00m;
        vehicle.Model = "Toyota Corolla Updated";

        // Act
        _repository.Update(vehicle);
        await _repository.SaveChangesAsync();

        // Assert
        var updatedVehicle = await _context.Vehicles.FindAsync(1);
        updatedVehicle!.PricePerDay.Should().Be(55.00m);
        updatedVehicle.Model.Should().Be("Toyota Corolla Updated");
    }

    [Fact]
    public async Task Remove_WithValidVehicle_ShouldDeleteVehicle()
    {
        // Arrange
        var vehicle = await _repository.GetByIdAsync(2);

        // Act
        _repository.Remove(vehicle!);
        await _repository.SaveChangesAsync();

        // Assert
        var deletedVehicle = await _context.Vehicles.FindAsync(2);
        deletedVehicle.Should().BeNull();
    }

    [Fact]
    public async Task GetAvailableVehiclesAsync_WithoutTypeFilter_ShouldReturnAvailableVehicles()
    {
        // Arrange
        var startDate = DateTime.Today.AddDays(1);
        var endDate = DateTime.Today.AddDays(5);

        // Act
        var result = await _repository.GetAvailableVehiclesAsync(startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3); // Only available vehicles (excluding Nissan Versa)
        result.All(v => v.Available).Should().BeTrue();
        result.Should().NotContain(v => v.Model == "Nissan Versa");
    }

    [Fact]
    public async Task GetAvailableVehiclesAsync_WithTypeFilter_ShouldReturnFilteredVehicles()
    {
        // Arrange
        var startDate = DateTime.Today.AddDays(1);
        var endDate = DateTime.Today.AddDays(5);
        var vehicleType = VehicleType.SUV;

        // Act
        var result = await _repository.GetAvailableVehiclesAsync(startDate, endDate, vehicleType);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2); // Honda CR-V and Ford Explorer
        result.All(v => v.Type == VehicleType.SUV).Should().BeTrue();
        result.All(v => v.Available).Should().BeTrue();
        result.Should().Contain(v => v.Model == "Honda CR-V");
        result.Should().Contain(v => v.Model == "Ford Explorer");
    }

    [Fact]
    public async Task GetAvailableVehiclesAsync_WithCompactType_ShouldReturnEmpty()
    {
        // Arrange
        var startDate = DateTime.Today.AddDays(1);
        var endDate = DateTime.Today.AddDays(5);
        var vehicleType = VehicleType.Compact; // Nissan Versa is Compact but not available

        // Act
        var result = await _repository.GetAvailableVehiclesAsync(startDate, endDate, vehicleType);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty(); // No available Compact vehicles
    }

    [Fact]
    public async Task GetVehicleWithReservationsAsync_WithValidId_ShouldReturnVehicle()
    {
        // Act - In MVP this just returns the vehicle without reservations
        var result = await _repository.GetVehicleWithReservationsAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Model.Should().Be("Toyota Corolla");
        // Note: In microservices architecture, reservations would be handled by BookingService
    }

    [Fact]
    public async Task GetVehicleWithReservationsAsync_WithInvalidId_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetVehicleWithReservationsAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task IsVehicleAvailableAsync_WithAvailableVehicle_ShouldReturnTrue()
    {
        // Arrange
        var startDate = DateTime.Today.AddDays(1);
        var endDate = DateTime.Today.AddDays(5);

        // Act
        var result = await _repository.IsVehicleAvailableAsync(1, startDate, endDate);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsVehicleAvailableAsync_WithUnavailableVehicle_ShouldReturnFalse()
    {
        // Arrange
        var startDate = DateTime.Today.AddDays(1);
        var endDate = DateTime.Today.AddDays(5);

        // Act
        var result = await _repository.IsVehicleAvailableAsync(3, startDate, endDate); // Nissan Versa is not available

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsVehicleAvailableAsync_WithNonExistentVehicle_ShouldReturnFalse()
    {
        // Arrange
        var startDate = DateTime.Today.AddDays(1);
        var endDate = DateTime.Today.AddDays(5);

        // Act
        var result = await _repository.IsVehicleAvailableAsync(999, startDate, endDate);

        // Assert
        result.Should().BeFalse();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
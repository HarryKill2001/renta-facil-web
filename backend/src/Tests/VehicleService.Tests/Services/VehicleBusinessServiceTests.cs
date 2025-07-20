using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using RentaFacil.Shared.DTOs;
using RentaFacil.Shared.Enums;
using VehicleService.Data;
using VehicleService.Repositories;
using VehicleService.Services;

namespace VehicleService.Tests.Services;

public class VehicleBusinessServiceTests : IDisposable
{
    private readonly VehicleDbContext _context;
    private readonly VehicleRepository _repository;
    private readonly Mock<ILogger<VehicleBusinessService>> _loggerMock;
    private readonly VehicleBusinessService _service;

    public VehicleBusinessServiceTests()
    {
        var options = new DbContextOptionsBuilder<VehicleDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new VehicleDbContext(options);
        _repository = new VehicleRepository(_context);
        _loggerMock = new Mock<ILogger<VehicleBusinessService>>();
        _service = new VehicleBusinessService(_repository, _loggerMock.Object);

        // Seed test data
        SeedTestData();
    }

    private void SeedTestData()
    {
        var vehicles = new List<RentaFacil.Shared.Models.Vehicle>
        {
            new RentaFacil.Shared.Models.Vehicle
            {
                Id = 1,
                Type = VehicleType.SUV,
                Model = "Toyota RAV4",
                Year = 2023,
                PricePerDay = 85.00m,
                Available = true,
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            },
            new RentaFacil.Shared.Models.Vehicle
            {
                Id = 2,
                Type = VehicleType.Sedan,
                Model = "Honda Civic",
                Year = 2022,
                PricePerDay = 65.00m,
                Available = true,
                CreatedAt = DateTime.UtcNow.AddDays(-25)
            }
        };

        _context.Vehicles.AddRange(vehicles);
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetAllVehiclesAsync_ShouldReturnAllVehicles()
    {
        // Act
        var result = await _service.GetAllVehiclesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain(v => v.Model == "Toyota RAV4");
        result.Should().Contain(v => v.Model == "Honda Civic");
    }

    [Fact]
    public async Task GetVehicleByIdAsync_WithValidId_ShouldReturnVehicle()
    {
        // Act
        var result = await _service.GetVehicleByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Model.Should().Be("Toyota RAV4");
        result.Type.Should().Be(VehicleType.SUV);
        result.PricePerDay.Should().Be(85.00m);
    }

    [Fact]
    public async Task GetVehicleByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Act
        var result = await _service.GetVehicleByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateVehicleAsync_WithValidData_ShouldCreateVehicle()
    {
        // Arrange
        var createDto = new CreateVehicleDto
        {
            Type = VehicleType.Compact,
            Model = "Ford Focus",
            Year = 2023,
            PricePerDay = 55.00m
        };

        // Act
        var result = await _service.CreateVehicleAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Model.Should().Be("Ford Focus");
        result.Type.Should().Be(VehicleType.Compact);
        result.Available.Should().BeTrue();

        // Verify it was saved to database
        var savedVehicle = await _context.Vehicles.FindAsync(result.Id);
        savedVehicle.Should().NotBeNull();
        savedVehicle!.Model.Should().Be("Ford Focus");
    }

    [Fact]
    public async Task UpdateVehicleAsync_WithValidData_ShouldUpdateVehicle()
    {
        // Arrange
        var updateDto = new UpdateVehicleDto
        {
            Model = "Toyota RAV4 Updated",
            PricePerDay = 90.00m
        };

        // Act
        var result = await _service.UpdateVehicleAsync(1, updateDto);

        // Assert
        result.Should().NotBeNull();
        result!.Model.Should().Be("Toyota RAV4 Updated");
        result.PricePerDay.Should().Be(90.00m);

        // Verify database was updated
        var updatedVehicle = await _context.Vehicles.FindAsync(1);
        updatedVehicle!.Model.Should().Be("Toyota RAV4 Updated");
        updatedVehicle.PricePerDay.Should().Be(90.00m);
    }

    [Fact]
    public async Task DeleteVehicleAsync_WithValidId_ShouldDeleteVehicle()
    {
        // Act
        var result = await _service.DeleteVehicleAsync(2);

        // Assert
        result.Should().BeTrue();

        // Verify vehicle was deleted
        var deletedVehicle = await _context.Vehicles.FindAsync(2);
        deletedVehicle.Should().BeNull();
    }

    [Fact]
    public async Task DeleteVehicleAsync_WithInvalidId_ShouldReturnFalse()
    {
        // Act
        var result = await _service.DeleteVehicleAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetAvailableVehiclesAsync_WithDateRange_ShouldReturnOnlyAvailableVehicles()
    {
        // Arrange
        var request = new VehicleAvailabilityRequest
        {
            StartDate = DateTime.Today.AddDays(1),
            EndDate = DateTime.Today.AddDays(5)
        };

        // Act
        var result = await _service.GetAvailableVehiclesAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2); // Both vehicles are available
        result.Should().AllSatisfy(v => v.Available.Should().BeTrue());
    }

    [Fact]
    public async Task GetAvailableVehiclesAsync_WithVehicleTypeFilter_ShouldReturnFilteredVehicles()
    {
        // Arrange
        var request = new VehicleAvailabilityRequest
        {
            StartDate = DateTime.Today.AddDays(1),
            EndDate = DateTime.Today.AddDays(5),
            Type = VehicleType.SUV
        };

        // Act
        var result = await _service.GetAvailableVehiclesAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.First().Type.Should().Be(VehicleType.SUV);
        result.First().Model.Should().Be("Toyota RAV4");
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
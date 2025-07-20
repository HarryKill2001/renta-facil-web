using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using RentaFacil.Shared.DTOs;
using RentaFacil.Shared.Interfaces;
using BookingService.Data;
using BookingService.Repositories;
using BookingService.Services;

namespace BookingService.Tests.Services;

public class CustomerBusinessServiceTests : IDisposable
{
    private readonly BookingDbContext _context;
    private readonly CustomerRepository _customerRepository;
    private readonly ReservationRepository _reservationRepository;
    private readonly Mock<ILogger<CustomerBusinessService>> _loggerMock;
    private readonly CustomerBusinessService _service;

    public CustomerBusinessServiceTests()
    {
        var options = new DbContextOptionsBuilder<BookingDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new BookingDbContext(options);
        _customerRepository = new CustomerRepository(_context);
        _reservationRepository = new ReservationRepository(_context);
        _loggerMock = new Mock<ILogger<CustomerBusinessService>>();
        _service = new CustomerBusinessService(_customerRepository, _reservationRepository, _loggerMock.Object);

        SeedTestData();
    }

    private void SeedTestData()
    {
        var customers = new List<RentaFacil.Shared.Models.Customer>
        {
            new RentaFacil.Shared.Models.Customer
            {
                Id = 1,
                Name = "Juan Pérez",
                Email = "juan.perez@email.com",
                Phone = "+57 300 123 4567",
                DocumentNumber = "12345678",
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            },
            new RentaFacil.Shared.Models.Customer
            {
                Id = 2,
                Name = "María García",
                Email = "maria.garcia@email.com",
                Phone = "+57 300 987 6543",
                DocumentNumber = "87654321",
                CreatedAt = DateTime.UtcNow.AddDays(-15)
            }
        };

        // Add Vehicle entity for reservation tests
        var vehicles = new List<RentaFacil.Shared.Models.Vehicle>
        {
            new RentaFacil.Shared.Models.Vehicle
            {
                Id = 1,
                Type = RentaFacil.Shared.Enums.VehicleType.SUV,
                Model = "Toyota RAV4",
                Year = 2023,
                PricePerDay = 85.00m,
                Available = true,
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            }
        };

        _context.Customers.AddRange(customers);
        _context.Vehicles.AddRange(vehicles);
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetAllCustomersAsync_ShouldReturnAllCustomers()
    {
        // Act
        var result = await _service.GetAllCustomersAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain(c => c.Name == "Juan Pérez");
        result.Should().Contain(c => c.Name == "María García");
    }

    [Fact]
    public async Task GetCustomerByIdAsync_WithValidId_ShouldReturnCustomer()
    {
        // Act
        var result = await _service.GetCustomerByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("Juan Pérez");
        result.Email.Should().Be("juan.perez@email.com");
    }

    [Fact]
    public async Task GetCustomerByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Act
        var result = await _service.GetCustomerByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateCustomerAsync_WithValidData_ShouldCreateCustomer()
    {
        // Arrange
        var createDto = new CreateCustomerDto
        {
            Name = "Carlos López",
            Email = "carlos.lopez@email.com",
            Phone = "+57 300 555 1234",
            DocumentNumber = "55511234"
        };

        // Act
        var result = await _service.CreateCustomerAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Name.Should().Be("Carlos López");
        result.Email.Should().Be("carlos.lopez@email.com");

        // Verify it was saved to database
        var savedCustomer = await _context.Customers.FindAsync(result.Id);
        savedCustomer.Should().NotBeNull();
        savedCustomer!.Name.Should().Be("Carlos López");
    }

    [Fact]
    public async Task GetCustomerByEmailAsync_WithValidEmail_ShouldReturnCustomer()
    {
        // Act
        var result = await _service.GetCustomerByEmailAsync("juan.perez@email.com");

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be("juan.perez@email.com");
        result.Name.Should().Be("Juan Pérez");
    }

    [Fact]
    public async Task GetCustomerByEmailAsync_WithInvalidEmail_ShouldReturnNull()
    {
        // Act
        var result = await _service.GetCustomerByEmailAsync("nonexistent@email.com");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetCustomerByDocumentNumberAsync_WithValidDocument_ShouldReturnCustomer()
    {
        // Act
        var result = await _service.GetCustomerByDocumentNumberAsync("12345678");

        // Assert
        result.Should().NotBeNull();
        result!.DocumentNumber.Should().Be("12345678");
        result.Name.Should().Be("Juan Pérez");
    }

    [Fact]
    public async Task UpdateCustomerAsync_WithValidData_ShouldUpdateCustomer()
    {
        // Arrange
        var updateDto = new UpdateCustomerDto
        {
            Name = "Juan Pérez Updated",
            Phone = "+57 300 999 8888"
        };

        // Act
        var result = await _service.UpdateCustomerAsync(1, updateDto);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Juan Pérez Updated");
        result.Phone.Should().Be("+57 300 999 8888");

        // Verify database was updated
        var updatedCustomer = await _context.Customers.FindAsync(1);
        updatedCustomer!.Name.Should().Be("Juan Pérez Updated");
        updatedCustomer.Phone.Should().Be("+57 300 999 8888");
    }

    [Fact]
    public async Task DeleteCustomerAsync_WithValidId_ShouldDeleteCustomer()
    {
        // Act
        var result = await _service.DeleteCustomerAsync(2);

        // Assert
        result.Should().BeTrue();

        // Verify customer was deleted
        var deletedCustomer = await _context.Customers.FindAsync(2);
        deletedCustomer.Should().BeNull();
    }

    [Fact]
    public async Task DeleteCustomerAsync_WithInvalidId_ShouldReturnFalse()
    {
        // Act
        var result = await _service.DeleteCustomerAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetCustomerReservationsAsync_ShouldReturnCustomerReservations()
    {
        // Arrange - Add a reservation for customer 1
        var reservation = new RentaFacil.Shared.Models.Reservation
        {
            CustomerId = 1,
            VehicleId = 1,
            StartDate = DateTime.Today.AddDays(1),
            EndDate = DateTime.Today.AddDays(5),
            TotalPrice = 400m,
            Status = RentaFacil.Shared.Enums.ReservationStatus.Confirmed,
            ConfirmationNumber = "RF-TEST-001",
            CreatedAt = DateTime.UtcNow
        };
        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetCustomerReservationsAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.First().CustomerId.Should().Be(1);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
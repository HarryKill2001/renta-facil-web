using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RentaFacil.Shared.Enums;
using RentaFacil.Shared.Models;
using BookingService.Data;
using BookingService.Repositories;

namespace BookingService.Tests.Repositories;

public class CustomerRepositoryTests : IDisposable
{
    private readonly BookingDbContext _context;
    private readonly CustomerRepository _repository;

    public CustomerRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<BookingDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new BookingDbContext(options);
        _repository = new CustomerRepository(_context);

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
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                UpdatedAt = DateTime.UtcNow.AddDays(-30)
            },
            new Customer
            {
                Id = 2,
                Name = "María García",
                Email = "maria.garcia@email.com",
                Phone = "+57 300 987 6543",
                DocumentNumber = "87654321",
                CreatedAt = DateTime.UtcNow.AddDays(-15),
                UpdatedAt = DateTime.UtcNow.AddDays(-15)
            },
            new Customer
            {
                Id = 3,
                Name = "Carlos López",
                Email = "carlos.lopez@email.com",
                Phone = "+57 300 555 1234",
                DocumentNumber = "55511234",
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                UpdatedAt = DateTime.UtcNow.AddDays(-10)
            }
        };

        var vehicles = new List<Vehicle>
        {
            new Vehicle
            {
                Id = 1,
                Type = VehicleType.SUV,
                Model = "Toyota RAV4",
                Year = 2023,
                PricePerDay = 85.00m,
                Available = true,
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            }
        };

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
                VehicleId = 1,
                StartDate = DateTime.Today.AddDays(15),
                EndDate = DateTime.Today.AddDays(20),
                TotalPrice = 425.00m,
                Status = ReservationStatus.Pending,
                ConfirmationNumber = "RF-20241201-002",
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            }
        };

        _context.Customers.AddRange(customers);
        _context.Vehicles.AddRange(vehicles);
        _context.Reservations.AddRange(reservations);
        _context.SaveChanges();
    }

    [Fact]
    public async Task AddAsync_WithValidCustomer_ShouldAddCustomer()
    {
        // Arrange
        var customer = new Customer
        {
            Name = "Ana Torres",
            Email = "ana.torres@email.com",
            Phone = "+57 300 777 8888",
            DocumentNumber = "77788899",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        await _repository.AddAsync(customer);
        await _repository.SaveChangesAsync();

        // Assert
        customer.Id.Should().BeGreaterThan(0);
        var savedCustomer = await _context.Customers.FindAsync(customer.Id);
        savedCustomer.Should().NotBeNull();
        savedCustomer!.Name.Should().Be("Ana Torres");
        savedCustomer.Email.Should().Be("ana.torres@email.com");
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnCustomer()
    {
        // Act
        var result = await _repository.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("Juan Pérez");
        result.Email.Should().Be("juan.perez@email.com");
        result.DocumentNumber.Should().Be("12345678");
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
    public async Task GetAllAsync_ShouldReturnAllCustomers()
    {
        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().Contain(c => c.Name == "Juan Pérez");
        result.Should().Contain(c => c.Name == "María García");
        result.Should().Contain(c => c.Name == "Carlos López");
    }

    [Fact]
    public async Task Update_WithValidCustomer_ShouldUpdateCustomer()
    {
        // Arrange
        var customer = await _repository.GetByIdAsync(1);
        customer!.Phone = "+57 300 999 0000";
        customer.Name = "Juan Pérez Updated";
        customer.UpdatedAt = DateTime.UtcNow;

        // Act
        _repository.Update(customer);
        await _repository.SaveChangesAsync();

        // Assert
        var updatedCustomer = await _context.Customers.FindAsync(1);
        updatedCustomer!.Phone.Should().Be("+57 300 999 0000");
        updatedCustomer.Name.Should().Be("Juan Pérez Updated");
    }

    [Fact]
    public async Task Remove_WithValidCustomer_ShouldDeleteCustomer()
    {
        // Arrange
        var customer = await _repository.GetByIdAsync(3);

        // Act
        _repository.Remove(customer!);
        await _repository.SaveChangesAsync();

        // Assert
        var deletedCustomer = await _context.Customers.FindAsync(3);
        deletedCustomer.Should().BeNull();
    }

    [Fact]
    public async Task GetByEmailAsync_WithValidEmail_ShouldReturnCustomer()
    {
        // Act
        var result = await _repository.GetByEmailAsync("juan.perez@email.com");

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be("juan.perez@email.com");
        result.Name.Should().Be("Juan Pérez");
        result.Id.Should().Be(1);
    }

    [Fact]
    public async Task GetByEmailAsync_WithInvalidEmail_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetByEmailAsync("nonexistent@email.com");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByEmailAsync_WithEmptyEmail_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetByEmailAsync("");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByDocumentNumberAsync_WithValidDocument_ShouldReturnCustomer()
    {
        // Act
        var result = await _repository.GetByDocumentNumberAsync("12345678");

        // Assert
        result.Should().NotBeNull();
        result!.DocumentNumber.Should().Be("12345678");
        result.Name.Should().Be("Juan Pérez");
        result.Id.Should().Be(1);
    }

    [Fact]
    public async Task GetByDocumentNumberAsync_WithInvalidDocument_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetByDocumentNumberAsync("99999999");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByDocumentNumberAsync_WithEmptyDocument_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetByDocumentNumberAsync("");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetCustomerWithReservationsAsync_WithValidId_ShouldReturnCustomerWithReservations()
    {
        // Act
        var result = await _repository.GetCustomerWithReservationsAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("Juan Pérez");
        result.Reservations.Should().NotBeNull();
        result.Reservations!.Should().HaveCount(2);
        result.Reservations.Should().Contain(r => r.ConfirmationNumber == "RF-20241201-001");
        result.Reservations.Should().Contain(r => r.ConfirmationNumber == "RF-20241201-002");
        
        // Check that vehicle information is included
        var reservation = result.Reservations.First();
        reservation.Vehicle.Should().NotBeNull();
        reservation.Vehicle!.Model.Should().Be("Toyota RAV4");
    }

    [Fact]
    public async Task GetCustomerWithReservationsAsync_WithCustomerWithoutReservations_ShouldReturnCustomerWithEmptyReservations()
    {
        // Act
        var result = await _repository.GetCustomerWithReservationsAsync(2); // María García has no reservations

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(2);
        result.Name.Should().Be("María García");
        result.Reservations.Should().NotBeNull();
        result.Reservations!.Should().BeEmpty();
    }

    [Fact]
    public async Task GetCustomerWithReservationsAsync_WithInvalidId_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetCustomerWithReservationsAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ExistsByEmailAsync_WithExistingEmail_ShouldReturnTrue()
    {
        // Act
        var result = await _repository.ExistsByEmailAsync("juan.perez@email.com");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByEmailAsync_WithNonExistingEmail_ShouldReturnFalse()
    {
        // Act
        var result = await _repository.ExistsByEmailAsync("nonexistent@email.com");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsByEmailAsync_WithEmptyEmail_ShouldReturnFalse()
    {
        // Act
        var result = await _repository.ExistsByEmailAsync("");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsByDocumentNumberAsync_WithExistingDocument_ShouldReturnTrue()
    {
        // Act
        var result = await _repository.ExistsByDocumentNumberAsync("12345678");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByDocumentNumberAsync_WithNonExistingDocument_ShouldReturnFalse()
    {
        // Act
        var result = await _repository.ExistsByDocumentNumberAsync("99999999");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsByDocumentNumberAsync_WithEmptyDocument_ShouldReturnFalse()
    {
        // Act
        var result = await _repository.ExistsByDocumentNumberAsync("");

        // Assert
        result.Should().BeFalse();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
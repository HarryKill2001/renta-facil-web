using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using RentaFacil.Shared.DTOs;
using RentaFacil.Shared.Enums;
using BookingService.Controllers;
using BookingService.Services;

namespace BookingService.Tests.Controllers;

public class CustomersControllerTests
{
    private readonly Mock<ICustomerBusinessService> _customerServiceMock;
    private readonly Mock<ILogger<CustomersController>> _loggerMock;
    private readonly CustomersController _controller;

    public CustomersControllerTests()
    {
        _customerServiceMock = new Mock<ICustomerBusinessService>();
        _loggerMock = new Mock<ILogger<CustomersController>>();
        _controller = new CustomersController(_customerServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetAllCustomers_ShouldReturnOkWithCustomers()
    {
        // Arrange
        var customers = new List<CustomerDto>
        {
            new CustomerDto
            {
                Id = 1,
                Name = "Juan Pérez",
                Email = "juan.perez@email.com",
                Phone = "+57 300 123 4567",
                DocumentNumber = "12345678",
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            },
            new CustomerDto
            {
                Id = 2,
                Name = "María García",
                Email = "maria.garcia@email.com",
                Phone = "+57 300 987 6543",
                DocumentNumber = "87654321",
                CreatedAt = DateTime.UtcNow.AddDays(-15)
            }
        };

        _customerServiceMock
            .Setup(s => s.GetAllCustomersAsync())
            .ReturnsAsync(customers);

        // Act
        var result = await _controller.GetAllCustomers();

        // Assert
        var actionResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = actionResult.Value.Should().BeOfType<ApiResponse<IEnumerable<CustomerDto>>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().HaveCount(2);
        response.Message.Should().Be("Customers retrieved successfully");
    }

    [Fact]
    public async Task GetAllCustomers_WhenExceptionThrown_ShouldReturnInternalServerError()
    {
        // Arrange
        _customerServiceMock
            .Setup(s => s.GetAllCustomersAsync())
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetAllCustomers();

        // Assert
        var actionResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        actionResult.StatusCode.Should().Be(500);
        var response = actionResult.Value.Should().BeOfType<ApiResponse<IEnumerable<CustomerDto>>>().Subject;
        response.Success.Should().BeFalse();
        response.Message.Should().Be("Internal server error occurred");
    }

    [Fact]
    public async Task GetCustomer_WithValidId_ShouldReturnOkWithCustomer()
    {
        // Arrange
        var customerId = 1;
        var customer = new CustomerDto
        {
            Id = customerId,
            Name = "Juan Pérez",
            Email = "juan.perez@email.com",
            Phone = "+57 300 123 4567",
            DocumentNumber = "12345678",
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };

        _customerServiceMock
            .Setup(s => s.GetCustomerByIdAsync(customerId))
            .ReturnsAsync(customer);

        // Act
        var result = await _controller.GetCustomer(customerId);

        // Assert
        var actionResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = actionResult.Value.Should().BeOfType<ApiResponse<CustomerDto>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Data!.Id.Should().Be(customerId);
        response.Message.Should().Be("Customer retrieved successfully");
    }

    [Fact]
    public async Task GetCustomer_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var customerId = 999;
        _customerServiceMock
            .Setup(s => s.GetCustomerByIdAsync(customerId))
            .ReturnsAsync((CustomerDto?)null);

        // Act
        var result = await _controller.GetCustomer(customerId);

        // Assert
        var actionResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        var response = actionResult.Value.Should().BeOfType<ApiResponse<CustomerDto>>().Subject;
        response.Success.Should().BeFalse();
        response.Message.Should().Be("Customer not found");
    }

    [Fact]
    public async Task GetCustomer_WhenExceptionThrown_ShouldReturnInternalServerError()
    {
        // Arrange
        var customerId = 1;
        _customerServiceMock
            .Setup(s => s.GetCustomerByIdAsync(customerId))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetCustomer(customerId);

        // Assert
        var actionResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        actionResult.StatusCode.Should().Be(500);
        var response = actionResult.Value.Should().BeOfType<ApiResponse<CustomerDto>>().Subject;
        response.Success.Should().BeFalse();
        response.Message.Should().Be("Internal server error occurred");
    }

    [Fact]
    public async Task GetCustomerByEmail_WithValidEmail_ShouldReturnOkWithCustomer()
    {
        // Arrange
        var email = "juan.perez@email.com";
        var customer = new CustomerDto
        {
            Id = 1,
            Name = "Juan Pérez",
            Email = email,
            Phone = "+57 300 123 4567",
            DocumentNumber = "12345678",
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };

        _customerServiceMock
            .Setup(s => s.GetCustomerByEmailAsync(email))
            .ReturnsAsync(customer);

        // Act
        var result = await _controller.GetCustomerByEmail(email);

        // Assert
        var actionResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = actionResult.Value.Should().BeOfType<ApiResponse<CustomerDto>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Data!.Email.Should().Be(email);
        response.Message.Should().Be("Customer retrieved successfully");
    }

    [Fact]
    public async Task GetCustomerByEmail_WithInvalidEmail_ShouldReturnNotFound()
    {
        // Arrange
        var email = "nonexistent@email.com";
        _customerServiceMock
            .Setup(s => s.GetCustomerByEmailAsync(email))
            .ReturnsAsync((CustomerDto?)null);

        // Act
        var result = await _controller.GetCustomerByEmail(email);

        // Assert
        var actionResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        var response = actionResult.Value.Should().BeOfType<ApiResponse<CustomerDto>>().Subject;
        response.Success.Should().BeFalse();
        response.Message.Should().Be("Customer not found");
    }

    [Fact]
    public async Task GetCustomerByDocumentNumber_WithValidDocument_ShouldReturnOkWithCustomer()
    {
        // Arrange
        var documentNumber = "12345678";
        var customer = new CustomerDto
        {
            Id = 1,
            Name = "Juan Pérez",
            Email = "juan.perez@email.com",
            Phone = "+57 300 123 4567",
            DocumentNumber = documentNumber,
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };

        _customerServiceMock
            .Setup(s => s.GetCustomerByDocumentNumberAsync(documentNumber))
            .ReturnsAsync(customer);

        // Act
        var result = await _controller.GetCustomerByDocumentNumber(documentNumber);

        // Assert
        var actionResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = actionResult.Value.Should().BeOfType<ApiResponse<CustomerDto>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Data!.DocumentNumber.Should().Be(documentNumber);
        response.Message.Should().Be("Customer retrieved successfully");
    }

    [Fact]
    public async Task CreateCustomer_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var createDto = new CreateCustomerDto
        {
            Name = "Ana Torres",
            Email = "ana.torres@email.com",
            Phone = "+57 300 777 8888",
            DocumentNumber = "77788899"
        };

        var createdCustomer = new CustomerDto
        {
            Id = 1,
            Name = createDto.Name,
            Email = createDto.Email,
            Phone = createDto.Phone,
            DocumentNumber = createDto.DocumentNumber,
            CreatedAt = DateTime.UtcNow
        };

        _customerServiceMock
            .Setup(s => s.CreateCustomerAsync(createDto))
            .ReturnsAsync(createdCustomer);

        // Act
        var result = await _controller.CreateCustomer(createDto);

        // Assert
        var actionResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        actionResult.ActionName.Should().Be(nameof(CustomersController.GetCustomer));
        actionResult.RouteValues!["id"].Should().Be(1);
        var response = actionResult.Value.Should().BeOfType<ApiResponse<CustomerDto>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Data!.Name.Should().Be("Ana Torres");
        response.Message.Should().Be("Customer created successfully");
    }

    [Fact]
    public async Task CreateCustomer_WithDuplicateEmail_ShouldReturnConflict()
    {
        // Arrange
        var createDto = new CreateCustomerDto
        {
            Name = "Ana Torres",
            Email = "existing@email.com",
            Phone = "+57 300 777 8888",
            DocumentNumber = "77788899"
        };

        _customerServiceMock
            .Setup(s => s.CreateCustomerAsync(createDto))
            .ThrowsAsync(new InvalidOperationException("Customer with this email already exists"));

        // Act
        var result = await _controller.CreateCustomer(createDto);

        // Assert
        var actionResult = result.Result.Should().BeOfType<ConflictObjectResult>().Subject;
        var response = actionResult.Value.Should().BeOfType<ApiResponse<CustomerDto>>().Subject;
        response.Success.Should().BeFalse();
        response.Message.Should().Be("Customer with this email already exists");
    }

    [Fact]
    public async Task CreateCustomer_WhenExceptionThrown_ShouldReturnInternalServerError()
    {
        // Arrange
        var createDto = new CreateCustomerDto
        {
            Name = "Ana Torres",
            Email = "ana.torres@email.com",
            Phone = "+57 300 777 8888",
            DocumentNumber = "77788899"
        };

        _customerServiceMock
            .Setup(s => s.CreateCustomerAsync(createDto))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.CreateCustomer(createDto);

        // Assert
        var actionResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        actionResult.StatusCode.Should().Be(500);
        var response = actionResult.Value.Should().BeOfType<ApiResponse<CustomerDto>>().Subject;
        response.Success.Should().BeFalse();
        response.Message.Should().Be("Internal server error occurred");
    }

    [Fact]
    public async Task UpdateCustomer_WithValidData_ShouldReturnOkWithUpdatedCustomer()
    {
        // Arrange
        var customerId = 1;
        var updateDto = new UpdateCustomerDto
        {
            Phone = "+57 300 999 0000",
            Name = "Juan Pérez Updated"
        };

        var updatedCustomer = new CustomerDto
        {
            Id = customerId,
            Name = updateDto.Name!,
            Email = "juan.perez@email.com",
            Phone = updateDto.Phone!,
            DocumentNumber = "12345678",
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };

        _customerServiceMock
            .Setup(s => s.UpdateCustomerAsync(customerId, updateDto))
            .ReturnsAsync(updatedCustomer);

        // Act
        var result = await _controller.UpdateCustomer(customerId, updateDto);

        // Assert
        var actionResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = actionResult.Value.Should().BeOfType<ApiResponse<CustomerDto>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Data!.Phone.Should().Be("+57 300 999 0000");
        response.Data.Name.Should().Be("Juan Pérez Updated");
        response.Message.Should().Be("Customer updated successfully");
    }

    [Fact]
    public async Task UpdateCustomer_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var customerId = 999;
        var updateDto = new UpdateCustomerDto
        {
            Phone = "+57 300 999 0000"
        };

        _customerServiceMock
            .Setup(s => s.UpdateCustomerAsync(customerId, updateDto))
            .ReturnsAsync((CustomerDto?)null);

        // Act
        var result = await _controller.UpdateCustomer(customerId, updateDto);

        // Assert
        var actionResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        var response = actionResult.Value.Should().BeOfType<ApiResponse<CustomerDto>>().Subject;
        response.Success.Should().BeFalse();
        response.Message.Should().Be("Customer not found");
    }

    [Fact]
    public async Task DeleteCustomer_WithValidId_ShouldReturnOk()
    {
        // Arrange
        var customerId = 1;
        _customerServiceMock
            .Setup(s => s.DeleteCustomerAsync(customerId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteCustomer(customerId);

        // Assert
        var actionResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = actionResult.Value.Should().BeOfType<ApiResponse>().Subject;
        response.Success.Should().BeTrue();
        response.Message.Should().Be("Customer deleted successfully");
    }

    [Fact]
    public async Task DeleteCustomer_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var customerId = 999;
        _customerServiceMock
            .Setup(s => s.DeleteCustomerAsync(customerId))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteCustomer(customerId);

        // Assert
        var actionResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        var response = actionResult.Value.Should().BeOfType<ApiResponse>().Subject;
        response.Success.Should().BeFalse();
        response.Message.Should().Be("Customer not found");
    }

    [Fact]
    public async Task DeleteCustomer_WithActiveReservations_ShouldReturnConflict()
    {
        // Arrange
        var customerId = 1;
        _customerServiceMock
            .Setup(s => s.DeleteCustomerAsync(customerId))
            .ThrowsAsync(new InvalidOperationException("Cannot delete customer with active reservations"));

        // Act
        var result = await _controller.DeleteCustomer(customerId);

        // Assert
        var actionResult = result.Result.Should().BeOfType<ConflictObjectResult>().Subject;
        var response = actionResult.Value.Should().BeOfType<ApiResponse>().Subject;
        response.Success.Should().BeFalse();
        response.Message.Should().Be("Cannot delete customer with active reservations");
    }

    [Fact]
    public async Task GetCustomerHistory_WithValidCustomerId_ShouldReturnOkWithReservations()
    {
        // Arrange
        var customerId = 1;
        var reservations = new List<ReservationDto>
        {
            new ReservationDto
            {
                Id = 1,
                CustomerId = customerId,
                VehicleId = 1,
                StartDate = DateTime.Today.AddDays(5),
                EndDate = DateTime.Today.AddDays(10),
                TotalPrice = 425.00m,
                Status = ReservationStatus.Confirmed,
                ConfirmationNumber = "RF-20241201-001",
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new ReservationDto
            {
                Id = 2,
                CustomerId = customerId,
                VehicleId = 2,
                StartDate = DateTime.Today.AddDays(15),
                EndDate = DateTime.Today.AddDays(20),
                TotalPrice = 275.00m,
                Status = ReservationStatus.Pending,
                ConfirmationNumber = "RF-20241201-002",
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            }
        };

        _customerServiceMock
            .Setup(s => s.GetCustomerReservationsAsync(customerId))
            .ReturnsAsync(reservations);

        // Act
        var result = await _controller.GetCustomerHistory(customerId);

        // Assert
        var actionResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = actionResult.Value.Should().BeOfType<ApiResponse<IEnumerable<ReservationDto>>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().HaveCount(2);
        response.Data!.Should().Contain(r => r.ConfirmationNumber == "RF-20241201-001");
        response.Data.Should().Contain(r => r.ConfirmationNumber == "RF-20241201-002");
        response.Message.Should().Be("Customer history retrieved successfully");
    }

    [Fact]
    public async Task GetCustomerHistory_WithValidCustomerButNoReservations_ShouldReturnOkWithEmptyList()
    {
        // Arrange
        var customerId = 1;
        var emptyReservations = new List<ReservationDto>();

        var customer = new CustomerDto
        {
            Id = customerId,
            Name = "Juan Pérez",
            Email = "juan.perez@email.com",
            Phone = "+57 300 123 4567",
            DocumentNumber = "12345678",
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };

        _customerServiceMock
            .Setup(s => s.GetCustomerReservationsAsync(customerId))
            .ReturnsAsync(emptyReservations);

        _customerServiceMock
            .Setup(s => s.GetCustomerByIdAsync(customerId))
            .ReturnsAsync(customer);

        // Act
        var result = await _controller.GetCustomerHistory(customerId);

        // Assert
        var actionResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = actionResult.Value.Should().BeOfType<ApiResponse<IEnumerable<ReservationDto>>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().BeEmpty();
        response.Message.Should().Be("Customer history retrieved successfully");
    }

    [Fact]
    public async Task GetCustomerHistory_WithInvalidCustomerId_ShouldReturnNotFound()
    {
        // Arrange
        var customerId = 999;
        var emptyReservations = new List<ReservationDto>();

        _customerServiceMock
            .Setup(s => s.GetCustomerReservationsAsync(customerId))
            .ReturnsAsync(emptyReservations);

        _customerServiceMock
            .Setup(s => s.GetCustomerByIdAsync(customerId))
            .ReturnsAsync((CustomerDto?)null);

        // Act
        var result = await _controller.GetCustomerHistory(customerId);

        // Assert
        var actionResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        var response = actionResult.Value.Should().BeOfType<ApiResponse<IEnumerable<ReservationDto>>>().Subject;
        response.Success.Should().BeFalse();
        response.Message.Should().Be("Customer not found");
    }

    [Fact]
    public async Task GetCustomerHistory_WhenExceptionThrown_ShouldReturnInternalServerError()
    {
        // Arrange
        var customerId = 1;
        _customerServiceMock
            .Setup(s => s.GetCustomerReservationsAsync(customerId))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetCustomerHistory(customerId);

        // Assert
        var actionResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        actionResult.StatusCode.Should().Be(500);
        var response = actionResult.Value.Should().BeOfType<ApiResponse<IEnumerable<ReservationDto>>>().Subject;
        response.Success.Should().BeFalse();
        response.Message.Should().Be("Internal server error occurred");
    }
}
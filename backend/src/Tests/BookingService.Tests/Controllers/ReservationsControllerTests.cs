using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using RentaFacil.Shared.DTOs;
using RentaFacil.Shared.Enums;
using BookingService.Controllers;
using BookingService.Services;

namespace BookingService.Tests.Controllers;

public class ReservationsControllerTests
{
    private readonly Mock<IReservationBusinessService> _reservationServiceMock;
    private readonly Mock<ILogger<ReservationsController>> _loggerMock;
    private readonly ReservationsController _controller;

    public ReservationsControllerTests()
    {
        _reservationServiceMock = new Mock<IReservationBusinessService>();
        _loggerMock = new Mock<ILogger<ReservationsController>>();
        _controller = new ReservationsController(_reservationServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateReservation_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var createDto = new CreateReservationDto
        {
            VehicleId = 1,
            StartDate = DateTime.Today.AddDays(5),
            EndDate = DateTime.Today.AddDays(10),
            CustomerInfo = new CreateCustomerDto
            {
                Name = "Juan Pérez",
                Email = "juan.perez@email.com",
                Phone = "+57 300 123 4567",
                DocumentNumber = "12345678"
            }
        };

        var createdReservation = new ReservationDto
        {
            Id = 1,
            CustomerId = 1,
            VehicleId = createDto.VehicleId,
            StartDate = createDto.StartDate,
            EndDate = createDto.EndDate,
            TotalPrice = 425.00m,
            Status = ReservationStatus.Pending,
            ConfirmationNumber = "RF-20241201-001",
            CreatedAt = DateTime.UtcNow
        };

        _reservationServiceMock
            .Setup(s => s.CreateReservationAsync(createDto))
            .ReturnsAsync(createdReservation);

        // Act
        var result = await _controller.CreateReservation(createDto);

        // Assert
        var actionResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        actionResult.ActionName.Should().Be(nameof(ReservationsController.GetReservation));
        actionResult.RouteValues!["id"].Should().Be(1);
        var response = actionResult.Value.Should().BeOfType<ApiResponse<ReservationDto>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Data!.ConfirmationNumber.Should().Be("RF-20241201-001");
        response.Message.Should().Be("Reservation created successfully");
    }

    [Fact]
    public async Task CreateReservation_WithInvalidDateRange_ShouldReturnBadRequest()
    {
        // Arrange
        var createDto = new CreateReservationDto
        {
            VehicleId = 1,
            StartDate = DateTime.Today.AddDays(10),
            EndDate = DateTime.Today.AddDays(5), // End before start
            CustomerInfo = new CreateCustomerDto
            {
                Name = "Juan Pérez",
                Email = "juan.perez@email.com",
                Phone = "+57 300 123 4567",
                DocumentNumber = "12345678"
            }
        };

        _reservationServiceMock
            .Setup(s => s.CreateReservationAsync(createDto))
            .ThrowsAsync(new ArgumentException("End date must be after start date"));

        // Act
        var result = await _controller.CreateReservation(createDto);

        // Assert
        var actionResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var response = actionResult.Value.Should().BeOfType<ApiResponse<ReservationDto>>().Subject;
        response.Success.Should().BeFalse();
        response.Message.Should().Be("End date must be after start date");
    }

    [Fact]
    public async Task CreateReservation_WithVehicleNotAvailable_ShouldReturnConflict()
    {
        // Arrange
        var createDto = new CreateReservationDto
        {
            VehicleId = 1,
            StartDate = DateTime.Today.AddDays(5),
            EndDate = DateTime.Today.AddDays(10),
            CustomerInfo = new CreateCustomerDto
            {
                Name = "Juan Pérez",
                Email = "juan.perez@email.com",
                Phone = "+57 300 123 4567",
                DocumentNumber = "12345678"
            }
        };

        _reservationServiceMock
            .Setup(s => s.CreateReservationAsync(createDto))
            .ThrowsAsync(new InvalidOperationException("Vehicle is not available for the selected dates"));

        // Act
        var result = await _controller.CreateReservation(createDto);

        // Assert
        var actionResult = result.Result.Should().BeOfType<ConflictObjectResult>().Subject;
        var response = actionResult.Value.Should().BeOfType<ApiResponse<ReservationDto>>().Subject;
        response.Success.Should().BeFalse();
        response.Message.Should().Be("Vehicle is not available for the selected dates");
    }

    [Fact]
    public async Task CreateReservation_WhenExceptionThrown_ShouldReturnInternalServerError()
    {
        // Arrange
        var createDto = new CreateReservationDto
        {
            VehicleId = 1,
            StartDate = DateTime.Today.AddDays(5),
            EndDate = DateTime.Today.AddDays(10),
            CustomerInfo = new CreateCustomerDto
            {
                Name = "Juan Pérez",
                Email = "juan.perez@email.com",
                Phone = "+57 300 123 4567",
                DocumentNumber = "12345678"
            }
        };

        _reservationServiceMock
            .Setup(s => s.CreateReservationAsync(createDto))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.CreateReservation(createDto);

        // Assert
        var actionResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        actionResult.StatusCode.Should().Be(500);
        var response = actionResult.Value.Should().BeOfType<ApiResponse<ReservationDto>>().Subject;
        response.Success.Should().BeFalse();
        response.Message.Should().Be("Internal server error occurred");
    }

    [Fact]
    public async Task GetReservation_WithValidId_ShouldReturnOkWithReservation()
    {
        // Arrange
        var reservationId = 1;
        var reservation = new ReservationDto
        {
            Id = reservationId,
            CustomerId = 1,
            VehicleId = 1,
            StartDate = DateTime.Today.AddDays(5),
            EndDate = DateTime.Today.AddDays(10),
            TotalPrice = 425.00m,
            Status = ReservationStatus.Confirmed,
            ConfirmationNumber = "RF-20241201-001",
            CreatedAt = DateTime.UtcNow.AddDays(-5)
        };

        _reservationServiceMock
            .Setup(s => s.GetReservationByIdAsync(reservationId))
            .ReturnsAsync(reservation);

        // Act
        var result = await _controller.GetReservation(reservationId);

        // Assert
        var actionResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = actionResult.Value.Should().BeOfType<ApiResponse<ReservationDto>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Data!.Id.Should().Be(reservationId);
        response.Message.Should().Be("Reservation retrieved successfully");
    }

    [Fact]
    public async Task GetReservation_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var reservationId = 999;
        _reservationServiceMock
            .Setup(s => s.GetReservationByIdAsync(reservationId))
            .ReturnsAsync((ReservationDto?)null);

        // Act
        var result = await _controller.GetReservation(reservationId);

        // Assert
        var actionResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        var response = actionResult.Value.Should().BeOfType<ApiResponse<ReservationDto>>().Subject;
        response.Success.Should().BeFalse();
        response.Message.Should().Be("Reservation not found");
    }

    [Fact]
    public async Task GetReservationByConfirmationNumber_WithValidConfirmation_ShouldReturnOkWithReservation()
    {
        // Arrange
        var confirmationNumber = "RF-20241201-001";
        var reservation = new ReservationDto
        {
            Id = 1,
            CustomerId = 1,
            VehicleId = 1,
            StartDate = DateTime.Today.AddDays(5),
            EndDate = DateTime.Today.AddDays(10),
            TotalPrice = 425.00m,
            Status = ReservationStatus.Confirmed,
            ConfirmationNumber = confirmationNumber,
            CreatedAt = DateTime.UtcNow.AddDays(-5)
        };

        _reservationServiceMock
            .Setup(s => s.GetReservationByConfirmationNumberAsync(confirmationNumber))
            .ReturnsAsync(reservation);

        // Act
        var result = await _controller.GetReservationByConfirmationNumber(confirmationNumber);

        // Assert
        var actionResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = actionResult.Value.Should().BeOfType<ApiResponse<ReservationDto>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Data!.ConfirmationNumber.Should().Be(confirmationNumber);
        response.Message.Should().Be("Reservation retrieved successfully");
    }

    [Fact]
    public async Task GetReservationByConfirmationNumber_WithInvalidConfirmation_ShouldReturnNotFound()
    {
        // Arrange
        var confirmationNumber = "INVALID-CONFIRMATION";
        _reservationServiceMock
            .Setup(s => s.GetReservationByConfirmationNumberAsync(confirmationNumber))
            .ReturnsAsync((ReservationDto?)null);

        // Act
        var result = await _controller.GetReservationByConfirmationNumber(confirmationNumber);

        // Assert
        var actionResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        var response = actionResult.Value.Should().BeOfType<ApiResponse<ReservationDto>>().Subject;
        response.Success.Should().BeFalse();
        response.Message.Should().Be("Reservation not found");
    }

    [Fact]
    public async Task GetReservations_WithValidSearchCriteria_ShouldReturnOkWithReservations()
    {
        // Arrange
        var searchDto = new ReservationSearchDto
        {
            CustomerId = 1,
            Status = ReservationStatus.Confirmed,
            StartDate = DateTime.Today.AddDays(1),
            EndDate = DateTime.Today.AddDays(30)
        };

        var reservations = new List<ReservationDto>
        {
            new ReservationDto
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
            }
        };

        _reservationServiceMock
            .Setup(s => s.GetReservationsAsync(searchDto))
            .ReturnsAsync(reservations);

        // Act
        var result = await _controller.GetReservations(searchDto);

        // Assert
        var actionResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = actionResult.Value.Should().BeOfType<ApiResponse<IEnumerable<ReservationDto>>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().HaveCount(1);
        response.Data!.First().Status.Should().Be(ReservationStatus.Confirmed);
        response.Message.Should().Be("Reservations retrieved successfully");
    }

    [Fact]
    public async Task GetReservations_WhenExceptionThrown_ShouldReturnInternalServerError()
    {
        // Arrange
        var searchDto = new ReservationSearchDto();
        _reservationServiceMock
            .Setup(s => s.GetReservationsAsync(searchDto))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetReservations(searchDto);

        // Assert
        var actionResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        actionResult.StatusCode.Should().Be(500);
        var response = actionResult.Value.Should().BeOfType<ApiResponse<IEnumerable<ReservationDto>>>().Subject;
        response.Success.Should().BeFalse();
        response.Message.Should().Be("Internal server error occurred");
    }

    [Fact]
    public async Task CancelReservation_WithValidId_ShouldReturnOkWithCancelledReservation()
    {
        // Arrange
        var reservationId = 1;
        var cancelDto = new CancelReservationDto
        {
            Reason = "Customer requested cancellation"
        };

        var cancelledReservation = new ReservationDto
        {
            Id = reservationId,
            CustomerId = 1,
            VehicleId = 1,
            StartDate = DateTime.Today.AddDays(5),
            EndDate = DateTime.Today.AddDays(10),
            TotalPrice = 425.00m,
            Status = ReservationStatus.Cancelled,
            ConfirmationNumber = "RF-20241201-001",
            CreatedAt = DateTime.UtcNow.AddDays(-5)
        };

        _reservationServiceMock
            .Setup(s => s.CancelReservationAsync(reservationId, cancelDto))
            .ReturnsAsync(cancelledReservation);

        // Act
        var result = await _controller.CancelReservation(reservationId, cancelDto);

        // Assert
        var actionResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = actionResult.Value.Should().BeOfType<ApiResponse<ReservationDto>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Data!.Status.Should().Be(ReservationStatus.Cancelled);
        response.Message.Should().Be("Reservation cancelled successfully");
    }

    [Fact]
    public async Task CancelReservation_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var reservationId = 999;
        var cancelDto = new CancelReservationDto
        {
            Reason = "Customer requested cancellation"
        };

        _reservationServiceMock
            .Setup(s => s.CancelReservationAsync(reservationId, cancelDto))
            .ReturnsAsync((ReservationDto?)null);

        // Act
        var result = await _controller.CancelReservation(reservationId, cancelDto);

        // Assert
        var actionResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        var response = actionResult.Value.Should().BeOfType<ApiResponse<ReservationDto>>().Subject;
        response.Success.Should().BeFalse();
        response.Message.Should().Be("Reservation not found");
    }

    [Fact]
    public async Task CancelReservation_WithAlreadyCancelledReservation_ShouldReturnBadRequest()
    {
        // Arrange
        var reservationId = 1;
        var cancelDto = new CancelReservationDto
        {
            Reason = "Customer requested cancellation"
        };

        _reservationServiceMock
            .Setup(s => s.CancelReservationAsync(reservationId, cancelDto))
            .ThrowsAsync(new InvalidOperationException("Reservation is already cancelled"));

        // Act
        var result = await _controller.CancelReservation(reservationId, cancelDto);

        // Assert
        var actionResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var response = actionResult.Value.Should().BeOfType<ApiResponse<ReservationDto>>().Subject;
        response.Success.Should().BeFalse();
        response.Message.Should().Be("Reservation is already cancelled");
    }

    [Fact]
    public async Task ConfirmReservation_WithValidId_ShouldReturnOkWithConfirmedReservation()
    {
        // Arrange
        var reservationId = 1;
        var confirmedReservation = new ReservationDto
        {
            Id = reservationId,
            CustomerId = 1,
            VehicleId = 1,
            StartDate = DateTime.Today.AddDays(5),
            EndDate = DateTime.Today.AddDays(10),
            TotalPrice = 425.00m,
            Status = ReservationStatus.Confirmed,
            ConfirmationNumber = "RF-20241201-001",
            CreatedAt = DateTime.UtcNow.AddDays(-5)
        };

        _reservationServiceMock
            .Setup(s => s.ConfirmReservationAsync(reservationId))
            .ReturnsAsync(confirmedReservation);

        // Act
        var result = await _controller.ConfirmReservation(reservationId);

        // Assert
        var actionResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = actionResult.Value.Should().BeOfType<ApiResponse<ReservationDto>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Data!.Status.Should().Be(ReservationStatus.Confirmed);
        response.Message.Should().Be("Reservation confirmed successfully");
    }

    [Fact]
    public async Task ConfirmReservation_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var reservationId = 999;
        _reservationServiceMock
            .Setup(s => s.ConfirmReservationAsync(reservationId))
            .ReturnsAsync((ReservationDto?)null);

        // Act
        var result = await _controller.ConfirmReservation(reservationId);

        // Assert
        var actionResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        var response = actionResult.Value.Should().BeOfType<ApiResponse<ReservationDto>>().Subject;
        response.Success.Should().BeFalse();
        response.Message.Should().Be("Reservation not found");
    }

    [Fact]
    public async Task ConfirmReservation_WithAlreadyConfirmedReservation_ShouldReturnBadRequest()
    {
        // Arrange
        var reservationId = 1;
        _reservationServiceMock
            .Setup(s => s.ConfirmReservationAsync(reservationId))
            .ThrowsAsync(new InvalidOperationException("Reservation is already confirmed"));

        // Act
        var result = await _controller.ConfirmReservation(reservationId);

        // Assert
        var actionResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var response = actionResult.Value.Should().BeOfType<ApiResponse<ReservationDto>>().Subject;
        response.Success.Should().BeFalse();
        response.Message.Should().Be("Reservation is already confirmed");
    }

    [Fact]
    public async Task CheckVehicleAvailability_WithValidParameters_ShouldReturnOkWithAvailability()
    {
        // Arrange
        var vehicleId = 1;
        var startDate = DateTime.Today.AddDays(5);
        var endDate = DateTime.Today.AddDays(10);

        _reservationServiceMock
            .Setup(s => s.IsVehicleAvailableAsync(vehicleId, startDate, endDate))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.CheckVehicleAvailability(vehicleId, startDate, endDate);

        // Assert
        var actionResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = actionResult.Value.Should().BeOfType<ApiResponse<bool>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().BeTrue();
        response.Message.Should().Be("Vehicle is available");
    }

    [Fact]
    public async Task CheckVehicleAvailability_WithVehicleNotAvailable_ShouldReturnOkWithFalse()
    {
        // Arrange
        var vehicleId = 1;
        var startDate = DateTime.Today.AddDays(5);
        var endDate = DateTime.Today.AddDays(10);

        _reservationServiceMock
            .Setup(s => s.IsVehicleAvailableAsync(vehicleId, startDate, endDate))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.CheckVehicleAvailability(vehicleId, startDate, endDate);

        // Assert
        var actionResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = actionResult.Value.Should().BeOfType<ApiResponse<bool>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().BeFalse();
        response.Message.Should().Be("Vehicle is not available for the selected dates");
    }

    [Fact]
    public async Task CheckVehicleAvailability_WithInvalidDateRange_ShouldReturnBadRequest()
    {
        // Arrange
        var vehicleId = 1;
        var startDate = DateTime.Today.AddDays(10);
        var endDate = DateTime.Today.AddDays(5); // End before start

        // Act
        var result = await _controller.CheckVehicleAvailability(vehicleId, startDate, endDate);

        // Assert
        var actionResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var response = actionResult.Value.Should().BeOfType<ApiResponse<bool>>().Subject;
        response.Success.Should().BeFalse();
        response.Message.Should().Be("End date must be after start date");
    }

    [Fact]
    public async Task CheckVehicleAvailability_WithStartDateInPast_ShouldReturnBadRequest()
    {
        // Arrange
        var vehicleId = 1;
        var startDate = DateTime.Today.AddDays(-1); // Past date
        var endDate = DateTime.Today.AddDays(5);

        // Act
        var result = await _controller.CheckVehicleAvailability(vehicleId, startDate, endDate);

        // Assert
        var actionResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var response = actionResult.Value.Should().BeOfType<ApiResponse<bool>>().Subject;
        response.Success.Should().BeFalse();
        response.Message.Should().Be("Start date cannot be in the past");
    }

    [Fact]
    public async Task CheckVehicleAvailability_WhenExceptionThrown_ShouldReturnInternalServerError()
    {
        // Arrange
        var vehicleId = 1;
        var startDate = DateTime.Today.AddDays(5);
        var endDate = DateTime.Today.AddDays(10);

        _reservationServiceMock
            .Setup(s => s.IsVehicleAvailableAsync(vehicleId, startDate, endDate))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.CheckVehicleAvailability(vehicleId, startDate, endDate);

        // Assert
        var actionResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        actionResult.StatusCode.Should().Be(500);
        var response = actionResult.Value.Should().BeOfType<ApiResponse<bool>>().Subject;
        response.Success.Should().BeFalse();
        response.Message.Should().Be("Internal server error occurred");
    }
}
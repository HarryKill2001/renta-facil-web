using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using RentaFacil.Shared.DTOs;
using RentaFacil.Shared.Enums;
using VehicleService.Controllers;
using VehicleService.Services;

namespace VehicleService.Tests.Controllers;

public class VehiclesControllerTests
{
    private readonly Mock<IVehicleBusinessService> _vehicleServiceMock;
    private readonly Mock<ILogger<VehiclesController>> _loggerMock;
    private readonly VehiclesController _controller;

    public VehiclesControllerTests()
    {
        _vehicleServiceMock = new Mock<IVehicleBusinessService>();
        _loggerMock = new Mock<ILogger<VehiclesController>>();
        _controller = new VehiclesController(_vehicleServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetAllVehicles_ShouldReturnOkWithVehicles()
    {
        // Arrange
        var vehicles = new List<VehicleDto>
        {
            new VehicleDto
            {
                Id = 1,
                Type = VehicleType.Sedan,
                Model = "Toyota Corolla",
                Year = 2022,
                PricePerDay = 45.00m,
                Available = true,
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            },
            new VehicleDto
            {
                Id = 2,
                Type = VehicleType.SUV,
                Model = "Honda CR-V",
                Year = 2023,
                PricePerDay = 65.00m,
                Available = true,
                CreatedAt = DateTime.UtcNow.AddDays(-25)
            }
        };

        _vehicleServiceMock
            .Setup(s => s.GetAllVehiclesAsync())
            .ReturnsAsync(vehicles);

        // Act
        var result = await _controller.GetAllVehicles();

        // Assert
        var actionResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = actionResult.Value.Should().BeOfType<ApiResponse<IEnumerable<VehicleDto>>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().HaveCount(2);
        response.Message.Should().Be("Vehicles retrieved successfully");
    }

    [Fact]
    public async Task GetAllVehicles_WhenExceptionThrown_ShouldReturnInternalServerError()
    {
        // Arrange
        _vehicleServiceMock
            .Setup(s => s.GetAllVehiclesAsync())
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetAllVehicles();

        // Assert
        var actionResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        actionResult.StatusCode.Should().Be(500);
        var response = actionResult.Value.Should().BeOfType<ApiResponse<IEnumerable<VehicleDto>>>().Subject;
        response.Success.Should().BeFalse();
        response.Message.Should().Be("Internal server error occurred");
    }

    [Fact]
    public async Task GetVehicle_WithValidId_ShouldReturnOkWithVehicle()
    {
        // Arrange
        var vehicleId = 1;
        var vehicle = new VehicleDto
        {
            Id = vehicleId,
            Type = VehicleType.Sedan,
            Model = "Toyota Corolla",
            Year = 2022,
            PricePerDay = 45.00m,
            Available = true,
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };

        _vehicleServiceMock
            .Setup(s => s.GetVehicleByIdAsync(vehicleId))
            .ReturnsAsync(vehicle);

        // Act
        var result = await _controller.GetVehicle(vehicleId);

        // Assert
        var actionResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = actionResult.Value.Should().BeOfType<ApiResponse<VehicleDto>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Data!.Id.Should().Be(vehicleId);
        response.Message.Should().Be("Vehicle retrieved successfully");
    }

    [Fact]
    public async Task GetVehicle_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var vehicleId = 999;
        _vehicleServiceMock
            .Setup(s => s.GetVehicleByIdAsync(vehicleId))
            .ReturnsAsync((VehicleDto?)null);

        // Act
        var result = await _controller.GetVehicle(vehicleId);

        // Assert
        var actionResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        var response = actionResult.Value.Should().BeOfType<ApiResponse<VehicleDto>>().Subject;
        response.Success.Should().BeFalse();
        response.Message.Should().Be("Vehicle not found");
    }

    [Fact]
    public async Task GetVehicle_WhenExceptionThrown_ShouldReturnInternalServerError()
    {
        // Arrange
        var vehicleId = 1;
        _vehicleServiceMock
            .Setup(s => s.GetVehicleByIdAsync(vehicleId))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetVehicle(vehicleId);

        // Assert
        var actionResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        actionResult.StatusCode.Should().Be(500);
        var response = actionResult.Value.Should().BeOfType<ApiResponse<VehicleDto>>().Subject;
        response.Success.Should().BeFalse();
        response.Message.Should().Be("Internal server error occurred");
    }

    [Fact]
    public async Task CreateVehicle_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var createDto = new CreateVehicleDto
        {
            Type = VehicleType.Sedan,
            Model = "Toyota Corolla",
            Year = 2024,
            PricePerDay = 50.00m
        };

        var createdVehicle = new VehicleDto
        {
            Id = 1,
            Type = createDto.Type,
            Model = createDto.Model,
            Year = createDto.Year,
            PricePerDay = createDto.PricePerDay,
            Available = true,
            CreatedAt = DateTime.UtcNow
        };

        _vehicleServiceMock
            .Setup(s => s.CreateVehicleAsync(createDto))
            .ReturnsAsync(createdVehicle);

        // Act
        var result = await _controller.CreateVehicle(createDto);

        // Assert
        var actionResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        actionResult.ActionName.Should().Be(nameof(VehiclesController.GetVehicle));
        actionResult.RouteValues!["id"].Should().Be(1);
        var response = actionResult.Value.Should().BeOfType<ApiResponse<VehicleDto>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Data!.Model.Should().Be("Toyota Corolla");
        response.Message.Should().Be("Vehicle created successfully");
    }

    [Fact]
    public async Task CreateVehicle_WhenExceptionThrown_ShouldReturnInternalServerError()
    {
        // Arrange
        var createDto = new CreateVehicleDto
        {
            Type = VehicleType.Sedan,
            Model = "Toyota Corolla",
            Year = 2024,
            PricePerDay = 50.00m
        };

        _vehicleServiceMock
            .Setup(s => s.CreateVehicleAsync(createDto))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.CreateVehicle(createDto);

        // Assert
        var actionResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        actionResult.StatusCode.Should().Be(500);
        var response = actionResult.Value.Should().BeOfType<ApiResponse<VehicleDto>>().Subject;
        response.Success.Should().BeFalse();
        response.Message.Should().Be("Internal server error occurred");
    }

    [Fact]
    public async Task UpdateVehicle_WithValidData_ShouldReturnOkWithUpdatedVehicle()
    {
        // Arrange
        var vehicleId = 1;
        var updateDto = new UpdateVehicleDto
        {
            PricePerDay = 55.00m,
            Available = false
        };

        var updatedVehicle = new VehicleDto
        {
            Id = vehicleId,
            Type = VehicleType.Sedan,
            Model = "Toyota Corolla",
            Year = 2022,
            PricePerDay = updateDto.PricePerDay!.Value,
            Available = updateDto.Available!.Value,
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };

        _vehicleServiceMock
            .Setup(s => s.UpdateVehicleAsync(vehicleId, updateDto))
            .ReturnsAsync(updatedVehicle);

        // Act
        var result = await _controller.UpdateVehicle(vehicleId, updateDto);

        // Assert
        var actionResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = actionResult.Value.Should().BeOfType<ApiResponse<VehicleDto>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Data!.PricePerDay.Should().Be(55.00m);
        response.Data.Available.Should().BeFalse();
        response.Message.Should().Be("Vehicle updated successfully");
    }

    [Fact]
    public async Task UpdateVehicle_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var vehicleId = 999;
        var updateDto = new UpdateVehicleDto
        {
            PricePerDay = 55.00m
        };

        _vehicleServiceMock
            .Setup(s => s.UpdateVehicleAsync(vehicleId, updateDto))
            .ReturnsAsync((VehicleDto?)null);

        // Act
        var result = await _controller.UpdateVehicle(vehicleId, updateDto);

        // Assert
        var actionResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        var response = actionResult.Value.Should().BeOfType<ApiResponse<VehicleDto>>().Subject;
        response.Success.Should().BeFalse();
        response.Message.Should().Be("Vehicle not found");
    }

    [Fact]
    public async Task UpdateVehicle_WhenExceptionThrown_ShouldReturnInternalServerError()
    {
        // Arrange
        var vehicleId = 1;
        var updateDto = new UpdateVehicleDto
        {
            PricePerDay = 55.00m
        };

        _vehicleServiceMock
            .Setup(s => s.UpdateVehicleAsync(vehicleId, updateDto))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.UpdateVehicle(vehicleId, updateDto);

        // Assert
        var actionResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        actionResult.StatusCode.Should().Be(500);
        var response = actionResult.Value.Should().BeOfType<ApiResponse<VehicleDto>>().Subject;
        response.Success.Should().BeFalse();
        response.Message.Should().Be("Internal server error occurred");
    }

    [Fact]
    public async Task DeleteVehicle_WithValidId_ShouldReturnOk()
    {
        // Arrange
        var vehicleId = 1;
        _vehicleServiceMock
            .Setup(s => s.DeleteVehicleAsync(vehicleId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteVehicle(vehicleId);

        // Assert
        var actionResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = actionResult.Value.Should().BeOfType<ApiResponse>().Subject;
        response.Success.Should().BeTrue();
        response.Message.Should().Be("Vehicle deleted successfully");
    }

    [Fact]
    public async Task DeleteVehicle_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var vehicleId = 999;
        _vehicleServiceMock
            .Setup(s => s.DeleteVehicleAsync(vehicleId))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteVehicle(vehicleId);

        // Assert
        var actionResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        var response = actionResult.Value.Should().BeOfType<ApiResponse>().Subject;
        response.Success.Should().BeFalse();
        response.Message.Should().Be("Vehicle not found");
    }

    [Fact]
    public async Task DeleteVehicle_WhenExceptionThrown_ShouldReturnInternalServerError()
    {
        // Arrange
        var vehicleId = 1;
        _vehicleServiceMock
            .Setup(s => s.DeleteVehicleAsync(vehicleId))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.DeleteVehicle(vehicleId);

        // Assert
        var actionResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        actionResult.StatusCode.Should().Be(500);
        var response = actionResult.Value.Should().BeOfType<ApiResponse>().Subject;
        response.Success.Should().BeFalse();
        response.Message.Should().Be("Internal server error occurred");
    }

    [Fact]
    public async Task GetAvailableVehicles_WithValidRequest_ShouldReturnOkWithVehicles()
    {
        // Arrange
        var request = new VehicleAvailabilityRequest
        {
            StartDate = DateTime.Today.AddDays(1),
            EndDate = DateTime.Today.AddDays(5),
            Type = VehicleType.SUV
        };

        var availableVehicles = new List<VehicleDto>
        {
            new VehicleDto
            {
                Id = 1,
                Type = VehicleType.SUV,
                Model = "Honda CR-V",
                Year = 2023,
                PricePerDay = 65.00m,
                Available = true,
                CreatedAt = DateTime.UtcNow.AddDays(-25)
            }
        };

        _vehicleServiceMock
            .Setup(s => s.GetAvailableVehiclesAsync(request))
            .ReturnsAsync(availableVehicles);

        // Act
        var result = await _controller.GetAvailableVehicles(request);

        // Assert
        var actionResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = actionResult.Value.Should().BeOfType<ApiResponse<IEnumerable<VehicleDto>>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().HaveCount(1);
        response.Data!.First().Type.Should().Be(VehicleType.SUV);
        response.Message.Should().Be("Available vehicles retrieved successfully");
    }

    [Fact]
    public async Task GetAvailableVehicles_WhenExceptionThrown_ShouldReturnInternalServerError()
    {
        // Arrange
        var request = new VehicleAvailabilityRequest
        {
            StartDate = DateTime.Today.AddDays(1),
            EndDate = DateTime.Today.AddDays(5)
        };

        _vehicleServiceMock
            .Setup(s => s.GetAvailableVehiclesAsync(request))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetAvailableVehicles(request);

        // Assert
        var actionResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        actionResult.StatusCode.Should().Be(500);
        var response = actionResult.Value.Should().BeOfType<ApiResponse<IEnumerable<VehicleDto>>>().Subject;
        response.Success.Should().BeFalse();
        response.Message.Should().Be("Internal server error occurred");
    }
}
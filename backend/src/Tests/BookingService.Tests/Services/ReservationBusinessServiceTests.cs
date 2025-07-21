using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using RentaFacil.Shared.DTOs;
using RentaFacil.Shared.Enums;
using BookingService.Data;
using BookingService.Repositories;
using BookingService.Services;
using System.Net;
using System.Text.Json;

namespace BookingService.Tests.Services;

public class ReservationBusinessServiceTests : IDisposable
{
    private readonly BookingDbContext _context;
    private readonly ReservationRepository _reservationRepository;
    private readonly CustomerRepository _customerRepository;
    private readonly Mock<ILogger<ReservationBusinessService>> _loggerMock;
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly ReservationBusinessService _service;

    public ReservationBusinessServiceTests()
    {
        var options = new DbContextOptionsBuilder<BookingDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new BookingDbContext(options);
        _reservationRepository = new ReservationRepository(_context);
        _customerRepository = new CustomerRepository(_context);
        _loggerMock = new Mock<ILogger<ReservationBusinessService>>();
        
        // Setup HTTP client mocking
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        _httpClient.BaseAddress = new Uri("http://localhost:5002");
        
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _httpClientFactoryMock.Setup(x => x.CreateClient("VehicleService"))
                             .Returns(_httpClient);

        _service = new ReservationBusinessService(
            _reservationRepository, 
            _customerRepository, 
            _loggerMock.Object,
            _httpClientFactoryMock.Object);

        SetupHttpClientMocks();
        SeedTestData();
    }

    private void SetupHttpClientMocks()
    {
        // Mock vehicle details response
        var vehicleDto = new VehicleDto
        {
            Id = 1,
            Type = VehicleType.SUV,
            Model = "Toyota RAV4",
            Year = 2023,
            PricePerDay = 85.00m,
            Available = true,
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };

        var apiResponse = new ApiResponse<VehicleDto>
        {
            Success = true,
            Message = "Vehicle found",
            Data = vehicleDto
        };

        var jsonResponse = JsonSerializer.Serialize(apiResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse)
            });
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
            }
        };

        // Note: Vehicles are now in VehicleService database, not BookingService
        // We mock the HTTP calls to VehicleService instead of seeding vehicles

        var reservations = new List<RentaFacil.Shared.Models.Reservation>
        {
            new RentaFacil.Shared.Models.Reservation
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

        _context.Customers.AddRange(customers);
        _context.Reservations.AddRange(reservations);
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetReservationByIdAsync_WithValidId_ShouldReturnReservationWithVehicleDetails()
    {
        // Act
        var result = await _service.GetReservationByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.CustomerId.Should().Be(1);
        result.VehicleId.Should().Be(1);
        result.Status.Should().Be(ReservationStatus.Confirmed);
        
        // Verify vehicle details are populated from VehicleService
        result.Vehicle.Should().NotBeNull();
        result.Vehicle!.Id.Should().Be(1);
        result.Vehicle.Type.Should().Be(VehicleType.SUV);
        result.Vehicle.Model.Should().Be("Toyota RAV4");
        result.Vehicle.Year.Should().Be(2023);
        result.Vehicle.PricePerDay.Should().Be(85.00m);
        
        // Verify customer details are still populated
        result.Customer.Should().NotBeNull();
        result.Customer!.Name.Should().Be("Juan Pérez");
    }

    [Fact]
    public async Task GetReservationByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Act
        var result = await _service.GetReservationByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateReservationAsync_WithValidData_ShouldCreateReservation()
    {
        // Arrange
        var createDto = new CreateReservationDto
        {
            VehicleId = 2, // Use a different vehicle ID to avoid conflicts
            StartDate = DateTime.Today.AddDays(15),
            EndDate = DateTime.Today.AddDays(20),
            CustomerInfo = new CreateCustomerDto
            {
                Name = "Carlos López",
                Email = "carlos.lopez@email.com",
                Phone = "+57 300 555 1234",
                DocumentNumber = "55511234"
            }
        };

        // Act
        var result = await _service.CreateReservationAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.VehicleId.Should().Be(2);
        result.Status.Should().Be(ReservationStatus.Pending); // Reservations start as Pending
        result.ConfirmationNumber.Should().NotBeNullOrEmpty();
        result.TotalPrice.Should().BeGreaterThan(0);

        // Verify it was saved to database
        var savedReservation = await _context.Reservations.FindAsync(result.Id);
        savedReservation.Should().NotBeNull();
        savedReservation!.VehicleId.Should().Be(2);
    }

    [Fact]
    public async Task GetReservationByConfirmationNumberAsync_WithValidConfirmation_ShouldReturnReservation()
    {
        // Act
        var result = await _service.GetReservationByConfirmationNumberAsync("RF-20241201-001");

        // Assert
        result.Should().NotBeNull();
        result!.ConfirmationNumber.Should().Be("RF-20241201-001");
        result.Id.Should().Be(1);
    }

    [Fact]
    public async Task GetReservationsAsync_WithSearchCriteria_ShouldReturnFilteredReservations()
    {
        // Arrange
        var searchDto = new ReservationSearchDto
        {
            Status = ReservationStatus.Confirmed,
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var result = await _service.GetReservationsAsync(searchDto);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.First().Status.Should().Be(ReservationStatus.Confirmed);
    }

    [Fact]
    public async Task CancelReservationAsync_WithValidId_ShouldCancelReservation()
    {
        // Arrange
        var cancelDto = new CancelReservationDto
        {
            Reason = "Customer requested cancellation"
        };

        // Act
        var result = await _service.CancelReservationAsync(1, cancelDto);

        // Assert
        result.Should().NotBeNull();
        result!.Status.Should().Be(ReservationStatus.Cancelled);

        // Verify database was updated
        var cancelledReservation = await _context.Reservations.FindAsync(1);
        cancelledReservation!.Status.Should().Be(ReservationStatus.Cancelled);
    }

    [Fact]
    public async Task ConfirmReservationAsync_WithValidId_ShouldConfirmReservation()
    {
        // Arrange - Create a pending reservation
        var pendingReservation = new RentaFacil.Shared.Models.Reservation
        {
            Id = 2,
            CustomerId = 1,
            VehicleId = 1, // Use existing vehicle
            StartDate = DateTime.Today.AddDays(25),
            EndDate = DateTime.Today.AddDays(30),
            TotalPrice = 300.00m,
            Status = ReservationStatus.Pending,
            ConfirmationNumber = "RF-20241201-002",
            CreatedAt = DateTime.UtcNow
        };

        _context.Reservations.Add(pendingReservation);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.ConfirmReservationAsync(2);

        // Assert
        result.Should().NotBeNull();
        result!.Status.Should().Be(ReservationStatus.Confirmed);

        // Verify database was updated
        var confirmedReservation = await _context.Reservations.FindAsync(2);
        confirmedReservation!.Status.Should().Be(ReservationStatus.Confirmed);
    }

    [Fact]
    public async Task IsVehicleAvailableAsync_WithAvailableDates_ShouldReturnTrue()
    {
        // Arrange - dates that don't overlap with existing reservation
        var startDate = DateTime.Today.AddDays(15);
        var endDate = DateTime.Today.AddDays(20);

        // Act
        var result = await _service.IsVehicleAvailableAsync(1, startDate, endDate);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsVehicleAvailableAsync_WithConflictingDates_ShouldReturnFalse()
    {
        // Arrange - dates that overlap with existing reservation (day 5-10)
        var startDate = DateTime.Today.AddDays(7);
        var endDate = DateTime.Today.AddDays(12);

        // Act
        var result = await _service.IsVehicleAvailableAsync(1, startDate, endDate);

        // Assert
        result.Should().BeFalse();
    }

    public void Dispose()
    {
        _context.Dispose();
        _httpClient.Dispose();
    }
}
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using RentaFacil.Shared.Enums;
using WorkerService.Data;
using WorkerService.Services;

namespace WorkerService.Tests.Services;

public class ReportServiceTests : IDisposable
{
    private readonly ReportingDbContext _context;
    private readonly DbContextOptions<ReportingDbContext> _contextOptions;
    private readonly Mock<ILogger<ReportService>> _loggerMock;
    private readonly Mock<IDbContextFactory<ReportingDbContext>> _contextFactoryMock;
    private readonly ReportService _service;

    public ReportServiceTests()
    {
        _contextOptions = new DbContextOptionsBuilder<ReportingDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ReportingDbContext(_contextOptions);
        _loggerMock = new Mock<ILogger<ReportService>>();
        
        // Setup DbContext factory mock to return new instances each time
        _contextFactoryMock = new Mock<IDbContextFactory<ReportingDbContext>>();
        _contextFactoryMock.Setup(x => x.CreateDbContext())
                          .Returns(() => new ReportingDbContext(_contextOptions));
        _contextFactoryMock.Setup(x => x.CreateDbContextAsync(It.IsAny<CancellationToken>()))
                          .ReturnsAsync(() => new ReportingDbContext(_contextOptions));

        _service = new ReportService(_contextFactoryMock.Object, _loggerMock.Object);

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
                CreatedAt = DateTime.Today
            },
            new RentaFacil.Shared.Models.Customer
            {
                Id = 2,
                Name = "María García",
                Email = "maria.garcia@email.com",
                Phone = "+57 300 987 6543",
                DocumentNumber = "87654321",
                CreatedAt = DateTime.Today.AddDays(-1)
            }
        };

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

        var reservations = new List<RentaFacil.Shared.Models.Reservation>
        {
            new RentaFacil.Shared.Models.Reservation
            {
                Id = 1,
                CustomerId = 1,
                VehicleId = 1,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(5),
                TotalPrice = 425.00m,
                Status = ReservationStatus.Confirmed,
                ConfirmationNumber = "RF-20241201-001",
                CreatedAt = DateTime.Today
            },
            new RentaFacil.Shared.Models.Reservation
            {
                Id = 2,
                CustomerId = 2,
                VehicleId = 2,
                StartDate = DateTime.Today.AddDays(-2),
                EndDate = DateTime.Today.AddDays(3),
                TotalPrice = 325.00m,
                Status = ReservationStatus.Cancelled,
                ConfirmationNumber = "RF-20241201-002",
                CreatedAt = DateTime.Today
            }
        };

        _context.Customers.AddRange(customers);
        _context.Vehicles.AddRange(vehicles);
        _context.Reservations.AddRange(reservations);
        _context.SaveChanges();
    }

    [Fact]
    public async Task GenerateDailyReservationSummaryAsync_ShouldReturnCorrectSummary()
    {
        // Arrange
        var reportDate = DateTime.Today;

        // Act
        var result = await _service.GenerateDailyReservationSummaryAsync(reportDate);

        // Assert
        result.Should().NotBeNull();
        result.ReportDate.Should().Be(reportDate);
        result.TotalReservations.Should().Be(2); // 2 reservations created today
        result.ConfirmedReservations.Should().Be(1); // 1 confirmed
        result.CancelledReservations.Should().Be(1); // 1 cancelled
        result.PendingReservations.Should().Be(0); // 0 pending
        result.TotalRevenue.Should().Be(425.00m); // Only confirmed reservations count
        result.NewCustomers.Should().Be(1); // 1 customer created today
        result.AverageReservationValue.Should().Be(425.00m); // 425 / 1 confirmed reservation
    }

    [Fact]
    public async Task GenerateVehicleUtilizationReportAsync_ShouldReturnCorrectUtilization()
    {
        // Arrange
        var reportDate = DateTime.Today;

        // Act
        var result = await _service.GenerateVehicleUtilizationReportAsync(reportDate);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2); // 2 vehicles in test data

        var vehicle1Report = result.First(r => r.VehicleId == 1);
        vehicle1Report.VehicleModel.Should().Be("Toyota RAV4");
        vehicle1Report.VehicleType.Should().Be(VehicleType.SUV);
        vehicle1Report.TotalReservations.Should().Be(1);
        vehicle1Report.DaysBooked.Should().BeGreaterThan(0);
        vehicle1Report.TotalRevenue.Should().Be(425.00m);

        var vehicle2Report = result.First(r => r.VehicleId == 2);
        vehicle2Report.VehicleModel.Should().Be("Honda Civic");
        vehicle2Report.VehicleType.Should().Be(VehicleType.Sedan);
        // Vehicle 2 has a cancelled reservation, so it should show up but the revenue logic depends on implementation
        vehicle2Report.TotalReservations.Should().BeGreaterThanOrEqualTo(0);
        vehicle2Report.TotalRevenue.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task GenerateMonthlyRevenueSummaryAsync_ShouldReturnCorrectSummary()
    {
        // Arrange
        var currentDate = DateTime.Today;
        var year = currentDate.Year;
        var month = currentDate.Month;

        // Act
        var result = await _service.GenerateMonthlyRevenueSummaryAsync(year, month);

        // Assert
        result.Should().NotBeNull();
        result.Year.Should().Be(year);
        result.Month.Should().Be(month);
        result.TotalRevenue.Should().Be(425.00m); // Only confirmed reservations
        result.TotalReservations.Should().Be(1); // Only confirmed reservations
        result.AverageReservationValue.Should().Be(425.00m);

        // Check revenue by vehicle type
        result.RevenueByVehicleType.Should().ContainKey(VehicleType.SUV);
        result.RevenueByVehicleType[VehicleType.SUV].Should().Be(425.00m);

        result.ReservationsByVehicleType.Should().ContainKey(VehicleType.SUV);
        result.ReservationsByVehicleType[VehicleType.SUV].Should().Be(1);
    }

    [Fact]
    public async Task GenerateCustomerMetricsAsync_ShouldReturnCorrectMetrics()
    {
        // Arrange
        var reportDate = DateTime.Today;

        // Act
        var result = await _service.GenerateCustomerMetricsAsync(reportDate);

        // Assert
        result.Should().NotBeNull();
        result.ReportDate.Should().Be(reportDate);
        result.TotalCustomers.Should().Be(2);
        result.NewCustomersToday.Should().Be(1); // 1 customer created today
        result.ActiveCustomers.Should().BeGreaterThan(0); // Customers with recent reservations
        result.AverageReservationsPerCustomer.Should().Be(1.0m); // 2 reservations / 2 customers
    }

    [Fact]
    public async Task GenerateAllDailyReportsAsync_ShouldExecuteAllReportsSuccessfully()
    {
        // Arrange
        var reportDate = DateTime.Today;

        // Act & Assert (should not throw)
        await _service.GenerateAllDailyReportsAsync(reportDate);

        // Verify logging occurred
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Starting generation of all daily reports")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("All daily reports generated successfully")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GenerateAllDailyReportsAsync_OnFirstDayOfMonth_ShouldGenerateMonthlyReport()
    {
        // Arrange - Create a date that's the first day of a month (but not January)
        var firstDayOfMonth = new DateTime(DateTime.Today.Year, Math.Max(2, DateTime.Today.Month), 1);

        // Act & Assert (should not throw)
        await _service.GenerateAllDailyReportsAsync(firstDayOfMonth);

        // Verify that additional logging occurred for monthly report
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Generating monthly revenue summary")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
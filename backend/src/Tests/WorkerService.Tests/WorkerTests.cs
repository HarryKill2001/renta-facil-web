using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using WorkerService;
using WorkerService.Services;

namespace WorkerService.Tests;

public class WorkerTests : IDisposable
{
    private readonly Mock<ILogger<Worker>> _loggerMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly CancellationTokenSource _cancellationTokenSource;

    public WorkerTests()
    {
        _loggerMock = new Mock<ILogger<Worker>>();
        _configurationMock = new Mock<IConfiguration>();
        _cancellationTokenSource = new CancellationTokenSource();
    }

    [Fact]
    public void Worker_ShouldImplementBackgroundService()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();
        
        // Act
        var worker = new Worker(_loggerMock.Object, mockServiceProvider.Object, _configurationMock.Object);
        
        // Assert
        worker.Should().BeAssignableTo<BackgroundService>();
    }

    [Fact]
    public void Worker_ShouldAcceptRequiredDependencies()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();
        
        // Act & Assert - Constructor should not throw
        var worker = new Worker(_loggerMock.Object, mockServiceProvider.Object, _configurationMock.Object);
        worker.Should().NotBeNull();
    }

    [Fact]
    public void GetNextReportTime_ShouldReturnCorrectScheduledTime()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();
        var worker = new Worker(_loggerMock.Object, mockServiceProvider.Object, _configurationMock.Object);
        
        // This test uses reflection to access the private method for testing
        var method = typeof(Worker).GetMethod("GetNextReportTime", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        method.Should().NotBeNull("GetNextReportTime method should exist");

        // Act
        var result = (DateTime)method!.Invoke(worker, null)!;

        // Assert
        var now = DateTime.UtcNow;
        var expectedTime = new DateTime(now.Year, now.Month, now.Day, 2, 0, 0, DateTimeKind.Utc);
        
        // If current time is past 2 AM, expect next day
        if (now >= expectedTime)
        {
            expectedTime = expectedTime.AddDays(1);
        }

        result.Should().Be(expectedTime);
        result.Hour.Should().Be(2);
        result.Minute.Should().Be(0);
        result.Second.Should().Be(0);
        result.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public void Worker_ShouldHaveCorrectMethodSignatures()
    {
        // Arrange
        var workerType = typeof(Worker);
        
        // Assert - Check that required methods exist
        var executeMethod = workerType.GetMethod("ExecuteAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var performTasksMethod = workerType.GetMethod("PerformScheduledTasks", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var getNextReportTimeMethod = workerType.GetMethod("GetNextReportTime", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var stopMethod = workerType.GetMethod("StopAsync", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

        executeMethod.Should().NotBeNull();
        performTasksMethod.Should().NotBeNull();
        getNextReportTimeMethod.Should().NotBeNull();
        stopMethod.Should().NotBeNull();
    }

    public void Dispose()
    {
        _cancellationTokenSource?.Dispose();
    }
}
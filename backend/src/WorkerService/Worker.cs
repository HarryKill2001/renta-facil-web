using WorkerService.Services;

namespace WorkerService;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;

    public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider, IConfiguration configuration)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("RentaFacil Worker Service started at: {time}", DateTimeOffset.Now);

        // Wait for a short delay on startup to ensure services are ready
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PerformScheduledTasks(stoppingToken);
                
                // Wait until the next day at 2 AM to run daily reports
                var nextRun = GetNextReportTime();
                var delay = nextRun - DateTime.UtcNow;
                
                if (delay > TimeSpan.Zero)
                {
                    _logger.LogInformation("Next report generation scheduled for: {NextRun}", nextRun);
                    await Task.Delay(delay, stoppingToken);
                }
                else
                {
                    // If we're past today's schedule, wait until tomorrow
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Worker service is shutting down");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in worker service execution");
                // Wait before retrying
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }

    private async Task PerformScheduledTasks(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var reportService = scope.ServiceProvider.GetRequiredService<IReportService>();

        var today = DateTime.UtcNow.Date;
        var yesterday = today.AddDays(-1);

        _logger.LogInformation("Starting scheduled report generation for {Date}", yesterday.ToString("yyyy-MM-dd"));

        try
        {
            // Generate reports for the previous day
            await reportService.GenerateAllDailyReportsAsync(yesterday);
            
            _logger.LogInformation("Successfully completed all scheduled reports for {Date}", yesterday.ToString("yyyy-MM-dd"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate reports for {Date}", yesterday.ToString("yyyy-MM-dd"));
            throw;
        }
    }

    private DateTime GetNextReportTime()
    {
        var now = DateTime.UtcNow;
        var reportTime = new DateTime(now.Year, now.Month, now.Day, 2, 0, 0, DateTimeKind.Utc); // 2 AM UTC
        
        // If it's already past 2 AM today, schedule for tomorrow
        if (now >= reportTime)
        {
            reportTime = reportTime.AddDays(1);
        }
        
        return reportTime;
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker Service is stopping");
        await base.StopAsync(stoppingToken);
    }
}

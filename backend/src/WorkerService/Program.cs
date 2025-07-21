using Microsoft.EntityFrameworkCore;
using WorkerService;
using WorkerService.Data;
using WorkerService.Services;

var builder = Host.CreateApplicationBuilder(args);

// Configure Entity Framework with connection to BookingService database
builder.Services.AddDbContext<ReportingDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register IDbContextFactory for thread-safe DbContext creation in background services
builder.Services.AddDbContextFactory<ReportingDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register services
builder.Services.AddScoped<IReportService, ReportService>();

// Register the hosted service
builder.Services.AddHostedService<Worker>();

// Configure logging
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
});

var host = builder.Build();

// Ensure database connectivity on startup
using (var scope = host.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ReportingDbContext>();
    try
    {
        await context.Database.CanConnectAsync();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Successfully connected to reporting database");
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogWarning(ex, "Could not connect to reporting database on startup");
    }
}

host.Run();

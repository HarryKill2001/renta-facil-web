using Microsoft.EntityFrameworkCore;
using RentaFacil.Shared.Enums;
using WorkerService.Data;
using WorkerService.Models;

namespace WorkerService.Services;

public class ReportService : IReportService
{
    private readonly ReportingDbContext _context;
    private readonly ILogger<ReportService> _logger;

    public ReportService(ReportingDbContext context, ILogger<ReportService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<DailyReservationSummary> GenerateDailyReservationSummaryAsync(DateTime date)
    {
        _logger.LogInformation("Generating daily reservation summary for {Date}", date.ToString("yyyy-MM-dd"));

        var startOfDay = date.Date;
        var endOfDay = startOfDay.AddDays(1);

        var reservations = await _context.Reservations
            .Where(r => r.CreatedAt >= startOfDay && r.CreatedAt < endOfDay)
            .ToListAsync();

        var newCustomers = await _context.Customers
            .Where(c => c.CreatedAt >= startOfDay && c.CreatedAt < endOfDay)
            .CountAsync();

        var summary = new DailyReservationSummary
        {
            ReportDate = date,
            TotalReservations = reservations.Count,
            ConfirmedReservations = reservations.Count(r => r.Status == ReservationStatus.Confirmed),
            CancelledReservations = reservations.Count(r => r.Status == ReservationStatus.Cancelled),
            PendingReservations = reservations.Count(r => r.Status == ReservationStatus.Pending),
            TotalRevenue = reservations.Where(r => r.Status == ReservationStatus.Confirmed).Sum(r => r.TotalPrice),
            NewCustomers = newCustomers
        };

        summary.AverageReservationValue = summary.ConfirmedReservations > 0 
            ? summary.TotalRevenue / summary.ConfirmedReservations 
            : 0;

        _logger.LogInformation("Daily summary completed: {TotalReservations} reservations, ${TotalRevenue} revenue", 
            summary.TotalReservations, summary.TotalRevenue);

        return summary;
    }

    public async Task<VehicleUtilizationReport[]> GenerateVehicleUtilizationReportAsync(DateTime date)
    {
        _logger.LogInformation("Generating vehicle utilization report for {Date}", date.ToString("yyyy-MM-dd"));

        var startOfDay = date.Date;
        var endOfDay = startOfDay.AddDays(1);

        var vehicles = await _context.Vehicles.ToListAsync();
        var reports = new List<VehicleUtilizationReport>();

        foreach (var vehicle in vehicles)
        {
            var reservations = await _context.Reservations
                .Where(r => r.VehicleId == vehicle.Id && 
                           r.Status == ReservationStatus.Confirmed &&
                           ((r.StartDate <= startOfDay && r.EndDate > startOfDay) ||
                            (r.StartDate < endOfDay && r.EndDate >= endOfDay) ||
                            (r.StartDate >= startOfDay && r.EndDate <= endOfDay)))
                .ToListAsync();

            var daysBooked = reservations.Sum(r => 
            {
                var start = r.StartDate > startOfDay ? r.StartDate : startOfDay;
                var end = r.EndDate < endOfDay ? r.EndDate : endOfDay;
                return Math.Max(0, (end - start).Days);
            });

            var utilizationPercentage = daysBooked > 0 ? (decimal)daysBooked / 1 * 100 : 0; // 1 day = 100%

            var report = new VehicleUtilizationReport
            {
                ReportDate = date,
                VehicleId = vehicle.Id,
                VehicleModel = vehicle.Model,
                VehicleType = vehicle.Type,
                TotalReservations = reservations.Count,
                DaysBooked = daysBooked,
                UtilizationPercentage = utilizationPercentage,
                TotalRevenue = reservations.Sum(r => r.TotalPrice)
            };

            reports.Add(report);
        }

        _logger.LogInformation("Vehicle utilization report completed for {VehicleCount} vehicles", vehicles.Count);
        return reports.ToArray();
    }

    public async Task<MonthlyRevenueSummary> GenerateMonthlyRevenueSummaryAsync(int year, int month)
    {
        _logger.LogInformation("Generating monthly revenue summary for {Year}-{Month:D2}", year, month);

        var startOfMonth = new DateTime(year, month, 1);
        var endOfMonth = startOfMonth.AddMonths(1);

        var reservations = await _context.Reservations
            .Include(r => r.Vehicle)
            .Where(r => r.Status == ReservationStatus.Confirmed &&
                       r.CreatedAt >= startOfMonth && r.CreatedAt < endOfMonth)
            .ToListAsync();

        var revenueByType = reservations
            .GroupBy(r => r.Vehicle.Type)
            .ToDictionary(g => g.Key, g => g.Sum(r => r.TotalPrice));

        var reservationsByType = reservations
            .GroupBy(r => r.Vehicle.Type)
            .ToDictionary(g => g.Key, g => g.Count());

        var summary = new MonthlyRevenueSummary
        {
            Year = year,
            Month = month,
            TotalRevenue = reservations.Sum(r => r.TotalPrice),
            TotalReservations = reservations.Count,
            RevenueByVehicleType = revenueByType,
            ReservationsByVehicleType = reservationsByType
        };

        summary.AverageReservationValue = summary.TotalReservations > 0 
            ? summary.TotalRevenue / summary.TotalReservations 
            : 0;

        _logger.LogInformation("Monthly summary completed: {TotalReservations} reservations, ${TotalRevenue} revenue", 
            summary.TotalReservations, summary.TotalRevenue);

        return summary;
    }

    public async Task<CustomerMetrics> GenerateCustomerMetricsAsync(DateTime date)
    {
        _logger.LogInformation("Generating customer metrics for {Date}", date.ToString("yyyy-MM-dd"));

        var startOfDay = date.Date;
        var endOfDay = startOfDay.AddDays(1);
        var thirtyDaysAgo = startOfDay.AddDays(-30);

        var totalCustomers = await _context.Customers.CountAsync();
        
        var newCustomersToday = await _context.Customers
            .Where(c => c.CreatedAt >= startOfDay && c.CreatedAt < endOfDay)
            .CountAsync();

        var activeCustomers = await _context.Customers
            .Where(c => c.Reservations.Any(r => r.CreatedAt >= thirtyDaysAgo))
            .CountAsync();

        var returningCustomers = await _context.Customers
            .Where(c => c.Reservations.Count > 1)
            .CountAsync();

        var totalReservations = await _context.Reservations.CountAsync();
        var averageReservationsPerCustomer = totalCustomers > 0 
            ? (decimal)totalReservations / totalCustomers 
            : 0;

        var metrics = new CustomerMetrics
        {
            ReportDate = date,
            TotalCustomers = totalCustomers,
            NewCustomersToday = newCustomersToday,
            ActiveCustomers = activeCustomers,
            ReturningCustomers = returningCustomers,
            AverageReservationsPerCustomer = averageReservationsPerCustomer
        };

        _logger.LogInformation("Customer metrics completed: {TotalCustomers} total, {NewCustomers} new today", 
            metrics.TotalCustomers, metrics.NewCustomersToday);

        return metrics;
    }

    public async Task GenerateAllDailyReportsAsync(DateTime date)
    {
        _logger.LogInformation("Starting generation of all daily reports for {Date}", date.ToString("yyyy-MM-dd"));

        try
        {
            // Generate all reports in parallel for better performance
            var summaryTask = GenerateDailyReservationSummaryAsync(date);
            var metricsTask = GenerateCustomerMetricsAsync(date);
            var utilizationTask = GenerateVehicleUtilizationReportAsync(date);

            await Task.WhenAll(summaryTask, metricsTask, utilizationTask);

            // If it's the first day of the month, generate monthly report for previous month
            if (date.Day == 1 && date.Month > 1)
            {
                var previousMonth = date.AddMonths(-1);
                await GenerateMonthlyRevenueSummaryAsync(previousMonth.Year, previousMonth.Month);
            }

            _logger.LogInformation("All daily reports generated successfully for {Date}", date.ToString("yyyy-MM-dd"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating daily reports for {Date}", date.ToString("yyyy-MM-dd"));
            throw;
        }
    }
}
using WorkerService.Models;

namespace WorkerService.Services;

public interface IReportService
{
    Task<DailyReservationSummary> GenerateDailyReservationSummaryAsync(DateTime date);
    Task<VehicleUtilizationReport[]> GenerateVehicleUtilizationReportAsync(DateTime date);
    Task<MonthlyRevenueSummary> GenerateMonthlyRevenueSummaryAsync(int year, int month);
    Task<CustomerMetrics> GenerateCustomerMetricsAsync(DateTime date);
    Task GenerateAllDailyReportsAsync(DateTime date);
}
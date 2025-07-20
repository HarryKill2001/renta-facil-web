using RentaFacil.Shared.Enums;

namespace WorkerService.Models;

public class DailyReservationSummary
{
    public DateTime ReportDate { get; set; }
    public int TotalReservations { get; set; }
    public int ConfirmedReservations { get; set; }
    public int CancelledReservations { get; set; }
    public int PendingReservations { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageReservationValue { get; set; }
    public int NewCustomers { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

public class VehicleUtilizationReport
{
    public DateTime ReportDate { get; set; }
    public int VehicleId { get; set; }
    public string VehicleModel { get; set; } = string.Empty;
    public VehicleType VehicleType { get; set; }
    public int TotalReservations { get; set; }
    public int DaysBooked { get; set; }
    public decimal UtilizationPercentage { get; set; }
    public decimal TotalRevenue { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

public class MonthlyRevenueSummary
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TotalReservations { get; set; }
    public decimal AverageReservationValue { get; set; }
    public Dictionary<VehicleType, decimal> RevenueByVehicleType { get; set; } = new();
    public Dictionary<VehicleType, int> ReservationsByVehicleType { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

public class CustomerMetrics
{
    public DateTime ReportDate { get; set; }
    public int TotalCustomers { get; set; }
    public int NewCustomersToday { get; set; }
    public int ActiveCustomers { get; set; } // Customers with reservations in last 30 days
    public int ReturningCustomers { get; set; } // Customers with more than 1 reservation
    public decimal AverageReservationsPerCustomer { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}
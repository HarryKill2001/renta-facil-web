using RentaFacil.Shared.Enums;

namespace RentaFacil.Shared.Models;

public class Vehicle
{
    public int Id { get; set; }
    public VehicleType Type { get; set; }
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal PricePerDay { get; set; }
    public bool Available { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property for reservations
    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

    // Business logic methods
    public bool IsAvailable(DateTime startDate, DateTime endDate)
    {
        if (!Available) return false;

        return !Reservations.Any(r => 
            r.Status == ReservationStatus.Confirmed &&
            ((r.StartDate <= startDate && r.EndDate > startDate) ||
             (r.StartDate < endDate && r.EndDate >= endDate) ||
             (r.StartDate >= startDate && r.EndDate <= endDate)));
    }

    public decimal CalculatePrice(DateTime startDate, DateTime endDate)
    {
        var days = (endDate - startDate).Days;
        if (days <= 0) throw new ArgumentException("End date must be after start date");
        
        return PricePerDay * days;
    }
}
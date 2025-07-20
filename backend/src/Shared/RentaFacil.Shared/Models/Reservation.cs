using RentaFacil.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace RentaFacil.Shared.Models;

public class Reservation
{
    public int Id { get; set; }
    
    [Required]
    public string ConfirmationNumber { get; set; } = string.Empty;
    
    public int VehicleId { get; set; }
    public int CustomerId { get; set; }
    
    [Required]
    public DateTime StartDate { get; set; }
    
    [Required]
    public DateTime EndDate { get; set; }
    
    [Range(0.01, double.MaxValue)]
    public decimal TotalPrice { get; set; }
    
    public ReservationStatus Status { get; set; } = ReservationStatus.Pending;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual Vehicle Vehicle { get; set; } = null!;
    public virtual Customer Customer { get; set; } = null!;

    // Business logic methods
    public int GetDurationInDays()
    {
        return (EndDate - StartDate).Days;
    }

    public bool IsActive()
    {
        return Status == ReservationStatus.Confirmed && 
               StartDate <= DateTime.UtcNow && 
               EndDate > DateTime.UtcNow;
    }

    public bool CanBeCancelled()
    {
        return Status == ReservationStatus.Confirmed && 
               StartDate > DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (!CanBeCancelled())
            throw new InvalidOperationException("Reservation cannot be cancelled");
            
        Status = ReservationStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Confirm()
    {
        if (Status != ReservationStatus.Pending)
            throw new InvalidOperationException("Only pending reservations can be confirmed");
            
        Status = ReservationStatus.Confirmed;
        UpdatedAt = DateTime.UtcNow;
    }

    public static string GenerateConfirmationNumber()
    {
        return $"RF{DateTime.UtcNow:yyyyMMdd}{Random.Shared.Next(1000, 9999)}";
    }
}
using System.ComponentModel.DataAnnotations;
using RentaFacil.Shared.Enums;

namespace RentaFacil.Shared.Models;

public class Customer
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [Phone]
    [StringLength(20)]
    public string Phone { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    public string DocumentNumber { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property for reservations
    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

    // Business logic methods
    public bool HasActiveReservations()
    {
        return Reservations.Any(r => 
            r.Status == ReservationStatus.Confirmed && 
            r.EndDate > DateTime.UtcNow);
    }

    public int GetTotalReservations()
    {
        return Reservations.Count;
    }
}
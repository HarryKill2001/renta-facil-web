using RentaFacil.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace RentaFacil.Shared.DTOs;

public class ReservationDto
{
    public int Id { get; set; }
    public string ConfirmationNumber { get; set; } = string.Empty;
    public int VehicleId { get; set; }
    public int CustomerId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalPrice { get; set; }
    public ReservationStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public VehicleDto? Vehicle { get; set; }
    public CustomerDto? Customer { get; set; }
}

public class CreateReservationDto
{
    [Required]
    public int VehicleId { get; set; }
    
    [Required]
    public DateTime StartDate { get; set; }
    
    [Required]
    public DateTime EndDate { get; set; }
    
    [Required]
    public CreateCustomerDto CustomerInfo { get; set; } = new();
}

public class ReservationSearchDto
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public ReservationStatus? Status { get; set; }
    public int? CustomerId { get; set; }
    public int? VehicleId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class CancelReservationDto
{
    [Required]
    [StringLength(500)]
    public string Reason { get; set; } = string.Empty;
}
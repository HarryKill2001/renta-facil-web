using RentaFacil.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace RentaFacil.Shared.DTOs;

public class VehicleDto
{
    public int Id { get; set; }
    public VehicleType Type { get; set; }
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal PricePerDay { get; set; }
    public bool Available { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateVehicleDto
{
    [Required]
    public VehicleType Type { get; set; }
    
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Model { get; set; } = string.Empty;
    
    [Required]
    [Range(2000, 2030)]
    public int Year { get; set; }
    
    [Required]
    [Range(0.01, 1000.00)]
    public decimal PricePerDay { get; set; }
}

public class UpdateVehicleDto
{
    [StringLength(100, MinimumLength = 2)]
    public string? Model { get; set; }
    
    [Range(2000, 2030)]
    public int? Year { get; set; }
    
    [Range(0.01, 1000.00)]
    public decimal? PricePerDay { get; set; }
    
    public bool? Available { get; set; }
}

public class VehicleAvailabilityRequest
{
    [Required]
    public DateTime StartDate { get; set; }
    
    [Required]
    public DateTime EndDate { get; set; }
    
    public VehicleType? Type { get; set; }
}
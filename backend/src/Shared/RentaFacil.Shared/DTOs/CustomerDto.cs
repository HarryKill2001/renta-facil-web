using System.ComponentModel.DataAnnotations;

namespace RentaFacil.Shared.DTOs;

public class CustomerDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int TotalReservations { get; set; }
}

public class CreateCustomerDto
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
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
    [StringLength(50, MinimumLength = 5)]
    public string DocumentNumber { get; set; } = string.Empty;
}

public class UpdateCustomerDto
{
    [StringLength(100, MinimumLength = 2)]
    public string? Name { get; set; }
    
    [EmailAddress]
    [StringLength(100)]
    public string? Email { get; set; }
    
    [Phone]
    [StringLength(20)]
    public string? Phone { get; set; }
}
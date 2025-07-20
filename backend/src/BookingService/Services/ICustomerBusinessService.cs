using RentaFacil.Shared.DTOs;

namespace BookingService.Services;

public interface ICustomerBusinessService
{
    Task<IEnumerable<CustomerDto>> GetAllCustomersAsync();
    Task<CustomerDto?> GetCustomerByIdAsync(int id);
    Task<CustomerDto?> GetCustomerByEmailAsync(string email);
    Task<CustomerDto?> GetCustomerByDocumentNumberAsync(string documentNumber);
    Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto createDto);
    Task<CustomerDto?> UpdateCustomerAsync(int id, UpdateCustomerDto updateDto);
    Task<bool> DeleteCustomerAsync(int id);
    Task<IEnumerable<ReservationDto>> GetCustomerReservationsAsync(int customerId);
}
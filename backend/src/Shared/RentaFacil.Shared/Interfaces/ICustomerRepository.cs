using RentaFacil.Shared.Models;

namespace RentaFacil.Shared.Interfaces;

public interface ICustomerRepository : IRepository<Customer>
{
    Task<Customer?> GetByEmailAsync(string email);
    Task<Customer?> GetByDocumentNumberAsync(string documentNumber);
    Task<Customer?> GetCustomerWithReservationsAsync(int customerId);
    Task<bool> ExistsByEmailAsync(string email);
    Task<bool> ExistsByDocumentNumberAsync(string documentNumber);
}
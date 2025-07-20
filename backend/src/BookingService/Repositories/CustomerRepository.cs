using Microsoft.EntityFrameworkCore;
using RentaFacil.Shared.Interfaces;
using RentaFacil.Shared.Models;
using RentaFacil.Shared.Repositories;
using BookingService.Data;

namespace BookingService.Repositories;

public class CustomerRepository : BaseRepository<Customer>, ICustomerRepository
{
    public CustomerRepository(BookingDbContext context) : base(context)
    {
    }

    public async Task<Customer?> GetByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(c => c.Email == email);
    }

    public async Task<Customer?> GetByDocumentNumberAsync(string documentNumber)
    {
        return await _dbSet.FirstOrDefaultAsync(c => c.DocumentNumber == documentNumber);
    }

    public async Task<Customer?> GetCustomerWithReservationsAsync(int customerId)
    {
        return await _dbSet
            .Include(c => c.Reservations)
                .ThenInclude(r => r.Vehicle)
            .FirstOrDefaultAsync(c => c.Id == customerId);
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await _dbSet.AnyAsync(c => c.Email == email);
    }

    public async Task<bool> ExistsByDocumentNumberAsync(string documentNumber)
    {
        return await _dbSet.AnyAsync(c => c.DocumentNumber == documentNumber);
    }
}
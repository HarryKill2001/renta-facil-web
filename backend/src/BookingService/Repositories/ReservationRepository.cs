using Microsoft.EntityFrameworkCore;
using RentaFacil.Shared.Enums;
using RentaFacil.Shared.Interfaces;
using RentaFacil.Shared.Models;
using RentaFacil.Shared.Repositories;
using BookingService.Data;

namespace BookingService.Repositories;

public class ReservationRepository : BaseRepository<Reservation>, IReservationRepository
{
    public ReservationRepository(BookingDbContext context) : base(context)
    {
    }

    public async Task<Reservation?> GetByConfirmationNumberAsync(string confirmationNumber)
    {
        return await _dbSet
            .Include(r => r.Customer)
            .FirstOrDefaultAsync(r => r.ConfirmationNumber == confirmationNumber);
    }

    public async Task<IEnumerable<Reservation>> GetByCustomerIdAsync(int customerId)
    {
        return await _dbSet
            .Where(r => r.CustomerId == customerId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Reservation>> GetByVehicleIdAsync(int vehicleId)
    {
        return await _dbSet
            .Include(r => r.Customer)
            .Where(r => r.VehicleId == vehicleId)
            .OrderByDescending(r => r.StartDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Reservation>> GetActiveReservationsAsync()
    {
        var now = DateTime.UtcNow;
        return await _dbSet
            .Include(r => r.Customer)
            .Where(r => r.Status == ReservationStatus.Confirmed && 
                       r.StartDate <= now && 
                       r.EndDate > now)
            .ToListAsync();
    }

    public async Task<IEnumerable<Reservation>> GetReservationsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Include(r => r.Customer)
            .Where(r => r.StartDate <= endDate && r.EndDate >= startDate)
            .OrderBy(r => r.StartDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Reservation>> GetReservationsByStatusAsync(ReservationStatus status)
    {
        return await _dbSet
            .Include(r => r.Customer)
            .Where(r => r.Status == status)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<Reservation?> GetReservationWithDetailsAsync(int reservationId)
    {
        return await _dbSet
            .Include(r => r.Customer)
            .FirstOrDefaultAsync(r => r.Id == reservationId);
    }

    public async Task<bool> HasConflictingReservationsAsync(int vehicleId, DateTime startDate, DateTime endDate, int? excludeReservationId = null)
    {
        var query = _dbSet.Where(r => 
            r.VehicleId == vehicleId &&
            r.Status == ReservationStatus.Confirmed &&
            ((r.StartDate <= startDate && r.EndDate > startDate) ||
             (r.StartDate < endDate && r.EndDate >= endDate) ||
             (r.StartDate >= startDate && r.EndDate <= endDate)));

        if (excludeReservationId.HasValue)
        {
            query = query.Where(r => r.Id != excludeReservationId.Value);
        }

        return await query.AnyAsync();
    }
}
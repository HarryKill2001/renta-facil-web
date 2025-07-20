using Microsoft.EntityFrameworkCore;
using RentaFacil.Shared.Enums;
using RentaFacil.Shared.Interfaces;
using RentaFacil.Shared.Models;
using RentaFacil.Shared.Repositories;
using VehicleService.Data;

namespace VehicleService.Repositories;

public class VehicleRepository : BaseRepository<Vehicle>, IVehicleRepository
{
    public VehicleRepository(VehicleDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Vehicle>> GetAvailableVehiclesAsync(DateTime startDate, DateTime endDate, VehicleType? vehicleType = null)
    {
        var query = _dbSet.Where(v => v.Available);

        if (vehicleType.HasValue)
        {
            query = query.Where(v => v.Type == vehicleType.Value);
        }

        // For VehicleService, we don't have access to reservations data
        // This would typically be handled by calling BookingService
        // For now, return available vehicles (simple MVP approach)
        return await query.ToListAsync();
    }

    public async Task<Vehicle?> GetVehicleWithReservationsAsync(int vehicleId)
    {
        // In microservices architecture, VehicleService doesn't manage reservations
        // This would require a call to BookingService
        // For MVP, return vehicle without reservations
        return await GetByIdAsync(vehicleId);
    }

    public async Task<bool> IsVehicleAvailableAsync(int vehicleId, DateTime startDate, DateTime endDate)
    {
        var vehicle = await GetByIdAsync(vehicleId);
        if (vehicle == null || !vehicle.Available)
            return false;

        // In real implementation, this would call BookingService to check reservations
        // For MVP, just check if vehicle exists and is marked as available
        return true;
    }
}
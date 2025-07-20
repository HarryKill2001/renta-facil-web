using RentaFacil.Shared.Enums;
using RentaFacil.Shared.Models;

namespace RentaFacil.Shared.Interfaces;

public interface IVehicleRepository : IRepository<Vehicle>
{
    Task<IEnumerable<Vehicle>> GetAvailableVehiclesAsync(DateTime startDate, DateTime endDate, VehicleType? vehicleType = null);
    Task<Vehicle?> GetVehicleWithReservationsAsync(int vehicleId);
    Task<bool> IsVehicleAvailableAsync(int vehicleId, DateTime startDate, DateTime endDate);
}
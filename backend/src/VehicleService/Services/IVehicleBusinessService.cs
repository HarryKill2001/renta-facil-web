using RentaFacil.Shared.DTOs;
using RentaFacil.Shared.Enums;

namespace VehicleService.Services;

public interface IVehicleBusinessService
{
    Task<IEnumerable<VehicleDto>> GetAllVehiclesAsync();
    Task<VehicleDto?> GetVehicleByIdAsync(int id);
    Task<VehicleDto> CreateVehicleAsync(CreateVehicleDto createDto);
    Task<VehicleDto?> UpdateVehicleAsync(int id, UpdateVehicleDto updateDto);
    Task<bool> DeleteVehicleAsync(int id);
    Task<IEnumerable<VehicleDto>> GetAvailableVehiclesAsync(VehicleAvailabilityRequest request);
}
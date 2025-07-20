using RentaFacil.Shared.DTOs;
using RentaFacil.Shared.Enums;
using RentaFacil.Shared.Interfaces;
using RentaFacil.Shared.Models;

namespace VehicleService.Services;

public class VehicleBusinessService : IVehicleBusinessService
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly ILogger<VehicleBusinessService> _logger;

    public VehicleBusinessService(IVehicleRepository vehicleRepository, ILogger<VehicleBusinessService> logger)
    {
        _vehicleRepository = vehicleRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<VehicleDto>> GetAllVehiclesAsync()
    {
        _logger.LogInformation("Retrieving all vehicles");
        
        var vehicles = await _vehicleRepository.GetAllAsync();
        return vehicles.Select(MapToDto);
    }

    public async Task<VehicleDto?> GetVehicleByIdAsync(int id)
    {
        _logger.LogInformation("Retrieving vehicle with ID: {VehicleId}", id);
        
        if (id <= 0)
        {
            _logger.LogWarning("Invalid vehicle ID provided: {VehicleId}", id);
            return null;
        }

        var vehicle = await _vehicleRepository.GetByIdAsync(id);
        return vehicle != null ? MapToDto(vehicle) : null;
    }

    public async Task<VehicleDto> CreateVehicleAsync(CreateVehicleDto createDto)
    {
        _logger.LogInformation("Creating new vehicle: {Model}", createDto.Model);

        var vehicle = new Vehicle
        {
            Type = createDto.Type,
            Model = createDto.Model,
            Year = createDto.Year,
            PricePerDay = createDto.PricePerDay,
            Available = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _vehicleRepository.AddAsync(vehicle);
        await _vehicleRepository.SaveChangesAsync();

        _logger.LogInformation("Successfully created vehicle with ID: {VehicleId}", vehicle.Id);
        return MapToDto(vehicle);
    }

    public async Task<VehicleDto?> UpdateVehicleAsync(int id, UpdateVehicleDto updateDto)
    {
        _logger.LogInformation("Updating vehicle with ID: {VehicleId}", id);

        var vehicle = await _vehicleRepository.GetByIdAsync(id);
        if (vehicle == null)
        {
            _logger.LogWarning("Vehicle not found with ID: {VehicleId}", id);
            return null;
        }

        // Update only provided fields
        if (!string.IsNullOrEmpty(updateDto.Model))
            vehicle.Model = updateDto.Model;
        
        if (updateDto.Year.HasValue)
            vehicle.Year = updateDto.Year.Value;
        
        if (updateDto.PricePerDay.HasValue)
            vehicle.PricePerDay = updateDto.PricePerDay.Value;
        
        if (updateDto.Available.HasValue)
            vehicle.Available = updateDto.Available.Value;

        vehicle.UpdatedAt = DateTime.UtcNow;

        _vehicleRepository.Update(vehicle);
        await _vehicleRepository.SaveChangesAsync();

        _logger.LogInformation("Successfully updated vehicle with ID: {VehicleId}", id);
        return MapToDto(vehicle);
    }

    public async Task<bool> DeleteVehicleAsync(int id)
    {
        _logger.LogInformation("Deleting vehicle with ID: {VehicleId}", id);

        var vehicle = await _vehicleRepository.GetByIdAsync(id);
        if (vehicle == null)
        {
            _logger.LogWarning("Vehicle not found with ID: {VehicleId}", id);
            return false;
        }

        _vehicleRepository.Remove(vehicle);
        await _vehicleRepository.SaveChangesAsync();

        _logger.LogInformation("Successfully deleted vehicle with ID: {VehicleId}", id);
        return true;
    }

    public async Task<IEnumerable<VehicleDto>> GetAvailableVehiclesAsync(VehicleAvailabilityRequest request)
    {
        _logger.LogInformation("Searching for available vehicles from {StartDate} to {EndDate}, Type: {VehicleType}", 
            request.StartDate, request.EndDate, request.Type);

        // Validate date range
        if (request.StartDate >= request.EndDate)
        {
            _logger.LogWarning("Invalid date range: StartDate {StartDate} >= EndDate {EndDate}", 
                request.StartDate, request.EndDate);
            return Enumerable.Empty<VehicleDto>();
        }

        if (request.StartDate < DateTime.UtcNow.Date)
        {
            _logger.LogWarning("Start date {StartDate} is in the past", request.StartDate);
            return Enumerable.Empty<VehicleDto>();
        }

        var vehicles = await _vehicleRepository.GetAvailableVehiclesAsync(
            request.StartDate, request.EndDate, request.Type);

        var result = vehicles.Select(MapToDto).ToList();
        
        _logger.LogInformation("Found {Count} available vehicles", result.Count);
        return result;
    }

    private static VehicleDto MapToDto(Vehicle vehicle)
    {
        return new VehicleDto
        {
            Id = vehicle.Id,
            Type = vehicle.Type,
            Model = vehicle.Model,
            Year = vehicle.Year,
            PricePerDay = vehicle.PricePerDay,
            Available = vehicle.Available,
            CreatedAt = vehicle.CreatedAt
        };
    }
}
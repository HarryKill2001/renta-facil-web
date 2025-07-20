using Microsoft.AspNetCore.Mvc;
using RentaFacil.Shared.DTOs;
using VehicleService.Services;

namespace VehicleService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VehiclesController : ControllerBase
{
    private readonly IVehicleBusinessService _vehicleService;
    private readonly ILogger<VehiclesController> _logger;

    public VehiclesController(IVehicleBusinessService vehicleService, ILogger<VehiclesController> logger)
    {
        _vehicleService = vehicleService;
        _logger = logger;
    }

    /// <summary>
    /// Get all vehicles
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<VehicleDto>>>> GetAllVehicles()
    {
        try
        {
            var vehicles = await _vehicleService.GetAllVehiclesAsync();
            return Ok(ApiResponse<IEnumerable<VehicleDto>>.SuccessResult(vehicles, "Vehicles retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all vehicles");
            return StatusCode(500, ApiResponse<IEnumerable<VehicleDto>>.ErrorResult("Internal server error occurred"));
        }
    }

    /// <summary>
    /// Get vehicle by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<VehicleDto>>> GetVehicle(int id)
    {
        try
        {
            var vehicle = await _vehicleService.GetVehicleByIdAsync(id);
            if (vehicle == null)
            {
                return NotFound(ApiResponse<VehicleDto>.ErrorResult("Vehicle not found"));
            }

            return Ok(ApiResponse<VehicleDto>.SuccessResult(vehicle, "Vehicle retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving vehicle {VehicleId}", id);
            return StatusCode(500, ApiResponse<VehicleDto>.ErrorResult("Internal server error occurred"));
        }
    }

    /// <summary>
    /// Create a new vehicle
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<VehicleDto>>> CreateVehicle([FromBody] CreateVehicleDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<VehicleDto>.ErrorResult("Validation failed", errors));
            }

            var vehicle = await _vehicleService.CreateVehicleAsync(createDto);
            return CreatedAtAction(nameof(GetVehicle), new { id = vehicle.Id },
                ApiResponse<VehicleDto>.SuccessResult(vehicle, "Vehicle created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating vehicle");
            return StatusCode(500, ApiResponse<VehicleDto>.ErrorResult("Internal server error occurred"));
        }
    }

    /// <summary>
    /// Update an existing vehicle
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<VehicleDto>>> UpdateVehicle(int id, [FromBody] UpdateVehicleDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<VehicleDto>.ErrorResult("Validation failed", errors));
            }

            var vehicle = await _vehicleService.UpdateVehicleAsync(id, updateDto);
            if (vehicle == null)
            {
                return NotFound(ApiResponse<VehicleDto>.ErrorResult("Vehicle not found"));
            }

            return Ok(ApiResponse<VehicleDto>.SuccessResult(vehicle, "Vehicle updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating vehicle {VehicleId}", id);
            return StatusCode(500, ApiResponse<VehicleDto>.ErrorResult("Internal server error occurred"));
        }
    }

    /// <summary>
    /// Delete a vehicle
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse>> DeleteVehicle(int id)
    {
        try
        {
            var success = await _vehicleService.DeleteVehicleAsync(id);
            if (!success)
            {
                return NotFound(ApiResponse.ErrorResult("Vehicle not found"));
            }

            return Ok(ApiResponse.SuccessResult("Vehicle deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting vehicle {VehicleId}", id);
            return StatusCode(500, ApiResponse.ErrorResult("Internal server error occurred"));
        }
    }

    /// <summary>
    /// Get available vehicles for a date range and optional vehicle type
    /// </summary>
    [HttpGet("availability")]
    public async Task<ActionResult<ApiResponse<IEnumerable<VehicleDto>>>> GetAvailableVehicles([FromQuery] VehicleAvailabilityRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<IEnumerable<VehicleDto>>.ErrorResult("Validation failed", errors));
            }

            var vehicles = await _vehicleService.GetAvailableVehiclesAsync(request);
            return Ok(ApiResponse<IEnumerable<VehicleDto>>.SuccessResult(vehicles, "Available vehicles retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving available vehicles");
            return StatusCode(500, ApiResponse<IEnumerable<VehicleDto>>.ErrorResult("Internal server error occurred"));
        }
    }
}
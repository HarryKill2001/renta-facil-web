using Microsoft.AspNetCore.Mvc;
using RentaFacil.Shared.DTOs;
using BookingService.Services;

namespace BookingService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReservationsController : ControllerBase
{
    private readonly IReservationBusinessService _reservationService;
    private readonly ILogger<ReservationsController> _logger;

    public ReservationsController(IReservationBusinessService reservationService, ILogger<ReservationsController> logger)
    {
        _reservationService = reservationService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new reservation
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<ReservationDto>>> CreateReservation([FromBody] CreateReservationDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<ReservationDto>.ErrorResult("Validation failed", errors));
            }

            var reservation = await _reservationService.CreateReservationAsync(createDto);
            return CreatedAtAction(nameof(GetReservation), new { id = reservation.Id },
                ApiResponse<ReservationDto>.SuccessResult(reservation, "Reservation created successfully"));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid reservation data: {Message}", ex.Message);
            return BadRequest(ApiResponse<ReservationDto>.ErrorResult(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Reservation creation failed: {Message}", ex.Message);
            return Conflict(ApiResponse<ReservationDto>.ErrorResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating reservation");
            return StatusCode(500, ApiResponse<ReservationDto>.ErrorResult("Internal server error occurred"));
        }
    }

    /// <summary>
    /// Get reservation by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<ReservationDto>>> GetReservation(int id)
    {
        try
        {
            var reservation = await _reservationService.GetReservationByIdAsync(id);
            if (reservation == null)
            {
                return NotFound(ApiResponse<ReservationDto>.ErrorResult("Reservation not found"));
            }

            return Ok(ApiResponse<ReservationDto>.SuccessResult(reservation, "Reservation retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving reservation {ReservationId}", id);
            return StatusCode(500, ApiResponse<ReservationDto>.ErrorResult("Internal server error occurred"));
        }
    }

    /// <summary>
    /// Get reservation by confirmation number
    /// </summary>
    [HttpGet("by-confirmation/{confirmationNumber}")]
    public async Task<ActionResult<ApiResponse<ReservationDto>>> GetReservationByConfirmationNumber(string confirmationNumber)
    {
        try
        {
            var reservation = await _reservationService.GetReservationByConfirmationNumberAsync(confirmationNumber);
            if (reservation == null)
            {
                return NotFound(ApiResponse<ReservationDto>.ErrorResult("Reservation not found"));
            }

            return Ok(ApiResponse<ReservationDto>.SuccessResult(reservation, "Reservation retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving reservation by confirmation number {ConfirmationNumber}", confirmationNumber);
            return StatusCode(500, ApiResponse<ReservationDto>.ErrorResult("Internal server error occurred"));
        }
    }

    /// <summary>
    /// Search reservations with filters and pagination
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<ReservationDto>>>> GetReservations([FromQuery] ReservationSearchDto searchDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<IEnumerable<ReservationDto>>.ErrorResult("Validation failed", errors));
            }

            var reservations = await _reservationService.GetReservationsAsync(searchDto);
            return Ok(ApiResponse<IEnumerable<ReservationDto>>.SuccessResult(reservations, "Reservations retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving reservations");
            return StatusCode(500, ApiResponse<IEnumerable<ReservationDto>>.ErrorResult("Internal server error occurred"));
        }
    }

    /// <summary>
    /// Cancel a reservation
    /// </summary>
    [HttpPost("{id}/cancel")]
    public async Task<ActionResult<ApiResponse<ReservationDto>>> CancelReservation(int id, [FromBody] CancelReservationDto cancelDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<ReservationDto>.ErrorResult("Validation failed", errors));
            }

            var reservation = await _reservationService.CancelReservationAsync(id, cancelDto);
            if (reservation == null)
            {
                return NotFound(ApiResponse<ReservationDto>.ErrorResult("Reservation not found"));
            }

            return Ok(ApiResponse<ReservationDto>.SuccessResult(reservation, "Reservation cancelled successfully"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Reservation cancellation failed: {Message}", ex.Message);
            return BadRequest(ApiResponse<ReservationDto>.ErrorResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling reservation {ReservationId}", id);
            return StatusCode(500, ApiResponse<ReservationDto>.ErrorResult("Internal server error occurred"));
        }
    }

    /// <summary>
    /// Confirm a reservation
    /// </summary>
    [HttpPost("{id}/confirm")]
    public async Task<ActionResult<ApiResponse<ReservationDto>>> ConfirmReservation(int id)
    {
        try
        {
            var reservation = await _reservationService.ConfirmReservationAsync(id);
            if (reservation == null)
            {
                return NotFound(ApiResponse<ReservationDto>.ErrorResult("Reservation not found"));
            }

            return Ok(ApiResponse<ReservationDto>.SuccessResult(reservation, "Reservation confirmed successfully"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Reservation confirmation failed: {Message}", ex.Message);
            return BadRequest(ApiResponse<ReservationDto>.ErrorResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming reservation {ReservationId}", id);
            return StatusCode(500, ApiResponse<ReservationDto>.ErrorResult("Internal server error occurred"));
        }
    }

    /// <summary>
    /// Check vehicle availability for date range
    /// </summary>
    [HttpGet("check-availability")]
    public async Task<ActionResult<ApiResponse<bool>>> CheckVehicleAvailability(
        [FromQuery] int vehicleId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        try
        {
            if (startDate >= endDate)
            {
                return BadRequest(ApiResponse<bool>.ErrorResult("End date must be after start date"));
            }

            if (startDate < DateTime.UtcNow.Date)
            {
                return BadRequest(ApiResponse<bool>.ErrorResult("Start date cannot be in the past"));
            }

            var isAvailable = await _reservationService.IsVehicleAvailableAsync(vehicleId, startDate, endDate);
            var message = isAvailable ? "Vehicle is available" : "Vehicle is not available for the selected dates";
            
            return Ok(ApiResponse<bool>.SuccessResult(isAvailable, message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking vehicle availability for {VehicleId}", vehicleId);
            return StatusCode(500, ApiResponse<bool>.ErrorResult("Internal server error occurred"));
        }
    }
}
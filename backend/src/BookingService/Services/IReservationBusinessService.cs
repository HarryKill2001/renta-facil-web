using RentaFacil.Shared.DTOs;
using RentaFacil.Shared.Enums;

namespace BookingService.Services;

public interface IReservationBusinessService
{
    Task<ReservationDto> CreateReservationAsync(CreateReservationDto createDto);
    Task<ReservationDto?> GetReservationByIdAsync(int id);
    Task<ReservationDto?> GetReservationByConfirmationNumberAsync(string confirmationNumber);
    Task<IEnumerable<ReservationDto>> GetReservationsAsync(ReservationSearchDto searchDto);
    Task<ReservationDto?> CancelReservationAsync(int id, CancelReservationDto cancelDto);
    Task<ReservationDto?> ConfirmReservationAsync(int id);
    Task<bool> IsVehicleAvailableAsync(int vehicleId, DateTime startDate, DateTime endDate);
}
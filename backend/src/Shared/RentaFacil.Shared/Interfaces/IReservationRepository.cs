using RentaFacil.Shared.Enums;
using RentaFacil.Shared.Models;

namespace RentaFacil.Shared.Interfaces;

public interface IReservationRepository : IRepository<Reservation>
{
    Task<Reservation?> GetByConfirmationNumberAsync(string confirmationNumber);
    Task<IEnumerable<Reservation>> GetByCustomerIdAsync(int customerId);
    Task<IEnumerable<Reservation>> GetByVehicleIdAsync(int vehicleId);
    Task<IEnumerable<Reservation>> GetActiveReservationsAsync();
    Task<IEnumerable<Reservation>> GetReservationsByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<Reservation>> GetReservationsByStatusAsync(ReservationStatus status);
    Task<Reservation?> GetReservationWithDetailsAsync(int reservationId);
}
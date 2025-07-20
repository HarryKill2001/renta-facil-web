using RentaFacil.Shared.DTOs;
using RentaFacil.Shared.Enums;
using RentaFacil.Shared.Interfaces;
using RentaFacil.Shared.Models;

namespace BookingService.Services;

public class ReservationBusinessService : IReservationBusinessService
{
    private readonly IReservationRepository _reservationRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly ILogger<ReservationBusinessService> _logger;

    public ReservationBusinessService(
        IReservationRepository reservationRepository,
        ICustomerRepository customerRepository,
        ILogger<ReservationBusinessService> logger)
    {
        _reservationRepository = reservationRepository;
        _customerRepository = customerRepository;
        _logger = logger;
    }

    public async Task<ReservationDto> CreateReservationAsync(CreateReservationDto createDto)
    {
        _logger.LogInformation("Creating reservation for vehicle {VehicleId} from {StartDate} to {EndDate}",
            createDto.VehicleId, createDto.StartDate, createDto.EndDate);

        // Validate dates
        if (createDto.StartDate >= createDto.EndDate)
        {
            throw new ArgumentException("End date must be after start date");
        }

        if (createDto.StartDate < DateTime.UtcNow.Date)
        {
            throw new ArgumentException("Start date cannot be in the past");
        }

        // Check for conflicting reservations
        var hasConflicts = await ((BookingService.Repositories.ReservationRepository)_reservationRepository)
            .HasConflictingReservationsAsync(createDto.VehicleId, createDto.StartDate, createDto.EndDate);

        if (hasConflicts)
        {
            throw new InvalidOperationException("Vehicle is not available for the selected dates");
        }

        // Get or create customer
        var customer = await GetOrCreateCustomerAsync(createDto.CustomerInfo);

        // Calculate total price (simplified - in real system would get price from VehicleService)
        var days = (createDto.EndDate - createDto.StartDate).Days;
        var estimatedPricePerDay = 50.00m; // Placeholder - would come from VehicleService
        var totalPrice = days * estimatedPricePerDay;

        var reservation = new Reservation
        {
            ConfirmationNumber = Reservation.GenerateConfirmationNumber(),
            VehicleId = createDto.VehicleId,
            CustomerId = customer.Id,
            StartDate = createDto.StartDate,
            EndDate = createDto.EndDate,
            TotalPrice = totalPrice,
            Status = ReservationStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _reservationRepository.AddAsync(reservation);
        await _reservationRepository.SaveChangesAsync();

        _logger.LogInformation("Successfully created reservation with confirmation number: {ConfirmationNumber}",
            reservation.ConfirmationNumber);

        // Return with customer info
        reservation.Customer = customer;
        return MapToDto(reservation);
    }

    public async Task<ReservationDto?> GetReservationByIdAsync(int id)
    {
        _logger.LogInformation("Retrieving reservation with ID: {ReservationId}", id);

        var reservation = await _reservationRepository.GetReservationWithDetailsAsync(id);
        return reservation != null ? MapToDto(reservation) : null;
    }

    public async Task<ReservationDto?> GetReservationByConfirmationNumberAsync(string confirmationNumber)
    {
        _logger.LogInformation("Retrieving reservation with confirmation number: {ConfirmationNumber}", confirmationNumber);

        var reservation = await _reservationRepository.GetByConfirmationNumberAsync(confirmationNumber);
        return reservation != null ? MapToDto(reservation) : null;
    }

    public async Task<IEnumerable<ReservationDto>> GetReservationsAsync(ReservationSearchDto searchDto)
    {
        _logger.LogInformation("Searching reservations with filters");

        // Build query based on search criteria
        IEnumerable<Reservation> reservations;

        if (searchDto.Status.HasValue)
        {
            reservations = await _reservationRepository.GetReservationsByStatusAsync(searchDto.Status.Value);
        }
        else if (searchDto.StartDate.HasValue && searchDto.EndDate.HasValue)
        {
            reservations = await _reservationRepository.GetReservationsByDateRangeAsync(
                searchDto.StartDate.Value, searchDto.EndDate.Value);
        }
        else if (searchDto.CustomerId.HasValue)
        {
            reservations = await _reservationRepository.GetByCustomerIdAsync(searchDto.CustomerId.Value);
        }
        else if (searchDto.VehicleId.HasValue)
        {
            reservations = await _reservationRepository.GetByVehicleIdAsync(searchDto.VehicleId.Value);
        }
        else
        {
            reservations = await _reservationRepository.GetAllAsync();
        }

        // Apply pagination
        var pagedResults = reservations
            .Skip((searchDto.PageNumber - 1) * searchDto.PageSize)
            .Take(searchDto.PageSize);

        return pagedResults.Select(MapToDto);
    }

    public async Task<ReservationDto?> CancelReservationAsync(int id, CancelReservationDto cancelDto)
    {
        _logger.LogInformation("Cancelling reservation with ID: {ReservationId}", id);

        var reservation = await _reservationRepository.GetReservationWithDetailsAsync(id);
        if (reservation == null)
        {
            _logger.LogWarning("Reservation not found with ID: {ReservationId}", id);
            return null;
        }

        if (!reservation.CanBeCancelled())
        {
            throw new InvalidOperationException("Reservation cannot be cancelled");
        }

        reservation.Cancel();
        _reservationRepository.Update(reservation);
        await _reservationRepository.SaveChangesAsync();

        _logger.LogInformation("Successfully cancelled reservation {ReservationId}", id);
        return MapToDto(reservation);
    }

    public async Task<ReservationDto?> ConfirmReservationAsync(int id)
    {
        _logger.LogInformation("Confirming reservation with ID: {ReservationId}", id);

        var reservation = await _reservationRepository.GetReservationWithDetailsAsync(id);
        if (reservation == null)
        {
            _logger.LogWarning("Reservation not found with ID: {ReservationId}", id);
            return null;
        }

        reservation.Confirm();
        _reservationRepository.Update(reservation);
        await _reservationRepository.SaveChangesAsync();

        _logger.LogInformation("Successfully confirmed reservation {ReservationId}", id);
        return MapToDto(reservation);
    }

    public async Task<bool> IsVehicleAvailableAsync(int vehicleId, DateTime startDate, DateTime endDate)
    {
        _logger.LogInformation("Checking availability for vehicle {VehicleId} from {StartDate} to {EndDate}",
            vehicleId, startDate, endDate);

        var hasConflicts = await ((BookingService.Repositories.ReservationRepository)_reservationRepository)
            .HasConflictingReservationsAsync(vehicleId, startDate, endDate);

        return !hasConflicts;
    }

    private async Task<Customer> GetOrCreateCustomerAsync(CreateCustomerDto customerDto)
    {
        // Try to find existing customer by email or document number
        var existingCustomer = await _customerRepository.GetByEmailAsync(customerDto.Email);
        if (existingCustomer != null)
        {
            return existingCustomer;
        }

        existingCustomer = await _customerRepository.GetByDocumentNumberAsync(customerDto.DocumentNumber);
        if (existingCustomer != null)
        {
            return existingCustomer;
        }

        // Create new customer
        var newCustomer = new Customer
        {
            Name = customerDto.Name,
            Email = customerDto.Email,
            Phone = customerDto.Phone,
            DocumentNumber = customerDto.DocumentNumber,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _customerRepository.AddAsync(newCustomer);
        await _customerRepository.SaveChangesAsync();

        return newCustomer;
    }

    private static ReservationDto MapToDto(Reservation reservation)
    {
        return new ReservationDto
        {
            Id = reservation.Id,
            ConfirmationNumber = reservation.ConfirmationNumber,
            VehicleId = reservation.VehicleId,
            CustomerId = reservation.CustomerId,
            StartDate = reservation.StartDate,
            EndDate = reservation.EndDate,
            TotalPrice = reservation.TotalPrice,
            Status = reservation.Status,
            CreatedAt = reservation.CreatedAt,
            Vehicle = reservation.Vehicle != null ? new VehicleDto
            {
                Id = reservation.Vehicle.Id,
                Type = reservation.Vehicle.Type,
                Model = reservation.Vehicle.Model,
                Year = reservation.Vehicle.Year,
                PricePerDay = reservation.Vehicle.PricePerDay,
                Available = reservation.Vehicle.Available,
                CreatedAt = reservation.Vehicle.CreatedAt
            } : null,
            Customer = reservation.Customer != null ? new CustomerDto
            {
                Id = reservation.Customer.Id,
                Name = reservation.Customer.Name,
                Email = reservation.Customer.Email,
                Phone = reservation.Customer.Phone,
                DocumentNumber = reservation.Customer.DocumentNumber,
                CreatedAt = reservation.Customer.CreatedAt,
                TotalReservations = 0 // Would need separate query to get accurate count
            } : null
        };
    }
}
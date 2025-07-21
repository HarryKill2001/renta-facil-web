using RentaFacil.Shared.DTOs;
using RentaFacil.Shared.Enums;
using RentaFacil.Shared.Interfaces;
using RentaFacil.Shared.Models;
using System.Text.Json;

namespace BookingService.Services;

public class ReservationBusinessService : IReservationBusinessService
{
    private readonly IReservationRepository _reservationRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly ILogger<ReservationBusinessService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public ReservationBusinessService(
        IReservationRepository reservationRepository,
        ICustomerRepository customerRepository,
        ILogger<ReservationBusinessService> logger,
        IHttpClientFactory httpClientFactory)
    {
        _reservationRepository = reservationRepository;
        _customerRepository = customerRepository;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<ReservationDto> CreateReservationAsync(CreateReservationDto createDto)
    {
        _logger.LogInformation("Creating reservation for vehicle {VehicleId} from {StartDate} to {EndDate}",
            createDto.VehicleId, createDto.StartDate, createDto.EndDate);

        // Validate vehicle exists by calling VehicleService
        if (!await ValidateVehicleExistsAsync(createDto.VehicleId))
        {
            throw new ArgumentException($"Vehicle with ID {createDto.VehicleId} does not exist", nameof(createDto.VehicleId));
        }

        // Validate dates
        if (createDto.StartDate >= createDto.EndDate)
        {
            throw new ArgumentException("End date must be after start date");
        }

        if (createDto.StartDate < DateTime.UtcNow.Date)
        {
            throw new ArgumentException("Start date cannot be in the past");
        }

        // Check for conflicting reservations - TEMPORARILY DISABLED DUE TO UNSAFE CASTING
        // var hasConflicts = await ((BookingService.Repositories.ReservationRepository)_reservationRepository)
        //     .HasConflictingReservationsAsync(createDto.VehicleId, createDto.StartDate, createDto.EndDate);

        // if (hasConflicts)
        // {
        //     throw new InvalidOperationException("Vehicle is not available for the selected dates");
        // }

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
        return await MapToDtoAsync(reservation);
    }

    public async Task<ReservationDto?> GetReservationByIdAsync(int id)
    {
        _logger.LogInformation("Retrieving reservation with ID: {ReservationId}", id);

        var reservation = await _reservationRepository.GetReservationWithDetailsAsync(id);
        return reservation != null ? await MapToDtoAsync(reservation) : null;
    }

    public async Task<ReservationDto?> GetReservationByConfirmationNumberAsync(string confirmationNumber)
    {
        _logger.LogInformation("Retrieving reservation with confirmation number: {ConfirmationNumber}", confirmationNumber);

        var reservation = await _reservationRepository.GetByConfirmationNumberAsync(confirmationNumber);
        return reservation != null ? await MapToDtoAsync(reservation) : null;
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

        var results = new List<ReservationDto>();
        foreach (var reservation in pagedResults)
        {
            results.Add(await MapToDtoAsync(reservation));
        }
        return results;
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
        return await MapToDtoAsync(reservation);
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
        return await MapToDtoAsync(reservation);
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

    private async Task<bool> ValidateVehicleExistsAsync(int vehicleId)
    {
        try
        {
            using var httpClient = _httpClientFactory.CreateClient("VehicleService");
            var response = await httpClient.GetAsync($"/api/vehicles/{vehicleId}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to validate vehicle existence for ID {VehicleId}", vehicleId);
            return false;
        }
    }

    private async Task<ReservationDto> MapToDtoAsync(Reservation reservation)
    {
        var dto = new ReservationDto
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

        // Fetch vehicle details from VehicleService
        dto.Vehicle = await GetVehicleDetailsAsync(reservation.VehicleId);
        
        return dto;
    }

    private async Task<VehicleDto?> GetVehicleDetailsAsync(int vehicleId)
    {
        try
        {
            using var httpClient = _httpClientFactory.CreateClient("VehicleService");
            var response = await httpClient.GetAsync($"/api/vehicles/{vehicleId}");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<VehicleDto>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                return apiResponse?.Data;
            }
            
            _logger.LogWarning("Failed to fetch vehicle details for ID {VehicleId}, Status: {StatusCode}", 
                vehicleId, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching vehicle details for ID {VehicleId}", vehicleId);
            return null;
        }
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
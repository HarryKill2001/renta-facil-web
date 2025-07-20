using RentaFacil.Shared.DTOs;
using RentaFacil.Shared.Interfaces;
using RentaFacil.Shared.Models;

namespace BookingService.Services;

public class CustomerBusinessService : ICustomerBusinessService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IReservationRepository _reservationRepository;
    private readonly ILogger<CustomerBusinessService> _logger;

    public CustomerBusinessService(
        ICustomerRepository customerRepository,
        IReservationRepository reservationRepository,
        ILogger<CustomerBusinessService> logger)
    {
        _customerRepository = customerRepository;
        _reservationRepository = reservationRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<CustomerDto>> GetAllCustomersAsync()
    {
        _logger.LogInformation("Retrieving all customers");
        
        var customers = await _customerRepository.GetAllAsync();
        var customerDtos = new List<CustomerDto>();

        foreach (var customer in customers)
        {
            var reservations = await _reservationRepository.GetByCustomerIdAsync(customer.Id);
            customerDtos.Add(MapToDto(customer, reservations.Count()));
        }

        return customerDtos;
    }

    public async Task<CustomerDto?> GetCustomerByIdAsync(int id)
    {
        _logger.LogInformation("Retrieving customer with ID: {CustomerId}", id);
        
        if (id <= 0)
        {
            _logger.LogWarning("Invalid customer ID provided: {CustomerId}", id);
            return null;
        }

        var customer = await _customerRepository.GetByIdAsync(id);
        if (customer == null)
        {
            return null;
        }

        var reservations = await _reservationRepository.GetByCustomerIdAsync(id);
        return MapToDto(customer, reservations.Count());
    }

    public async Task<CustomerDto?> GetCustomerByEmailAsync(string email)
    {
        _logger.LogInformation("Retrieving customer with email: {Email}", email);
        
        if (string.IsNullOrEmpty(email))
        {
            _logger.LogWarning("Empty email provided");
            return null;
        }

        var customer = await _customerRepository.GetByEmailAsync(email);
        if (customer == null)
        {
            return null;
        }

        var reservations = await _reservationRepository.GetByCustomerIdAsync(customer.Id);
        return MapToDto(customer, reservations.Count());
    }

    public async Task<CustomerDto?> GetCustomerByDocumentNumberAsync(string documentNumber)
    {
        _logger.LogInformation("Retrieving customer with document number: {DocumentNumber}", documentNumber);
        
        if (string.IsNullOrEmpty(documentNumber))
        {
            _logger.LogWarning("Empty document number provided");
            return null;
        }

        var customer = await _customerRepository.GetByDocumentNumberAsync(documentNumber);
        if (customer == null)
        {
            return null;
        }

        var reservations = await _reservationRepository.GetByCustomerIdAsync(customer.Id);
        return MapToDto(customer, reservations.Count());
    }

    public async Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto createDto)
    {
        _logger.LogInformation("Creating new customer: {Email}", createDto.Email);

        // Check if customer already exists
        var existingByEmail = await _customerRepository.ExistsByEmailAsync(createDto.Email);
        if (existingByEmail)
        {
            throw new InvalidOperationException($"Customer with email {createDto.Email} already exists");
        }

        var existingByDocument = await _customerRepository.ExistsByDocumentNumberAsync(createDto.DocumentNumber);
        if (existingByDocument)
        {
            throw new InvalidOperationException($"Customer with document number {createDto.DocumentNumber} already exists");
        }

        var customer = new Customer
        {
            Name = createDto.Name,
            Email = createDto.Email,
            Phone = createDto.Phone,
            DocumentNumber = createDto.DocumentNumber,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _customerRepository.AddAsync(customer);
        await _customerRepository.SaveChangesAsync();

        _logger.LogInformation("Successfully created customer with ID: {CustomerId}", customer.Id);
        return MapToDto(customer, 0);
    }

    public async Task<CustomerDto?> UpdateCustomerAsync(int id, UpdateCustomerDto updateDto)
    {
        _logger.LogInformation("Updating customer with ID: {CustomerId}", id);

        var customer = await _customerRepository.GetByIdAsync(id);
        if (customer == null)
        {
            _logger.LogWarning("Customer not found with ID: {CustomerId}", id);
            return null;
        }

        // Check for email conflicts if email is being updated
        if (!string.IsNullOrEmpty(updateDto.Email) && updateDto.Email != customer.Email)
        {
            var existingByEmail = await _customerRepository.ExistsByEmailAsync(updateDto.Email);
            if (existingByEmail)
            {
                throw new InvalidOperationException($"Customer with email {updateDto.Email} already exists");
            }
            customer.Email = updateDto.Email;
        }

        // Update other fields if provided
        if (!string.IsNullOrEmpty(updateDto.Name))
            customer.Name = updateDto.Name;

        if (!string.IsNullOrEmpty(updateDto.Phone))
            customer.Phone = updateDto.Phone;

        customer.UpdatedAt = DateTime.UtcNow;

        _customerRepository.Update(customer);
        await _customerRepository.SaveChangesAsync();

        var reservations = await _reservationRepository.GetByCustomerIdAsync(id);
        _logger.LogInformation("Successfully updated customer with ID: {CustomerId}", id);
        return MapToDto(customer, reservations.Count());
    }

    public async Task<bool> DeleteCustomerAsync(int id)
    {
        _logger.LogInformation("Deleting customer with ID: {CustomerId}", id);

        var customer = await _customerRepository.GetByIdAsync(id);
        if (customer == null)
        {
            _logger.LogWarning("Customer not found with ID: {CustomerId}", id);
            return false;
        }

        // Check if customer has active reservations
        var reservations = await _reservationRepository.GetByCustomerIdAsync(id);
        var hasActiveReservations = reservations.Any(r => r.IsActive());
        
        if (hasActiveReservations)
        {
            throw new InvalidOperationException("Cannot delete customer with active reservations");
        }

        _customerRepository.Remove(customer);
        await _customerRepository.SaveChangesAsync();

        _logger.LogInformation("Successfully deleted customer with ID: {CustomerId}", id);
        return true;
    }

    public async Task<IEnumerable<ReservationDto>> GetCustomerReservationsAsync(int customerId)
    {
        _logger.LogInformation("Retrieving reservations for customer ID: {CustomerId}", customerId);

        var customer = await _customerRepository.GetByIdAsync(customerId);
        if (customer == null)
        {
            _logger.LogWarning("Customer not found with ID: {CustomerId}", customerId);
            return Enumerable.Empty<ReservationDto>();
        }

        var reservations = await _reservationRepository.GetByCustomerIdAsync(customerId);
        return reservations.Select(MapReservationToDto);
    }

    private static CustomerDto MapToDto(Customer customer, int totalReservations)
    {
        return new CustomerDto
        {
            Id = customer.Id,
            Name = customer.Name,
            Email = customer.Email,
            Phone = customer.Phone,
            DocumentNumber = customer.DocumentNumber,
            CreatedAt = customer.CreatedAt,
            TotalReservations = totalReservations
        };
    }

    private static ReservationDto MapReservationToDto(Reservation reservation)
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
            } : null
        };
    }
}
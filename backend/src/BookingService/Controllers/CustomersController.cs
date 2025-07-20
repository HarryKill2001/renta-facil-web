using Microsoft.AspNetCore.Mvc;
using RentaFacil.Shared.DTOs;
using BookingService.Services;

namespace BookingService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerBusinessService _customerService;
    private readonly ILogger<CustomersController> _logger;

    public CustomersController(ICustomerBusinessService customerService, ILogger<CustomersController> logger)
    {
        _customerService = customerService;
        _logger = logger;
    }

    /// <summary>
    /// Get all customers
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<CustomerDto>>>> GetAllCustomers()
    {
        try
        {
            var customers = await _customerService.GetAllCustomersAsync();
            return Ok(ApiResponse<IEnumerable<CustomerDto>>.SuccessResult(customers, "Customers retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all customers");
            return StatusCode(500, ApiResponse<IEnumerable<CustomerDto>>.ErrorResult("Internal server error occurred"));
        }
    }

    /// <summary>
    /// Get customer by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<CustomerDto>>> GetCustomer(int id)
    {
        try
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer == null)
            {
                return NotFound(ApiResponse<CustomerDto>.ErrorResult("Customer not found"));
            }

            return Ok(ApiResponse<CustomerDto>.SuccessResult(customer, "Customer retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customer {CustomerId}", id);
            return StatusCode(500, ApiResponse<CustomerDto>.ErrorResult("Internal server error occurred"));
        }
    }

    /// <summary>
    /// Get customer by email
    /// </summary>
    [HttpGet("by-email/{email}")]
    public async Task<ActionResult<ApiResponse<CustomerDto>>> GetCustomerByEmail(string email)
    {
        try
        {
            var customer = await _customerService.GetCustomerByEmailAsync(email);
            if (customer == null)
            {
                return NotFound(ApiResponse<CustomerDto>.ErrorResult("Customer not found"));
            }

            return Ok(ApiResponse<CustomerDto>.SuccessResult(customer, "Customer retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customer by email {Email}", email);
            return StatusCode(500, ApiResponse<CustomerDto>.ErrorResult("Internal server error occurred"));
        }
    }

    /// <summary>
    /// Get customer by document number
    /// </summary>
    [HttpGet("by-document/{documentNumber}")]
    public async Task<ActionResult<ApiResponse<CustomerDto>>> GetCustomerByDocumentNumber(string documentNumber)
    {
        try
        {
            var customer = await _customerService.GetCustomerByDocumentNumberAsync(documentNumber);
            if (customer == null)
            {
                return NotFound(ApiResponse<CustomerDto>.ErrorResult("Customer not found"));
            }

            return Ok(ApiResponse<CustomerDto>.SuccessResult(customer, "Customer retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customer by document {DocumentNumber}", documentNumber);
            return StatusCode(500, ApiResponse<CustomerDto>.ErrorResult("Internal server error occurred"));
        }
    }

    /// <summary>
    /// Create a new customer
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<CustomerDto>>> CreateCustomer([FromBody] CreateCustomerDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<CustomerDto>.ErrorResult("Validation failed", errors));
            }

            var customer = await _customerService.CreateCustomerAsync(createDto);
            return CreatedAtAction(nameof(GetCustomer), new { id = customer.Id },
                ApiResponse<CustomerDto>.SuccessResult(customer, "Customer created successfully"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Customer creation failed: {Message}", ex.Message);
            return Conflict(ApiResponse<CustomerDto>.ErrorResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating customer");
            return StatusCode(500, ApiResponse<CustomerDto>.ErrorResult("Internal server error occurred"));
        }
    }

    /// <summary>
    /// Update an existing customer
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<CustomerDto>>> UpdateCustomer(int id, [FromBody] UpdateCustomerDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<CustomerDto>.ErrorResult("Validation failed", errors));
            }

            var customer = await _customerService.UpdateCustomerAsync(id, updateDto);
            if (customer == null)
            {
                return NotFound(ApiResponse<CustomerDto>.ErrorResult("Customer not found"));
            }

            return Ok(ApiResponse<CustomerDto>.SuccessResult(customer, "Customer updated successfully"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Customer update failed: {Message}", ex.Message);
            return Conflict(ApiResponse<CustomerDto>.ErrorResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating customer {CustomerId}", id);
            return StatusCode(500, ApiResponse<CustomerDto>.ErrorResult("Internal server error occurred"));
        }
    }

    /// <summary>
    /// Delete a customer
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse>> DeleteCustomer(int id)
    {
        try
        {
            var success = await _customerService.DeleteCustomerAsync(id);
            if (!success)
            {
                return NotFound(ApiResponse.ErrorResult("Customer not found"));
            }

            return Ok(ApiResponse.SuccessResult("Customer deleted successfully"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Customer deletion failed: {Message}", ex.Message);
            return Conflict(ApiResponse.ErrorResult(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting customer {CustomerId}", id);
            return StatusCode(500, ApiResponse.ErrorResult("Internal server error occurred"));
        }
    }

    /// <summary>
    /// Get customer's reservation history
    /// </summary>
    [HttpGet("{id}/history")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ReservationDto>>>> GetCustomerHistory(int id)
    {
        try
        {
            var reservations = await _customerService.GetCustomerReservationsAsync(id);
            if (!reservations.Any())
            {
                // Check if customer exists
                var customer = await _customerService.GetCustomerByIdAsync(id);
                if (customer == null)
                {
                    return NotFound(ApiResponse<IEnumerable<ReservationDto>>.ErrorResult("Customer not found"));
                }
            }

            return Ok(ApiResponse<IEnumerable<ReservationDto>>.SuccessResult(reservations, "Customer history retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customer history for {CustomerId}", id);
            return StatusCode(500, ApiResponse<IEnumerable<ReservationDto>>.ErrorResult("Internal server error occurred"));
        }
    }
}
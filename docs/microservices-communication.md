# Microservices Communication Patterns - RentaFácil

## Overview
This document describes the communication patterns implemented in the RentaFácil microservices architecture, including the specific solutions implemented to resolve cross-service dependencies and data consistency challenges.

## Communication Architecture

```
┌─────────────────┐    HTTP/REST     ┌─────────────────┐
│   Frontend      │──────────────────▶│  VehicleService │
│  Angular 17+    │                   │   Port: 5002    │
│  Port: 4200     │                   └─────────────────┘
└─────────────────┘                            ▲
         │                                     │
         │ HTTP/REST                           │ HTTP/REST
         ▼                                     │ (Validation)
┌─────────────────┐                           │
│  BookingService │───────────────────────────┘
│   Port: 5257    │    Vehicle Validation
└─────────────────┘    GET /vehicles/{id}
         │
         │ Database Access
         ▼
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│  BookingService │    │  VehicleService │    │ ReportingService│
│    Database     │    │    Database     │    │    Database     │
└─────────────────┘    └─────────────────┘    └─────────────────┘
                                                        ▲
                                                        │
                                               ┌─────────────────┐
                                               │  WorkerService  │
                                               │   Port: 5000    │
                                               │  (Background)   │
                                               └─────────────────┘
```

## Communication Patterns Implemented

### 1. Frontend to Backend Communication

**Pattern**: Direct HTTP REST API calls
**Implementation**: Centralized `ApiService` in Angular

```typescript
// Frontend ApiService
export class ApiService {
  private readonly vehicleServiceUrl = 'http://localhost:5002/api';
  private readonly bookingServiceUrl = 'http://localhost:5257/api';

  // Direct calls to VehicleService
  getVehicles(): Observable<any[]> {
    return this.http.get<ApiResponse>(`${this.vehicleServiceUrl}/vehicles`);
  }

  // Direct calls to BookingService
  createReservation(reservation: any): Observable<any> {
    return this.http.post<ApiResponse>(`${this.bookingServiceUrl}/reservations`, reservation);
  }
}
```

**Benefits**:
- Simple and direct communication
- No service discovery complexity
- Easy to debug and trace

**Challenges Addressed**:
- ✅ Port configuration mismatches fixed
- ✅ CORS configuration properly set up
- ✅ Error handling and response mapping implemented

### 2. Service-to-Service HTTP Communication

**Pattern**: HTTP Client with Factory Pattern
**Use Cases**: 
- BookingService validates vehicles with VehicleService
- BookingService fetches complete vehicle details for reservation responses

```csharp
// BookingService Program.cs
builder.Services.AddHttpClient("VehicleService", client =>
{
    var baseUrl = builder.Configuration["Services:VehicleService:BaseUrl"];
    client.BaseAddress = new Uri(baseUrl ?? "http://localhost:5002");
});

// ReservationBusinessService.cs
public class ReservationBusinessService
{
    private readonly IHttpClientFactory _httpClientFactory;
    
    // Vehicle existence validation during reservation creation
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

    // Vehicle details fetching for reservation responses
    private async Task<VehicleDto?> GetVehicleDetailsAsync(int vehicleId)
    {
        try
        {
            using var httpClient = _httpClientFactory.CreateClient("VehicleService");
            var response = await httpClient.GetAsync($"/api/vehicles/{vehicleId}");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<VehicleDto>>(content, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                
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

    // Async DTO mapping with vehicle details population
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
                TotalReservations = 0
            } : null
        };

        // Fetch vehicle details from VehicleService
        dto.Vehicle = await GetVehicleDetailsAsync(reservation.VehicleId);
        
        return dto;
    }
}
```

**Configuration**:
```json
{
  "Services": {
    "VehicleService": {
      "BaseUrl": "http://localhost:5002"
    }
  }
}
```

**Benefits**:
- Proper separation of concerns
- Resilient error handling
- Configurable service endpoints
- HTTP client lifecycle management

### 3. Database Access Patterns

#### 3.1 Database Per Service Pattern
Each service maintains its own database:

```csharp
// VehicleService
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost,1433;Database=RentaFacil_VehicleService;..."
}

// BookingService  
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost,1433;Database=RentaFacil_BookingService;..."
}

// WorkerService
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost,1433;Database=RentaFacil_ReportingService;..."
}
```

#### 3.2 Foreign Key Constraint Removal
**Problem**: Cross-service foreign key constraints violated microservices principles

**Solution**: Removed database-level constraints and implemented application-level validation

```csharp
// BookingService Program.cs - Automatic constraint removal
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<BookingDbContext>();
    context.Database.EnsureCreated();
    
    try
    {
        context.Database.ExecuteSqlRaw("ALTER TABLE Reservations DROP CONSTRAINT FK_Reservations_Vehicles_VehicleId");
        app.Logger.LogInformation("Successfully removed FK_Reservations_Vehicles_VehicleId constraint");
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "Failed to remove FK constraint (may not exist): {Message}", ex.Message);
    }
}
```

### 4. Background Service Communication

**Pattern**: DbContext Factory for Thread-Safe Operations
**Use Case**: WorkerService generates reports across multiple databases

```csharp
// WorkerService Program.cs
builder.Services.AddDbContextFactory<ReportingDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ReportService.cs
public class ReportService : IReportService
{
    private readonly IDbContextFactory<ReportingDbContext> _contextFactory;

    public async Task<DailyReservationSummary> GenerateDailyReservationSummaryAsync(DateTime date)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        // Thread-safe database operations
        
        var reservations = await context.Reservations
            .Where(r => r.CreatedAt >= startOfDay && r.CreatedAt < endOfDay)
            .ToListAsync();
            
        return summary;
    }
}
```

**Parallel Report Generation**:
```csharp
public async Task GenerateAllDailyReportsAsync(DateTime date)
{
    try
    {
        // Generate all reports in parallel for better performance
        var summaryTask = GenerateDailyReservationSummaryAsync(date);
        var metricsTask = GenerateCustomerMetricsAsync(date);
        var utilizationTask = GenerateVehicleUtilizationReportAsync(date);

        await Task.WhenAll(summaryTask, metricsTask, utilizationTask);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error generating daily reports for {Date}", date.ToString("yyyy-MM-dd"));
        throw;
    }
}
```

## Data Consistency Strategies

### 1. Eventual Consistency
**Approach**: HTTP validation calls ensure data consistency across services

**Example Flow**:
1. Frontend requests reservation creation
2. BookingService validates vehicle exists via HTTP call to VehicleService
3. If valid, reservation is created with VehicleId reference
4. No database-level foreign key constraint enforced

### 2. Compensating Actions
**Strategy**: Handle validation failures gracefully

```csharp
public async Task<ReservationDto> CreateReservationAsync(CreateReservationDto createDto)
{
    // Validate vehicle exists by calling VehicleService
    if (!await ValidateVehicleExistsAsync(createDto.VehicleId))
    {
        throw new ArgumentException($"Vehicle with ID {createDto.VehicleId} does not exist", 
            nameof(createDto.VehicleId));
    }
    
    // Proceed with reservation creation...
}
```

### 3. Error Handling and Resilience

**HTTP Client Error Handling**:
```csharp
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
        // Fail closed - reject reservation if validation fails
        return false;
    }
}
```

**Frontend Error Handling**:
```typescript
private handleError = (error: any): Observable<never> => {
  console.error('API Error:', error);
  console.error('Error status:', error.status);
  console.error('Error Body:', error.error);
  console.error('Full error object JSON:', JSON.stringify(error.error, null, 2));
  
  return throwError(() => error);
};
```

## Service Discovery and Configuration

### 1. Static Configuration
**Approach**: Hard-coded service URLs for development simplicity

```typescript
// Frontend
private readonly vehicleServiceUrl = 'http://localhost:5002/api';
private readonly bookingServiceUrl = 'http://localhost:5257/api';
```

```json
// Backend appsettings.json
{
  "Services": {
    "VehicleService": {
      "BaseUrl": "http://localhost:5002"
    }
  }
}
```

### 2. CORS Configuration
**Implementation**: Allow frontend access to backend services

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
```

## Security Considerations

### 1. Service Authentication
**Current State**: No authentication implemented (MVP)
**Future**: JWT tokens, service-to-service authentication

### 2. Input Validation
**Implementation**: Validation at multiple layers

```csharp
// DTO Validation
public class CreateReservationDto
{
    [Required]
    public int VehicleId { get; set; }
    
    [Required]
    public DateTime StartDate { get; set; }
    
    [Required]
    public DateTime EndDate { get; set; }
}

// Business Logic Validation
if (createDto.StartDate >= createDto.EndDate)
{
    throw new ArgumentException("End date must be after start date");
}
```

### 3. Error Information Disclosure
**Strategy**: Log detailed errors internally, return generic errors externally

```csharp
try
{
    // Business logic
}
catch (Exception ex)
{
    _logger.LogError(ex, "Internal error details");
    throw new InvalidOperationException("A reservation error occurred");
}
```

## Performance Optimizations

### 1. HTTP Client Pooling
**Implementation**: HttpClientFactory manages connection pooling automatically

### 2. Async/Await Throughout
**Pattern**: All I/O operations use async/await for better scalability

### 3. Database Query Optimization
```csharp
// Efficient queries with proper indexing
var reservations = await context.Reservations
    .Where(r => r.CreatedAt >= startOfDay && r.CreatedAt < endOfDay)
    .ToListAsync();
```

## Monitoring and Observability

### 1. Structured Logging
```csharp
_logger.LogInformation("Creating reservation for vehicle {VehicleId} from {StartDate} to {EndDate}",
    createDto.VehicleId, createDto.StartDate, createDto.EndDate);
```

### 2. HTTP Request Logging
```csharp
_logger.LogInformation("Start processing HTTP request GET {Url}", requestUrl);
_logger.LogInformation("End processing HTTP request after {Duration}ms - {StatusCode}", duration, statusCode);
```

### 3. Error Tracking
- Service-level error logging
- Cross-service call failure tracking
- Database operation error monitoring

## Future Enhancements

### 1. Service Discovery
- Consul or Eureka integration
- Dynamic service registration
- Health checks and load balancing

### 2. Circuit Breaker Pattern
- Resilience4j or Polly integration
- Fail-fast behavior for dependent services
- Graceful degradation

### 3. Event-Driven Architecture
- Message queues (RabbitMQ/Service Bus)
- Event sourcing for audit trails
- Eventual consistency through events

### 4. API Gateway
- Single entry point for frontend
- Request routing and aggregation
- Cross-cutting concerns (auth, rate limiting)

This communication architecture provides a solid foundation for the RentaFácil microservices while addressing real-world challenges of distributed systems.
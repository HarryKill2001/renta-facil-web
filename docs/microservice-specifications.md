# Microservice Specifications - RentaFácil MVP

## Overview
This document details the specifications for each microservice in the RentaFácil system, including their responsibilities, APIs, data models, and implementation details.

---

## VehicleService ✅ FULLY IMPLEMENTED

### Purpose
Manages vehicle inventory, registration, and availability checking for the rental system.

### Port & Base URL
- **Development**: http://localhost:5001
- **Base API Path**: `/api/vehicles`

### Core Responsibilities
- Vehicle registration and management
- Availability checking by date range and vehicle type
- Vehicle inventory tracking
- Basic vehicle reporting

### API Endpoints

#### 1. Get All Vehicles
```http
GET /api/vehicles
```

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "type": "SUV",
      "model": "Toyota RAV4",
      "year": 2023,
      "available": true,
      "pricePerDay": 85.00,
      "createdAt": "2024-01-15T10:30:00Z"
    }
  ],
  "message": "Vehicles retrieved successfully"
}
```

#### 2. Get Vehicle by ID
```http
GET /api/vehicles/{id}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "type": "SUV",
    "model": "Toyota RAV4",
    "year": 2023,
    "available": true,
    "pricePerDay": 85.00,
    "createdAt": "2024-01-15T10:30:00Z"
  }
}
```

#### 3. Register New Vehicle
```http
POST /api/vehicles
```

**Request Body:**
```json
{
  "type": "SUV",
  "model": "Honda CR-V",
  "year": 2023,
  "pricePerDay": 90.00
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "id": 2,
    "type": "SUV",
    "model": "Honda CR-V",
    "year": 2023,
    "available": true,
    "pricePerDay": 90.00,
    "createdAt": "2024-01-15T11:00:00Z"
  },
  "message": "Vehicle registered successfully"
}
```

#### 4. Check Vehicle Availability
```http
GET /api/vehicles/availability?startDate=2024-01-20&endDate=2024-01-25&type=SUV
```

**Query Parameters:**
- `startDate` (required): Start date for availability check (YYYY-MM-DD)
- `endDate` (required): End date for availability check (YYYY-MM-DD)
- `type` (optional): Filter by vehicle type

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "type": "SUV",
      "model": "Toyota RAV4",
      "year": 2023,
      "pricePerDay": 85.00,
      "totalPrice": 425.00
    }
  ],
  "message": "Available vehicles found"
}
```

### Data Access Layer (MVP)

#### Entity Framework Setup
```csharp
// Infrastructure/Data/ApplicationDbContext.cs
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
    
    public DbSet<Vehicle> Vehicles { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Type).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Model).HasMaxLength(100).IsRequired();
            entity.Property(e => e.PricePerDay).HasColumnType("decimal(10,2)");
        });
    }
}
```

#### Repository Implementation (Simple)
```csharp
// Infrastructure/Repositories/Repository.cs
public class Repository<T> : IRepository<T> where T : class
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<T> _dbSet;

    public Repository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T> GetByIdAsync(int id) => await _dbSet.FindAsync(id);
    public async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();
    public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);
    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
}
```

#### Service Implementation (MVP)
```csharp
// Application/Services/VehicleService.cs
public class VehicleService
{
    private readonly IRepository<Vehicle> _vehicleRepository;

    public VehicleService(IRepository<Vehicle> vehicleRepository)
    {
        _vehicleRepository = vehicleRepository;
    }

    public async Task<VehicleDto> GetVehicleAsync(int id)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(id);
        if (vehicle == null) throw new NotFoundException("Vehicle not found");
        
        return new VehicleDto
        {
            Id = vehicle.Id,
            Type = vehicle.Type,
            Model = vehicle.Model,
            Year = vehicle.Year,
            PricePerDay = vehicle.PricePerDay,
            Available = vehicle.Available
        };
    }

    public async Task<IEnumerable<VehicleDto>> GetAvailableVehiclesAsync(
        DateTime startDate, DateTime endDate, string vehicleType = null)
    {
        // Simple availability logic for MVP
        var allVehicles = await _vehicleRepository.GetAllAsync();
        var availableVehicles = allVehicles.Where(v => v.Available);
        
        if (!string.IsNullOrEmpty(vehicleType))
            availableVehicles = availableVehicles.Where(v => v.Type == vehicleType);
            
        return availableVehicles.Select(v => new VehicleDto
        {
            Id = v.Id,
            Type = v.Type,
            Model = v.Model,
            Year = v.Year,
            PricePerDay = v.PricePerDay,
            Available = v.Available
        });
    }
}
```

### Data Models

#### Vehicle Entity
```csharp
public class Vehicle
{
    public int Id { get; set; }
    public string Type { get; set; }           // SUV, Sedan, Compact, etc.
    public string Model { get; set; }          // Toyota RAV4, Honda Civic
    public int Year { get; set; }
    public decimal PricePerDay { get; set; }
    public bool Available { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation property
    public ICollection<Reservation> Reservations { get; set; }
}
```

#### DTOs
```csharp
public class VehicleDto
{
    public int Id { get; set; }
    public string Type { get; set; }
    public string Model { get; set; }
    public int Year { get; set; }
    public decimal PricePerDay { get; set; }
    public bool Available { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateVehicleDto
{
    [Required]
    [StringLength(50)]
    public string Type { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Model { get; set; }
    
    [Range(2000, 2030)]
    public int Year { get; set; }
    
    [Range(0.01, 1000.00)]
    public decimal PricePerDay { get; set; }
}

public class AvailabilityRequestDto
{
    [Required]
    public DateTime StartDate { get; set; }
    
    [Required]
    public DateTime EndDate { get; set; }
    
    public string Type { get; set; }
}
```

### Business Logic

#### Availability Checking
```csharp
public async Task<IEnumerable<VehicleDto>> GetAvailableVehiclesAsync(
    DateTime startDate, DateTime endDate, string vehicleType = null)
{
    // Validate date range
    if (startDate >= endDate || startDate < DateTime.Today)
        throw new BusinessException("Invalid date range");

    // Get vehicles not reserved during the specified period
    var availableVehicles = await _repository.GetAvailableAsync(
        startDate, endDate, vehicleType);
    
    return _mapper.Map<IEnumerable<VehicleDto>>(availableVehicles);
}
```

### Error Handling
- **400 Bad Request**: Invalid input data
- **404 Not Found**: Vehicle not found
- **409 Conflict**: Vehicle already exists
- **500 Internal Server Error**: System errors

---

## BookingService ✅ FULLY IMPLEMENTED

### Purpose
Handles customer management, reservation creation, and booking-related operations with advanced features including auto-customer creation and conflict detection.

### Port & Base URL
- **Development**: http://localhost:5002
- **Base API Path**: `/api`

### Core Responsibilities
- Customer registration and management with duplicate detection
- Reservation creation and validation with conflict checking
- Reservation history and reporting with pagination
- Customer-reservation relationship management
- Advanced filtering and search capabilities

### API Endpoints

#### Customer Management

##### 1. Register Customer
```http
POST /api/customers
```

**Request Body:**
```json
{
  "name": "Juan Pérez",
  "email": "juan.perez@email.com",
  "phone": "+57 300 123 4567",
  "documentNumber": "12345678"
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "name": "Juan Pérez",
    "email": "juan.perez@email.com",
    "phone": "+57 300 123 4567",
    "documentNumber": "12345678",
    "createdAt": "2024-01-15T10:30:00Z"
  }
}
```

##### 2. Get Customer by ID
```http
GET /api/customers/{id}
```

##### 3. Find Customer by Email
```http
GET /api/customers/by-email/{email}
```

##### 4. Find Customer by Document Number
```http
GET /api/customers/by-document/{documentNumber}
```

##### 5. Update Customer
```http
PUT /api/customers/{id}
```

##### 6. Delete Customer
```http
DELETE /api/customers/{id}
```

##### 7. Get Customer History
```http
GET /api/customers/{id}/history
```

**Response:**
```json
{
  "success": true,
  "data": {
    "customer": {
      "id": 1,
      "name": "Juan Pérez",
      "email": "juan.perez@email.com"
    },
    "reservations": [
      {
        "id": 1,
        "vehicleModel": "Toyota RAV4",
        "startDate": "2024-01-20",
        "endDate": "2024-01-25",
        "totalPrice": 425.00,
        "status": "Confirmed"
      }
    ]
  }
}
```

#### Reservation Management

##### 1. Create Reservation with Auto-Customer Creation
```http
POST /api/reservations
```

**Request Body:**
```json
{
  "customerId": null,
  "vehicleId": 1,
  "startDate": "2024-01-20",
  "endDate": "2024-01-25",
  "customerInfo": {
    "name": "Juan Pérez",
    "email": "juan.perez@email.com",
    "phone": "+57 300 123 4567",
    "documentNumber": "12345678"
  }
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "customerId": 1,
    "vehicleId": 1,
    "startDate": "2024-01-20T00:00:00Z",
    "endDate": "2024-01-25T00:00:00Z",
    "totalPrice": 425.00,
    "status": "Confirmed",
    "confirmationNumber": "RF-20240115-001",
    "createdAt": "2024-01-15T10:30:00Z"
  }
}
```

##### 2. Get All Reservations with Advanced Filtering
```http
GET /api/reservations?page=1&pageSize=10&status=Confirmed&startDate=2024-01-20&endDate=2024-01-25&vehicleType=SUV&customerId=1
```

**Query Parameters:**
- `page` (optional): Page number for pagination
- `pageSize` (optional): Number of items per page
- `status` (optional): Filter by reservation status
- `startDate` (optional): Filter by start date
- `endDate` (optional): Filter by end date
- `vehicleType` (optional): Filter by vehicle type
- `customerId` (optional): Filter by customer ID

**Response:**
```json
{
  "success": true,
  "data": {
    "reservations": [
      {
        "id": 1,
        "customer": "Juan Pérez",
        "vehicleModel": "Toyota RAV4",
        "startDate": "2024-01-20",
        "endDate": "2024-01-25",
        "totalPrice": 425.00,
        "status": "Confirmed"
      }
    ],
    "pagination": {
      "currentPage": 1,
      "pageSize": 10,
      "totalCount": 25,
      "totalPages": 3
    }
  }
}
```

##### 3. Get Reservation by ID
```http
GET /api/reservations/{id}
```

##### 4. Find Reservation by Confirmation Number
```http
GET /api/reservations/by-confirmation/{confirmationNumber}
```

##### 5. Cancel Reservation
```http
POST /api/reservations/{id}/cancel
```

##### 6. Confirm Reservation
```http
POST /api/reservations/{id}/confirm
```

##### 7. Check Vehicle Availability
```http
GET /api/reservations/check-availability?vehicleId=1&startDate=2024-01-20&endDate=2024-01-25
```

**Response:**
```json
{
  "success": true,
  "data": {
    "available": true,
    "vehicleId": 1,
    "startDate": "2024-01-20",
    "endDate": "2024-01-25"
  }
}
```

### Data Models

#### Customer Entity
```csharp
public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string DocumentNumber { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation property
    public ICollection<Reservation> Reservations { get; set; }
}
```

#### Reservation Entity
```csharp
public class Reservation
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public int VehicleId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalPrice { get; set; }
    public string Status { get; set; }           // Confirmed, Cancelled, Completed
    public string ConfirmationNumber { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public Customer Customer { get; set; }
    public Vehicle Vehicle { get; set; }          // Reference to VehicleService
}
```

#### DTOs
```csharp
public class CreateReservationDto
{
    public int? CustomerId { get; set; }
    public int VehicleId { get; set; }
    
    [Required]
    public DateTime StartDate { get; set; }
    
    [Required]
    public DateTime EndDate { get; set; }
    
    public CustomerInfoDto CustomerInfo { get; set; }
}

public class CustomerInfoDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; }
    
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    
    [Required]
    [Phone]
    public string Phone { get; set; }
    
    [Required]
    public string DocumentNumber { get; set; }
}
```

### Business Logic

#### Reservation Creation
```csharp
public async Task<ReservationDto> CreateReservationAsync(CreateReservationDto dto)
{
    // 1. Validate dates
    if (dto.StartDate >= dto.EndDate || dto.StartDate < DateTime.Today)
        throw new BusinessException("Invalid reservation dates");

    // 2. Check vehicle availability
    var isAvailable = await _vehicleService.CheckAvailabilityAsync(
        dto.VehicleId, dto.StartDate, dto.EndDate);
    
    if (!isAvailable)
        throw new BusinessException("Vehicle not available for selected dates");

    // 3. Handle customer (existing or new)
    var customer = dto.CustomerId.HasValue 
        ? await _customerRepository.GetByIdAsync(dto.CustomerId.Value)
        : await CreateOrUpdateCustomerAsync(dto.CustomerInfo);

    // 4. Calculate total price
    var vehicle = await _vehicleService.GetVehicleAsync(dto.VehicleId);
    var days = (dto.EndDate - dto.StartDate).Days;
    var totalPrice = vehicle.PricePerDay * days;

    // 5. Create reservation
    var reservation = new Reservation
    {
        CustomerId = customer.Id,
        VehicleId = dto.VehicleId,
        StartDate = dto.StartDate,
        EndDate = dto.EndDate,
        TotalPrice = totalPrice,
        Status = "Confirmed",
        ConfirmationNumber = GenerateConfirmationNumber(),
        CreatedAt = DateTime.UtcNow
    };

    await _reservationRepository.AddAsync(reservation);
    return _mapper.Map<ReservationDto>(reservation);
}
```

### Service Integration
The BookingService communicates with VehicleService for:
- Vehicle availability checking
- Vehicle details retrieval
- Price calculation

---

## WorkerService ✅ FULLY IMPLEMENTED

### Purpose
Background processing service for scheduled tasks, comprehensive analytics, and business intelligence reporting with automated daily execution.

### Core Responsibilities
- Daily reservation summaries with detailed metrics
- Vehicle utilization reporting and analytics
- Monthly revenue reports with vehicle type breakdown
- Customer metrics and behavioral analysis
- Automated report generation with intelligent scheduling

### Architecture
```csharp
public class Worker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<Worker> _logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;
            var scheduledTime = DateTime.Today.AddDays(1).AddHours(2); // 2 AM UTC next day
            
            var delay = scheduledTime - now;
            if (delay <= TimeSpan.Zero)
            {
                delay = TimeSpan.FromDays(1); // If past 2 AM, wait until tomorrow
            }

            await Task.Delay(delay, stoppingToken);
            await ProcessDailyReportsAsync();
            
            // Wait 23 hours to avoid multiple executions in same day
            await Task.Delay(TimeSpan.FromHours(23), stoppingToken);
        }
    }
}
```

### Report Processing Features

#### 1. Daily Reservation Summary
```csharp
public async Task<DailyReservationSummary> GenerateDailyReservationSummaryAsync(DateTime date)
{
    var summary = new DailyReservationSummary
    {
        ReportDate = date,
        TotalReservations = reservations.Count,
        ConfirmedReservations = reservations.Count(r => r.Status == ReservationStatus.Confirmed),
        CancelledReservations = reservations.Count(r => r.Status == ReservationStatus.Cancelled),
        PendingReservations = reservations.Count(r => r.Status == ReservationStatus.Pending),
        TotalRevenue = reservations.Where(r => r.Status == ReservationStatus.Confirmed).Sum(r => r.TotalPrice),
        NewCustomers = newCustomers,
        AverageReservationValue = summary.ConfirmedReservations > 0 
            ? summary.TotalRevenue / summary.ConfirmedReservations : 0
    };
}
```

#### 2. Vehicle Utilization Analytics
```csharp
public async Task<VehicleUtilizationReport[]> GenerateVehicleUtilizationReportAsync(DateTime date)
{
    foreach (var vehicle in vehicles)
    {
        // Complex utilization calculation with overlapping reservation detection
        var reservations = await _context.Reservations
            .Where(r => r.VehicleId == vehicle.Id && 
                       r.Status == ReservationStatus.Confirmed &&
                       ((r.StartDate <= startOfDay && r.EndDate > startOfDay) ||
                        (r.StartDate < endOfDay && r.EndDate >= endOfDay) ||
                        (r.StartDate >= startOfDay && r.EndDate <= endOfDay)))
            .ToListAsync();

        var daysBooked = reservations.Sum(r => 
        {
            var start = r.StartDate > startOfDay ? r.StartDate : startOfDay;
            var end = r.EndDate < endOfDay ? r.EndDate : endOfDay;
            return Math.Max(0, (end - start).Days);
        });

        var utilizationPercentage = daysBooked > 0 ? (decimal)daysBooked / 1 * 100 : 0;
    }
}
```

#### 3. Monthly Revenue Analytics
```csharp
public async Task<MonthlyRevenueSummary> GenerateMonthlyRevenueSummaryAsync(int year, int month)
{
    var reservations = await _context.Reservations
        .Include(r => r.Vehicle)
        .Where(r => r.Status == ReservationStatus.Confirmed &&
                   r.CreatedAt >= startOfMonth && r.CreatedAt < endOfMonth)
        .ToListAsync();

    var revenueByType = reservations
        .GroupBy(r => r.Vehicle.Type)
        .ToDictionary(g => g.Key, g => g.Sum(r => r.TotalPrice));

    var reservationsByType = reservations
        .GroupBy(r => r.Vehicle.Type)
        .ToDictionary(g => g.Key, g => g.Count());
}
```

#### 4. Customer Metrics & Analytics
```csharp
public async Task<CustomerMetrics> GenerateCustomerMetricsAsync(DateTime date)
{
    var metrics = new CustomerMetrics
    {
        ReportDate = date,
        TotalCustomers = totalCustomers,
        NewCustomersToday = newCustomersToday,
        ActiveCustomers = activeCustomers, // Customers with reservations in last 30 days
        ReturningCustomers = returningCustomers, // Customers with > 1 reservation
        AverageReservationsPerCustomer = totalCustomers > 0 
            ? (decimal)totalReservations / totalCustomers : 0
    };
}
```

### Data Models

#### Report Entities
```csharp
public class DailySummaryReport
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public int TotalReservations { get; set; }
    public decimal TotalRevenue { get; set; }
    public double AverageUtilization { get; set; }
    public string TopVehicleTypes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class VehicleUtilization
{
    public int VehicleId { get; set; }
    public string Model { get; set; }
    public double UtilizationPercentage { get; set; }
}
```

### Logging and Monitoring
```csharp
private async Task ProcessDailyReportsAsync()
{
    _logger.LogInformation("Starting daily report processing at {Time}", DateTime.UtcNow);
    
    try
    {
        var yesterday = DateTime.Today.AddDays(-1);
        
        // Generate reports
        var summary = await _reportService.GenerateDailySummaryAsync(yesterday);
        var utilization = await _reportService.GenerateUtilizationReportAsync(yesterday);
        
        _logger.LogInformation("Daily reports completed successfully for {Date}", yesterday);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to process daily reports");
    }
}
```

### Parallel Report Processing
```csharp
public async Task GenerateAllDailyReportsAsync(DateTime date)
{
    _logger.LogInformation("Starting generation of all daily reports for {Date}", date.ToString("yyyy-MM-dd"));

    try
    {
        // Generate all reports in parallel for better performance
        var summaryTask = GenerateDailyReservationSummaryAsync(date);
        var metricsTask = GenerateCustomerMetricsAsync(date);
        var utilizationTask = GenerateVehicleUtilizationReportAsync(date);

        await Task.WhenAll(summaryTask, metricsTask, utilizationTask);

        // If it's the first day of the month, generate monthly report for previous month
        if (date.Day == 1 && date.Month > 1)
        {
            var previousMonth = date.AddMonths(-1);
            await GenerateMonthlyRevenueSummaryAsync(previousMonth.Year, previousMonth.Month);
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error generating daily reports for {Date}", date.ToString("yyyy-MM-dd"));
        throw;
    }
}
```

### Configuration & Database Setup
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=RentaFacil_BookingService;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=true;"
  }
}
```

### Implementation Features
- **Intelligent Scheduling**: Runs daily at 2 AM UTC with automatic delay calculation
- **Parallel Processing**: All daily reports generated simultaneously for optimal performance
- **Monthly Automation**: Automatic monthly report generation on first day of each month
- **Database Connectivity**: Read-only access to BookingService database for analytics
- **Comprehensive Logging**: Detailed logging for monitoring and debugging
- **Error Handling**: Robust error handling with detailed exception logging

---

## Cross-Service Communication

### Service Discovery (Local Development)
Services communicate using hardcoded URLs for MVP simplicity:

```csharp
public class VehicleServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl = "http://localhost:5001";

    public async Task<bool> CheckAvailabilityAsync(int vehicleId, DateTime start, DateTime end)
    {
        var response = await _httpClient.GetAsync(
            $"{_baseUrl}/api/vehicles/availability?vehicleId={vehicleId}&startDate={start:yyyy-MM-dd}&endDate={end:yyyy-MM-dd}");
        
        // Handle response
    }
}
```

### Error Handling Strategy
All services implement consistent error handling:

```csharp
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public T Data { get; set; }
    public List<string> Errors { get; set; } = new();
}
```

### Health Checks
Each service exposes health check endpoints:

```csharp
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
```

---

This specification provides the foundation for implementing the three core microservices of the RentaFácil MVP system, ensuring consistency and maintainability across all components.
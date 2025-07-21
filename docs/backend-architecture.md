# Backend Architecture - RentaFácil .NET Microservices

## Overview
The RentaFácil backend is built as a microservices architecture using .NET 8, following Clean Architecture principles and Domain-Driven Design patterns. Each service is independently deployable and maintains its own database.

## Microservices Architecture

### Service Communication Patterns
```
┌─────────────────┐    HTTP GET     ┌─────────────────┐
│  BookingService │───────────────→│  VehicleService │
│   Port: 5257    │    /vehicles/1  │   Port: 5002    │
└─────────────────┘                 └─────────────────┘
         │                                   │
         │                                   │
    ┌─────────┴─────────┐              ┌─────┴──────┐
    │ BookingService DB │              │ Vehicle DB │
    └───────────────────┘              └────────────┘
                  │
                  │
         ┌────────┴────────┐
         │  WorkerService  │
         │   Port: 5000    │
         │ (Background)    │
         └─────────────────┘
                  │
         ┌────────┴────────┐
         │ Reporting DB    │
         └─────────────────┘
```

## Service Details

### 1. VehicleService (Port 5002)

**Purpose**: Manages vehicle inventory, availability checking, and vehicle-related operations.

**Key Features**:
- Vehicle registration and management
- Availability checking by date range
- Vehicle catalog with seed data
- RESTful API with Swagger documentation

**Project Structure**:
```
VehicleService/
├── Controllers/
│   └── VehiclesController.cs          # API endpoints
├── Services/
│   ├── IVehicleBusinessService.cs     # Business logic interface
│   └── VehicleBusinessService.cs      # Business logic implementation
├── Repositories/
│   ├── IVehicleRepository.cs          # Data access interface
│   └── VehicleRepository.cs           # Data access implementation
├── Data/
│   └── VehicleDbContext.cs            # EF Core context with seed data
├── Properties/
│   └── launchSettings.json            # Port configuration (5002)
└── Program.cs                         # Service configuration
```

**Database Schema**:
```sql
Vehicles (
    Id INT PRIMARY KEY IDENTITY,
    Type NVARCHAR(50) NOT NULL,
    Model NVARCHAR(100) NOT NULL,
    Year INT NOT NULL,
    PricePerDay DECIMAL(18,2) NOT NULL,
    Available BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2 NOT NULL
)
```

**Seed Data**:
- Toyota Camry 2023 (Sedan, $45.00/day)
- Honda CR-V 2023 (SUV, $65.00/day)
- Nissan Versa 2023 (Compact, $35.00/day)
- Ford Explorer 2023 (SUV, $75.00/day)
- Chevrolet Malibu 2023 (Sedan, $40.00/day)

### 2. BookingService (Port 5257)

**Purpose**: Handles reservations, customer management, and booking-related operations with cross-service validation.

**Key Features**:
- Reservation creation with vehicle validation
- Customer management
- Reservation history and tracking
- HTTP client integration for vehicle validation

**Project Structure**:
```
BookingService/
├── Controllers/
│   ├── ReservationsController.cs      # Reservation endpoints
│   └── CustomersController.cs         # Customer endpoints
├── Services/
│   ├── IReservationBusinessService.cs # Reservation business logic
│   ├── ReservationBusinessService.cs  # With HTTP vehicle validation
│   └── CustomerBusinessService.cs     # Customer operations
├── Repositories/
│   ├── ReservationRepository.cs       # No Vehicle Include operations
│   └── CustomerRepository.cs          # Customer data access
├── Data/
│   └── BookingDbContext.cs            # Context without FK constraints
├── DTOs/
│   ├── CreateReservationDto.cs        # Request models
│   └── ReservationDto.cs              # Response models
├── appsettings.json                   # VehicleService URL configuration
└── Program.cs                         # HTTP client registration
```

**Database Schema**:
```sql
Customers (
    Id INT PRIMARY KEY IDENTITY,
    Name NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    Phone NVARCHAR(20) NOT NULL,
    DocumentNumber NVARCHAR(20) NOT NULL UNIQUE,
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2 NOT NULL
)

Reservations (
    Id INT PRIMARY KEY IDENTITY,
    ConfirmationNumber NVARCHAR(20) NOT NULL UNIQUE,
    VehicleId INT NOT NULL,  -- No FK constraint (microservices)
    CustomerId INT NOT NULL FOREIGN KEY REFERENCES Customers(Id),
    StartDate DATETIME2 NOT NULL,
    EndDate DATETIME2 NOT NULL,
    TotalPrice DECIMAL(18,2) NOT NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Pending',
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2 NOT NULL,
    CONSTRAINT CK_Reservation_EndDate CHECK (EndDate > StartDate)
)
```

**HTTP Client Configuration**:
```csharp
// Program.cs
builder.Services.AddHttpClient("VehicleService", client =>
{
    client.BaseAddress = new Uri("http://localhost:5002");
});

// ReservationBusinessService.cs
private async Task<bool> ValidateVehicleExistsAsync(int vehicleId)
{
    using var httpClient = _httpClientFactory.CreateClient("VehicleService");
    var response = await httpClient.GetAsync($"/api/vehicles/{vehicleId}");
    return response.IsSuccessStatusCode;
}
```

### 3. WorkerService (Background Service)

**Purpose**: Generates daily reports and analytics across all service databases.

**Key Features**:
- Thread-safe database operations with `IDbContextFactory`
- Daily, monthly, and custom reporting
- Multi-database report generation
- Background service with scheduled execution

**Project Structure**:
```
WorkerService/
├── Services/
│   ├── IReportService.cs               # Report generation interface
│   └── ReportService.cs                # With DbContextFactory pattern
├── Data/
│   └── ReportingDbContext.cs           # Reporting database context
├── Models/
│   ├── DailyReservationSummary.cs      # Report models
│   ├── VehicleUtilizationReport.cs
│   ├── CustomerMetrics.cs
│   └── MonthlyRevenueSummary.cs
├── Worker.cs                           # Background service worker
└── Program.cs                          # DbContextFactory registration
```

**DbContext Factory Implementation**:
```csharp
// Program.cs
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
    }
}
```

**Report Types**:
- **Daily Reservation Summary**: Total reservations, revenue, customer metrics
- **Vehicle Utilization Report**: Usage statistics per vehicle
- **Customer Metrics**: New customers, active customers, returning customers
- **Monthly Revenue Summary**: Monthly financial reporting

### 4. Shared Library (RentaFacil.Shared)

**Purpose**: Common functionality shared across all microservices.

**Components**:
```
RentaFacil.Shared/
├── DTOs/                              # Data Transfer Objects
│   ├── CreateReservationDto.cs
│   ├── ReservationDto.cs
│   ├── VehicleDto.cs
│   └── CustomerDto.cs
├── Entities/                          # Domain Entities
│   ├── Vehicle.cs
│   ├── Customer.cs
│   └── Reservation.cs
├── Enums/
│   ├── ReservationStatus.cs
│   └── VehicleType.cs
├── Interfaces/                        # Service Contracts
│   ├── IVehicleBusinessService.cs
│   ├── IReservationBusinessService.cs
│   └── ICustomerBusinessService.cs
├── Repositories/                      # Base Repository Pattern
│   ├── IBaseRepository.cs
│   └── BaseRepository.cs
└── Models/                            # Shared Models
    └── ApiResponse.cs
```

## Database Architecture

### Database Per Service Pattern
Each microservice maintains its own database to ensure loose coupling and independent scaling:

1. **RentaFacil_VehicleService**: Vehicle inventory data
2. **RentaFacil_BookingService**: Reservations and customers
3. **RentaFacil_ReportingService**: Aggregated reporting data

### Foreign Key Strategy
- **Within Service**: Traditional FK constraints (Customer → Reservation)
- **Cross-Service**: No database constraints, HTTP validation instead
- **Data Consistency**: Eventual consistency through API calls

### Connection Strings
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=RentaFacil_{ServiceName};User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=true;"
  }
}
```

## Key Architectural Decisions Made

### 1. Removed Database Foreign Key Constraints
**Problem**: FK constraints between services violated microservices principles
**Solution**: 
- Removed `FK_Reservations_Vehicles_VehicleId` constraint
- Added HTTP validation in BookingService
- Automatic constraint removal on service startup

### 2. Implemented DbContext Factory Pattern
**Problem**: Concurrent database operations in WorkerService
**Solution**:
- Replaced singleton DbContext with `IDbContextFactory<T>`
- Thread-safe database operations
- Proper resource disposal

### 3. Fixed Repository Navigation Issues
**Problem**: EF Core Include operations on ignored navigation properties
**Solution**:
- Removed all `.Include(r => r.Vehicle)` operations
- Updated repository methods to exclude vehicle navigation
- Maintained customer navigation where appropriate

### 4. Service Discovery Configuration
**Problem**: Port mismatches between frontend and backend services
**Solution**:
- Standardized VehicleService on port 5002
- Updated all configuration files
- Added HTTP client factory registration

## Security Considerations

### CORS Configuration
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

### Input Validation
- Data annotations on DTOs
- Custom validation attributes
- Business rule validation in services

### Connection Security
- SQL Server authentication
- TrustServerCertificate for development
- Connection string encryption in production

## Performance Optimizations

### Database Operations
- Async/await throughout
- Proper DbContext disposal
- Connection pooling
- Query optimization with indexes

### HTTP Client Management
- HttpClient factory pattern
- Connection pooling
- Timeout configurations
- Retry policies (to be implemented)

### Background Processing
- Thread-safe operations
- Scheduled execution
- Error handling and logging
- Resource cleanup

## Logging and Monitoring

### Structured Logging
```csharp
_logger.LogInformation("Creating reservation for vehicle {VehicleId} from {StartDate} to {EndDate}",
    createDto.VehicleId, createDto.StartDate, createDto.EndDate);
```

### Error Handling
- Global exception handling
- Service-specific error responses
- HTTP client error handling
- Database operation error handling

## Testing Strategy

### Unit Testing
- Service layer testing with mocks
- Repository testing with in-memory database
- HTTP client testing with mock handlers

### Integration Testing
- Cross-service communication testing
- Database integration testing
- End-to-end API testing

### Performance Testing
- Load testing individual services
- Database performance testing
- Inter-service communication testing

This architecture provides a solid foundation for a scalable, maintainable microservices application while addressing the specific challenges encountered during development.
# Technical Architecture - RentaFácil MVP

## Overview
RentaFácil follows a simple microservices architecture with Clean Architecture principles applied to each service. The design prioritizes simplicity and functionality over complex patterns.

## Architecture Patterns

### 1. Microservices Architecture ✅ IMPLEMENTED
```
┌─────────────────────────────────────────────────────────────┐
│                    Frontend Layer                           │
│                  Angular 17 SPA                            │
│                 (src/ directory)                           │
└─────────────┬───────────────┬───────────────────────────────┘
              │               │
    ┌─────────▼─────────┐   ┌─▼─────────────────┐
    │  VehicleService   │   │   BookingService  │
    │    Port 5001      │   │     Port 5002     │
    │ ✅ IMPLEMENTED    │   │ ✅ IMPLEMENTED    │
    │ (backend/src/)    │   │  (backend/src/)   │
    └─────────┬─────────┘   └─┬─────────────────┘
              │               │
         ┌────▼────┐     ┌────▼────┐
         │Vehicle  │     │Booking  │
         │Database │     │Database │
         │(Separate)│     │(Separate)│
         └────┬────┘     └────┬────┘
              │               │
              └───────┬───────┘
                      │
            ┌─────────▼─────────┐
            │   Worker Service  │
            │   ✅ IMPLEMENTED  │
            │ (Background/2AM)  │
            │ (backend/src/)    │
            └───────────────────┘
```

### 2. Clean Architecture per Service
Each microservice follows a simplified Clean Architecture:

```
┌─────────────────────────────────────────────────────────────┐
│                  Presentation Layer                         │
│              Controllers & DTOs                             │
└─────────────────────┬───────────────────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────────────────┐
│                Application Layer                            │
│            Services & Business Logic                        │
└─────────────────────┬───────────────────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────────────────┐
│                Infrastructure Layer                         │
│        Repository & Entity Framework                        │
└─────────────────────────────────────────────────────────────┘
```

## Service Responsibilities

### VehicleService ✅ IMPLEMENTED
**Purpose**: Manage vehicle inventory and availability with rich business logic
- Vehicle registration and management with validation
- Availability checking by date ranges with conflict detection
- Vehicle type and model information with seed data
- Basic inventory reporting and CRUD operations

**Key Components**:
- `VehiclesController`: Complete REST API with CRUD operations
- `VehicleBusinessService`: Rich business logic with availability calculation and validation
- `VehicleRepository`: Data access layer using EF Core with specialized queries
- `VehicleDbContext`: Dedicated database context with 5 seeded vehicles
- `Vehicle` entity: Domain model with business rules and navigation properties

### BookingService ✅ IMPLEMENTED
**Purpose**: Handle complex reservations and customer management with advanced features
- Customer registration and management with duplicate detection
- Reservation creation with auto-customer creation and conflict detection
- Advanced reservation filtering, search, and pagination
- Comprehensive business rules enforcement with status management

**Key Components**:
- `ReservationsController`: Advanced reservation management API with 8 endpoints
- `CustomersController`: Complete customer management API with 7 endpoints
- `ReservationBusinessService`: Complex business logic with conflict detection and price calculation
- `CustomerBusinessService`: Customer operations with duplicate prevention
- `ReservationRepository` & `CustomerRepository`: Specialized data access with advanced querying
- `BookingDbContext`: Dedicated database context with proper relationships
- `Reservation` & `Customer` entities: Rich domain models with validation and business rules

### WorkerService ✅ IMPLEMENTED
**Purpose**: Comprehensive background processing, analytics, and business intelligence
- Daily reservation summaries with detailed metrics and status breakdown
- Vehicle utilization analytics with complex overlapping reservation detection
- Monthly revenue reports with vehicle type analysis
- Customer behavioral analytics and retention metrics
- Intelligent scheduling with parallel processing

**Key Components**:
- `Worker`: Background service with intelligent 2 AM UTC scheduling
- `ReportService`: Comprehensive analytics engine with parallel report generation
- `ReportingDbContext`: Read-only database context for analytics
- Multiple report models: `DailyReservationSummary`, `VehicleUtilizationReport`, `MonthlyRevenueSummary`, `CustomerMetrics`
- Advanced scheduling logic with automatic monthly report generation

## Design Patterns Applied

### 1. Repository Pattern
```csharp
public interface IVehicleRepository
{
    Task<IEnumerable<Vehicle>> GetAllAsync();
    Task<Vehicle> GetByIdAsync(int id);
    Task<IEnumerable<Vehicle>> GetAvailableAsync(DateTime start, DateTime end);
    Task AddAsync(Vehicle vehicle);
    Task UpdateAsync(Vehicle vehicle);
}
```

### 2. Service Layer Pattern
```csharp
public interface IVehicleService
{
    Task<VehicleDto> GetVehicleAsync(int id);
    Task<IEnumerable<VehicleDto>> GetAvailableVehiclesAsync(
        DateTime startDate, DateTime endDate, string vehicleType = null);
    Task<VehicleDto> CreateVehicleAsync(CreateVehicleDto createDto);
}
```

### 3. DTO Pattern
```csharp
public class VehicleDto
{
    public int Id { get; set; }
    public string Type { get; set; }
    public string Model { get; set; }
    public int Year { get; set; }
    public bool Available { get; set; }
}
```

## Data Flow

### Vehicle Availability Check
1. **Frontend**: User searches for available vehicles
2. **VehicleService**: Receives request with date range and type
3. **Business Logic**: Validates dates and filters by type
4. **Repository**: Queries database for available vehicles
5. **Response**: Returns filtered vehicle list as DTOs

### Reservation Creation
1. **Frontend**: User submits reservation form
2. **BookingService**: Validates customer and reservation data
3. **Cross-Service**: Checks vehicle availability with VehicleService
4. **Business Logic**: Applies reservation rules and validation
5. **Repository**: Persists reservation and customer data
6. **Response**: Returns reservation confirmation

### Daily Reporting
1. **Worker Service**: Triggers daily at midnight
2. **Data Collection**: Aggregates previous day's reservations
3. **Report Generation**: Creates summary reports
4. **Storage**: Saves reports for future access

## Database Design ✅ IMPLEMENTED

### Microservices Database Separation
Each service has its own dedicated database following microservices best practices:

```
┌─────────────────────────┐    ┌──────────────────────────┐    ┌─────────────────────────┐
│    VehicleService       │    │     BookingService       │    │     WorkerService       │
│                         │    │                          │    │                         │
│ RentaFacil_VehicleService│    │RentaFacil_BookingService │    │  Read-Only Access to    │
│                         │    │                          │    │ BookingService Database │
│ ┌─────────────────────┐ │    │ ┌──────────────────────┐ │    │                         │
│ │      Vehicles       │ │    │ │      Customers       │ │    │ ┌─────────────────────┐ │
│ │   - Id (PK)         │ │    │ │    - Id (PK)         │ │    │ │   Report Models     │ │
│ │   - Type            │ │    │ │    - Name            │ │    │ │ - DailyReservation  │ │
│ │   - Model           │ │    │ │    - Email (Unique)  │ │    │ │ - VehicleUtilization│ │
│ │   - Year            │ │    │ │    - Phone           │ │    │ │ - MonthlyRevenue    │ │
│ │   - PricePerDay     │ │    │ │    - DocumentNumber  │ │    │ │ - CustomerMetrics   │ │
│ │   - Available       │ │    │ │    - CreatedAt       │ │    │ └─────────────────────┘ │
│ │   - CreatedAt       │ │    │ └──────────────────────┘ │    └─────────────────────────┘
│ │   - UpdatedAt       │ │    │                          │
│ └─────────────────────┘ │    │ ┌──────────────────────┐ │
└─────────────────────────┘    │ │     Reservations     │ │
                               │ │   - Id (PK)          │ │
                               │ │   - CustomerId (FK)  │ │
                               │ │   - VehicleId        │ │
                               │ │   - StartDate        │ │
                               │ │   - EndDate          │ │
                               │ │   - TotalPrice       │ │
                               │ │   - Status (enum)    │ │
                               │ │   - ConfirmationNum  │ │
                               │ │   - CreatedAt        │ │
                               │ └──────────────────────┘ │
                               └──────────────────────────┘
```

### Entity Relationships Within BookingService
```
Customer 1 ←── * Reservation
     ↑              ↑
     │              │
 CustomerId     VehicleId (External Reference)
```

### Key Constraints & Business Rules
- **Vehicle Service**: Each vehicle has unique identification and availability tracking
- **Booking Service**: Customer can have multiple reservations; overlapping reservations prevented by business logic
- **Cross-Service**: VehicleId in reservations references vehicles in VehicleService (eventual consistency)
- **Data Integrity**: Email uniqueness enforced in Customer entity
- **Status Management**: Reservation status follows workflow (Pending → Confirmed → Cancelled/Completed)
- **Historical Preservation**: All entities include audit timestamps for historical tracking

## Communication Patterns

### Synchronous Communication
- Frontend ↔ APIs: REST over HTTP
- Service-to-Service: HTTP calls for real-time validation
- Database Access: Entity Framework Core

### Asynchronous Processing
- Worker Service: Background processing using `IHostedService`
- Scheduled Tasks: Timer-based execution for reports

## Configuration Management ✅ IMPLEMENTED

### Service-Specific Configuration
Each microservice has its own appsettings.json with dedicated database connections:

**VehicleService Configuration:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=RentaFacil_VehicleService;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=true;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  }
}
```

**BookingService Configuration:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=RentaFacil_BookingService;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=true;"
  },
  "ApiUrls": {
    "VehicleService": "http://localhost:5001"
  }
}
```

**WorkerService Configuration:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=RentaFacil_BookingService;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=true;"
  },
  "WorkerSettings": {
    "ScheduleTime": "02:00:00",
    "TimeZone": "UTC",
    "EnableDailyReports": true,
    "EnableMonthlyReports": true
  }
}
```

## Error Handling Strategy

### Global Exception Handling
```csharp
public class GlobalExceptionMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (BusinessException ex)
        {
            await HandleBusinessExceptionAsync(context, ex);
        }
        catch (ValidationException ex)
        {
            await HandleValidationExceptionAsync(context, ex);
        }
        catch (Exception ex)
        {
            await HandleGenericExceptionAsync(context, ex);
        }
    }
}
```

### API Response Format
```json
{
  "success": false,
  "message": "Vehicle not available for selected dates",
  "errors": [
    {
      "field": "startDate",
      "message": "Vehicle is already reserved"
    }
  ],
  "data": null
}
```

## Security Considerations (MVP Level)

### Basic Security Measures
- Input validation using Data Annotations
- SQL injection prevention through EF Core
- CORS configuration for frontend access
- Basic request size limits

### Future Security Enhancements
- JWT authentication and authorization
- API rate limiting
- Input sanitization
- Audit logging

## Performance Considerations

### Database Optimization
- Indexed foreign keys for relationships
- Pagination for large result sets
- Connection pooling through EF Core
- Basic query optimization

### Caching Strategy (Future)
- In-memory caching for vehicle types
- Redis for session management
- API response caching for static data

## Monitoring and Logging

### Logging Strategy
```csharp
public class VehicleService
{
    private readonly ILogger<VehicleService> _logger;
    
    public async Task<VehicleDto> GetVehicleAsync(int id)
    {
        _logger.LogInformation("Retrieving vehicle with ID: {VehicleId}", id);
        
        try
        {
            // Business logic
            _logger.LogInformation("Successfully retrieved vehicle {VehicleId}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve vehicle {VehicleId}", id);
            throw;
        }
    }
}
```

### Health Checks
```csharp
builder.Services.AddHealthChecks()
    .AddDbContext<ApplicationDbContext>()
    .AddUrlGroup(new Uri("http://localhost:5001/health"), "VehicleService")
    .AddUrlGroup(new Uri("http://localhost:5002/health"), "BookingService");
```

## Deployment Architecture

### Local Development
- Each service runs on different ports
- Shared SQL Server instance
- Angular dev server with proxy configuration

### Docker Containerization
- Individual Dockerfiles per service
- Docker Compose for orchestration
- Shared network for service communication
- Volume mounting for persistent data

## Testing Strategy

### **MANDATORY** Code Coverage Requirements
- **Overall Minimum**: 80% line coverage across all services
- **Business Logic**: 90% coverage for domain services and critical business rules
- **Quality Gates**: All builds MUST meet coverage thresholds to pass
- **Reporting**: Generate coverage reports for each service in CI/CD pipeline

#### Coverage Thresholds by Component:
```
VehicleService:     80% minimum
BookingService:     80% minimum  
WorkerService:      75% minimum
Frontend (Angular): 75% minimum
```

#### Coverage Tools Configuration:
```bash
# .NET Coverage Commands
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage
reportgenerator -reports:"coverage/**/coverage.cobertura.xml" -targetdir:"coverage/report"

# Angular Coverage
ng test --code-coverage --watch=false
```

### Unit Testing
- Service layer business logic (90% coverage target)
- Repository layer data access (80% coverage target)
- DTO mapping validation (85% coverage target)
- Domain model business rules (95% coverage target)

### Integration Testing
- API endpoint testing (70% coverage target)
- Database integration tests (75% coverage target)
- Service-to-service communication (80% coverage target)

### End-to-End Testing
- Frontend user workflows (60% coverage target)
- Complete reservation scenarios (80% coverage target)
- Cross-service functionality (70% coverage target)

## Scalability Considerations

### Horizontal Scaling (Future)
- Stateless service design
- Database connection pooling
- Load balancer configuration
- Container orchestration

### Data Partitioning (Future)
- Customer-based sharding
- Date-based partitioning for reservations
- Read replicas for reporting

---

This architecture provides a solid foundation for the RentaFácil MVP while maintaining simplicity and focusing on core functionality. The design allows for future enhancements without major restructuring.
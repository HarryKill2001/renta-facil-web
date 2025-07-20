# Development Guidelines - RentaFácil MVP

## Overview
This document establishes **MANDATORY** coding standards, development practices, and architectural requirements for the RentaFácil project.

## ⚠️ CRITICAL REQUIREMENTS
**ALL CODE MUST COMPLY WITH:**
1. **[Architectural Principles](architectural-principles.md)** - SOLID, Clean Architecture, and DDD are **MANDATORY**
2. **Code Review Standards** - Non-compliant code will be **REJECTED**
3. **Layer Separation** - Strict adherence to Clean Architecture layers
4. **Domain-Driven Design** - Proper bounded contexts and domain modeling

**📋 READ [architectural-principles.md](architectural-principles.md) BEFORE WRITING ANY CODE**

## Project Structure Standards

### Solution Organization ✅ IMPLEMENTED
```
renta-facil-web/
├── src/                            # Angular 17 Frontend (Current Structure)
│   ├── app/                        # Angular components & services
│   ├── assets/                     # Static assets (images, icons)
│   ├── index.html                  # Main HTML file
│   ├── main.ts                     # Angular bootstrap
│   └── styles.scss                 # Global styles
├── backend/                        # .NET 8 Backend Services ✅
│   ├── RentaFacil.sln             # Main solution file ✅
│   └── src/
│       ├── VehicleService/         # ✅ Vehicle management microservice
│       │   ├── VehicleService.csproj
│       │   ├── Program.cs
│       │   ├── Controllers/        # VehiclesController with full CRUD
│       │   │   └── VehiclesController.cs
│       │   ├── Services/          # VehicleBusinessService with business logic
│       │   │   ├── IVehicleBusinessService.cs
│       │   │   └── VehicleBusinessService.cs
│       │   ├── Data/              # VehicleDbContext and Repository
│       │   │   ├── VehicleDbContext.cs
│       │   │   ├── IVehicleRepository.cs
│       │   │   └── VehicleRepository.cs
│       │   ├── DTOs/              # Vehicle DTOs and validation
│       │   │   ├── VehicleDto.cs
│       │   │   ├── CreateVehicleDto.cs
│       │   │   └── UpdateVehicleDto.cs
│       │   └── appsettings.json   # VehicleService database config
│       ├── BookingService/         # ✅ Booking & customer microservice
│       │   ├── BookingService.csproj
│       │   ├── Program.cs
│       │   ├── Controllers/        # CustomersController & ReservationsController
│       │   │   ├── CustomersController.cs
│       │   │   └── ReservationsController.cs
│       │   ├── Services/          # Advanced business logic
│       │   │   ├── ICustomerBusinessService.cs
│       │   │   ├── CustomerBusinessService.cs
│       │   │   ├── IReservationBusinessService.cs
│       │   │   └── ReservationBusinessService.cs
│       │   ├── Data/              # BookingDbContext and repositories
│       │   │   ├── BookingDbContext.cs
│       │   │   ├── ICustomerRepository.cs
│       │   │   ├── CustomerRepository.cs
│       │   │   ├── IReservationRepository.cs
│       │   │   └── ReservationRepository.cs
│       │   ├── DTOs/              # Comprehensive DTOs with validation
│       │   │   ├── CustomerDto.cs
│       │   │   ├── CreateCustomerDto.cs
│       │   │   ├── ReservationDto.cs
│       │   │   ├── CreateReservationDto.cs
│       │   │   └── ReservationFilterDto.cs
│       │   └── appsettings.json   # BookingService database config
│       ├── WorkerService/          # ✅ Background processing & analytics
│       │   ├── WorkerService.csproj
│       │   ├── Program.cs
│       │   ├── Worker.cs          # Background service with 2 AM scheduling
│       │   ├── Services/          # Comprehensive reporting engine
│       │   │   ├── IReportService.cs
│       │   │   └── ReportService.cs
│       │   ├── Data/              # Read-only analytics context
│       │   │   └── ReportingDbContext.cs
│       │   ├── Models/            # Report data models
│       │   │   ├── DailyReservationSummary.cs
│       │   │   ├── VehicleUtilizationReport.cs
│       │   │   ├── MonthlyRevenueSummary.cs
│       │   │   └── CustomerMetrics.cs
│       │   └── appsettings.json   # Worker service config with analytics DB
│       └── Shared/                 # ✅ Common libraries (RentaFacil.Shared)
│           ├── RentaFacil.Shared.csproj
│           ├── Domain/            # Rich domain models with business logic
│           │   ├── Vehicle.cs     # Vehicle entity with availability logic
│           │   ├── Customer.cs    # Customer entity with validation
│           │   └── Reservation.cs # Reservation entity with status workflow
│           ├── DTOs/              # Standardized API response wrappers
│           │   ├── ApiResponse.cs
│           │   └── PagedResult.cs
│           ├── Enums/             # Business enums and constants
│           │   ├── ReservationStatus.cs
│           │   └── VehicleType.cs
│           ├── Interfaces/        # Generic repository pattern
│           │   ├── IBaseRepository.cs
│           │   └── IRepository.cs
│           └── Repositories/      # Base repository implementation
│               └── BaseRepository.cs
├── database/                       # SQL scripts and migrations (to be created)
├── docker/                         # Docker configurations (to be created)
├── scripts/                        # Build and deployment scripts (to be created)
├── docs/                           # ✅ Comprehensive documentation
│   ├── tasks.md                   # Updated with Phase 2 completion
│   ├── architecture.md            # Updated with actual implementation
│   ├── development-guidelines.md  # This file
│   └── microservice-specifications.md # Updated with actual APIs
├── angular.json                    # Angular CLI configuration
├── package.json                    # Node.js dependencies
├── tsconfig.json                   # TypeScript configuration
├── .env.example                    # Environment template ✅
├── .gitignore                      # Git ignore file ✅
└── server.ts                       # Angular dev server config
```

### Implementation Status Summary ✅
**Phase 2: Backend Infrastructure & Core Services - COMPLETED**

The following components have been fully implemented with Clean Architecture compliance:

#### ✅ VehicleService (Port 5001)
- **Complete CRUD API**: 6 endpoints with full vehicle management
- **Rich Business Logic**: VehicleBusinessService with availability checking
- **Data Access**: VehicleRepository with specialized queries
- **Database**: VehicleDbContext with 5 seeded vehicles
- **Clean Architecture**: Proper layer separation (Controllers → Services → Repositories)

#### ✅ BookingService (Port 5002) 
- **Advanced Customer Management**: 7 endpoints with duplicate detection
- **Complex Reservation System**: 8 endpoints with auto-customer creation
- **Conflict Detection**: Advanced business logic preventing overlapping reservations
- **Advanced Filtering**: Pagination, search, and filtering capabilities
- **Clean Architecture**: Full implementation with rich domain models

#### ✅ WorkerService (Background)
- **Intelligent Scheduling**: Runs daily at 2 AM UTC with automatic delay calculation
- **Comprehensive Analytics**: 4 different report types with parallel processing
- **Monthly Automation**: Automatic monthly reports on first day of each month
- **Business Intelligence**: Customer metrics, vehicle utilization, revenue analysis

#### ✅ Shared Infrastructure (RentaFacil.Shared)
- **Rich Domain Models**: Vehicle, Customer, Reservation with business logic
- **Generic Repository Pattern**: BaseRepository<T> with full CRUD
- **Standardized Responses**: ApiResponse<T> and PagedResult<T>
- **Business Enums**: ReservationStatus, VehicleType with proper validation

### **MANDATORY** Environment Configuration Rules

#### Environment Variables Management
```csharp
// REQUIRED: Use IConfiguration for all settings
public class VehicleService
{
    private readonly IConfiguration _configuration;
    
    public VehicleService(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public void SomeMethod()
    {
        // CORRECT: Read from configuration
        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        var logLevel = _configuration["Logging:LogLevel:Default"];
        
        // WRONG: Hard-coded values
        var connectionString = "Server=localhost;Database=...";
    }
}
```

#### **FORBIDDEN** Practices:
- ❌ Hard-coding connection strings, passwords, or API keys
- ❌ Committing .env files to Git
- ❌ Using production credentials in development
- ❌ Storing secrets in appsettings.json

#### **REQUIRED** Practices:
- ✅ Use .env files for local development
- ✅ Use Azure Key Vault / AWS Secrets Manager for production
- ✅ Always provide .env.example template
- ✅ Add .env to .gitignore

### **MANDATORY** ORM Standards (MVP)

#### Entity Framework Core Requirements
```csharp
// REQUIRED: Simple entity design
public class Vehicle
{
    public int Id { get; set; }
    public string Type { get; set; }
    public string Model { get; set; }
    public decimal PricePerDay { get; set; }
    public bool Available { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Simple navigation property
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}

// REQUIRED: Generic repository pattern
public interface IRepository<T> where T : class
{
    Task<T> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task SaveChangesAsync();
}

// REQUIRED: Simple service implementation
public class VehicleService
{
    private readonly IRepository<Vehicle> _repository;

    public VehicleService(IRepository<Vehicle> repository)
    {
        _repository = repository;
    }

    public async Task<Vehicle> CreateVehicleAsync(CreateVehicleDto dto)
    {
        var vehicle = new Vehicle
        {
            Type = dto.Type,
            Model = dto.Model,
            PricePerDay = dto.PricePerDay
        };

        await _repository.AddAsync(vehicle);
        await _repository.SaveChangesAsync();
        return vehicle;
    }
}
```

#### **FORBIDDEN** ORM Practices (Too Complex for MVP):
- ❌ Complex LINQ queries with multiple joins
- ❌ Raw SQL for basic operations
- ❌ Complex many-to-many relationships
- ❌ Advanced Entity Framework features (change tracking, etc.)
- ❌ Stored procedures for CRUD operations

#### **REQUIRED** Database Operations:
```csharp
// CORRECT: Simple queries
var vehicles = await _repository.GetAllAsync();
var vehicle = await _repository.GetByIdAsync(1);

// CORRECT: Simple filtering with basic LINQ
var availableVehicles = await _context.Vehicles
    .Where(v => v.Available)
    .ToListAsync();

// WRONG: Complex queries (save for production)
var complexQuery = await _context.Vehicles
    .Include(v => v.Reservations)
        .ThenInclude(r => r.Customer)
    .Where(v => v.Reservations.Any(r => r.Status == "Active"))
    .GroupBy(v => v.Type)
    .Select(g => new { Type = g.Key, Count = g.Count() })
    .ToListAsync();
```

### Service Project Structure
```
VehicleService/
├── Controllers/                    # API controllers
├── Services/                       # Business logic
├── Repositories/                   # Data access layer
├── Models/                         # DTOs and view models
├── Data/                           # DbContext and configurations
├── Extensions/                     # Service extensions
├── Middleware/                     # Custom middleware
├── Program.cs                      # Application entry point
└── appsettings.json               # Configuration
```

## Coding Standards

### C# Coding Standards

#### 1. Naming Conventions
```csharp
// Classes, interfaces, methods, properties - PascalCase
public class VehicleService : IVehicleService
{
    public async Task<Vehicle> GetVehicleAsync(int id) { }
    public decimal PricePerDay { get; set; }
}

// Private fields - camelCase with underscore prefix
private readonly ILogger<VehicleService> _logger;
private readonly ApplicationDbContext _context;

// Local variables, parameters - camelCase
public void ProcessReservation(int vehicleId, DateTime startDate)
{
    var reservation = new Reservation();
    var totalPrice = CalculatePrice(startDate, endDate);
}

// Constants - PascalCase
public const int MaxReservationDays = 30;
private const string DefaultCurrency = "COP";

// Enums - PascalCase for type and values
public enum ReservationStatus
{
    Pending,
    Confirmed,
    Cancelled,
    Completed
}
```

#### 2. Method Structure
```csharp
// Good: Clear, single responsibility, proper error handling
public async Task<VehicleDto> GetVehicleAsync(int id)
{
    _logger.LogInformation("Retrieving vehicle with ID: {VehicleId}", id);
    
    if (id <= 0)
    {
        throw new ArgumentException("Vehicle ID must be positive", nameof(id));
    }
    
    try
    {
        var vehicle = await _repository.GetByIdAsync(id);
        if (vehicle == null)
        {
            throw new NotFoundException($"Vehicle with ID {id} not found");
        }
        
        return _mapper.Map<VehicleDto>(vehicle);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to retrieve vehicle {VehicleId}", id);
        throw;
    }
}

// Bad: Multiple responsibilities, poor error handling
public VehicleDto GetVehicle(int id)
{
    var vehicle = _context.Vehicles.Find(id);
    vehicle.LastAccessed = DateTime.Now;
    _context.SaveChanges();
    return new VehicleDto { Id = vehicle.Id, Model = vehicle.Model };
}
```

#### 3. Class Design Principles
```csharp
// Good: Single responsibility, dependency injection, immutable DTOs
[ApiController]
[Route("api/[controller]")]
public class VehiclesController : ControllerBase
{
    private readonly IVehicleService _vehicleService;
    private readonly ILogger<VehiclesController> _logger;

    public VehiclesController(IVehicleService vehicleService, ILogger<VehiclesController> logger)
    {
        _vehicleService = vehicleService ?? throw new ArgumentNullException(nameof(vehicleService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<VehicleDto>>> GetVehicle(int id)
    {
        try
        {
            var vehicle = await _vehicleService.GetVehicleAsync(id);
            return Ok(ApiResponse<VehicleDto>.Success(vehicle));
        }
        catch (NotFoundException)
        {
            return NotFound(ApiResponse<VehicleDto>.Error("Vehicle not found"));
        }
    }
}

// Immutable DTO design
public record VehicleDto
{
    public int Id { get; init; }
    public string Type { get; init; } = string.Empty;
    public string Model { get; init; } = string.Empty;
    public int Year { get; init; }
    public decimal PricePerDay { get; init; }
    public bool Available { get; init; }
}
```

#### 4. Error Handling Standards
```csharp
// Custom exception hierarchy
public abstract class BusinessException : Exception
{
    protected BusinessException(string message) : base(message) { }
    protected BusinessException(string message, Exception innerException) : base(message, innerException) { }
}

public class NotFoundException : BusinessException
{
    public NotFoundException(string message) : base(message) { }
}

public class ValidationException : BusinessException
{
    public ValidationException(string message) : base(message) { }
    public ValidationException(string message, Dictionary<string, string[]> errors) : base(message)
    {
        Errors = errors;
    }
    
    public Dictionary<string, string[]> Errors { get; } = new();
}

// Global exception middleware
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var response = exception switch
        {
            NotFoundException => new { status = 404, message = exception.Message },
            ValidationException validationEx => new { status = 400, message = exception.Message, errors = validationEx.Errors },
            BusinessException => new { status = 400, message = exception.Message },
            _ => new { status = 500, message = "An internal server error occurred" }
        };
        
        context.Response.StatusCode = response.status;
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
```

### TypeScript/Angular Coding Standards

#### 1. Component Structure
```typescript
// Good: Well-structured component with clear separation
@Component({
  selector: 'app-vehicle-search',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './vehicle-search.component.html',
  styleUrls: ['./vehicle-search.component.scss']
})
export class VehicleSearchComponent implements OnInit, OnDestroy {
  // Public properties first
  searchForm: FormGroup;
  vehicles$ = new BehaviorSubject<Vehicle[]>([]);
  loading = false;
  
  // Private properties
  private destroy$ = new Subject<void>();
  
  constructor(
    private fb: FormBuilder,
    private vehicleService: VehicleService,
    private router: Router
  ) {
    this.searchForm = this.createSearchForm();
  }
  
  ngOnInit(): void {
    this.setupFormValidation();
  }
  
  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
  
  // Public methods
  onSearch(): void {
    if (this.searchForm.valid) {
      this.performSearch();
    }
  }
  
  // Private methods
  private createSearchForm(): FormGroup {
    return this.fb.group({
      startDate: ['', [Validators.required]],
      endDate: ['', [Validators.required]],
      vehicleType: ['']
    });
  }
  
  private performSearch(): void {
    this.loading = true;
    const searchParams = this.searchForm.value;
    
    this.vehicleService.getAvailableVehicles(searchParams)
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => this.loading = false)
      )
      .subscribe({
        next: (vehicles) => this.vehicles$.next(vehicles),
        error: (error) => this.handleError(error)
      });
  }
  
  private handleError(error: any): void {
    console.error('Search error:', error);
    // Handle error appropriately
  }
}
```

#### 2. Service Structure
```typescript
// Good: Strongly typed service with proper error handling
@Injectable({ providedIn: 'root' })
export class VehicleService {
  private readonly apiUrl = `${this.baseUrl}/api/vehicles`;
  
  constructor(
    private http: HttpClient,
    @Inject('API_BASE_URL') private baseUrl: string
  ) {}
  
  getAvailableVehicles(params: VehicleSearchParams): Observable<Vehicle[]> {
    const httpParams = this.buildSearchParams(params);
    
    return this.http.get<ApiResponse<Vehicle[]>>(`${this.apiUrl}/availability`, { params: httpParams })
      .pipe(
        map(response => response.data),
        catchError(this.handleError)
      );
  }
  
  getVehicleById(id: number): Observable<Vehicle> {
    return this.http.get<ApiResponse<Vehicle>>(`${this.apiUrl}/${id}`)
      .pipe(
        map(response => response.data),
        catchError(this.handleError)
      );
  }
  
  private buildSearchParams(params: VehicleSearchParams): HttpParams {
    let httpParams = new HttpParams()
      .set('startDate', params.startDate.toISOString().split('T')[0])
      .set('endDate', params.endDate.toISOString().split('T')[0]);
    
    if (params.type) {
      httpParams = httpParams.set('type', params.type);
    }
    
    return httpParams;
  }
  
  private handleError = (error: HttpErrorResponse): Observable<never> => {
    let errorMessage = 'An unknown error occurred';
    
    if (error.error instanceof ErrorEvent) {
      errorMessage = `Error: ${error.error.message}`;
    } else {
      errorMessage = `Error Code: ${error.status}\nMessage: ${error.message}`;
    }
    
    console.error(errorMessage);
    return throwError(() => new Error(errorMessage));
  };
}
```

#### 3. Interface Definitions
```typescript
// Good: Well-defined interfaces with documentation
export interface Vehicle {
  readonly id: number;
  readonly type: VehicleType;
  readonly model: string;
  readonly year: number;
  readonly pricePerDay: number;
  readonly available: boolean;
  readonly createdAt: Date;
}

export interface VehicleSearchParams {
  readonly startDate: Date;
  readonly endDate: Date;
  readonly type?: VehicleType;
}

export enum VehicleType {
  SUV = 'SUV',
  Sedan = 'Sedan',
  Compact = 'Compact'
}

export interface ApiResponse<T> {
  readonly success: boolean;
  readonly data: T;
  readonly message: string;
  readonly errors?: readonly string[];
}
```

## Git Workflow

### Branch Naming Convention
```
main                    # Production-ready code
develop                 # Integration branch for features
feature/vehicle-search  # New features
feature/booking-system
bugfix/reservation-validation
hotfix/security-patch
release/v1.0.0         # Release preparation
```

### Commit Message Format
```
type(scope): description

body (optional)

footer (optional)
```

#### Types:
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `style`: Code style changes (formatting, etc.)
- `refactor`: Code refactoring
- `test`: Adding or modifying tests
- `chore`: Build process or auxiliary tool changes

#### Examples:
```
feat(vehicle): add availability checking endpoint

fix(reservation): validate date range properly

docs(api): update swagger documentation for booking service

refactor(database): optimize vehicle availability query

test(booking): add unit tests for reservation service
```

### Pull Request Process
1. **Create Feature Branch**: `git checkout -b feature/new-feature develop`
2. **Implement Changes**: Follow coding standards and write tests
3. **Self Review**: Review your own code before submitting
4. **Create PR**: Provide clear description and link to issues
5. **Code Review**: Address feedback and make necessary changes
6. **Merge**: Squash and merge after approval

### Code Review Guidelines

#### **MANDATORY** Review Criteria:
- **🚫 REJECT** if violates SOLID principles
- **🚫 REJECT** if violates Clean Architecture layer separation
- **🚫 REJECT** if business logic is outside domain layer
- **🚫 REJECT** if dependencies flow in wrong direction
- **🚫 REJECT** if domain entities are anemic (no behavior)
- **Correctness**: Does the code do what it's supposed to do?
- **Tests**: Does the code have appropriate tests?
- **Naming**: Are variables, functions, and classes well-named?

#### **MANDATORY** Review Template:
```markdown
## ⚠️ ARCHITECTURAL COMPLIANCE (MANDATORY)
- [ ] **SOLID Principles**: No violations of SRP, OCP, LSP, ISP, DIP
- [ ] **Clean Architecture**: Dependencies point inward, proper layer separation
- [ ] **Domain-Driven Design**: Business logic in domain layer, proper aggregates
- [ ] **Repository Pattern**: Data access properly abstracted
- [ ] **No God Classes**: Each class has single responsibility

## Summary
Brief description of changes

## Testing
- [ ] Unit tests pass
- [ ] Integration tests pass
- [ ] Manual testing completed

## Code Quality
- [ ] Follows coding standards
- [ ] No code smells or anti-patterns
- [ ] Proper error handling
- [ ] Adequate logging

## Security
- [ ] No secrets in code
- [ ] Input validation present
- [ ] No SQL injection vulnerabilities

**❌ REJECTION REASONS**
- Architectural principle violations
- Layer boundary violations
- Domain logic leakage
- SOLID principle violations
```

## Testing Standards

### **MANDATORY** Code Coverage Requirements
- **Minimum Code Coverage**: 80% line coverage across all projects
- **Critical Components**: 90% coverage for business logic and domain services
- **Exception**: UI components and integration tests may have lower coverage
- **Quality Gates**: CI/CD pipeline MUST enforce coverage thresholds
- **Tools**: Use built-in .NET code coverage tools and Angular coverage reports

#### Coverage Requirements by Layer:
- **Domain Layer**: 90% minimum coverage (business rules are critical)
- **Application Services**: 85% minimum coverage
- **API Controllers**: 70% minimum coverage
- **Repositories**: 75% minimum coverage
- **Angular Components**: 70% minimum coverage
- **Angular Services**: 85% minimum coverage

#### Coverage Enforcement:
```xml
<!-- Add to test projects -->
<PropertyGroup>
  <Threshold>80</Threshold>
  <ThresholdType>line</ThresholdType>
  <CoverletOutputFormat>cobertura</CoverletOutputFormat>
</PropertyGroup>
```

#### Angular Coverage Configuration:
```json
// karma.conf.js
coverageReporter: {
  type: 'html',
  dir: 'coverage/',
  check: {
    global: {
      statements: 80,
      branches: 75,
      functions: 80,
      lines: 80
    }
  }
}
```

### Unit Testing Guidelines
```csharp
// Good: Focused, isolated unit test
[Test]
public async Task GetVehicleAsync_WithValidId_ReturnsVehicle()
{
    // Arrange
    var vehicleId = 1;
    var expectedVehicle = new Vehicle { Id = vehicleId, Model = "Toyota Camry" };
    _mockRepository.Setup(r => r.GetByIdAsync(vehicleId))
               .ReturnsAsync(expectedVehicle);

    // Act
    var result = await _vehicleService.GetVehicleAsync(vehicleId);

    // Assert
    Assert.That(result, Is.Not.Null);
    Assert.That(result.Id, Is.EqualTo(vehicleId));
    Assert.That(result.Model, Is.EqualTo("Toyota Camry"));
}

[Test]
public async Task GetVehicleAsync_WithInvalidId_ThrowsNotFoundException()
{
    // Arrange
    var invalidId = 999;
    _mockRepository.Setup(r => r.GetByIdAsync(invalidId))
               .ReturnsAsync((Vehicle)null);

    // Act & Assert
    var exception = await Assert.ThrowsAsync<NotFoundException>(
        () => _vehicleService.GetVehicleAsync(invalidId));
    
    Assert.That(exception.Message, Contains.Substring("not found"));
}
```

### Integration Testing
```csharp
[Test]
public async Task CreateReservation_WithValidData_ReturnsCreatedReservation()
{
    // Arrange
    using var factory = new WebApplicationFactory<Program>();
    var client = factory.CreateClient();
    
    var request = new CreateReservationRequest
    {
        VehicleId = 1,
        StartDate = DateTime.Today.AddDays(1),
        EndDate = DateTime.Today.AddDays(5),
        CustomerInfo = new CustomerInfo
        {
            Name = "John Doe",
            Email = "john@example.com",
            Phone = "+1234567890",
            DocumentNumber = "12345678"
        }
    };

    // Act
    var response = await client.PostAsJsonAsync("/api/reservations", request);

    // Assert
    response.EnsureSuccessStatusCode();
    var reservation = await response.Content.ReadFromJsonAsync<Reservation>();
    Assert.That(reservation.VehicleId, Is.EqualTo(request.VehicleId));
}
```

## Documentation Standards

### API Documentation
```csharp
/// <summary>
/// Retrieves a vehicle by its unique identifier
/// </summary>
/// <param name="id">The unique identifier of the vehicle</param>
/// <returns>The vehicle details if found</returns>
/// <response code="200">Vehicle retrieved successfully</response>
/// <response code="404">Vehicle not found</response>
/// <response code="400">Invalid vehicle ID provided</response>
[HttpGet("{id}")]
[ProducesResponseType(typeof(ApiResponse<VehicleDto>), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
public async Task<ActionResult<ApiResponse<VehicleDto>>> GetVehicle(int id)
{
    // Implementation
}
```

### Code Comments
```csharp
// Good: Explains WHY, not WHAT
// Calculate total price including weekend surcharge
// Weekend bookings have 20% surcharge as per business rules
var weekendMultiplier = IsWeekendBooking(startDate, endDate) ? 1.2m : 1.0m;
var totalPrice = basePricePerDay * numberOfDays * weekendMultiplier;

// Bad: Explains obvious WHAT
// Set the total price variable to the base price times days times multiplier
var totalPrice = basePricePerDay * numberOfDays * weekendMultiplier;
```

## Performance Guidelines

### Database Performance
```csharp
// Good: Efficient query with appropriate indexes
public async Task<IEnumerable<Vehicle>> GetAvailableVehiclesAsync(
    DateTime startDate, DateTime endDate, string vehicleType = null)
{
    var query = _context.Vehicles
        .Where(v => v.Available)
        .Where(v => !v.Reservations.Any(r => 
            r.Status == ReservationStatus.Confirmed &&
            ((r.StartDate <= startDate && r.EndDate > startDate) ||
             (r.StartDate < endDate && r.EndDate >= endDate) ||
             (r.StartDate >= startDate && r.EndDate <= endDate))));
    
    if (!string.IsNullOrEmpty(vehicleType))
    {
        query = query.Where(v => v.Type == vehicleType);
    }
    
    return await query.ToListAsync();
}

// Bad: N+1 query problem
public async Task<IEnumerable<VehicleDto>> GetVehiclesWithReservationCount()
{
    var vehicles = await _context.Vehicles.ToListAsync();
    var result = new List<VehicleDto>();
    
    foreach (var vehicle in vehicles)
    {
        // This creates N+1 queries
        var reservationCount = await _context.Reservations
            .CountAsync(r => r.VehicleId == vehicle.Id);
        
        result.Add(new VehicleDto
        {
            Id = vehicle.Id,
            Model = vehicle.Model,
            ReservationCount = reservationCount
        });
    }
    
    return result;
}
```

### Frontend Performance
```typescript
// Good: OnPush change detection and trackBy
@Component({
  selector: 'app-vehicle-list',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div *ngFor="let vehicle of vehicles; trackBy: trackByVehicleId">
      {{ vehicle.model }}
    </div>
  `
})
export class VehicleListComponent {
  @Input() vehicles: Vehicle[] = [];
  
  trackByVehicleId(index: number, vehicle: Vehicle): number {
    return vehicle.id;
  }
}

// Good: Reactive programming with observables
loadVehicles(): void {
  this.vehicles$ = this.searchForm.valueChanges.pipe(
    debounceTime(300),
    distinctUntilChanged(),
    switchMap(params => this.vehicleService.getAvailableVehicles(params)),
    shareReplay(1)
  );
}
```

## Security Guidelines

### Input Validation
```csharp
// Good: Comprehensive validation
public class CreateVehicleRequestValidator : AbstractValidator<CreateVehicleRequest>
{
    public CreateVehicleRequestValidator()
    {
        RuleFor(x => x.Type)
            .NotEmpty()
            .Must(BeValidVehicleType)
            .WithMessage("Invalid vehicle type");
            
        RuleFor(x => x.Model)
            .NotEmpty()
            .Length(1, 100)
            .Matches("^[a-zA-Z0-9 \\-]+$")
            .WithMessage("Model contains invalid characters");
            
        RuleFor(x => x.Year)
            .InclusiveBetween(2000, DateTime.Now.Year + 1);
            
        RuleFor(x => x.PricePerDay)
            .GreaterThan(0)
            .LessThanOrEqualTo(10000);
    }
    
    private bool BeValidVehicleType(string type)
    {
        return Enum.TryParse<VehicleType>(type, out _);
    }
}
```

### SQL Injection Prevention
```csharp
// Good: Parameterized queries via Entity Framework
var vehicles = await _context.Vehicles
    .Where(v => v.Type == vehicleType)  // EF Core handles parameterization
    .ToListAsync();

// Good: Explicit parameterization when needed
var sql = "SELECT * FROM Vehicles WHERE Type = @type AND Year >= @year";
var vehicles = await _context.Vehicles
    .FromSqlRaw(sql, new SqlParameter("@type", vehicleType), new SqlParameter("@year", year))
    .ToListAsync();

// Bad: String concatenation (vulnerable to SQL injection)
var sql = $"SELECT * FROM Vehicles WHERE Type = '{vehicleType}'";
```

This comprehensive development guide ensures consistent, maintainable, and secure code across the RentaFácil project while maintaining focus on MVP functionality.
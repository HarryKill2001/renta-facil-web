# Architectural Principles - RentaFácil MVP

## 🎯 MANDATORY PRINCIPLES

**CRITICAL**: All code written for the RentaFácil MVP **MUST** adhere to the following architectural principles. These are not suggestions but **REQUIREMENTS** for this project.

## SOLID Principles (MANDATORY)

### 1. Single Responsibility Principle (SRP)
**RULE**: Every class must have one and only one reason to change.

#### ✅ REQUIRED Implementation:
```csharp
// CORRECT: Each class has a single responsibility
public class VehicleAvailabilityChecker
{
    public async Task<bool> IsAvailableAsync(int vehicleId, DateTime start, DateTime end)
    {
        // Only handles availability checking logic
    }
}

public class ReservationPriceCalculator
{
    public decimal CalculatePrice(Vehicle vehicle, DateTime start, DateTime end)
    {
        // Only handles price calculation logic
    }
}

public class ReservationService
{
    private readonly IVehicleAvailabilityChecker _availabilityChecker;
    private readonly IReservationPriceCalculator _priceCalculator;
    private readonly IReservationRepository _repository;
    
    // Orchestrates reservation creation using specialized services
}
```

#### ❌ FORBIDDEN Implementation:
```csharp
// WRONG: God class violating SRP
public class ReservationService
{
    public async Task<Reservation> CreateReservationAsync(CreateReservationRequest request)
    {
        // Violates SRP: handles validation, availability, pricing, persistence, notification
        ValidateRequest(request);
        CheckAvailability(request.VehicleId, request.StartDate, request.EndDate);
        CalculatePrice(request);
        SaveToDatabase(reservation);
        SendEmailNotification(reservation);
        UpdateInventory(request.VehicleId);
        LogActivity(reservation);
    }
}
```

### 2. Open/Closed Principle (OCP)
**RULE**: Classes must be open for extension but closed for modification.

#### ✅ REQUIRED Implementation:
```csharp
// CORRECT: Use abstractions and strategy pattern
public interface IPricingStrategy
{
    decimal CalculatePrice(Vehicle vehicle, int days);
}

public class StandardPricingStrategy : IPricingStrategy
{
    public decimal CalculatePrice(Vehicle vehicle, int days)
    {
        return vehicle.PricePerDay * days;
    }
}

public class WeekendPricingStrategy : IPricingStrategy
{
    public decimal CalculatePrice(Vehicle vehicle, int days)
    {
        return vehicle.PricePerDay * days * 1.2m; // 20% weekend surcharge
    }
}

public class PriceCalculatorService
{
    public decimal Calculate(Vehicle vehicle, DateTime start, DateTime end, IPricingStrategy strategy)
    {
        var days = (end - start).Days;
        return strategy.CalculatePrice(vehicle, days);
    }
}
```

### 3. Liskov Substitution Principle (LSP)
**RULE**: Derived classes must be substitutable for their base classes.

#### ✅ REQUIRED Implementation:
```csharp
// CORRECT: All implementations honor the contract
public abstract class BaseRepository<T> where T : class
{
    public abstract Task<T> GetByIdAsync(int id);
    public abstract Task<IEnumerable<T>> GetAllAsync();
    public abstract Task AddAsync(T entity);
}

public class VehicleRepository : BaseRepository<Vehicle>
{
    // Must honor all base class contracts
    public override async Task<Vehicle> GetByIdAsync(int id)
    {
        if (id <= 0) throw new ArgumentException("ID must be positive");
        // Implementation that returns null if not found (as expected)
    }
}
```

### 4. Interface Segregation Principle (ISP)
**RULE**: Clients should not depend on interfaces they don't use.

#### ✅ REQUIRED Implementation:
```csharp
// CORRECT: Segregated interfaces
public interface IVehicleReader
{
    Task<Vehicle> GetByIdAsync(int id);
    Task<IEnumerable<Vehicle>> GetAllAsync();
}

public interface IVehicleWriter
{
    Task AddAsync(Vehicle vehicle);
    Task UpdateAsync(Vehicle vehicle);
    Task DeleteAsync(int id);
}

public interface IVehicleAvailabilityChecker
{
    Task<bool> IsAvailableAsync(int vehicleId, DateTime start, DateTime end);
    Task<IEnumerable<Vehicle>> GetAvailableVehiclesAsync(DateTime start, DateTime end);
}

// Services implement only what they need
public class VehicleSearchService
{
    private readonly IVehicleReader _reader;
    private readonly IVehicleAvailabilityChecker _availabilityChecker;
    
    // Doesn't depend on write operations
}
```

### 5. Dependency Inversion Principle (DIP)
**RULE**: High-level modules must not depend on low-level modules. Both should depend on abstractions.

#### ✅ REQUIRED Implementation:
```csharp
// CORRECT: Depend on abstractions
public class ReservationService
{
    private readonly IReservationRepository _repository;
    private readonly IVehicleService _vehicleService;
    private readonly ICustomerService _customerService;
    private readonly ILogger<ReservationService> _logger;

    public ReservationService(
        IReservationRepository repository,
        IVehicleService vehicleService,
        ICustomerService customerService,
        ILogger<ReservationService> logger)
    {
        _repository = repository;
        _vehicleService = vehicleService;
        _customerService = customerService;
        _logger = logger;
    }
}
```

## Clean Architecture (MANDATORY)

### Layer Structure (REQUIRED)
Every microservice **MUST** implement these layers:

```
┌─────────────────────────────────────────────────────────────┐
│                    Presentation Layer                       │
│              (Controllers, DTOs, Validators)                │
└─────────────────────┬───────────────────────────────────────┘
                      │ Dependencies flow inward
┌─────────────────────▼───────────────────────────────────────┐
│                 Application Layer                           │
│           (Services, Use Cases, Commands/Queries)           │
└─────────────────────┬───────────────────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────────────────┐
│                   Domain Layer                              │
│         (Entities, Value Objects, Domain Services)          │
└─────────────────────▲───────────────────────────────────────┘
                      │
┌─────────────────────┴───────────────────────────────────────┐
│                Infrastructure Layer                         │
│        (Repositories, External Services, Database)          │
└─────────────────────────────────────────────────────────────┘
```

### MANDATORY Layer Responsibilities:

#### 1. Domain Layer (Core - NO DEPENDENCIES)
```csharp
// REQUIRED: Pure domain logic, no framework dependencies
namespace RentaFacil.Domain.Entities
{
    public class Vehicle
    {
        public int Id { get; private set; }
        public string Type { get; private set; }
        public string Model { get; private set; }
        public int Year { get; private set; }
        public decimal PricePerDay { get; private set; }
        public bool Available { get; private set; }

        // REQUIRED: Domain logic in entity
        public bool IsAvailableForPeriod(DateTime start, DateTime end, IEnumerable<Reservation> existingReservations)
        {
            return Available && !existingReservations.Any(r => r.OverlapsWith(start, end));
        }

        // REQUIRED: Business rules enforcement
        public void SetUnavailable(string reason)
        {
            if (string.IsNullOrEmpty(reason))
                throw new DomainException("Reason is required when setting vehicle unavailable");
            
            Available = false;
        }
    }
}

// REQUIRED: Value Objects for domain concepts
namespace RentaFacil.Domain.ValueObjects
{
    public class DateRange
    {
        public DateTime Start { get; }
        public DateTime End { get; }

        public DateRange(DateTime start, DateTime end)
        {
            if (start >= end)
                throw new DomainException("Start date must be before end date");
            if (start < DateTime.Today)
                throw new DomainException("Start date cannot be in the past");
            
            Start = start;
            End = end;
        }

        public int Days => (End - Start).Days;
        public bool OverlapsWith(DateRange other) => Start < other.End && End > other.Start;
    }
}
```

#### 2. Application Layer (Use Cases)
```csharp
// REQUIRED: Application services implementing use cases
namespace RentaFacil.Application.Services
{
    public class CreateReservationUseCase
    {
        private readonly IReservationRepository _reservationRepository;
        private readonly IVehicleRepository _vehicleRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IDomainEventDispatcher _eventDispatcher;

        public async Task<ReservationDto> ExecuteAsync(CreateReservationCommand command)
        {
            // REQUIRED: Validate business rules
            var vehicle = await _vehicleRepository.GetByIdAsync(command.VehicleId);
            if (vehicle == null)
                throw new NotFoundException("Vehicle not found");

            var dateRange = new DateRange(command.StartDate, command.EndDate);
            var existingReservations = await _reservationRepository.GetByVehicleAndDateRangeAsync(
                command.VehicleId, dateRange.Start, dateRange.End);

            if (!vehicle.IsAvailableForPeriod(dateRange.Start, dateRange.End, existingReservations))
                throw new BusinessRuleViolationException("Vehicle not available for selected dates");

            // REQUIRED: Create domain entity
            var reservation = Reservation.Create(
                vehicle,
                customer,
                dateRange,
                new ReservationPriceCalculator());

            await _reservationRepository.AddAsync(reservation);
            
            // REQUIRED: Dispatch domain events
            await _eventDispatcher.DispatchAsync(new ReservationCreatedEvent(reservation));

            return ReservationDto.FromDomain(reservation);
        }
    }
}
```

#### 3. Infrastructure Layer (External Concerns)
```csharp
// REQUIRED: Repository implementations
namespace RentaFacil.Infrastructure.Repositories
{
    public class ReservationRepository : IReservationRepository
    {
        private readonly ApplicationDbContext _context;

        // REQUIRED: Implement domain repository interface
        public async Task<IEnumerable<Reservation>> GetByVehicleAndDateRangeAsync(
            int vehicleId, DateTime start, DateTime end)
        {
            return await _context.Reservations
                .Where(r => r.VehicleId == vehicleId &&
                           r.DateRange.Start < end &&
                           r.DateRange.End > start)
                .ToListAsync();
        }
    }
}
```

## Domain-Driven Design (MANDATORY)

### Bounded Contexts (REQUIRED)
Each microservice **MUST** represent a bounded context:

#### 1. Vehicle Management Context
```csharp
// REQUIRED: Vehicle aggregate with business rules
public class Vehicle : AggregateRoot
{
    private readonly List<MaintenanceRecord> _maintenanceHistory;
    
    public VehicleType Type { get; private set; }
    public VehicleModel Model { get; private set; }
    public Money PricePerDay { get; private set; }
    public AvailabilityStatus Status { get; private set; }

    // REQUIRED: Domain service for complex business logic
    public bool CanBeReservedBy(Customer customer, DateRange period, 
        IVehicleEligibilityService eligibilityService)
    {
        return Status.IsAvailable && 
               eligibilityService.IsEligible(customer, this) &&
               !HasConflictingReservations(period);
    }
}
```

#### 2. Booking Context
```csharp
// REQUIRED: Reservation aggregate
public class Reservation : AggregateRoot
{
    public ReservationId Id { get; private set; }
    public VehicleId VehicleId { get; private set; }
    public CustomerId CustomerId { get; private set; }
    public DateRange Period { get; private set; }
    public Money TotalAmount { get; private set; }
    public ReservationStatus Status { get; private set; }

    // REQUIRED: Factory method with business rules
    public static Reservation Create(Vehicle vehicle, Customer customer, 
        DateRange period, IPricingService pricingService)
    {
        // Business rule validation
        if (!vehicle.CanBeReservedBy(customer, period))
            throw new DomainException("Vehicle cannot be reserved by this customer");

        var totalAmount = pricingService.CalculatePrice(vehicle, period);
        
        var reservation = new Reservation
        {
            Id = ReservationId.New(),
            VehicleId = vehicle.Id,
            CustomerId = customer.Id,
            Period = period,
            TotalAmount = totalAmount,
            Status = ReservationStatus.Pending
        };

        // REQUIRED: Domain event
        reservation.AddDomainEvent(new ReservationCreatedEvent(reservation));
        
        return reservation;
    }
}
```

### REQUIRED Domain Services
```csharp
// REQUIRED: Domain service for complex business logic
public class VehicleEligibilityService
{
    public bool IsEligible(Customer customer, Vehicle vehicle)
    {
        // Complex business rules that don't belong to a single entity
        return customer.HasValidLicense() &&
               customer.MeetsAgeRequirement(vehicle.Type) &&
               !customer.HasOutstandingViolations();
    }
}
```

### REQUIRED Repository Patterns
```csharp
// REQUIRED: Domain repository interface (in Domain layer)
public interface IReservationRepository
{
    Task<Reservation> GetByIdAsync(ReservationId id);
    Task<IEnumerable<Reservation>> GetByCustomerAsync(CustomerId customerId);
    Task AddAsync(Reservation reservation);
    Task<bool> HasConflictingReservationsAsync(VehicleId vehicleId, DateRange period);
}
```

## ORM Requirements (MANDATORY)

### Entity Framework Core - MVP Standards
**RULE**: All data access must use Entity Framework Core with these simple, straightforward patterns:

#### ✅ REQUIRED ORM Practices (MVP Level):
```csharp
// CORRECT: Simple DbContext for MVP
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Reservation> Reservations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Simple, straightforward configurations only
        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Model).HasMaxLength(100).IsRequired();
            entity.Property(e => e.PricePerDay).HasColumnType("decimal(10,2)");
        });

        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.HasOne(r => r.Vehicle)
                .WithMany(v => v.Reservations)
                .HasForeignKey(r => r.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
```

#### ✅ REQUIRED Repository Pattern (Simple):
```csharp
// CORRECT: Basic repository interface
public interface IRepository<T> where T : class
{
    Task<T> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
    Task SaveChangesAsync();
}

// CORRECT: Simple EF implementation
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T> GetByIdAsync(int id) => await _dbSet.FindAsync(id);
    public async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();
    public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);
    public async Task UpdateAsync(T entity) => _dbSet.Update(entity);
    public async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null) _dbSet.Remove(entity);
    }
    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
}
```

#### ✅ REQUIRED Migration Strategy (MVP):
```bash
# Simple migration workflow for MVP
dotnet ef migrations add InitialCreate
dotnet ef database update

# For changes during development
dotnet ef migrations add AddReservationTable
dotnet ef database update
```

#### ✅ REQUIRED Entity Design (Simple):
```csharp
// CORRECT: Simple entities with basic relationships
public class Vehicle
{
    public int Id { get; set; }
    public string Type { get; set; }
    public string Model { get; set; }
    public int Year { get; set; }
    public decimal PricePerDay { get; set; }
    public bool Available { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Simple navigation property
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string DocumentNumber { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}
```

#### ✅ REQUIRED Service Integration:
```csharp
// CORRECT: Simple service using repository
public class VehicleService
{
    private readonly IRepository<Vehicle> _vehicleRepository;

    public VehicleService(IRepository<Vehicle> vehicleRepository)
    {
        _vehicleRepository = vehicleRepository;
    }

    public async Task<Vehicle> GetVehicleAsync(int id)
    {
        return await _vehicleRepository.GetByIdAsync(id);
    }

    public async Task<Vehicle> CreateVehicleAsync(CreateVehicleDto dto)
    {
        var vehicle = new Vehicle
        {
            Type = dto.Type,
            Model = dto.Model,
            Year = dto.Year,
            PricePerDay = dto.PricePerDay
        };

        await _vehicleRepository.AddAsync(vehicle);
        await _vehicleRepository.SaveChangesAsync();
        return vehicle;
    }
}
```

#### ❌ FORBIDDEN ORM Practices (Too Complex for MVP):
- Complex many-to-many relationships with join entities
- Advanced query splitting or explicit loading strategies
- Custom SQL functions or raw SQL for simple operations
- Complex inheritance strategies (TPH, TPT, TPC)
- Advanced concurrency handling with optimistic locking
- Custom conventions or complex model configurations
- Stored procedures for basic CRUD operations

#### ✅ REQUIRED Dependency Injection Setup:
```csharp
// Program.cs - Simple EF setup
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IVehicleService, VehicleService>();
```

## Mandatory Implementation Checklist

### ✅ SOLID Compliance Checklist (REQUIRED)
- [ ] **SRP**: Each class has only one reason to change
- [ ] **OCP**: New features added through extension, not modification
- [ ] **LSP**: All derived classes are substitutable for base classes
- [ ] **ISP**: No class depends on methods it doesn't use
- [ ] **DIP**: All dependencies are on abstractions, not concretions

### ✅ Clean Architecture Checklist (REQUIRED)
- [ ] **Domain Layer**: Contains only business logic, no framework dependencies
- [ ] **Application Layer**: Contains use cases and application services
- [ ] **Infrastructure Layer**: Contains external concerns (database, API calls)
- [ ] **Presentation Layer**: Contains controllers and DTOs
- [ ] **Dependency Direction**: All dependencies point inward to the domain

### ✅ DDD Compliance Checklist (REQUIRED)
- [ ] **Bounded Contexts**: Each microservice represents one bounded context
- [ ] **Aggregates**: Business entities grouped into aggregates with clear boundaries
- [ ] **Domain Services**: Complex business logic in domain services
- [ ] **Value Objects**: Immutable objects for domain concepts
- [ ] **Domain Events**: Business events captured and handled appropriately
- [ ] **Repository Pattern**: Data access abstracted through repositories

### ✅ ORM Compliance Checklist (REQUIRED)
- [ ] **Entity Framework Core**: All data access uses EF Core
- [ ] **Simple DbContext**: One DbContext per microservice with basic configuration
- [ ] **Repository Pattern**: Generic repository implementation for basic CRUD
- [ ] **Simple Entities**: POCOs with basic properties and navigation properties
- [ ] **Code First Migrations**: Database created and updated through migrations
- [ ] **Basic Relationships**: One-to-many relationships only (no complex many-to-many)
- [ ] **Connection String**: Uses environment variables for database connection
- [ ] **Dependency Injection**: DbContext and repositories properly registered

## Code Review Requirements

### MANDATORY Review Criteria
Every pull request **MUST** be reviewed for:

1. **SOLID Violations**: Any violation of SOLID principles is grounds for rejection
2. **Layer Violations**: Dependencies flowing in wrong direction = automatic rejection
3. **Domain Logic Leakage**: Business logic outside domain layer = rejection
4. **Anemic Domain Model**: Entities without behavior = rejection
5. **God Classes**: Classes with too many responsibilities = rejection

### Example Review Comments (REQUIRED)
```
❌ REJECT: "This class violates SRP by handling both validation and persistence"
❌ REJECT: "Application service directly uses Entity Framework - violates Clean Architecture"
❌ REJECT: "Business logic in controller - must be moved to domain layer"
✅ APPROVE: "Clean separation of concerns, proper dependency injection, domain-driven design followed"
```

## Training Requirements

### MANDATORY for All Developers
1. **SOLID Principles**: Must demonstrate understanding before writing code
2. **Clean Architecture**: Must understand layer responsibilities and dependencies
3. **DDD Concepts**: Must understand aggregates, value objects, and domain services
4. **Code Review**: Must be able to identify architectural violations

**REMEMBER**: These are not guidelines - they are **MANDATORY REQUIREMENTS** for the RentaFácil MVP. Any code that violates these principles will be rejected during code review.
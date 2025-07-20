# Database Design - RentaFácil MVP

## Overview
The RentaFácil database is designed using SQL Server with **Entity Framework Core Code First** approach for the MVP. The design follows simple normalization principles while prioritizing **straightforward implementation** over complex patterns.

## 🎯 **MVP ORM Strategy**
- **Code First Approach**: Database schema generated from C# entities
- **Simple Entity Framework Core**: Basic CRUD operations, no complex queries
- **Generic Repository Pattern**: One repository interface for all entities
- **Basic Relationships**: One-to-many only (Vehicle → Reservations, Customer → Reservations)
- **Automatic Migrations**: Database updates through EF migrations

## Database Schema

### Entity Relationship Diagram
```
┌─────────────────┐         ┌─────────────────┐         ┌─────────────────┐
│    Vehicles     │         │   Reservations  │         │    Customers    │
├─────────────────┤         ├─────────────────┤         ├─────────────────┤
│ Id (PK)         │◄────────┤ VehicleId (FK)  │         │ Id (PK)         │
│ Type            │         │ CustomerId (FK) ├────────►│ Name            │
│ Model           │         │ StartDate       │         │ Email           │
│ Year            │         │ EndDate         │         │ Phone           │
│ PricePerDay     │         │ TotalPrice      │         │ DocumentNumber  │
│ Available       │         │ Status          │         │ CreatedAt       │
│ CreatedAt       │         │ ConfirmationNum │         │ UpdatedAt       │
│ UpdatedAt       │         │ CreatedAt       │         └─────────────────┘
└─────────────────┘         │ UpdatedAt       │
                            └─────────────────┘
                                     │
                            ┌─────────────────┐
                            │ DailyReports    │
                            ├─────────────────┤
                            │ Id (PK)         │
                            │ ReportDate      │
                            │ TotalReservs    │
                            │ TotalRevenue    │
                            │ AvgUtilization  │
                            │ TopVehicleTypes │
                            │ CreatedAt       │
                            └─────────────────┘
```

## Entity Definitions (Code First)

### 1. Vehicle Entity (MVP)
Simple vehicle entity with basic properties and relationships.

```csharp
// Domain/Entities/Vehicle.cs
public class Vehicle
{
    public int Id { get; set; }
    public string Type { get; set; }              // SUV, Sedan, Compact
    public string Model { get; set; }             // Toyota RAV4, Honda Civic
    public int Year { get; set; }
    public decimal PricePerDay { get; set; }
    public bool Available { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Simple navigation property
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}
```

### Entity Framework Configuration:
```csharp
// Infrastructure/Data/Configurations/VehicleConfiguration.cs
public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.HasKey(v => v.Id);
        
        builder.Property(v => v.Type)
            .HasMaxLength(50)
            .IsRequired();
            
        builder.Property(v => v.Model)
            .HasMaxLength(100)
            .IsRequired();
            
        builder.Property(v => v.PricePerDay)
            .HasColumnType("decimal(10,2)")
            .IsRequired();
            
        builder.Property(v => v.Available)
            .HasDefaultValue(true);
            
        builder.Property(v => v.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");
    }
}
```

### 2. Customer Entity (MVP)
Simple customer entity with basic validation.

```csharp
// Domain/Entities/Customer.cs
public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string DocumentNumber { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Simple navigation property
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}
```

### Entity Framework Configuration:
```csharp
// Infrastructure/Data/Configurations/CustomerConfiguration.cs
public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.HasKey(c => c.Id);
        
        builder.Property(c => c.Name)
            .HasMaxLength(100)
            .IsRequired();
            
        builder.Property(c => c.Email)
            .HasMaxLength(255)
            .IsRequired();
            
        builder.Property(c => c.Phone)
            .HasMaxLength(20)
            .IsRequired();
            
        builder.Property(c => c.DocumentNumber)
            .HasMaxLength(50)
            .IsRequired();
            
        builder.Property(c => c.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // Simple unique constraints
        builder.HasIndex(c => c.Email).IsUnique();
        builder.HasIndex(c => c.DocumentNumber).IsUnique();
    }
}
```

### 3. Reservation Entity (MVP)
Simple reservation entity linking customers and vehicles.

```csharp
// Domain/Entities/Reservation.cs
public class Reservation
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    public int CustomerId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = "Confirmed";
    public string ConfirmationNumber { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Simple navigation properties
    public Vehicle Vehicle { get; set; }
    public Customer Customer { get; set; }
}
```

### Entity Framework Configuration:
```csharp
// Infrastructure/Data/Configurations/ReservationConfiguration.cs
public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.HasKey(r => r.Id);
        
        builder.Property(r => r.StartDate)
            .IsRequired();
            
        builder.Property(r => r.EndDate)
            .IsRequired();
            
        builder.Property(r => r.TotalPrice)
            .HasColumnType("decimal(10,2)")
            .IsRequired();
            
        builder.Property(r => r.Status)
            .HasMaxLength(20)
            .HasDefaultValue("Confirmed")
            .IsRequired();
            
        builder.Property(r => r.ConfirmationNumber)
            .HasMaxLength(50)
            .IsRequired();
            
        builder.Property(r => r.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // Simple relationships
        builder.HasOne(r => r.Vehicle)
            .WithMany(v => v.Reservations)
            .HasForeignKey(r => r.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne(r => r.Customer)
            .WithMany(c => c.Reservations)
            .HasForeignKey(r => r.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Simple constraints
        builder.HasIndex(r => r.ConfirmationNumber).IsUnique();
        builder.HasIndex(r => new { r.VehicleId, r.StartDate, r.EndDate });
    }
}
```

### 4. DailyReports Table
Stores daily aggregated reports generated by the Worker Service.

```sql
CREATE TABLE DailyReports (
    Id int IDENTITY(1,1) PRIMARY KEY,
    ReportDate date NOT NULL,
    TotalReservations int NOT NULL DEFAULT 0,
    TotalRevenue decimal(15,2) NOT NULL DEFAULT 0,
    AverageUtilization decimal(5,2) NOT NULL DEFAULT 0,
    TopVehicleTypes nvarchar(500) NULL,
    CreatedAt datetime2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT UQ_DailyReports_ReportDate UNIQUE (ReportDate)
);

-- Indexes
CREATE INDEX IX_DailyReports_ReportDate ON DailyReports(ReportDate);
CREATE INDEX IX_DailyReports_CreatedAt ON DailyReports(CreatedAt);
```

## Entity Framework Core Setup (MVP)

### DbContext Configuration (Simple)
```csharp
// Infrastructure/Data/ApplicationDbContext.cs
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    // Simple DbSets for MVP
    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Reservation> Reservations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Apply simple configurations
        modelBuilder.ApplyConfiguration(new VehicleConfiguration());
        modelBuilder.ApplyConfiguration(new CustomerConfiguration());
        modelBuilder.ApplyConfiguration(new ReservationConfiguration());
    }
}
```

### Program.cs Configuration (MVP)
```csharp
// Program.cs - Simple EF setup for MVP
var builder = WebApplication.CreateBuilder(args);

// Add DbContext with SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add simple repository pattern
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

var app = builder.Build();

// Apply migrations automatically for MVP
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate(); // Creates/updates database automatically
}
```

### Migration Commands (MVP Workflow)
```bash
# Initial setup
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools

# Create initial migration
dotnet ef migrations add InitialCreate

# Create and update database
dotnet ef database update

# For changes during development
dotnet ef migrations add AddNewFeature
dotnet ef database update
```

### Entity Configurations

#### Vehicle Configuration
```csharp
// Infrastructure/Data/Configurations/VehicleConfiguration.cs
public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.ToTable("Vehicles");
        
        builder.HasKey(v => v.Id);
        
        builder.Property(v => v.Type)
            .HasMaxLength(50)
            .IsRequired();
            
        builder.Property(v => v.Model)
            .HasMaxLength(100)
            .IsRequired();
            
        builder.Property(v => v.Year)
            .IsRequired();
            
        builder.Property(v => v.PricePerDay)
            .HasColumnType("decimal(10,2)")
            .IsRequired();
            
        builder.Property(v => v.Available)
            .HasDefaultValue(true)
            .IsRequired();
            
        builder.Property(v => v.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()")
            .IsRequired();
            
        builder.Property(v => v.UpdatedAt)
            .IsRequired(false);

        // Indexes
        builder.HasIndex(v => v.Type).HasDatabaseName("IX_Vehicles_Type");
        builder.HasIndex(v => v.Available).HasDatabaseName("IX_Vehicles_Available");
        builder.HasIndex(v => v.CreatedAt).HasDatabaseName("IX_Vehicles_CreatedAt");
    }
}
```

#### Customer Configuration
```csharp
// Infrastructure/Data/Configurations/CustomerConfiguration.cs
public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");
        
        builder.HasKey(c => c.Id);
        
        builder.Property(c => c.Name)
            .HasMaxLength(100)
            .IsRequired();
            
        builder.Property(c => c.Email)
            .HasMaxLength(255)
            .IsRequired();
            
        builder.Property(c => c.Phone)
            .HasMaxLength(20)
            .IsRequired();
            
        builder.Property(c => c.DocumentNumber)
            .HasMaxLength(50)
            .IsRequired();
            
        builder.Property(c => c.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()")
            .IsRequired();
            
        builder.Property(c => c.UpdatedAt)
            .IsRequired(false);

        // Unique constraints
        builder.HasIndex(c => c.Email)
            .IsUnique()
            .HasDatabaseName("UQ_Customers_Email");
            
        builder.HasIndex(c => c.DocumentNumber)
            .IsUnique()
            .HasDatabaseName("UQ_Customers_DocumentNumber");

        // Additional indexes
        builder.HasIndex(c => c.CreatedAt).HasDatabaseName("IX_Customers_CreatedAt");
    }
}
```

#### Reservation Configuration
```csharp
// Infrastructure/Data/Configurations/ReservationConfiguration.cs
public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.ToTable("Reservations");
        
        builder.HasKey(r => r.Id);
        
        builder.Property(r => r.VehicleId)
            .IsRequired();
            
        builder.Property(r => r.CustomerId)
            .IsRequired();
            
        builder.Property(r => r.StartDate)
            .HasColumnType("date")
            .IsRequired();
            
        builder.Property(r => r.EndDate)
            .HasColumnType("date")
            .IsRequired();
            
        builder.Property(r => r.TotalPrice)
            .HasColumnType("decimal(10,2)")
            .IsRequired();
            
        builder.Property(r => r.Status)
            .HasMaxLength(20)
            .HasDefaultValue("Confirmed")
            .IsRequired();
            
        builder.Property(r => r.ConfirmationNumber)
            .HasMaxLength(50)
            .IsRequired();
            
        builder.Property(r => r.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()")
            .IsRequired();
            
        builder.Property(r => r.UpdatedAt)
            .IsRequired(false);

        // Foreign key relationships
        builder.HasOne(r => r.Vehicle)
            .WithMany(v => v.Reservations)
            .HasForeignKey(r => r.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne(r => r.Customer)
            .WithMany(c => c.Reservations)
            .HasForeignKey(r => r.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Unique constraints
        builder.HasIndex(r => r.ConfirmationNumber)
            .IsUnique()
            .HasDatabaseName("UQ_Reservations_ConfirmationNumber");

        // Indexes for performance
        builder.HasIndex(r => r.VehicleId).HasDatabaseName("IX_Reservations_VehicleId");
        builder.HasIndex(r => r.CustomerId).HasDatabaseName("IX_Reservations_CustomerId");
        builder.HasIndex(r => r.StartDate).HasDatabaseName("IX_Reservations_StartDate");
        builder.HasIndex(r => r.EndDate).HasDatabaseName("IX_Reservations_EndDate");
        builder.HasIndex(r => r.Status).HasDatabaseName("IX_Reservations_Status");
        builder.HasIndex(r => r.CreatedAt).HasDatabaseName("IX_Reservations_CreatedAt");
        
        // Composite index for availability checking
        builder.HasIndex(r => new { r.VehicleId, r.StartDate, r.EndDate, r.Status })
            .HasDatabaseName("IX_Reservations_Availability");

        // Check constraints
        builder.HasCheckConstraint("CK_Reservations_DateRange", "EndDate > StartDate");
        builder.HasCheckConstraint("CK_Reservations_Status", "Status IN ('Confirmed', 'Cancelled', 'Completed')");
    }
}
```

## Sample Data Scripts

### Initial Vehicle Data
```sql
-- Sample vehicles for testing
INSERT INTO Vehicles (Type, Model, Year, PricePerDay, Available) VALUES
('SUV', 'Toyota RAV4', 2023, 85.00, 1),
('SUV', 'Honda CR-V', 2023, 90.00, 1),
('Sedan', 'Toyota Camry', 2023, 75.00, 1),
('Sedan', 'Honda Accord', 2023, 80.00, 1),
('Compact', 'Nissan Versa', 2023, 55.00, 1),
('Compact', 'Toyota Corolla', 2023, 60.00, 1),
('SUV', 'Ford Explorer', 2022, 95.00, 1),
('Sedan', 'Hyundai Elantra', 2023, 65.00, 1),
('Compact', 'Kia Rio', 2023, 50.00, 1),
('SUV', 'Mazda CX-5', 2023, 88.00, 1);
```

### Sample Customer Data
```sql
-- Sample customers for testing
INSERT INTO Customers (Name, Email, Phone, DocumentNumber) VALUES
('Juan Pérez', 'juan.perez@email.com', '+57 300 123 4567', '12345678'),
('María García', 'maria.garcia@email.com', '+57 301 234 5678', '23456789'),
('Carlos Rodríguez', 'carlos.rodriguez@email.com', '+57 302 345 6789', '34567890'),
('Ana López', 'ana.lopez@email.com', '+57 303 456 7890', '45678901'),
('Luis Martínez', 'luis.martinez@email.com', '+57 304 567 8901', '56789012');
```

### Sample Reservation Data
```sql
-- Sample reservations for testing
INSERT INTO Reservations (VehicleId, CustomerId, StartDate, EndDate, TotalPrice, Status, ConfirmationNumber) VALUES
(1, 1, '2024-01-20', '2024-01-25', 425.00, 'Confirmed', 'RF-20240115-001'),
(3, 2, '2024-01-22', '2024-01-26', 300.00, 'Confirmed', 'RF-20240115-002'),
(5, 3, '2024-01-18', '2024-01-21', 165.00, 'Completed', 'RF-20240115-003'),
(2, 4, '2024-01-25', '2024-01-30', 450.00, 'Confirmed', 'RF-20240115-004'),
(7, 5, '2024-01-16', '2024-01-19', 285.00, 'Completed', 'RF-20240115-005');
```

## Database Queries for Business Logic

### Check Vehicle Availability
```sql
-- Query to check if a vehicle is available for specific dates
SELECT v.Id, v.Type, v.Model, v.Year, v.PricePerDay
FROM Vehicles v
WHERE v.Available = 1
  AND v.Id NOT IN (
    SELECT r.VehicleId 
    FROM Reservations r 
    WHERE r.Status = 'Confirmed'
      AND (
        (r.StartDate <= @StartDate AND r.EndDate > @StartDate) OR
        (r.StartDate < @EndDate AND r.EndDate >= @EndDate) OR
        (r.StartDate >= @StartDate AND r.EndDate <= @EndDate)
      )
  )
  AND (@VehicleType IS NULL OR v.Type = @VehicleType);
```

### Customer Reservation History
```sql
-- Query to get customer reservation history
SELECT 
    r.Id,
    r.ConfirmationNumber,
    v.Model as VehicleModel,
    v.Type as VehicleType,
    r.StartDate,
    r.EndDate,
    r.TotalPrice,
    r.Status,
    r.CreatedAt
FROM Reservations r
INNER JOIN Vehicles v ON r.VehicleId = v.Id
WHERE r.CustomerId = @CustomerId
ORDER BY r.CreatedAt DESC;
```

### Daily Revenue Report
```sql
-- Query for daily revenue calculation
SELECT 
    CAST(r.CreatedAt AS date) as ReservationDate,
    COUNT(*) as TotalReservations,
    SUM(r.TotalPrice) as TotalRevenue,
    AVG(r.TotalPrice) as AverageReservationValue
FROM Reservations r
WHERE r.Status = 'Confirmed'
  AND CAST(r.CreatedAt AS date) = @ReportDate
GROUP BY CAST(r.CreatedAt AS date);
```

### Vehicle Utilization Report
```sql
-- Query for vehicle utilization calculation
SELECT 
    v.Id,
    v.Model,
    v.Type,
    COUNT(r.Id) as TotalReservations,
    SUM(DATEDIFF(day, r.StartDate, r.EndDate)) as TotalDaysReserved,
    CAST(SUM(DATEDIFF(day, r.StartDate, r.EndDate)) * 100.0 / 30 AS decimal(5,2)) as UtilizationPercentage
FROM Vehicles v
LEFT JOIN Reservations r ON v.Id = r.VehicleId 
    AND r.Status IN ('Confirmed', 'Completed')
    AND r.StartDate >= DATEADD(month, -1, @ReportDate)
    AND r.StartDate <= @ReportDate
GROUP BY v.Id, v.Model, v.Type
ORDER BY UtilizationPercentage DESC;
```

## Migration Scripts

### Initial Migration
```csharp
// Migrations/001_InitialCreate.cs
public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Vehicles",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                Model = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Year = table.Column<int>(type: "int", nullable: false),
                PricePerDay = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                Available = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Vehicles", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Customers",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                DocumentNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Customers", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Reservations",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                VehicleId = table.Column<int>(type: "int", nullable: false),
                CustomerId = table.Column<int>(type: "int", nullable: false),
                StartDate = table.Column<DateTime>(type: "date", nullable: false),
                EndDate = table.Column<DateTime>(type: "date", nullable: false),
                TotalPrice = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Confirmed"),
                ConfirmationNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Reservations", x => x.Id);
                table.ForeignKey(
                    name: "FK_Reservations_Customers_CustomerId",
                    column: x => x.CustomerId,
                    principalTable: "Customers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Reservations_Vehicles_VehicleId",
                    column: x => x.VehicleId,
                    principalTable: "Vehicles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.CheckConstraint("CK_Reservations_DateRange", "EndDate > StartDate");
                table.CheckConstraint("CK_Reservations_Status", "Status IN ('Confirmed', 'Cancelled', 'Completed')");
            });

        // Create indexes
        migrationBuilder.CreateIndex(
            name: "IX_Vehicles_Type",
            table: "Vehicles",
            column: "Type");

        migrationBuilder.CreateIndex(
            name: "UQ_Customers_Email",
            table: "Customers",
            column: "Email",
            unique: true);

        // ... additional indexes
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Reservations");
        migrationBuilder.DropTable(name: "Customers");
        migrationBuilder.DropTable(name: "Vehicles");
    }
}
```

## Performance Optimization

### Indexing Strategy
- **Primary Keys**: Clustered indexes on all ID columns
- **Foreign Keys**: Non-clustered indexes for join performance
- **Search Columns**: Indexes on frequently searched columns (Type, Email, Status)
- **Composite Indexes**: Multi-column indexes for complex queries (vehicle availability)

### Query Optimization Tips
1. Use appropriate WHERE clauses to leverage indexes
2. Avoid SELECT * in production queries
3. Use appropriate data types (date vs datetime2)
4. Consider pagination for large result sets
5. Use parameterized queries to prevent SQL injection

## Backup and Maintenance

### Regular Maintenance Tasks
```sql
-- Update statistics (weekly)
UPDATE STATISTICS Vehicles;
UPDATE STATISTICS Customers;
UPDATE STATISTICS Reservations;

-- Rebuild indexes (monthly)
ALTER INDEX ALL ON Vehicles REBUILD;
ALTER INDEX ALL ON Customers REBUILD;
ALTER INDEX ALL ON Reservations REBUILD;

-- Clean up old report data (monthly)
DELETE FROM DailyReports 
WHERE CreatedAt < DATEADD(month, -6, GETDATE());
```

This database design provides a solid foundation for the RentaFácil MVP with proper normalization, indexing, and performance considerations while maintaining simplicity and extensibility.
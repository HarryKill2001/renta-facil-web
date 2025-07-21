# RentaFácil S.A.S. - Vehicle Reservation System (MVP)

## 🚗 Project Overview
RentaFácil is a simple vehicle reservation system MVP built with modern technologies. This project demonstrates clean architecture principles, microservices design, and full-stack development practices.

### MVP Scope
- Vehicle availability checking by type and dates
- Customer reservation management
- Reservation history viewing
- Basic daily reporting

## 🛠 Tech Stack
- **Backend**: .NET 8+ with **MANDATORY Clean Architecture**
- **Frontend**: Angular 18+
- **Database**: SQL Server with Entity Framework Core
- **Containerization**: Docker & Docker Compose
- **API Documentation**: Swagger/OpenAPI

## 🏗 **MANDATORY ARCHITECTURAL PRINCIPLES**
**⚠️ CRITICAL**: All development **MUST** follow these principles:
- **SOLID Principles** - Every class must adhere to Single Responsibility, Open/Closed, Liskov Substitution, Interface Segregation, and Dependency Inversion
- **Clean Architecture** - Strict layer separation with dependencies pointing inward
- **Domain-Driven Design** - Bounded contexts, aggregates, domain services, and value objects
- **Code Review Requirement** - All code violating these principles will be rejected

📋 **See [Architectural Principles](docs/architectural-principles.md) for mandatory implementation details**

## 🏗 Architecture Overview
```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Angular 18+   │    │  VehicleService │    │  BookingService │
│   Frontend      │────┤    (.NET 8)    │◄───┤    (.NET 8)    │
│   Port: 4200    │    │   Port: 5002    │    │   Port: 5257   │
└─────────────────┘    └─────────────────┘    └─────────────────┘
                              │                        │
                     ┌────────────────┐      ┌─────────────────┐
                     │ RentaFacil_    │      │ RentaFacil_     │
                     │ VehicleService │      │ BookingService  │
                     │   Database     │      │   Database      │
                     └────────────────┘      └─────────────────┘
                                                       │
                              ┌─────────────────────────┤
                              │                         │
                     ┌─────────────────┐      ┌─────────────────┐
                     │  Worker Service │      │ RentaFacil_     │
                     │ (Daily Reports) │      │ ReportingService│
                     │   Port: 5000    │      │   Database      │
                     └─────────────────┘      └─────────────────┘
```

### Key Architectural Changes Made:
- **Microservices Communication**: BookingService validates vehicles via HTTP calls to VehicleService
- **Database Separation**: Each service has its own dedicated database
- **Foreign Key Constraints**: Removed database-level constraints to support microservices architecture
- **DbContext Factory Pattern**: Implemented thread-safe database operations in background services
- **Service Discovery**: HTTP clients configured for inter-service communication

## 🚀 Quick Start

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 18+](https://nodejs.org/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)

### ⭐ Fastest Option: One-Click Start (Recommended)
```bash
# Windows
scripts\quick-start.bat

# Linux/macOS  
chmod +x scripts/quick-start.sh
./scripts/quick-start.sh
```
This script automatically:
- ✅ Starts SQL Server in Docker
- ✅ Starts all .NET backend services
- ✅ Starts Angular frontend
- ✅ Opens browser to http://localhost:4200

### Option 1: Using Individual Scripts

**Windows:**
```batch
# Start SQL Server
scripts\start-sql-server.bat

# Start Backend Services  
scripts\start-backend.bat

# Start Frontend
scripts\start-frontend.bat

# OR start everything at once
scripts\start-all.bat
```

**Linux/macOS:**
```bash
# Make scripts executable
chmod +x scripts/*.sh

# Start SQL Server
./scripts/start-sql-server.sh

# Start Backend Services
./scripts/start-backend.sh  

# Start Frontend
./scripts/start-frontend.sh

# OR start everything at once
./scripts/start-all.sh
```

### Option 2: Docker Compose (SQL Server Only)
```bash
# Start SQL Server only (for development)
docker-compose -f docker-compose.dev.yml up -d

# Then manually start .NET services and frontend
cd backend/src/VehicleService
dotnet run --urls="http://localhost:5002"
# ... (continue with other services)
```

### Option 3: Full Docker Compose
```bash
# Start complete application in containers
docker-compose up -d

# View logs
docker-compose logs -f
```

**Access Points:**
- 🌐 **Frontend**: http://localhost:4200
- 🚗 **VehicleService API**: http://localhost:5002/swagger
- 📅 **BookingService API**: http://localhost:5257/swagger
- ⚙️ **WorkerService**: Background service (no web interface)
- 🗄️ **Database**: localhost:1433 (sa/YourStrong!Passw0rd)

## 📁 Project Structure
```
renta-facil-web/
├── backend/src/
│   ├── VehicleService/          # Vehicle management microservice
│   │   ├── Controllers/         # API controllers
│   │   ├── Services/            # Business logic services
│   │   ├── Repositories/        # Data access layer
│   │   ├── Data/               # DbContext and configurations
│   │   └── Properties/         # Launch settings
│   ├── BookingService/          # Reservation management microservice
│   │   ├── Controllers/         # API controllers
│   │   ├── Services/            # Business logic with HTTP client validation
│   │   ├── Repositories/        # Data access layer (no Vehicle includes)
│   │   ├── Data/               # DbContext with removed FK constraints
│   │   └── DTOs/               # Data transfer objects
│   ├── WorkerService/           # Background processing service
│   │   ├── Services/            # Report generation with DbContextFactory
│   │   ├── Data/               # Reporting DbContext
│   │   ├── Models/             # Reporting models
│   │   └── Worker.cs           # Background service
│   └── Shared/RentaFacil.Shared/  # Common libraries and DTOs
│       ├── DTOs/               # Shared data transfer objects
│       ├── Entities/           # Domain entities
│       ├── Enums/              # Shared enumerations
│       ├── Interfaces/         # Service contracts
│       └── Repositories/       # Base repository patterns
├── src/app/                     # Angular 17+ application
│   ├── core/                   # Core functionality
│   │   ├── services/           # API services with corrected endpoints
│   │   ├── guards/             # Route guards
│   │   └── interceptors/       # HTTP interceptors
│   ├── shared/                 # Shared components
│   │   ├── components/         # Reusable UI components
│   │   ├── models/             # TypeScript models
│   │   └── validators/         # Custom validators
│   ├── features/               # Feature modules (lazy loaded)
│   └── app.routes.ts           # Standalone component routing
├── docs/                       # Updated documentation
├── scripts/                     # Startup and utility scripts
│   ├── quick-start.bat/sh      # ⭐ One-click application startup
│   ├── start-sql-server.bat/sh # SQL Server Docker container
│   ├── start-backend.bat/sh    # All .NET backend services
│   ├── start-frontend.bat/sh   # Angular development server
│   └── start-all.bat/sh        # Complete application startup
├── docker-compose.yml          # Full application containers
├── docker-compose.dev.yml      # SQL Server only (for development)
├── angular.json                # Angular CLI configuration
├── package.json                # Frontend dependencies
├── .env.example                # Environment template (updated ports)
└── README.md                   # This file
```

## 🎯 Core Features

### Vehicle Management
- Register new vehicles with type, model, and availability
- Check vehicle availability by date range and type
- View vehicle inventory

### Reservation System
- Search available vehicles by dates
- Create reservations with customer information
- View customer reservation history
- Basic reservation validation

### Reporting
- Daily reservation summary
- Vehicle utilization reports
- Customer activity tracking

## 🔗 API Endpoints

### VehicleService (Port 5002)
```
GET    /api/vehicles              # List all vehicles (with seed data)
POST   /api/vehicles              # Register new vehicle
GET    /api/vehicles/availability # Check availability by date range
GET    /api/vehicles/{id}         # Get vehicle details (used for validation)
```

### BookingService (Port 5257)
```
POST   /api/reservations          # Create reservation (with vehicle validation)
GET    /api/reservations          # List reservations (paginated)
GET    /api/reservations/{id}     # Get reservation details
GET    /api/reservations/confirmation/{number} # Get by confirmation number
GET    /api/customers/{id}/history # Get customer history
POST   /api/customers             # Register customer
PUT    /api/reservations/{id}/cancel # Cancel reservation
PUT    /api/reservations/{id}/confirm # Confirm reservation
```

### Inter-Service Communication
- **BookingService → VehicleService**: HTTP validation calls to verify vehicle existence
- **WorkerService → All Databases**: Generates reports across all service databases

## 🗄 Database Schema

### RentaFacil_VehicleService Database
```sql
Vehicles (Id, Type, Model, Year, PricePerDay, Available, CreatedAt, UpdatedAt)
-- Seeded with 5 vehicles (Toyota Camry, Honda CR-V, Nissan Versa, Ford Explorer, Chevrolet Malibu)
```

### RentaFacil_BookingService Database
```sql
Customers (Id, Name, Email, Phone, DocumentNumber, CreatedAt, UpdatedAt)
Reservations (Id, VehicleId, CustomerId, StartDate, EndDate, TotalPrice, Status, 
             ConfirmationNumber, CreatedAt, UpdatedAt)
-- Note: No FK constraint to Vehicles (microservices architecture)
-- Vehicle validation done via HTTP calls
```

### RentaFacil_ReportingService Database
```sql
-- Used by WorkerService for report generation
-- Consolidates data from all service databases
DailyReservationSummary (ReportDate, TotalReservations, TotalRevenue, ...)
VehicleUtilizationReport (ReportDate, VehicleId, UtilizationPercentage, ...)
CustomerMetrics (ReportDate, TotalCustomers, NewCustomers, ...)
MonthlyRevenueSummary (Year, Month, TotalRevenue, ...)
```

## 🧪 Testing the System
1. **Access Frontend**: Navigate to http://localhost:4200
2. **Search Vehicles**: Use the search form to find available vehicles
3. **Create Reservation**: Fill out the reservation form with customer details
4. **View History**: Check the customer history section
5. **API Testing**: Use Swagger UI at the service endpoints

## 📚 Documentation

### Core Architecture
- **[🏗 Backend Architecture](docs/backend-architecture.md)** - Complete .NET microservices architecture guide
- **[🖥️ Frontend Structure](docs/frontend-structure.md)** - Angular 17+ application structure and implementation
- **[🔗 Microservices Communication](docs/microservices-communication.md)** - Inter-service communication patterns and solutions

### API Documentation  
- [API Specifications](docs/api-specifications.md) - Endpoint documentation
- [Docker Setup](docs/docker-setup.md) - Container deployment guide

### Issues Resolved
All major architectural issues have been resolved:
- ✅ **DbContext Concurrency**: Fixed with IDbContextFactory pattern
- ✅ **Foreign Key Violations**: Removed constraints, added HTTP validation  
- ✅ **Service Communication**: HTTP client configuration and port alignment
- ✅ **Navigation Property Errors**: Cleaned up Include statements
- ✅ **Port Configuration**: Standardized service ports and frontend integration

## 📜 Available Scripts

### Startup Scripts
```bash
# Quick Start (Recommended)
scripts/quick-start.bat          # Windows - One-click startup
scripts/quick-start.sh           # Linux/macOS - One-click startup

# Individual Components
scripts/start-sql-server.bat/.sh    # SQL Server in Docker
scripts/start-backend.bat/.sh       # All .NET services
scripts/start-frontend.bat/.sh      # Angular development server
scripts/start-all.bat/.sh          # Everything in sequence
```

### Docker Commands
```bash
# Development (SQL Server only)
docker-compose -f docker-compose.dev.yml up -d    # Start SQL Server
docker-compose -f docker-compose.dev.yml down     # Stop SQL Server

# Full Application
docker-compose up -d             # Start all services in containers
docker-compose logs -f           # View logs
docker-compose down              # Stop all services
```

### Development Commands
```bash
# Backend (.NET)
dotnet build                     # Build all services
dotnet test                      # Run tests
dotnet run --urls=http://localhost:5002  # Run specific service

# Frontend (Angular)
npm install                      # Install dependencies
npm start                        # Development server (ng serve)
npm run build                    # Production build
npm test                         # Run unit tests
```

## ⚠️ Troubleshooting

### Common Issues Fixed:
- ✅ **DbContext Concurrency Errors**: Fixed with `IDbContextFactory` pattern in WorkerService
- ✅ **Foreign Key Constraint Violations**: Removed database constraints, added HTTP validation
- ✅ **Port Mismatch Issues**: All services now properly configured (VehicleService: 5002, BookingService: 5257)
- ✅ **Include Navigation Errors**: Removed `Include(r => r.Vehicle)` from repositories

### Current Troubleshooting:
- **Database Connection**: Ensure SQL Server is running and connection string is correct
- **Port Conflicts**: Check if ports 4200, 5002, 5257, 1433 are available
- **CORS Issues**: Verify frontend and backend configurations match
- **Service Communication**: Ensure VehicleService is running when BookingService validates vehicles
- **Docker Issues**: Try `docker-compose down && docker-compose up -d`

### Service Health Checks:
- VehicleService: http://localhost:5002/swagger
- BookingService: http://localhost:5257/swagger  
- Frontend: http://localhost:4200
- SQL Server: Connect with SSMS to localhost:1433

## 🤝 Contributing
1. Create feature branch: `git checkout -b feature/your-feature`
2. Commit changes: `git commit -m "Add your feature"`
3. Push branch: `git push origin feature/your-feature`
4. Create Pull Request

## 📄 License
This project is for demonstration purposes as part of a technical assessment.

---
**MVP Focus**: This system prioritizes functionality over production-ready features. Perfect for demonstrating technical skills and rapid development capabilities.
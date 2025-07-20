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
│   Frontend      │────┤    (.NET 8)    │────┤    (.NET 8)    │
└─────────────────┘    └─────────────────┘    └─────────────────┘
                              │                        │
                              └────────┬───────────────┘
                                       │
                              ┌─────────────────┐
                              │   SQL Server    │
                              │   Database      │
                              └─────────────────┘
                                       │
                              ┌─────────────────┐
                              │  Worker Service │
                              │  (Daily Reports)│
                              └─────────────────┘
```

## 🚀 Quick Start

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 18+](https://nodejs.org/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [Angular CLI](https://angular.io/cli): `npm install -g @angular/cli`

### Option 1: Docker Compose (Recommended)
```bash
# Clone and navigate to project
git clone <repository-url>
cd renta-facil-web

# Setup environment variables
cp .env.example .env
# Edit .env file with your local settings (see Docker Setup guide)

# Start all services
docker-compose up -d

# Verify services are running
docker-compose ps
```

**Access Points:**
- Frontend: http://localhost:4200
- Vehicle API: http://localhost:5001/swagger
- Booking API: http://localhost:5002/swagger
- Database: localhost:1433 (sa/YourPassword123!)

### Option 2: Local Development
```bash
# 0. Setup environment variables
cp .env.example .env
# Edit .env file with your local settings

# 1. Start SQL Server (Docker)
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourPassword123!" -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest

# 2. Start Backend Services
cd src/VehicleService
dotnet run --urls="http://localhost:5001"

cd ../BookingService  
dotnet run --urls="http://localhost:5002"

cd ../WorkerService
dotnet run

# 3. Start Frontend
cd ../../frontend
npm install
ng serve
```

## 📁 Project Structure
```
renta-facil-web/
├── src/
│   ├── VehicleService/          # Vehicle management microservice
│   ├── BookingService/          # Reservation management microservice
│   ├── WorkerService/           # Background processing service
│   └── Shared/                  # Common libraries and DTOs
├── frontend/                    # Angular application
├── database/                    # SQL scripts and migrations
├── docker/                      # Docker configurations
├── docs/                        # Documentation
├── scripts/                     # Build and utility scripts
├── .env                         # Environment variables (DO NOT COMMIT)
├── .env.example                 # Environment template (safe to commit)
├── .gitignore                   # Git ignore file
├── docker-compose.yml           # Local development setup
├── docker-compose.override.yml  # Development overrides
└── README.md
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

### VehicleService (Port 5001)
```
GET    /api/vehicles              # List all vehicles
POST   /api/vehicles              # Register new vehicle
GET    /api/vehicles/availability # Check availability
GET    /api/vehicles/{id}         # Get vehicle details
```

### BookingService (Port 5002)
```
POST   /api/reservations          # Create reservation
GET    /api/reservations          # List reservations
GET    /api/reservations/{id}     # Get reservation details
GET    /api/customers/{id}/history # Get customer history
POST   /api/customers             # Register customer
```

## 🗄 Database Schema
```sql
-- Core entities
Vehicles (Id, Type, Model, Year, Available, CreatedAt)
Customers (Id, Name, Email, Phone, DocumentNumber, CreatedAt)
Reservations (Id, VehicleId, CustomerId, StartDate, EndDate, Status, CreatedAt)
```

## 🧪 Testing the System
1. **Access Frontend**: Navigate to http://localhost:4200
2. **Search Vehicles**: Use the search form to find available vehicles
3. **Create Reservation**: Fill out the reservation form with customer details
4. **View History**: Check the customer history section
5. **API Testing**: Use Swagger UI at the service endpoints

## 📚 Additional Documentation
- **[🏗 Architectural Principles](docs/architectural-principles.md)** - **MANDATORY READING** - SOLID, Clean Architecture, DDD requirements
- [Technical Architecture](docs/architecture.md)
- [Microservice Specifications](docs/microservice-specifications.md)
- [API Specifications](docs/api-specifications.md)
- [Database Design](docs/database-design.md)
- [Frontend Structure](docs/frontend-structure.md)
- [Development Guidelines](docs/development-guidelines.md)
- [Docker Setup](docs/docker-setup.md)

## 🔧 Development Commands
```bash
# Backend
dotnet build                     # Build all services
dotnet test                      # Run tests
dotnet ef database update        # Apply migrations

# Frontend
ng serve                         # Development server
ng build                         # Production build
ng test                          # Run unit tests

# Docker
docker-compose up -d             # Start all services
docker-compose logs -f           # View logs
docker-compose down              # Stop services
```

## ⚠️ Troubleshooting
- **Database Connection**: Ensure SQL Server is running and connection string is correct
- **Port Conflicts**: Check if ports 4200, 5001, 5002, 1433 are available
- **CORS Issues**: Verify frontend and backend configurations match
- **Docker Issues**: Try `docker-compose down && docker-compose up -d`

## 🤝 Contributing
1. Create feature branch: `git checkout -b feature/your-feature`
2. Commit changes: `git commit -m "Add your feature"`
3. Push branch: `git push origin feature/your-feature`
4. Create Pull Request

## 📄 License
This project is for demonstration purposes as part of a technical assessment.

---
**MVP Focus**: This system prioritizes functionality over production-ready features. Perfect for demonstrating technical skills and rapid development capabilities.
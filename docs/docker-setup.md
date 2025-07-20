# Docker Setup - RentaFácil MVP

## Overview
This document provides comprehensive Docker configuration for the RentaFácil system, including individual Dockerfiles for each service and a complete Docker Compose setup for local development.

## Docker Architecture

### Container Structure
```
┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐
│   Frontend      │  │  VehicleService │  │  BookingService │
│   (Angular)     │  │    (.NET 8)     │  │    (.NET 8)     │
│   Port: 4200    │  │   Port: 5001    │  │   Port: 5002    │
└─────────────────┘  └─────────────────┘  └─────────────────┘
         │                     │                     │
         └─────────────────────┼─────────────────────┘
                               │
                    ┌─────────────────┐
                    │   SQL Server    │
                    │   Port: 1433    │
                    └─────────────────┘
                               │
                    ┌─────────────────┐
                    │  Worker Service │
                    │    (.NET 8)     │
                    └─────────────────┘
```

## Individual Dockerfiles

### 1. VehicleService Dockerfile
```dockerfile
# src/VehicleService/Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5001

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["VehicleService/VehicleService.csproj", "VehicleService/"]
COPY ["Shared/Shared.csproj", "Shared/"]

# Restore dependencies
RUN dotnet restore "VehicleService/VehicleService.csproj"

# Copy source code
COPY . .
WORKDIR "/src/VehicleService"

# Build the application
RUN dotnet build "VehicleService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "VehicleService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create non-root user
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

ENTRYPOINT ["dotnet", "VehicleService.dll"]
```

### 2. BookingService Dockerfile
```dockerfile
# src/BookingService/Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5002

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["BookingService/BookingService.csproj", "BookingService/"]
COPY ["Shared/Shared.csproj", "Shared/"]

# Restore dependencies
RUN dotnet restore "BookingService/BookingService.csproj"

# Copy source code
COPY . .
WORKDIR "/src/BookingService"

# Build the application
RUN dotnet build "BookingService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BookingService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create non-root user
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

ENTRYPOINT ["dotnet", "BookingService.dll"]
```

### 3. WorkerService Dockerfile
```dockerfile
# src/WorkerService/Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["WorkerService/WorkerService.csproj", "WorkerService/"]
COPY ["Shared/Shared.csproj", "Shared/"]

# Restore dependencies
RUN dotnet restore "WorkerService/WorkerService.csproj"

# Copy source code
COPY . .
WORKDIR "/src/WorkerService"

# Build the application
RUN dotnet build "WorkerService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WorkerService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create non-root user
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

ENTRYPOINT ["dotnet", "WorkerService.dll"]
```

### 4. Frontend Dockerfile
```dockerfile
# frontend/Dockerfile
# Stage 1: Build Angular application
FROM node:18-alpine AS build
WORKDIR /app

# Copy package files
COPY package*.json ./
RUN npm ci --only=production

# Copy source code and build
COPY . .
RUN npm run build --prod

# Stage 2: Serve with nginx
FROM nginx:alpine AS final

# Copy built application
COPY --from=build /app/dist/renta-facil-frontend /usr/share/nginx/html

# Copy nginx configuration
COPY nginx.conf /etc/nginx/nginx.conf

# Expose port
EXPOSE 80

# Start nginx
CMD ["nginx", "-g", "daemon off;"]
```

### 5. Frontend Nginx Configuration
```nginx
# frontend/nginx.conf
events {
    worker_connections 1024;
}

http {
    include /etc/nginx/mime.types;
    default_type application/octet-stream;

    # Gzip compression
    gzip on;
    gzip_vary on;
    gzip_min_length 1024;
    gzip_proxied any;
    gzip_comp_level 6;
    gzip_types
        text/plain
        text/css
        text/xml
        text/javascript
        application/json
        application/javascript
        application/xml+rss
        application/atom+xml
        image/svg+xml;

    server {
        listen 80;
        server_name localhost;
        root /usr/share/nginx/html;
        index index.html;

        # Handle Angular routing
        location / {
            try_files $uri $uri/ /index.html;
        }

        # API proxy to backend services
        location /api/vehicles/ {
            proxy_pass http://vehicleservice:5001/api/vehicles/;
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto $scheme;
        }

        location /api/ {
            proxy_pass http://bookingservice:5002/api/;
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto $scheme;
        }

        # Security headers
        add_header X-Frame-Options "SAMEORIGIN" always;
        add_header X-Content-Type-Options "nosniff" always;
        add_header X-XSS-Protection "1; mode=block" always;
        add_header Referrer-Policy "no-referrer-when-downgrade" always;

        # Cache static assets
        location ~* \.(js|css|png|jpg|jpeg|gif|ico|svg)$ {
            expires 1y;
            add_header Cache-Control "public, immutable";
        }
    }
}
```

## Docker Compose Configuration

### Main Docker Compose File
```yaml
# docker-compose.yml
version: '3.8'

services:
  # SQL Server Database
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: ${COMPOSE_PROJECT_NAME:-rentafacil}-sqlserver
    environment:
      ACCEPT_EULA: "Y"
      SA_PASSWORD: ${SA_PASSWORD}
      MSSQL_PID: ${MSSQL_PID:-Express}
    ports:
      - "1433:1433"
    volumes:
      - sqlserver_data:/var/opt/mssql
      - ./database/init:/docker-entrypoint-initdb.d
    networks:
      - rentafacil-network
    restart: unless-stopped
    healthcheck:
      test: /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "YourPassword123!" -Q "SELECT 1" || exit 1
      interval: 30s
      timeout: 10s
      retries: 5
      start_period: 30s

  # Vehicle Service
  vehicleservice:
    build:
      context: ./src
      dockerfile: VehicleService/Dockerfile
    container_name: rentafacil-vehicleservice
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ASPNETCORE_URLS=http://+:5001
      - ConnectionStrings__DefaultConnection=Server=sqlserver,1433;Database=RentaFacilVehicles;User Id=sa;Password=YourPassword123!;TrustServerCertificate=true;
      - Logging__LogLevel__Default=Information
    ports:
      - "5001:5001"
    depends_on:
      sqlserver:
        condition: service_healthy
    networks:
      - rentafacil-network
    restart: unless-stopped
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:5001/health"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 40s

  # Booking Service
  bookingservice:
    build:
      context: ./src
      dockerfile: BookingService/Dockerfile
    container_name: rentafacil-bookingservice
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ASPNETCORE_URLS=http://+:5002
      - ConnectionStrings__DefaultConnection=Server=sqlserver,1433;Database=RentaFacilBookings;User Id=sa;Password=YourPassword123!;TrustServerCertificate=true;
      - VehicleService__BaseUrl=http://vehicleservice:5001
      - Logging__LogLevel__Default=Information
    ports:
      - "5002:5002"
    depends_on:
      sqlserver:
        condition: service_healthy
      vehicleservice:
        condition: service_healthy
    networks:
      - rentafacil-network
    restart: unless-stopped
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:5002/health"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 40s

  # Worker Service
  workerservice:
    build:
      context: ./src
      dockerfile: WorkerService/Dockerfile
    container_name: rentafacil-workerservice
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Server=sqlserver,1433;Database=RentaFacilReports;User Id=sa;Password=YourPassword123!;TrustServerCertificate=true;
      - VehicleService__BaseUrl=http://vehicleservice:5001
      - BookingService__BaseUrl=http://bookingservice:5002
      - WorkerSettings__ReportSchedule=0 2 * * *
      - Logging__LogLevel__Default=Information
    depends_on:
      sqlserver:
        condition: service_healthy
      vehicleservice:
        condition: service_healthy
      bookingservice:
        condition: service_healthy
    networks:
      - rentafacil-network
    restart: unless-stopped

  # Frontend Application
  frontend:
    build:
      context: ./frontend
      dockerfile: Dockerfile
    container_name: rentafacil-frontend
    ports:
      - "4200:80"
    depends_on:
      vehicleservice:
        condition: service_healthy
      bookingservice:
        condition: service_healthy
    networks:
      - rentafacil-network
    restart: unless-stopped
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:80"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 30s

networks:
  rentafacil-network:
    driver: bridge
    name: rentafacil-network

volumes:
  sqlserver_data:
    driver: local
    name: rentafacil-sqlserver-data
```

### Development Override File
```yaml
# docker-compose.override.yml
version: '3.8'

services:
  sqlserver:
    ports:
      - "1433:1433"
    volumes:
      - ./database/dev-data:/tmp/dev-data

  vehicleservice:
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ASPNETCORE_URLS=http://+:5001
    volumes:
      - ./src/VehicleService:/app/source
      - ./logs:/app/logs
    command: ["dotnet", "watch", "run", "--project", "/app/source"]

  bookingservice:
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ASPNETCORE_URLS=http://+:5002
    volumes:
      - ./src/BookingService:/app/source
      - ./logs:/app/logs
    command: ["dotnet", "watch", "run", "--project", "/app/source"]

  workerservice:
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - WorkerSettings__ReportSchedule=*/5 * * * *  # Every 5 minutes for testing
    volumes:
      - ./src/WorkerService:/app/source
      - ./logs:/app/logs

  frontend:
    volumes:
      - ./frontend/src:/app/src
      - ./frontend/angular.json:/app/angular.json
      - ./frontend/package.json:/app/package.json
    command: ["ng", "serve", "--host", "0.0.0.0", "--port", "4200"]
```

### Production Configuration
```yaml
# docker-compose.prod.yml
version: '3.8'

services:
  sqlserver:
    environment:
      MSSQL_PID: "Standard"
    volumes:
      - sqlserver_prod_data:/var/opt/mssql
    deploy:
      resources:
        limits:
          memory: 2G
          cpus: '1.0'

  vehicleservice:
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://+:5001
    deploy:
      replicas: 2
      resources:
        limits:
          memory: 512M
          cpus: '0.5'

  bookingservice:
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://+:5002
    deploy:
      replicas: 2
      resources:
        limits:
          memory: 512M
          cpus: '0.5'

  workerservice:
    deploy:
      resources:
        limits:
          memory: 256M
          cpus: '0.25'

  frontend:
    deploy:
      resources:
        limits:
          memory: 128M
          cpus: '0.25'

volumes:
  sqlserver_prod_data:
    driver: local
```

## Database Initialization

### Database Setup Script
```sql
-- database/init/01-create-databases.sql
USE master;
GO

-- Create databases
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'RentaFacilVehicles')
BEGIN
    CREATE DATABASE RentaFacilVehicles;
END
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'RentaFacilBookings')
BEGIN
    CREATE DATABASE RentaFacilBookings;
END
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'RentaFacilReports')
BEGIN
    CREATE DATABASE RentaFacilReports;
END
GO
```

### Sample Data Script
```sql
-- database/init/02-seed-data.sql
USE RentaFacilVehicles;
GO

-- Wait for tables to be created by Entity Framework
WAITFOR DELAY '00:00:30';
GO

-- Insert sample vehicles
IF NOT EXISTS (SELECT 1 FROM Vehicles)
BEGIN
    INSERT INTO Vehicles (Type, Model, Year, PricePerDay, Available) VALUES
    ('SUV', 'Toyota RAV4', 2023, 85.00, 1),
    ('SUV', 'Honda CR-V', 2023, 90.00, 1),
    ('Sedan', 'Toyota Camry', 2023, 75.00, 1),
    ('Sedan', 'Honda Accord', 2023, 80.00, 1),
    ('Compact', 'Nissan Versa', 2023, 55.00, 1);
END
GO
```

## Environment Configuration

### Environment Variables Configuration

#### .env File (DO NOT COMMIT - Add to .gitignore)
```bash
# .env - Local development environment variables
# WARNING: This file contains sensitive information and should NOT be committed to Git

# Database Configuration
SA_PASSWORD=YourPassword123!
MSSQL_PID=Express
CONNECTION_STRING=Server=localhost,1433;Database=RentaFacil;User Id=sa;Password=YourPassword123!;TrustServerCertificate=true;

# Service URLs
VEHICLE_SERVICE_URL=http://localhost:5001
BOOKING_SERVICE_URL=http://localhost:5002

# Application Settings
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS_VEHICLE=http://+:5001
ASPNETCORE_URLS_BOOKING=http://+:5002

# Logging Configuration
LOG_LEVEL=Information
SERILOG_MINIMUM_LEVEL=Information

# Frontend Configuration
FRONTEND_PORT=4200
API_BASE_URL=http://localhost:5002

# JWT Configuration (if implementing authentication)
JWT_SECRET_KEY=your-super-secret-jwt-key-here-32-characters-minimum
JWT_ISSUER=RentaFacil
JWT_AUDIENCE=RentaFacilUsers

# External Services (future use)
EMAIL_SERVICE_API_KEY=your-email-service-key
SMS_SERVICE_API_KEY=your-sms-service-key

# Docker Specific
COMPOSE_PROJECT_NAME=rentafacil
DOCKER_BUILDKIT=1
```

#### .env.example File (SAFE TO COMMIT - Template)
```bash
# .env.example - Environment variables template
# Copy this file to .env and update values for your local environment

# Database Configuration
SA_PASSWORD=your-secure-password-here
MSSQL_PID=Express
CONNECTION_STRING=Server=localhost,1433;Database=RentaFacil;User Id=sa;Password=your-password;TrustServerCertificate=true;

# Service URLs
VEHICLE_SERVICE_URL=http://localhost:5001
BOOKING_SERVICE_URL=http://localhost:5002

# Application Settings
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS_VEHICLE=http://+:5001
ASPNETCORE_URLS_BOOKING=http://+:5002

# Logging Configuration
LOG_LEVEL=Information
SERILOG_MINIMUM_LEVEL=Information

# Frontend Configuration
FRONTEND_PORT=4200
API_BASE_URL=http://localhost:5002

# JWT Configuration
JWT_SECRET_KEY=generate-a-secure-secret-key-32-chars-min
JWT_ISSUER=RentaFacil
JWT_AUDIENCE=RentaFacilUsers

# External Services
EMAIL_SERVICE_API_KEY=your-email-service-key
SMS_SERVICE_API_KEY=your-sms-service-key

# Docker Configuration
COMPOSE_PROJECT_NAME=rentafacil
DOCKER_BUILDKIT=1
```

#### .gitignore Requirements
```bash
# .gitignore - Add these entries to prevent committing sensitive files
.env
.env.local
.env.production
*.env
!.env.example

# Docker volumes and logs
docker-data/
logs/
*.log

# IDE and OS files
.vscode/
.idea/
.DS_Store
Thumbs.db

# Build outputs
bin/
obj/
dist/
node_modules/
```

## Docker Commands Reference

### Basic Operations
```bash
# Start all services
docker-compose up -d

# Start specific service
docker-compose up -d vehicleservice

# View logs
docker-compose logs -f
docker-compose logs -f vehicleservice

# Stop all services
docker-compose down

# Stop and remove volumes
docker-compose down -v

# Rebuild and start
docker-compose up -d --build

# Scale services
docker-compose up -d --scale vehicleservice=2
```

### Development Commands
```bash
# Start in development mode
docker-compose -f docker-compose.yml -f docker-compose.override.yml up -d

# Run database migrations
docker-compose exec vehicleservice dotnet ef database update

# Access SQL Server
docker-compose exec sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "YourPassword123!"

# View container status
docker-compose ps

# Execute commands in container
docker-compose exec vehicleservice bash
```

### Production Commands
```bash
# Deploy to production
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d

# Update specific service
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d --no-deps vehicleservice

# Monitor resources
docker stats
```

### Maintenance Commands
```bash
# Backup database
docker-compose exec sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "YourPassword123!" -Q "BACKUP DATABASE RentaFacilVehicles TO DISK='/tmp/backup.bak'"

# Clean up unused images
docker image prune -f

# Clean up unused volumes
docker volume prune -f

# Clean up everything
docker system prune -af
```

## Health Checks and Monitoring

### Health Check Endpoints
Each service exposes health check endpoints:
- VehicleService: http://localhost:5001/health
- BookingService: http://localhost:5002/health
- Frontend: http://localhost:4200 (basic HTTP check)
- SQL Server: Custom SQL query check

### Monitoring Script
```bash
#!/bin/bash
# scripts/monitor.sh

echo "Checking service health..."

services=("vehicleservice:5001" "bookingservice:5002" "frontend:80")

for service in "${services[@]}"
do
    name=${service%:*}
    port=${service#*:}
    
    if curl -f "http://localhost:$port/health" >/dev/null 2>&1; then
        echo "✅ $name is healthy"
    else
        echo "❌ $name is unhealthy"
    fi
done

# Check database
if docker-compose exec -T sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "YourPassword123!" -Q "SELECT 1" >/dev/null 2>&1; then
    echo "✅ SQL Server is healthy"
else
    echo "❌ SQL Server is unhealthy"
fi
```

## Troubleshooting

### Common Issues

#### 1. SQL Server Connection Issues
```bash
# Check if SQL Server is running
docker-compose ps sqlserver

# Check SQL Server logs
docker-compose logs sqlserver

# Test connection
docker-compose exec sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "YourPassword123!"
```

#### 2. Service Communication Issues
```bash
# Check network
docker network ls
docker network inspect rentafacil-network

# Test service connectivity
docker-compose exec vehicleservice curl -f http://bookingservice:5002/health
```

#### 3. Build Issues
```bash
# Clean build
docker-compose down
docker system prune -f
docker-compose build --no-cache
docker-compose up -d
```

#### 4. Volume Issues
```bash
# Remove volumes and recreate
docker-compose down -v
docker volume prune -f
docker-compose up -d
```

### Performance Optimization

#### 1. Resource Limits
```yaml
deploy:
  resources:
    limits:
      memory: 512M
      cpus: '0.5'
    reservations:
      memory: 256M
      cpus: '0.25'
```

#### 2. Multi-stage Build Optimization
- Use slim base images
- Minimize layers
- Use .dockerignore files
- Leverage build cache

#### 3. Production Recommendations
- Use specific image tags (not 'latest')
- Enable restart policies
- Implement proper logging
- Monitor resource usage
- Regular security updates

This Docker setup provides a complete containerization solution for the RentaFácil system with development, testing, and production configurations.
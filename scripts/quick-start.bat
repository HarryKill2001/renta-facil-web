@echo off
REM Quick Start using Docker Compose for SQL Server
REM Then start .NET services and frontend locally

echo =============================================================================
echo RentaFácil - Quick Start (Development Mode)
echo =============================================================================
echo.
echo This script will:
echo 1. Start SQL Server using Docker Compose
echo 2. Start all .NET backend services locally
echo 3. Start Angular frontend locally
echo.

REM Check prerequisites
docker-compose --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: Docker Compose is not installed or not available!
    echo Please install Docker Desktop with Docker Compose.
    pause
    exit /b 1
)

dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: .NET SDK is not installed!
    echo Please install .NET 8 SDK.
    pause
    exit /b 1
)

node --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: Node.js is not installed!
    echo Please install Node.js 18+.
    pause
    exit /b 1
)

echo Prerequisites check passed!
echo.

REM Navigate to project root
cd /d "%~dp0\.."

echo Starting SQL Server with Docker Compose...
docker-compose -f docker-compose.dev.yml up -d

if %errorlevel% neq 0 (
    echo ERROR: Failed to start SQL Server container!
    pause
    exit /b 1
)

echo SQL Server started successfully!
echo Waiting for SQL Server to be ready...
timeout /t 15 /nobreak

echo.
echo Starting .NET Backend Services...
echo.

REM Navigate to backend directory
cd backend

REM Build shared library first to avoid file locking issues
echo Building shared dependencies...
dotnet build src\Shared\RentaFacil.Shared\RentaFacil.Shared.csproj
if %errorlevel% neq 0 (
    echo ERROR: Failed to build RentaFacil.Shared!
    pause
    exit /b 1
)

echo Building complete backend solution...
dotnet build
if %errorlevel% neq 0 (
    echo ERROR: Failed to build backend solution!
    pause
    exit /b 1
)

echo Build successful! Starting services...
cd src

REM Start VehicleService
echo Starting VehicleService...
start "VehicleService" cmd /k "cd VehicleService && dotnet run --urls=http://localhost:5002"
timeout /t 3 /nobreak

REM Start BookingService
echo Starting BookingService...
start "BookingService" cmd /k "cd BookingService && dotnet run --urls=http://localhost:5257"
timeout /t 3 /nobreak

REM Start WorkerService
echo Starting WorkerService...
start "WorkerService" cmd /k "cd WorkerService && dotnet run"
timeout /t 2 /nobreak

echo Backend services are starting...
echo Waiting for services to initialize...
timeout /t 10 /nobreak

REM Go back to root and start frontend
cd ..\..

echo.
echo Installing frontend dependencies (if needed)...
if not exist "node_modules" (
    npm install
)

echo.
echo Starting Angular Frontend...
start "Frontend" cmd /k "npm start"

echo.
echo =============================================================================
echo RentaFácil Application Started Successfully!
echo =============================================================================
echo.
echo Services Status:
echo [✓] SQL Server:        localhost:1433 (Docker container)
echo [✓] VehicleService:    http://localhost:5002/swagger
echo [✓] BookingService:    http://localhost:5257/swagger
echo [✓] WorkerService:     Background service (reports)
echo [✓] Frontend:          http://localhost:4200
echo.
echo Database Credentials:
echo Username: sa
echo Password: YourStrong!Passw0rd
echo.
echo To stop all services:
echo 1. Close the .NET service windows
echo 2. Close the frontend window  
echo 3. Run: docker-compose -f docker-compose.dev.yml down
echo.
echo Opening browser to frontend...
timeout /t 3 /nobreak
start http://localhost:4200

echo.
echo Startup complete! Check the opened windows for any errors.
pause
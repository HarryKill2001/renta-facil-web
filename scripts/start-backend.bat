@echo off
REM Start all RentaFácil Backend Services
REM This script starts VehicleService, BookingService, and WorkerService in order

echo =============================================================================
echo Starting RentaFácil Backend Services
echo =============================================================================

REM Check if .NET is installed
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: .NET SDK is not installed or not in PATH!
    echo Please install .NET 8 SDK and try again.
    pause
    exit /b 1
)

REM Check if SQL Server container is running
docker ps | findstr rentafacil-sql >nul 2>&1
if %errorlevel% neq 0 (
    echo WARNING: SQL Server container (rentafacil-sql) is not running!
    echo Please run 'scripts\start-sql-server.bat' first.
    echo.
    set /p choice="Do you want to start SQL Server now? (y/n): "
    if /i "%choice%"=="y" (
        call "%~dp0start-sql-server.bat"
        echo.
        echo Continuing with backend services...
        echo.
    ) else (
        echo Exiting...
        pause
        exit /b 1
    )
)

REM Navigate to backend directory
cd /d "%~dp0..\backend\src"

REM Start VehicleService (Port 5002)
echo Starting VehicleService on port 5002...
start "VehicleService" cmd /k "cd VehicleService && echo Starting VehicleService... && dotnet run --urls=http://localhost:5002"

REM Wait a bit for VehicleService to start
timeout /t 3 /nobreak >nul

REM Start BookingService (Port 5257)
echo Starting BookingService on port 5257...
start "BookingService" cmd /k "cd BookingService && echo Starting BookingService... && dotnet run --urls=http://localhost:5257"

REM Wait a bit for BookingService to start
timeout /t 3 /nobreak >nul

REM Start WorkerService (Background service)
echo Starting WorkerService (background reports)...
start "WorkerService" cmd /k "cd WorkerService && echo Starting WorkerService... && dotnet run"

echo.
echo =============================================================================
echo All Backend Services Started!
echo =============================================================================
echo.
echo Service URLs:
echo - VehicleService:  http://localhost:5002/swagger
echo - BookingService:  http://localhost:5257/swagger
echo - WorkerService:   Background service (no web interface)
echo.
echo Database:
echo - SQL Server: localhost:1433
echo - Username: sa
echo - Password: YourStrong!Passw0rd
echo.
echo Next Steps:
echo 1. Wait for all services to fully start (check console windows)
echo 2. Test APIs using Swagger UI at the URLs above
echo 3. Start frontend using: scripts\start-frontend.bat
echo.
echo Press any key to continue...
pause >nul
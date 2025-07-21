@echo off
REM Start Complete RentaFácil Application
REM This script starts SQL Server, Backend Services, and Frontend in sequence

echo =============================================================================
echo RentaFácil - Complete Application Startup
echo =============================================================================
echo.
echo This script will start all components of the RentaFácil application:
echo 1. SQL Server (Docker container)
echo 2. VehicleService (.NET - Port 5002)
echo 3. BookingService (.NET - Port 5257)
echo 4. WorkerService (.NET - Background)
echo 5. Frontend (Angular - Port 4200)
echo.
echo Prerequisites:
echo - Docker Desktop (for SQL Server)
echo - .NET 8 SDK
echo - Node.js 18+
echo.
set /p choice="Continue with startup? (y/n): "
if /i "%choice%" neq "y" (
    echo Startup cancelled.
    pause
    exit /b 0
)

echo.
echo =============================================================================
echo Step 1: Starting SQL Server
echo =============================================================================
call "%~dp0start-sql-server.bat"
if %errorlevel% neq 0 (
    echo ERROR: Failed to start SQL Server!
    pause
    exit /b 1
)

echo.
echo =============================================================================
echo Step 2: Starting Backend Services
echo =============================================================================
call "%~dp0start-backend.bat"
if %errorlevel% neq 0 (
    echo ERROR: Failed to start backend services!
    pause
    exit /b 1
)

echo.
echo =============================================================================
echo Step 3: Waiting for backend services to initialize...
echo =============================================================================
echo Please wait while all backend services start up completely...
timeout /t 15 /nobreak

echo.
echo =============================================================================
echo Step 4: Starting Frontend
echo =============================================================================
call "%~dp0start-frontend.bat"
if %errorlevel% neq 0 (
    echo ERROR: Failed to start frontend!
    pause
    exit /b 1
)

echo.
echo =============================================================================
echo RentaFácil Application Started Successfully!
echo =============================================================================
echo.
echo Access Points:
echo - Frontend Application: http://localhost:4200
echo - VehicleService API:   http://localhost:5002/swagger
echo - BookingService API:   http://localhost:5257/swagger
echo - SQL Server:           localhost:1433 (sa/YourStrong!Passw0rd)
echo.
echo All services are now running in separate windows.
echo Close those windows to stop the services.
echo.
pause
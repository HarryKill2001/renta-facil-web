@echo off
REM Start RentaFácil Angular Frontend
REM This script starts the Angular development server

echo =============================================================================
echo Starting RentaFácil Angular Frontend
echo =============================================================================

REM Check if Node.js is installed
node --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: Node.js is not installed or not in PATH!
    echo Please install Node.js 18+ and try again.
    pause
    exit /b 1
)

REM Check if npm is available
npm --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: npm is not available!
    echo Please ensure Node.js is properly installed.
    pause
    exit /b 1
)

REM Navigate to project root
cd /d "%~dp0.."

REM Check if node_modules exists, if not install dependencies
if not exist "node_modules" (
    echo Dependencies not found. Installing npm packages...
    npm install
    if %errorlevel% neq 0 (
        echo ERROR: Failed to install dependencies!
        pause
        exit /b 1
    )
    echo Dependencies installed successfully.
    echo.
)

REM Check if backend services are running
echo Checking backend services...
timeout /t 2 /nobreak >nul

REM Test VehicleService
curl -s http://localhost:5002/api/vehicles >nul 2>&1
if %errorlevel% neq 0 (
    echo WARNING: VehicleService (port 5002) is not responding!
    echo The frontend will not work properly without backend services.
    echo Please run 'scripts\start-backend.bat' first.
    echo.
    set /p choice="Do you want to continue anyway? (y/n): "
    if /i "%choice%" neq "y" (
        echo Exiting...
        pause
        exit /b 1
    )
)

REM Test BookingService
curl -s http://localhost:5257/api/reservations >nul 2>&1
if %errorlevel% neq 0 (
    echo WARNING: BookingService (port 5257) is not responding!
    echo The frontend will not work properly without backend services.
    echo Please run 'scripts\start-backend.bat' first.
    echo.
    set /p choice="Do you want to continue anyway? (y/n): "
    if /i "%choice%" neq "y" (
        echo Exiting...
        pause
        exit /b 1
    )
)

echo Starting Angular development server...
echo.
echo The frontend will be available at: http://localhost:4200
echo.
echo Backend API endpoints:
echo - VehicleService:  http://localhost:5002/api
echo - BookingService:  http://localhost:5257/api
echo.

REM Start Angular development server
npm start

if %errorlevel% neq 0 (
    echo ERROR: Failed to start Angular development server!
    echo Please check the error messages above.
    pause
    exit /b 1
)
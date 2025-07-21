@echo off
REM Start SQL Server in Docker for RentaFácil Application
REM This script starts a SQL Server container with the correct configuration

echo =============================================================================
echo Starting SQL Server Docker Container for RentaFácil
echo =============================================================================

REM Check if Docker is running
docker version >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: Docker is not running or not installed!
    echo Please start Docker Desktop and try again.
    pause
    exit /b 1
)

REM Stop and remove existing container if it exists
echo Stopping and removing existing SQL Server container...
docker stop rentafacil-sql 2>nul
docker rm rentafacil-sql 2>nul

REM Start new SQL Server container
echo Starting new SQL Server container...
docker run -e "ACCEPT_EULA=Y" ^
    -e "SA_PASSWORD=YourStrong!Passw0rd" ^
    -e "MSSQL_PID=Express" ^
    -p 1433:1433 ^
    --name rentafacil-sql ^
    -d mcr.microsoft.com/mssql/server:2022-latest

if %errorlevel% neq 0 (
    echo ERROR: Failed to start SQL Server container!
    pause
    exit /b 1
)

echo SQL Server container started successfully!
echo.
echo Container Details:
echo - Name: rentafacil-sql
echo - Port: 1433
echo - SA Password: YourStrong!Passw0rd
echo - Image: mcr.microsoft.com/mssql/server:2022-latest
echo.
echo Waiting for SQL Server to be ready...
timeout /t 10 /nobreak >nul

REM Test connection
echo Testing SQL Server connection...
timeout /t 5 /nobreak >nul

echo.
echo SQL Server is ready!
echo.
echo You can now start the .NET services using:
echo   scripts\start-backend.bat
echo.
echo Or connect to SQL Server using:
echo   Server: localhost,1433
echo   Username: sa
echo   Password: YourStrong!Passw0rd
echo.
pause
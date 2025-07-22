#!/bin/bash

# Start all RentaFácil Backend Services
# This script starts VehicleService, BookingService, and WorkerService in order

echo "============================================================================="
echo "Starting RentaFácil Backend Services"
echo "============================================================================="

# Check if .NET is installed
if ! command -v dotnet &> /dev/null; then
    echo "ERROR: .NET SDK is not installed or not in PATH!"
    echo "Please install .NET 8 SDK and try again."
    exit 1
fi

# Check if SQL Server container is running
if ! docker ps | grep -q "rentafacil-sql"; then
    echo "WARNING: SQL Server container (rentafacil-sql) is not running!"
    echo "Please run './scripts/start-sql-server.sh' first."
    echo
    read -p "Do you want to start SQL Server now? (y/n): " choice
    if [[ "$choice" == "y" || "$choice" == "Y" ]]; then
        ./scripts/start-sql-server.sh
        echo
        echo "Continuing with backend services..."
        echo
    else
        echo "Exiting..."
        exit 1
    fi
fi

# Navigate to backend directory
cd "$(dirname "$0")/../backend"

# Build shared library first to avoid file locking issues
echo "Building shared dependencies..."
echo "- Building RentaFacil.Shared..."
dotnet build src/Shared/RentaFacil.Shared/RentaFacil.Shared.csproj
if [ $? -ne 0 ]; then
    echo "ERROR: Failed to build RentaFacil.Shared!"
    exit 1
fi

echo "- Building complete backend solution..."
dotnet build
if [ $? -ne 0 ]; then
    echo "ERROR: Failed to build backend solution!"
    exit 1
fi

echo
echo "Build successful! Starting services..."
echo

cd src

# Start VehicleService (Port 5002)
echo "Starting VehicleService on port 5002..."
cd VehicleService
gnome-terminal --title="VehicleService" -- bash -c "echo 'Starting VehicleService...'; dotnet run --urls=http://localhost:5002; exec bash" 2>/dev/null || \
xterm -title "VehicleService" -e bash -c "echo 'Starting VehicleService...'; dotnet run --urls=http://localhost:5002; exec bash" 2>/dev/null || \
osascript -e 'tell application "Terminal" to do script "cd \"'$(pwd)'\"; echo \"Starting VehicleService...\"; dotnet run --urls=http://localhost:5002"' 2>/dev/null || \
echo "Started VehicleService in background (no terminal available)"
cd ..

# Wait a bit for VehicleService to start
sleep 3

# Start BookingService (Port 5257)
echo "Starting BookingService on port 5257..."
cd BookingService
gnome-terminal --title="BookingService" -- bash -c "echo 'Starting BookingService...'; dotnet run --urls=http://localhost:5257; exec bash" 2>/dev/null || \
xterm -title "BookingService" -e bash -c "echo 'Starting BookingService...'; dotnet run --urls=http://localhost:5257; exec bash" 2>/dev/null || \
osascript -e 'tell application "Terminal" to do script "cd \"'$(pwd)'\"; echo \"Starting BookingService...\"; dotnet run --urls=http://localhost:5257"' 2>/dev/null || \
echo "Started BookingService in background (no terminal available)"
cd ..

# Wait a bit for BookingService to start
sleep 3

# Start WorkerService (Background service)
echo "Starting WorkerService (background reports)..."
cd WorkerService
gnome-terminal --title="WorkerService" -- bash -c "echo 'Starting WorkerService...'; dotnet run; exec bash" 2>/dev/null || \
xterm -title "WorkerService" -e bash -c "echo 'Starting WorkerService...'; dotnet run; exec bash" 2>/dev/null || \
osascript -e 'tell application "Terminal" to do script "cd \"'$(pwd)'\"; echo \"Starting WorkerService...\"; dotnet run"' 2>/dev/null || \
echo "Started WorkerService in background (no terminal available)"

echo
echo "============================================================================="
echo "All Backend Services Started!"
echo "============================================================================="
echo
echo "Service URLs:"
echo "- VehicleService:  http://localhost:5002/swagger"
echo "- BookingService:  http://localhost:5257/swagger"
echo "- WorkerService:   Background service (no web interface)"
echo
echo "Database:"
echo "- SQL Server: localhost:1433"
echo "- Username: sa"
echo "- Password: YourStrong!Passw0rd"
echo
echo "Next Steps:"
echo "1. Wait for all services to fully start (check terminal windows)"
echo "2. Test APIs using Swagger UI at the URLs above"
echo "3. Start frontend using: ./scripts/start-frontend.sh"
echo
echo "Press any key to continue..."
read -n 1
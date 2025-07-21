#!/bin/bash

# Quick Start using Docker Compose for SQL Server
# Then start .NET services and frontend locally

echo "============================================================================="
echo "RentaFácil - Quick Start (Development Mode)"
echo "============================================================================="
echo
echo "This script will:"
echo "1. Start SQL Server using Docker Compose"
echo "2. Start all .NET backend services locally"
echo "3. Start Angular frontend locally"
echo

# Check prerequisites
if ! command -v docker-compose &> /dev/null; then
    echo "ERROR: Docker Compose is not installed or not available!"
    echo "Please install Docker Desktop with Docker Compose."
    exit 1
fi

if ! command -v dotnet &> /dev/null; then
    echo "ERROR: .NET SDK is not installed!"
    echo "Please install .NET 8 SDK."
    exit 1
fi

if ! command -v node &> /dev/null; then
    echo "ERROR: Node.js is not installed!"
    echo "Please install Node.js 18+."
    exit 1
fi

echo "Prerequisites check passed!"
echo

# Navigate to project root
cd "$(dirname "$0")/.."

echo "Starting SQL Server with Docker Compose..."
docker-compose -f docker-compose.dev.yml up -d

if [ $? -ne 0 ]; then
    echo "ERROR: Failed to start SQL Server container!"
    exit 1
fi

echo "SQL Server started successfully!"
echo "Waiting for SQL Server to be ready..."
sleep 15

echo
echo "Starting .NET Backend Services..."
echo

# Navigate to backend directory
cd backend/src

# Start VehicleService
echo "Starting VehicleService..."
cd VehicleService
gnome-terminal --title="VehicleService" -- bash -c "dotnet run --urls=http://localhost:5002; exec bash" 2>/dev/null || \
xterm -title "VehicleService" -e bash -c "dotnet run --urls=http://localhost:5002; exec bash" 2>/dev/null || \
osascript -e 'tell application "Terminal" to do script "cd \"'$(pwd)'\"; dotnet run --urls=http://localhost:5002"' 2>/dev/null || \
(echo "Starting VehicleService in background..."; nohup dotnet run --urls=http://localhost:5002 > /dev/null 2>&1 &)
cd ..
sleep 3

# Start BookingService
echo "Starting BookingService..."
cd BookingService
gnome-terminal --title="BookingService" -- bash -c "dotnet run --urls=http://localhost:5257; exec bash" 2>/dev/null || \
xterm -title "BookingService" -e bash -c "dotnet run --urls=http://localhost:5257; exec bash" 2>/dev/null || \
osascript -e 'tell application "Terminal" to do script "cd \"'$(pwd)'\"; dotnet run --urls=http://localhost:5257"' 2>/dev/null || \
(echo "Starting BookingService in background..."; nohup dotnet run --urls=http://localhost:5257 > /dev/null 2>&1 &)
cd ..
sleep 3

# Start WorkerService
echo "Starting WorkerService..."
cd WorkerService
gnome-terminal --title="WorkerService" -- bash -c "dotnet run; exec bash" 2>/dev/null || \
xterm -title "WorkerService" -e bash -c "dotnet run; exec bash" 2>/dev/null || \
osascript -e 'tell application "Terminal" to do script "cd \"'$(pwd)'\"; dotnet run"' 2>/dev/null || \
(echo "Starting WorkerService in background..."; nohup dotnet run > /dev/null 2>&1 &)
cd ..

echo "Backend services are starting..."
echo "Waiting for services to initialize..."
sleep 10

# Go back to root and start frontend
cd ../..

echo
echo "Installing frontend dependencies (if needed)..."
if [ ! -d "node_modules" ]; then
    npm install
fi

echo
echo "Starting Angular Frontend..."
gnome-terminal --title="Frontend" -- bash -c "npm start; exec bash" 2>/dev/null || \
xterm -title "Frontend" -e bash -c "npm start; exec bash" 2>/dev/null || \
osascript -e 'tell application "Terminal" to do script "cd \"'$(pwd)'\"; npm start"' 2>/dev/null || \
(echo "Starting frontend in background..."; npm start > /dev/null 2>&1 &)

echo
echo "============================================================================="
echo "RentaFácil Application Started Successfully!"
echo "============================================================================="
echo
echo "Services Status:"
echo "[✓] SQL Server:        localhost:1433 (Docker container)"
echo "[✓] VehicleService:    http://localhost:5002/swagger"
echo "[✓] BookingService:    http://localhost:5257/swagger"
echo "[✓] WorkerService:     Background service (reports)"
echo "[✓] Frontend:          http://localhost:4200"
echo
echo "Database Credentials:"
echo "Username: sa"
echo "Password: YourStrong!Passw0rd"
echo
echo "To stop all services:"
echo "1. Close the .NET service terminals"
echo "2. Close the frontend terminal"
echo "3. Run: docker-compose -f docker-compose.dev.yml down"
echo
echo "Opening browser to frontend..."
sleep 3

# Try to open browser
if command -v xdg-open &> /dev/null; then
    xdg-open http://localhost:4200 2>/dev/null &
elif command -v open &> /dev/null; then
    open http://localhost:4200 2>/dev/null &
elif command -v firefox &> /dev/null; then
    firefox http://localhost:4200 2>/dev/null &
else
    echo "Please open http://localhost:4200 in your browser manually."
fi

echo
echo "Startup complete! Check the opened terminals for any errors."
#!/bin/bash

# Start Complete RentaFácil Application
# This script starts SQL Server, Backend Services, and Frontend in sequence

echo "============================================================================="
echo "RentaFácil - Complete Application Startup"
echo "============================================================================="
echo
echo "This script will start all components of the RentaFácil application:"
echo "1. SQL Server (Docker container)"
echo "2. VehicleService (.NET - Port 5002)"
echo "3. BookingService (.NET - Port 5257)"
echo "4. WorkerService (.NET - Background)"
echo "5. Frontend (Angular - Port 4200)"
echo
echo "Prerequisites:"
echo "- Docker (for SQL Server)"
echo "- .NET 8 SDK"
echo "- Node.js 18+"
echo
read -p "Continue with startup? (y/n): " choice
if [[ "$choice" != "y" && "$choice" != "Y" ]]; then
    echo "Startup cancelled."
    exit 0
fi

echo
echo "============================================================================="
echo "Step 1: Starting SQL Server"
echo "============================================================================="
./scripts/start-sql-server.sh
if [ $? -ne 0 ]; then
    echo "ERROR: Failed to start SQL Server!"
    exit 1
fi

echo
echo "============================================================================="
echo "Step 2: Starting Backend Services"
echo "============================================================================="
./scripts/start-backend.sh
if [ $? -ne 0 ]; then
    echo "ERROR: Failed to start backend services!"
    exit 1
fi

echo
echo "============================================================================="
echo "Step 3: Waiting for backend services to initialize..."
echo "============================================================================="
echo "Please wait while all backend services start up completely..."
sleep 15

echo
echo "============================================================================="
echo "Step 4: Starting Frontend"
echo "============================================================================="
./scripts/start-frontend.sh
if [ $? -ne 0 ]; then
    echo "ERROR: Failed to start frontend!"
    exit 1
fi

echo
echo "============================================================================="
echo "RentaFácil Application Started Successfully!"
echo "============================================================================="
echo
echo "Access Points:"
echo "- Frontend Application: http://localhost:4200"
echo "- VehicleService API:   http://localhost:5002/swagger"
echo "- BookingService API:   http://localhost:5257/swagger"
echo "- SQL Server:           localhost:1433 (sa/YourStrong!Passw0rd)"
echo
echo "All services are now running in separate terminals."
echo "Close those terminals to stop the services."
echo
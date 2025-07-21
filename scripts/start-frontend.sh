#!/bin/bash

# Start RentaFácil Angular Frontend
# This script starts the Angular development server

echo "============================================================================="
echo "Starting RentaFácil Angular Frontend"
echo "============================================================================="

# Check if Node.js is installed
if ! command -v node &> /dev/null; then
    echo "ERROR: Node.js is not installed or not in PATH!"
    echo "Please install Node.js 18+ and try again."
    exit 1
fi

# Check if npm is available
if ! command -v npm &> /dev/null; then
    echo "ERROR: npm is not available!"
    echo "Please ensure Node.js is properly installed."
    exit 1
fi

# Navigate to project root
cd "$(dirname "$0")/.."

# Check if node_modules exists, if not install dependencies
if [ ! -d "node_modules" ]; then
    echo "Dependencies not found. Installing npm packages..."
    npm install
    if [ $? -ne 0 ]; then
        echo "ERROR: Failed to install dependencies!"
        exit 1
    fi
    echo "Dependencies installed successfully."
    echo
fi

# Check if backend services are running
echo "Checking backend services..."
sleep 2

# Test VehicleService
if ! curl -s http://localhost:5002/api/vehicles > /dev/null 2>&1; then
    echo "WARNING: VehicleService (port 5002) is not responding!"
    echo "The frontend will not work properly without backend services."
    echo "Please run './scripts/start-backend.sh' first."
    echo
    read -p "Do you want to continue anyway? (y/n): " choice
    if [[ "$choice" != "y" && "$choice" != "Y" ]]; then
        echo "Exiting..."
        exit 1
    fi
fi

# Test BookingService
if ! curl -s http://localhost:5257/api/reservations > /dev/null 2>&1; then
    echo "WARNING: BookingService (port 5257) is not responding!"
    echo "The frontend will not work properly without backend services."
    echo "Please run './scripts/start-backend.sh' first."
    echo
    read -p "Do you want to continue anyway? (y/n): " choice
    if [[ "$choice" != "y" && "$choice" != "Y" ]]; then
        echo "Exiting..."
        exit 1
    fi
fi

echo "Starting Angular development server..."
echo
echo "The frontend will be available at: http://localhost:4200"
echo
echo "Backend API endpoints:"
echo "- VehicleService:  http://localhost:5002/api"
echo "- BookingService:  http://localhost:5257/api"
echo

# Start Angular development server
npm start

if [ $? -ne 0 ]; then
    echo "ERROR: Failed to start Angular development server!"
    echo "Please check the error messages above."
    exit 1
fi
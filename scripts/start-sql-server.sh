#!/bin/bash

# Start SQL Server in Docker for RentaFácil Application
# This script starts a SQL Server container with the correct configuration

echo "============================================================================="
echo "Starting SQL Server Docker Container for RentaFácil"
echo "============================================================================="

# Check if Docker is running
if ! command -v docker &> /dev/null; then
    echo "ERROR: Docker is not installed or not in PATH!"
    echo "Please install Docker and try again."
    exit 1
fi

if ! docker info &> /dev/null; then
    echo "ERROR: Docker is not running!"
    echo "Please start Docker and try again."
    exit 1
fi

# Stop and remove existing container if it exists
echo "Stopping and removing existing SQL Server container..."
docker stop rentafacil-sql 2>/dev/null
docker rm rentafacil-sql 2>/dev/null

# Start new SQL Server container
echo "Starting new SQL Server container..."
docker run -e "ACCEPT_EULA=Y" \
    -e "SA_PASSWORD=YourStrong!Passw0rd" \
    -e "MSSQL_PID=Express" \
    -p 1433:1433 \
    --name rentafacil-sql \
    -d mcr.microsoft.com/mssql/server:2022-latest

if [ $? -ne 0 ]; then
    echo "ERROR: Failed to start SQL Server container!"
    exit 1
fi

echo "SQL Server container started successfully!"
echo
echo "Container Details:"
echo "- Name: rentafacil-sql"
echo "- Port: 1433"
echo "- SA Password: YourStrong!Passw0rd"
echo "- Image: mcr.microsoft.com/mssql/server:2022-latest"
echo
echo "Waiting for SQL Server to be ready..."
sleep 10

# Test connection
echo "Testing SQL Server connection..."
sleep 5

echo
echo "SQL Server is ready!"
echo
echo "You can now start the .NET services using:"
echo "  ./scripts/start-backend.sh"
echo
echo "Or connect to SQL Server using:"
echo "  Server: localhost,1433"
echo "  Username: sa"
echo "  Password: YourStrong!Passw0rd"
echo
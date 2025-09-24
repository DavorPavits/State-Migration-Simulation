#!/bin/bash

echo = "Container State Migration Simulation Setup"

# Stop and remove any old containers to ensure a clean start
echo "Cleaning up previous run..."
docker-compose down

#Build the Docker image
echo "Build Docker Image"
#docker build -t state-migration-sim .
docker-compose build 

#---Stage 1: Run on Host A----
echo ""
echo "Starting application on container-host-a..."
docker-compose up -d container-host-a

echo "Simulating work on host-a for 15 seconds"
sleep 15
echo "Logs from host-a"
docker logs container-host-a

# --- STAGE 2: Migrate to Host B ---
echo ""
echo "Stopping container-host-a gracefully to save state..."
# 'stop' sends a SIGTERM, which our C# app handles for a graceful shutdown.
docker-compose stop container-host-a

echo "Migrating... Starting application on container-host-b..."
docker-compose up -d container-host-b

echo "Allowing host-b to start and process..."
sleep 5

echo "Logs from host-b (should show resumed state):"
docker logs container-host-b

# #Create the project structure if it doesn't exist
# if [ ! -f "StateMigrationApp.cs" ]; then
#     echo "Creating StateMigrationApp file"
# fi

# if [ ! -f "StateMigratiionSimulation.csproj" ]; then
#     echo "Creating project file..."
# fi

# echo "Starting containers..."
# docker-compose up -d

# echo "Waiting for containers to be ready..."
# sleep 5

# echo "Running migration simulation..."
# docker exec -it migration-simulator dotnet StateMigrationSimulation.dll

# echo ""
# echo "Container Status"
# docker ps --format "table{{.Names}}\t{{.Status}}\t{{.Ports}}"

# echo ""
# echo "To view logs from a specific container:"
# echo "docker logs container-host-a"
# echo "docker logs container-host-b" 
# echo "docker logs container-host-c"

# echo ""
# echo "To stop the simulation:"
# echo "docker-compose down"

# echo ""
# echo "To view shared state volume:"
# echo "docker exec -it migration-simulator ls -la /app/snapshots"
# PowerShell script to build and run the MarkItDown server Docker image

# Stop and remove existing container if it exists
$containerName = "markitdown-server"
if (docker ps -a --format "table {{.Names}}" | Select-String -Pattern "^${containerName}$") {
    Write-Host "Stopping and removing existing container: $containerName"
    docker stop $containerName
    docker rm $containerName
}

# Build the Docker image
Write-Host "Building Docker image: markitdown-server"
docker build --no-cache -t markitdown-server .

# Run the container
Write-Host "Starting container: $containerName"
docker run -d --name $containerName -p 8000:8000 markitdown-server

# Wait a moment for the container to start
Start-Sleep -Seconds 5

# Check if the container is running
$running = docker ps --format "table {{.Names}}" | Select-String -Pattern "^${containerName}$"
if ($running) {
    Write-Host "Container $containerName is running successfully!"
    Write-Host "Server should be available at http://localhost:8000"
    Write-Host "Health check: http://localhost:8000/health"
} else {
    Write-Host "Failed to start container. Checking logs..."
    docker logs $containerName
}
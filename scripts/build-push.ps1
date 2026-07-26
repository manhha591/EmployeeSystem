param(
  [Parameter(Mandatory=$true)]
  [string]$DockerUser,
  [string]$Tag = "latest"
)

$ErrorActionPreference = "Stop"

Write-Host "=== Build & Push Docker images ===" -ForegroundColor Cyan

# 1. Publish .NET API
Write-Host "[1/4] Publishing .NET API..." -ForegroundColor Yellow
Remove-Item -Recurse -Force "docker-publish" -ErrorAction SilentlyContinue
dotnet publish EmployeeManagement.API/EmployeeManagement.API.csproj -c Release -o docker-publish

# 2. Build API image
Write-Host "[2/4] Building API image..." -ForegroundColor Yellow
docker build -t "$DockerUser/employee-api:$Tag" .

# 3. Build frontend image
Write-Host "[3/4] Building frontend image..." -ForegroundColor Yellow
docker build -t "$DockerUser/employee-frontend:$Tag" ./frontend

# 4. Push to Docker Hub
Write-Host "[4/4] Pushing to Docker Hub..." -ForegroundColor Yellow
docker push "$DockerUser/employee-api:$Tag"
docker push "$DockerUser/employee-frontend:$Tag"

Write-Host "=== Done! ===" -ForegroundColor Green
Write-Host "Images:"
Write-Host "  $DockerUser/employee-api:$Tag"
Write-Host "  $DockerUser/employee-frontend:$Tag"

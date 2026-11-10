
$ErrorActionPreference = "Stop"

Write-Host "====================================" -ForegroundColor Cyan
Write-Host " Employee Leave Management System" -ForegroundColor Cyan
Write-Host " Starting backend services..." -ForegroundColor Cyan
Write-Host "====================================" -ForegroundColor Cyan

if (-not (Test-Path ".env")) {
    Write-Host "ERROR: .env file not found." -ForegroundColor Red
    Write-Host "Copy .env.example to .env and configure your secrets."
    exit 1
}

Write-Host "`nValidating Docker Compose configuration..." -ForegroundColor Yellow
docker compose config --quiet

if ($LASTEXITCODE -ne 0) {
    throw "Docker Compose configuration is invalid."
}

Write-Host "`nBuilding and starting backend services..." -ForegroundColor Yellow
docker compose up --build -d

if ($LASTEXITCODE -ne 0) {
    throw "Failed to start backend services."
}

Write-Host "`nBackend startup command completed." -ForegroundColor Green
Write-Host "`nService URLs:"
Write-Host "EmployeeService: http://localhost:5076"
Write-Host "LeaveService:    http://localhost:5080"

Write-Host "`nContainer status:" -ForegroundColor Cyan
docker compose ps

Write-Host "`nTo view logs: docker compose logs -f"
Write-Host "To stop:      .\stop-backend.ps1"

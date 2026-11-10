
$ErrorActionPreference = "Stop"

Write-Host "Stopping Employee Leave Management backend..." -ForegroundColor Yellow

docker compose down

if ($LASTEXITCODE -ne 0) {
    throw "Failed to stop backend services."
}

Write-Host "Backend services stopped." -ForegroundColor Green
Write-Host "Database data is preserved in the Docker volume."

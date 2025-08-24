# 🧵 Custom Tailoring Management System - Quick Start
# Double-click this file to start the application

Write-Host ""
Write-Host "🧵 Starting Tailoring Management System..." -ForegroundColor Cyan
Write-Host ""

# Add Docker to PATH
$env:PATH += ";C:\Program Files\Docker\Docker\resources\bin"

# Start Docker Desktop if not running
Start-Process "C:\Program Files\Docker\Docker\Docker Desktop.exe" -WindowStyle Hidden -ErrorAction SilentlyContinue

# Wait for Docker and start application
Write-Host "⏳ Starting Docker and building application..." -ForegroundColor Yellow
Start-Sleep 15
docker compose up --build

Write-Host ""
Write-Host "🎉 Application should now be running at:" -ForegroundColor Green
Write-Host "   Frontend: http://localhost:3000" -ForegroundColor White
Write-Host "   API Docs: http://localhost:5000/swagger" -ForegroundColor White
Write-Host ""
Write-Host "Login credentials:" -ForegroundColor Yellow
Write-Host "   Username: admin" -ForegroundColor White
Write-Host "   Password: admin123" -ForegroundColor White
Write-Host ""
Read-Host "Press Enter to exit"

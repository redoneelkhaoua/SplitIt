@echo off
echo.
echo ============================================================
echo 🧵 Custom Tailoring Management System - Quick Start
echo ============================================================
echo.

REM Add Docker to PATH if it exists
if exist "C:\Program Files\Docker\Docker\resources\bin\docker.exe" (
    set "PATH=%PATH%;C:\Program Files\Docker\Docker\resources\bin"
    echo ✓ Docker found, adding to PATH...
) else (
    echo ❌ Docker not found. Please install Docker Desktop.
    echo Download from: https://www.docker.com/products/docker-desktop/
    pause
    exit /b 1
)

echo.
echo 🔍 Checking Docker status...
docker version >nul 2>&1
if %errorlevel% neq 0 (
    echo ⚠️  Docker is not running. Starting Docker Desktop...
    start "" "C:\Program Files\Docker\Docker\Docker Desktop.exe"
    echo ⏳ Waiting for Docker to start (this may take 1-2 minutes)...
    
    :wait_loop
    timeout /t 10 /nobreak >nul
    docker version >nul 2>&1
    if %errorlevel% neq 0 (
        echo    Still waiting for Docker...
        goto wait_loop
    )
)

echo ✓ Docker is ready!
echo.

echo 🚀 Starting the Tailoring Management System...
echo    This will take 2-3 minutes to build and start all services.
echo.

docker compose up --build

echo.
echo 🎉 Application should now be running at:
echo    Frontend: http://localhost:3000
echo    API Docs: http://localhost:5000/swagger
echo.
echo Login credentials:
echo    Username: admin
echo    Password: admin123
echo.
pause

# 🔍 TailoringApp Deployment Verification Script (Windows PowerShell)
# This script checks if your Windows system is ready for deployment

Write-Host "🧵 Custom Tailoring Management System - Deployment Checker" -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host ""

# Function to print colored output
function Print-Status {
    param(
        [string]$Message,
        [string]$Status
    )
    
    switch ($Status) {
        "OK" { 
            Write-Host "✓ $Message" -ForegroundColor Green 
        }
        "WARN" { 
            Write-Host "⚠ $Message" -ForegroundColor Yellow 
        }
        "FAIL" { 
            Write-Host "✗ $Message" -ForegroundColor Red 
        }
        default { 
            Write-Host "ℹ $Message" -ForegroundColor Blue 
        }
    }
}

# Check if Docker is installed
function Check-Docker {
    Write-Host "🐳 Checking Docker Installation..." -ForegroundColor Cyan
    
    try {
        $dockerVersion = docker --version 2>$null
        if ($dockerVersion) {
            Print-Status "Docker is installed: $dockerVersion" "OK"
            
            # Check if Docker daemon is running
            try {
                docker info 2>$null | Out-Null
                Print-Status "Docker daemon is running" "OK"
                return $true
            }
            catch {
                Print-Status "Docker daemon is not running. Please start Docker Desktop." "FAIL"
                return $false
            }
        }
        else {
            Print-Status "Docker is not installed. Please install Docker Desktop." "FAIL"
            Write-Host "  💡 Download from: https://www.docker.com/products/docker-desktop/" -ForegroundColor Yellow
            return $false
        }
    }
    catch {
        Print-Status "Docker is not installed. Please install Docker Desktop." "FAIL"
        Write-Host "  💡 Download from: https://www.docker.com/products/docker-desktop/" -ForegroundColor Yellow
        return $false
    }
}

# Check if Docker Compose is available
function Check-DockerCompose {
    Write-Host ""
    Write-Host "🔧 Checking Docker Compose..." -ForegroundColor Cyan
    
    try {
        $composeVersion = docker compose version 2>$null
        if ($composeVersion) {
            Print-Status "Docker Compose is available: $composeVersion" "OK"
            return $true
        }
        else {
            # Try legacy docker-compose
            $legacyVersion = docker-compose --version 2>$null
            if ($legacyVersion) {
                Print-Status "Docker Compose (legacy) is available: $legacyVersion" "WARN"
                Print-Status "Consider upgrading to Docker Compose V2" "WARN"
                return $true
            }
            else {
                Print-Status "Docker Compose is not available" "FAIL"
                return $false
            }
        }
    }
    catch {
        Print-Status "Docker Compose is not available" "FAIL"
        return $false
    }
}

# Check system resources
function Check-Resources {
    Write-Host ""
    Write-Host "💻 Checking System Resources..." -ForegroundColor Cyan
    
    # Check available disk space
    try {
        $diskFilter = "DeviceID='C:'"
        $disk = Get-WmiObject -Class Win32_LogicalDisk -Filter $diskFilter
        $freeSpaceGB = [math]::Round($disk.FreeSpace / 1GB, 1)
        $totalSpaceGB = [math]::Round($disk.Size / 1GB, 1)
        Print-Status "C: Drive - $freeSpaceGB GB available of $totalSpaceGB GB total" "INFO"
    }
    catch {
        Print-Status "Could not check disk space" "WARN"
    }
    
    # Check memory
    try {
        $memory = Get-WmiObject -Class Win32_ComputerSystem
        $totalMemoryGB = [math]::Round($memory.TotalPhysicalMemory / 1GB, 1)
        Print-Status "Total RAM: $totalMemoryGB GB" "INFO"
        
        if ($totalMemoryGB -lt 4) {
            Print-Status "Recommended: 4GB+ RAM for optimal performance" "WARN"
        }
    }
    catch {
        Print-Status "Could not check memory information" "WARN"
    }
}

# Check network connectivity
function Check-Network {
    Write-Host ""
    Write-Host "🌐 Checking Network Connectivity..." -ForegroundColor Cyan
    
    try {
        $response = Invoke-WebRequest -Uri "https://hub.docker.com" -TimeoutSec 5 -UseBasicParsing
        if ($response.StatusCode -eq 200) {
            Print-Status "Docker Hub is reachable" "OK"
        }
        else {
            Print-Status "Docker Hub returned status: $($response.StatusCode)" "WARN"
        }
    }
    catch {
        Print-Status "Cannot reach Docker Hub. Check your internet connection." "WARN"
    }
}

# Check ports availability
function Check-Ports {
    Write-Host ""
    Write-Host "🔌 Checking Port Availability..." -ForegroundColor Cyan
    
    $ports = @(
        @{Port = 3000; Name = "Frontend"},
        @{Port = 5000; Name = "API"},
        @{Port = 1433; Name = "Database"}
    )
    
    foreach ($portInfo in $ports) {
        $port = $portInfo.Port
        $name = $portInfo.Name
        
        try {
            $connections = Get-NetTCPConnection -LocalPort $port -ErrorAction SilentlyContinue
            if ($connections) {
                Print-Status "Port $port ($name) is in use" "WARN"
            }
            else {
                Print-Status "Port $port ($name) is available" "OK"
            }
        }
        catch {
            # Try alternative method with netstat
            try {
                $netstat = netstat -an | Select-String ":$port "
                if ($netstat) {
                    Print-Status "Port $port ($name) is in use" "WARN"
                }
                else {
                    Print-Status "Port $port ($name) is available" "OK"
                }
            }
            catch {
                Print-Status "Cannot check port $port ($name)" "WARN"
            }
        }
    }
    
    Write-Host ""
    Print-Status "Note: Ports in use will be automatically freed when you stop conflicting services" "INFO"
}

# Check project structure
function Check-ProjectStructure {
    Write-Host ""
    Write-Host "📁 Checking Project Structure..." -ForegroundColor Cyan
    
    $requiredFiles = @(
        "docker-compose.yml",
        "backend\Dockerfile",
        "frontend\Dockerfile",
        "frontend\nginx.conf"
    )
    
    foreach ($file in $requiredFiles) {
        if (Test-Path $file) {
            Print-Status "$file exists" "OK"
        }
        else {
            Print-Status "$file is missing" "FAIL"
        }
    }
}

# Main execution
function Main {
    $dockerOK = Check-Docker
    $composeOK = Check-DockerCompose
    
    Check-Resources
    Check-Network
    Check-Ports
    Check-ProjectStructure
    
    Write-Host ""
    Write-Host "📋 Summary" -ForegroundColor Cyan
    Write-Host "==========" -ForegroundColor Cyan
    
    if ($dockerOK -and $composeOK) {
        Print-Status "System is ready for deployment! 🚀" "OK"
        Write-Host ""
        Write-Host "Next steps:" -ForegroundColor Yellow
        Write-Host "1. Run: docker compose up --build" -ForegroundColor White
        Write-Host "2. Wait for all services to start (2-3 minutes)" -ForegroundColor White
        Write-Host "3. Visit: http://localhost:3000" -ForegroundColor White
        Write-Host "4. Login with: admin / admin123" -ForegroundColor White
        Write-Host ""
        Write-Host "For detailed deployment instructions, see DEPLOYMENT.md" -ForegroundColor Blue
    }
    else {
        Print-Status "System requires setup before deployment" "FAIL"
        Write-Host ""
        Write-Host "Please install the missing requirements and run this script again." -ForegroundColor Yellow
        Write-Host ""
        Write-Host "Installation links:" -ForegroundColor Yellow
        Write-Host "• Docker Desktop: https://www.docker.com/products/docker-desktop/" -ForegroundColor White
    }
}

# Run the main function
Main

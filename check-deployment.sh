#!/bin/bash

# 🔍 TailoringApp Deployment Verification Script
# This script checks if your system is ready for deployment

echo "🧵 Custom Tailoring Management System - Deployment Checker"
echo "============================================================"
echo ""

# Color codes for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to print colored output
print_status() {
    if [ "$2" = "OK" ]; then
        echo -e "${GREEN}✓${NC} $1"
    elif [ "$2" = "WARN" ]; then
        echo -e "${YELLOW}⚠${NC} $1"
    elif [ "$2" = "FAIL" ]; then
        echo -e "${RED}✗${NC} $1"
    else
        echo -e "${BLUE}ℹ${NC} $1"
    fi
}

# Check if Docker is installed
check_docker() {
    echo "🐳 Checking Docker Installation..."
    
    if command -v docker &> /dev/null; then
        DOCKER_VERSION=$(docker --version)
        print_status "Docker is installed: $DOCKER_VERSION" "OK"
        
        # Check if Docker daemon is running
        if docker info &> /dev/null; then
            print_status "Docker daemon is running" "OK"
        else
            print_status "Docker daemon is not running. Please start Docker Desktop." "FAIL"
            return 1
        fi
    else
        print_status "Docker is not installed. Please install Docker Desktop." "FAIL"
        echo "  💡 Download from: https://www.docker.com/products/docker-desktop/"
        return 1
    fi
}

# Check if Docker Compose is available
check_docker_compose() {
    echo ""
    echo "🔧 Checking Docker Compose..."
    
    if docker compose version &> /dev/null; then
        COMPOSE_VERSION=$(docker compose version)
        print_status "Docker Compose is available: $COMPOSE_VERSION" "OK"
    elif command -v docker-compose &> /dev/null; then
        COMPOSE_VERSION=$(docker-compose --version)
        print_status "Docker Compose (legacy) is available: $COMPOSE_VERSION" "WARN"
        print_status "Consider upgrading to Docker Compose V2" "WARN"
    else
        print_status "Docker Compose is not available" "FAIL"
        return 1
    fi
}

# Check system resources
check_resources() {
    echo ""
    echo "💻 Checking System Resources..."
    
    # Check available disk space (requires different commands on different OS)
    if command -v df &> /dev/null; then
        DISK_USAGE=$(df -h . | awk 'NR==2{print $4}')
        print_status "Available disk space: $DISK_USAGE" "INFO"
    fi
    
    # Check memory (Linux/macOS)
    if command -v free &> /dev/null; then
        MEMORY=$(free -h | awk 'NR==2{printf "%.1fG available of %.1fG total", $7/1024, $2/1024}')
        print_status "Memory: $MEMORY" "INFO"
    elif command -v vm_stat &> /dev/null; then
        # macOS memory check
        print_status "Memory check available on macOS" "INFO"
    fi
}

# Check network connectivity
check_network() {
    echo ""
    echo "🌐 Checking Network Connectivity..."
    
    # Check if we can reach Docker Hub
    if curl -s --max-time 5 https://hub.docker.com > /dev/null; then
        print_status "Docker Hub is reachable" "OK"
    else
        print_status "Cannot reach Docker Hub. Check your internet connection." "WARN"
    fi
}

# Check ports availability
check_ports() {
    echo ""
    echo "🔌 Checking Port Availability..."
    
    ports=(3000 5000 1433)
    port_names=("Frontend" "API" "Database")
    
    for i in "${!ports[@]}"; do
        port=${ports[$i]}
        name=${port_names[$i]}
        
        if command -v netstat &> /dev/null; then
            if netstat -ln | grep ":$port " > /dev/null; then
                print_status "Port $port ($name) is in use" "WARN"
            else
                print_status "Port $port ($name) is available" "OK"
            fi
        elif command -v lsof &> /dev/null; then
            if lsof -i :$port > /dev/null 2>&1; then
                print_status "Port $port ($name) is in use" "WARN"
            else
                print_status "Port $port ($name) is available" "OK"
            fi
        else
            print_status "Cannot check port $port ($name) - no netstat/lsof available" "WARN"
        fi
    done
    
    echo ""
    print_status "Note: Ports in use will be automatically freed when you stop conflicting services" "INFO"
}

# Check project structure
check_project_structure() {
    echo ""
    echo "📁 Checking Project Structure..."
    
    required_files=(
        "docker-compose.yml"
        "backend/Dockerfile"
        "frontend/Dockerfile"
        "frontend/nginx.conf"
    )
    
    for file in "${required_files[@]}"; do
        if [ -f "$file" ]; then
            print_status "$file exists" "OK"
        else
            print_status "$file is missing" "FAIL"
        fi
    done
}

# Main execution
main() {
    check_docker
    DOCKER_OK=$?
    
    check_docker_compose
    COMPOSE_OK=$?
    
    check_resources
    check_network
    check_ports
    check_project_structure
    
    echo ""
    echo "📋 Summary"
    echo "=========="
    
    if [ $DOCKER_OK -eq 0 ] && [ $COMPOSE_OK -eq 0 ]; then
        print_status "System is ready for deployment! 🚀" "OK"
        echo ""
        echo "Next steps:"
        echo "1. Run: docker compose up --build"
        echo "2. Wait for all services to start (2-3 minutes)"
        echo "3. Visit: http://localhost:3000"
        echo "4. Login with: admin / admin123"
        echo ""
        echo "For detailed deployment instructions, see DEPLOYMENT.md"
    else
        print_status "System requires setup before deployment" "FAIL"
        echo ""
        echo "Please install the missing requirements and run this script again."
    fi
}

# Run the main function
main

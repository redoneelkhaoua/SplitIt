# 🚀 Deployment Guide

This document provides detailed instructions for deploying the Custom Tailoring Management System in various environments.

## 📋 Prerequisites

### System Requirements
- **RAM**: 4GB minimum, 8GB recommended
- **Storage**: 10GB free space
- **Network**: Internet connection for Docker image downloads

### Software Requirements
- **Docker**: Version 20.10 or later
- **Docker Compose**: Version 2.0 or later

## 🐳 Docker Installation

### Windows
1. Download Docker Desktop from https://www.docker.com/products/docker-desktop/
2. Run the installer and follow the setup wizard
3. Restart your computer when prompted
4. Verify installation:
   ```powershell
   docker --version
   docker compose version
   ```

### macOS
```bash
# Install via Homebrew
brew install --cask docker

# Or download from https://www.docker.com/products/docker-desktop/
```

### Linux (Ubuntu/Debian)
```bash
# Update package index
sudo apt-get update

# Install Docker
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh

# Install Docker Compose
sudo apt-get install docker-compose-plugin

# Add user to docker group
sudo usermod -aG docker $USER
```

## 🚀 Production Deployment

### 1. Quick Start
```bash
# Clone the repository
git clone https://github.com/redoneelkhaoua/SplitIt.git
cd SplitIt

# Start all services
docker compose up --build -d
```

### 2. Verify Services
After deployment, check that all services are running:

```bash
# Check container status
docker compose ps

# View logs
docker compose logs -f
```

Expected output:
```
NAME                  COMMAND                  SERVICE     STATUS       PORTS
splitit-api-1         "dotnet TailoringApp…"   api         running      0.0.0.0:5000->5000/tcp
splitit-db-1          "/opt/mssql/bin/perm…"   db          running      0.0.0.0:1433->1433/tcp
splitit-frontend-1    "/docker-entrypoint.…"   frontend    running      0.0.0.0:3000->80/tcp
```

### 3. Access Points
- **Frontend Application**: http://localhost:3000
- **API & Swagger Documentation**: http://localhost:5000/swagger
- **Database**: localhost:1433 (sa/TailoringApp123!)

### 4. Health Checks
Visit these URLs to verify the deployment:

1. **Frontend Health**: http://localhost:3000
   - Should display the login page
   - No console errors in browser developer tools

2. **API Health**: http://localhost:5000/swagger
   - Should display Swagger documentation
   - Green status indicators for all endpoints

3. **Database Connection**: 
   - API logs should show successful database connection
   - No migration errors in `docker compose logs api`

## 🔧 Configuration

### Environment Variables

#### Production Overrides
Create a `.env` file in the root directory:

```env
# Database Configuration
SA_PASSWORD=YourStrongPassword123!
DB_NAME=TailoringProd

# API Configuration
ASPNETCORE_ENVIRONMENT=Production
JWT_KEY=YourSuperSecretProductionKey32BytesOrMore

# Frontend Configuration
VITE_API_BASE_URL=http://localhost:5000
```

#### Security Considerations
- Change default database password
- Use a strong JWT secret key (32+ characters)
- Enable HTTPS in production
- Set up proper firewall rules

### Custom Domain Setup

#### 1. Update docker-compose.yml
```yaml
services:
  frontend:
    environment:
      - VITE_API_BASE_URL=https://api.yourdomain.com
    labels:
      - "traefik.enable=true"
      - "traefik.http.routers.frontend.rule=Host(`yourdomain.com`)"
      - "traefik.http.routers.frontend.tls=true"
      - "traefik.http.routers.frontend.tls.certresolver=letsencrypt"
```

#### 2. Add Reverse Proxy (Traefik example)
```yaml
services:
  traefik:
    image: traefik:v2.9
    command:
      - "--api.dashboard=true"
      - "--providers.docker=true"
      - "--entrypoints.web.address=:80"
      - "--entrypoints.websecure.address=:443"
      - "--certificatesresolvers.letsencrypt.acme.email=your-email@domain.com"
      - "--certificatesresolvers.letsencrypt.acme.storage=/letsencrypt/acme.json"
      - "--certificatesresolvers.letsencrypt.acme.httpchallenge.entrypoint=web"
    ports:
      - "80:80"
      - "443:443"
      - "8080:8080"
    volumes:
      - "/var/run/docker.sock:/var/run/docker.sock:ro"
      - "./letsencrypt:/letsencrypt"
```

## 📊 Monitoring & Logging

### Log Management
```bash
# View all logs
docker compose logs

# View specific service logs
docker compose logs api
docker compose logs frontend
docker compose logs db

# Follow logs in real-time
docker compose logs -f --tail=100

# Save logs to file
docker compose logs > deployment.log
```

### Log Locations
- **API Logs**: Container stdout/stderr + `/app/logs/` (if configured)
- **Database Logs**: SQL Server error logs in container
- **Frontend Logs**: Nginx access/error logs

### Performance Monitoring
```bash
# Check resource usage
docker stats

# Check disk usage
docker system df

# Clean up unused resources
docker system prune -a
```

## 🔧 Troubleshooting

### Common Issues

#### 1. Port Conflicts
**Problem**: `Port 3000 is already allocated`
**Solution**:
```bash
# Find process using the port
netstat -ano | findstr :3000

# Kill the process (Windows)
taskkill /PID <PID> /F

# Or change ports in docker-compose.yml
ports:
  - "3001:80"  # Changed from 3000:80
```

#### 2. Database Connection Issues
**Problem**: API can't connect to database
**Solutions**:
```bash
# Check database container
docker compose logs db

# Restart database service
docker compose restart db

# Reset database (WARNING: Data loss)
docker compose down -v
docker compose up --build
```

#### 3. Frontend Build Failures
**Problem**: Frontend container fails to build
**Solutions**:
```bash
# Clear Docker build cache
docker builder prune

# Build with no cache
docker compose build --no-cache frontend

# Check frontend logs
docker compose logs frontend
```

#### 4. Memory Issues
**Problem**: Container out of memory
**Solutions**:
```bash
# Increase Docker Desktop memory limit (Windows/Mac)
# Settings → Resources → Memory → 4GB+

# Check memory usage
docker stats --no-stream
```

### Service Health Checks

#### API Health Check
```bash
curl -f http://localhost:5000/health || echo "API is down"
```

#### Frontend Health Check
```bash
curl -f http://localhost:3000 || echo "Frontend is down"
```

#### Database Health Check
```bash
docker compose exec db /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P "TailoringApp123!" \
  -Q "SELECT 1 AS Healthy"
```

## 🔄 Updates & Maintenance

### Updating the Application
```bash
# Pull latest changes
git pull origin main

# Rebuild and restart
docker compose down
docker compose up --build -d

# Verify update
docker compose ps
```

### Database Migrations
```bash
# Check migration status
docker compose exec api dotnet ef migrations list

# Apply pending migrations
docker compose exec api dotnet ef database update
```

### Backup & Restore

#### Database Backup
```bash
# Create backup
docker compose exec -T db /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P "TailoringApp123!" \
  -Q "BACKUP DATABASE [TailoringDB] TO DISK = '/var/opt/mssql/data/TailoringDB.bak'"

# Copy backup to host
docker cp splitit-db-1:/var/opt/mssql/data/TailoringDB.bak ./backup/
```

#### Database Restore
```bash
# Copy backup to container
docker cp ./backup/TailoringDB.bak splitit-db-1:/var/opt/mssql/data/

# Restore database
docker compose exec -T db /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P "TailoringApp123!" \
  -Q "RESTORE DATABASE [TailoringDB] FROM DISK = '/var/opt/mssql/data/TailoringDB.bak' WITH REPLACE"
```

## 🌐 Cloud Deployment

### Azure Container Instances
```bash
# Create resource group
az group create --name tailoring-app --location eastus

# Deploy container group
az container create \
  --resource-group tailoring-app \
  --file deploy/azure-container-group.yaml
```

### AWS ECS
```bash
# Create ECS cluster
aws ecs create-cluster --cluster-name tailoring-app

# Deploy using docker-compose
docker compose -f docker-compose.aws.yml up
```

### Google Cloud Run
```bash
# Build and push images
docker build -t gcr.io/PROJECT_ID/tailoring-app .
docker push gcr.io/PROJECT_ID/tailoring-app

# Deploy to Cloud Run
gcloud run deploy tailoring-app \
  --image gcr.io/PROJECT_ID/tailoring-app \
  --platform managed \
  --region us-central1
```

## 📞 Support

### Getting Help
1. Check this deployment guide first
2. Review logs using the commands above
3. Search for similar issues in the repository
4. Create an issue with:
   - Full error messages
   - Docker version info
   - System specifications
   - Steps to reproduce

### Performance Tuning
- **Database**: Increase memory allocation for SQL Server
- **API**: Set `ASPNETCORE_ENVIRONMENT=Production` for optimizations
- **Frontend**: Enable gzip compression in nginx.conf
- **Docker**: Allocate sufficient memory/CPU to Docker Desktop

---

**For additional support, please refer to the main README.md or create an issue on GitHub.**

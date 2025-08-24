# 🧪 Manual Testing Guide

This guide provides instructions for testing the Custom Tailoring Management System without Docker, for development and verification purposes.

## 🔧 Prerequisites

### Required Software
- **.NET 9 SDK**: Download from https://dotnet.microsoft.com/download
- **Node.js 18+**: Download from https://nodejs.org/
- **SQL Server Express** or **SQL Server LocalDB**: For database

### Environment Setup
- **IDE**: Visual Studio 2022, VS Code, or JetBrains Rider
- **Browser**: Chrome, Firefox, Edge, or Safari for testing
- **Git**: For version control

## 🗄️ Database Setup

### Option 1: SQL Server LocalDB (Recommended for development)
```powershell
# Check if LocalDB is installed
sqllocaldb info

# If not installed, download SQL Server Express with LocalDB
# https://www.microsoft.com/en-us/sql-server/sql-server-downloads

# Create LocalDB instance
sqllocaldb create MSSQLLocalDB
sqllocaldb start MSSQLLocalDB
```

### Option 2: SQL Server Express
1. Download and install SQL Server Express
2. Note the connection string (usually includes `localhost\SQLEXPRESS`)
3. Enable SQL Server authentication if needed

## 🚀 Backend Setup & Testing

### 1. Navigate to Backend Directory
```powershell
cd backend
```

### 2. Restore NuGet Packages
```powershell
dotnet restore
```

### 3. Configure Connection String
Edit `src/TailoringApp.API/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "Default": "Server=(localdb)\\mssqllocaldb;Database=TailoringDB;Trusted_Connection=true;MultipleActiveResultSets=true"
  },
  "Jwt": {
    "Key": "SuperSecretKeyThatIsAtLeast32BytesLongForJWTSigning123456789",
    "Issuer": "TailoringApp",
    "Audience": "TailoringAppClient"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

### 4. Create and Apply Database Migrations
```powershell
# Create initial migration (if not exists)
dotnet ef migrations add InitialCreate -p src/TailoringApp.Infrastructure/TailoringApp.Infrastructure.csproj -s src/TailoringApp.API/TailoringApp.API.csproj

# Apply migrations to database
dotnet ef database update -p src/TailoringApp.Infrastructure/TailoringApp.Infrastructure.csproj -s src/TailoringApp.API/TailoringApp.API.csproj
```

### 5. Run the API
```powershell
cd src/TailoringApp.API
dotnet run
```

Expected output:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:52244
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:52244
```

### 6. Test API Endpoints
- **Swagger UI**: https://localhost:52244/swagger
- **Health Check**: https://localhost:52244/health (if implemented)

#### Authentication Test
```powershell
# Using PowerShell
$headers = @{'Content-Type'='application/json'}
$body = @{
    username = "admin"
    password = "admin123"
} | ConvertTo-Json

$response = Invoke-RestMethod -Uri "https://localhost:52244/api/auth/login" -Method Post -Headers $headers -Body $body
Write-Host "Token received: $($response.token)"
```

#### Customer API Test
```powershell
# Create a customer (replace TOKEN with actual token)
$authHeaders = @{
    'Content-Type'='application/json'
    'Authorization'="Bearer YOUR_TOKEN_HERE"
}

$customerBody = @{
    firstName = "Test"
    lastName = "Customer"
    email = "test@example.com"
    phone = "+1234567890"
} | ConvertTo-Json

$customer = Invoke-RestMethod -Uri "https://localhost:52244/api/customers" -Method Post -Headers $authHeaders -Body $customerBody
Write-Host "Customer created with ID: $($customer.id)"
```

## 🎨 Frontend Setup & Testing

### 1. Navigate to Frontend Directory
```powershell
cd frontend
```

### 2. Install Dependencies
```powershell
npm install
```

### 3. Configure API URL
Create or edit `.env.local`:
```env
VITE_API_BASE_URL=https://localhost:52244
```

### 4. Start Development Server
```powershell
npm run dev
```

Expected output:
```
  VITE v4.x.x ready in xxx ms

  ➜  Local:   http://localhost:3002
  ➜  Network: use --host to expose
```

### 5. Test Frontend Features

#### Login Test
1. Navigate to `http://localhost:3002`
2. Enter credentials:
   - Username: `admin`
   - Password: `admin123`
3. Verify successful login and token storage

#### Create Customer Test
1. Navigate to Customers page
2. Click "Create Customer"
3. Fill out the form:
   - First Name: "John"
   - Last Name: "Doe"
   - Email: "john.doe@example.com"
   - Phone: "+1234567890"
4. Submit and verify customer appears in list

#### Create Work Order Test
1. From customer list, click on a customer
2. Click "Create Work Order"
3. Fill out the form:
   - Currency: "USD"
   - Due Date: Future date
   - Notes: "Test work order"
4. Submit and verify work order is created

#### Add Item Test
1. From work order details page
2. Click "Add Item"
3. Fill out the form:
   - Description: "Custom suit"
   - Garment Type: "Suit"
   - Quantity: 1
   - Unit Price: 500
   - Measurements: Add chest, waist, etc.
4. Submit and verify:
   - Item appears in items table
   - Total calculations are correct
   - Measurements display properly

## 🧪 Test Scenarios

### Scenario 1: Complete Work Order Flow
1. **Create Customer**: John Doe
2. **Create Work Order**: Custom tailoring order
3. **Add Items**: Suit jacket, pants, vest
4. **Verify Calculations**: Check totals are correct
5. **Update Status**: Draft → InProgress → Completed
6. **Verify Business Rules**: Cannot add items to completed orders

### Scenario 2: Currency and Calculations
1. **Create Work Order**: With EUR currency
2. **Add Item**: Quantity 2, Unit Price 100 EUR
3. **Verify Total**: Should show 200 EUR
4. **Frontend Calculation**: Verify client-side calculation works
5. **Multiple Items**: Add several items and verify subtotals

### Scenario 3: Measurements and Garment Types
1. **Add Suit Item**: Select "Suit" from dropdown
2. **Enter Measurements**: Chest, waist, hips, sleeve
3. **Verify Display**: Measurements show in items table
4. **Different Garments**: Test other garment types

### Scenario 4: Status-Based Business Logic
1. **Draft Order**: Can add/edit/remove items
2. **InProgress Order**: Cannot modify items
3. **Completed Order**: No modifications allowed
4. **UI Updates**: Verify buttons disable appropriately

## 🔍 Troubleshooting

### Backend Issues

#### Migration Errors
```powershell
# Drop and recreate database
dotnet ef database drop -p src/TailoringApp.Infrastructure/TailoringApp.Infrastructure.csproj -s src/TailoringApp.API/TailoringApp.API.csproj
dotnet ef database update -p src/TailoringApp.Infrastructure/TailoringApp.Infrastructure.csproj -s src/TailoringApp.API/TailoringApp.API.csproj
```

#### Connection String Issues
- Verify SQL Server/LocalDB is running
- Check connection string format
- Test connection with SQL Server Management Studio

#### Port Conflicts
```powershell
# Find process using port 52244
netstat -ano | findstr :52244

# Kill process if needed
taskkill /PID <PID> /F
```

### Frontend Issues

#### API Connection Errors
- Verify backend is running on correct port
- Check CORS configuration in backend
- Verify API URL in `.env.local`

#### Build Errors
```powershell
# Clear cache and reinstall
Remove-Item -Recurse -Force node_modules
Remove-Item package-lock.json
npm install
```

#### Browser Console Errors
- Check Network tab for failed API calls
- Verify JWT token is being sent in headers
- Check for CORS errors

## ✅ Verification Checklist

### Backend Verification
- [ ] API starts without errors
- [ ] Database migrations applied successfully
- [ ] Swagger UI accessible
- [ ] Authentication endpoints work
- [ ] CRUD operations function correctly
- [ ] JWT tokens generated and validated

### Frontend Verification
- [ ] Development server starts
- [ ] Login page loads
- [ ] Authentication flow works
- [ ] All pages accessible after login
- [ ] API calls successful
- [ ] Data displays correctly
- [ ] Forms submit properly
- [ ] Calculations work correctly

### Integration Verification
- [ ] End-to-end customer creation
- [ ] Complete work order workflow
- [ ] Item addition with measurements
- [ ] Status transitions work
- [ ] Business rules enforced
- [ ] Error handling functional

## 📊 Performance Testing

### Load Testing (Optional)
```powershell
# Install Apache Bench (if available)
# Test API performance
ab -n 100 -c 10 https://localhost:52244/api/customers

# Or use PowerShell for simple load test
1..100 | ForEach-Object -Parallel {
    Invoke-RestMethod -Uri "https://localhost:52244/api/customers" -Headers $authHeaders
} -ThrottleLimit 10
```

### Memory Usage
- Monitor Task Manager during testing
- Check for memory leaks in long-running sessions
- Verify garbage collection in .NET

## 📝 Test Report Template

```markdown
# Test Execution Report

**Date**: [Date]
**Tester**: [Name]
**Environment**: Windows/macOS/Linux

## Test Results

### Backend Tests
- [ ] Database connection: ✅/❌
- [ ] API startup: ✅/❌
- [ ] Authentication: ✅/❌
- [ ] Customer CRUD: ✅/❌
- [ ] Work Order CRUD: ✅/❌
- [ ] Item Management: ✅/❌

### Frontend Tests
- [ ] Application load: ✅/❌
- [ ] Login functionality: ✅/❌
- [ ] Customer management: ✅/❌
- [ ] Work order flow: ✅/❌
- [ ] Calculations: ✅/❌
- [ ] Responsive design: ✅/❌

### Issues Found
1. [Description of any issues]
2. [Steps to reproduce]
3. [Expected vs actual behavior]

### Performance Notes
- API response times: [Average response time]
- Frontend load time: [Time to first meaningful paint]
- Memory usage: [Peak memory consumption]
```

---

**This manual testing guide ensures comprehensive verification of all system components without requiring Docker infrastructure.**

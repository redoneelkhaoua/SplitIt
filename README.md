# 🧵 Custom Tailoring Management System (TailoringPro)

A modern, full-stack tailoring business management application built with Clean Architecture principles, featuring customer management, work order tracking, and item management with detailed measurements.

## 📋 Table of Contents
- [Features](#-features)
- [Technology Stack](#-technology-stack)
- [Quick Start with Docker](#-quick-start-with-docker)
- [Development Setup](#-development-setup)
- [Project Structure](#-project-structure)
- [Authentication & Authorization](#-authentication--authorization)
- [API Documentation](#-api-documentation)
- [Work Order Lifecycle](#-work-order-lifecycle)
- [Frontend Features](#-frontend-features)
- [Testing](#-testing)
- [Deployment](#-deployment)
- [Contributing](#-contributing)

## ✨ Features

### 👥 Customer Management
- Create and manage customer profiles
- Customer search and filtering
- Customer contact information tracking

### 📝 Work Order Management
- Create work orders with multiple statuses (Draft, InProgress, Completed, Cancelled)
- Real-time status tracking and updates
- Currency selection for international customers
- Due date management and priority settings

### 👔 Item Management
- Add multiple garment items to work orders
- Detailed measurement tracking (Chest, Waist, Hips, Sleeve)
- Real-time price calculations (Quantity × Unit Price)
- Garment type categorization (Suit, Jacket, Pant, Vest, etc.)

### 💰 Financial Calculations
- Automatic total calculations with frontend validation
- Currency-specific pricing
- Discount management
- Subtotal and grand total tracking

### 🔐 Security & Authentication
- JWT-based authentication
- Role-based access control (Admin/Staff)
- Secure API endpoints
- Token-based session management

## 🛠 Technology Stack

### Backend (.NET 9)
- **Framework**: ASP.NET Core Web API
- **Architecture**: Clean Architecture + DDD + CQRS
- **Database**: Entity Framework Core with SQL Server
- **Authentication**: JWT (JSON Web Tokens)
- **Validation**: FluentValidation
- **Mediation**: MediatR for CQRS pattern
- **Testing**: xUnit + FluentAssertions

### Frontend (React + TypeScript)
- **Framework**: React 18 with TypeScript
- **Build Tool**: Vite for fast development and builds
- **State Management**: TanStack Query (React Query)
- **HTTP Client**: Axios
- **Routing**: React Router v6
- **Styling**: Custom CSS with CSS Variables
- **Form Handling**: React Hook Form + Zod validation

### Infrastructure
- **Containerization**: Docker & Docker Compose
- **Database**: SQL Server 2022
- **Web Server**: Nginx (for frontend in production)
- **Reverse Proxy**: Built-in ASP.NET Core

## 🚀 Quick Start with Docker

The fastest way to get the application running is with Docker Compose:

```bash
# Clone the repository
git clone https://github.com/redoneelkhaoua/TailoringManagement2-Elkhaoua.git
cd TailoringManagement2-Elkhaoua

# Start all services
docker compose up --build

# Or run in detached mode
docker compose up --build -d
```

### 🌐 Access Points
- **Frontend**: http://localhost:3000
- **API**: http://localhost:5000
- **API Documentation**: http://localhost:5000/swagger
- **Database**: localhost:1433 (sa/TailoringApp123!)

### 👤 Default Login Credentials
| Username | Password | Role  | Description |
|----------|----------|-------|-------------|
| admin    | admin123 | Admin | Full access to all features |
| staff    | staff123 | Staff | Standard user access |

*Note: These are the default credentials for the TailoringPro application demo.*

## 💻 Development Setup

### Prerequisites
- .NET 9 SDK
- Node.js 18+ and npm
- SQL Server or SQL Server Express
- Docker (optional, for containerized development)

### Backend Setup
```bash
# Navigate to backend directory
cd backend

# Restore packages
dotnet restore

# Create and apply database migrations
dotnet ef migrations add InitialCreate -p src/TailoringApp.Infrastructure/TailoringApp.Infrastructure.csproj -s src/TailoringApp.API/TailoringApp.API.csproj

# Update database
dotnet ef database update -p src/TailoringApp.Infrastructure/TailoringApp.Infrastructure.csproj -s src/TailoringApp.API/TailoringApp.API.csproj

# Run the API
cd src/TailoringApp.API
dotnet run
```

The API will be available at `https://localhost:52244` or `http://localhost:52244`

### Frontend Setup
```bash
# Navigate to frontend directory
cd frontend

# Install dependencies
npm install

# Start development server
npm run dev
```

The frontend will be available at `http://localhost:3002`

### Environment Configuration

#### Backend (`appsettings.Development.json`)
```json
{
  "ConnectionStrings": {
    "Default": "Server=(localdb)\\mssqllocaldb;Database=TailoringDB;Trusted_Connection=true;MultipleActiveResultSets=true"
  },
  "Jwt": {
    "Key": "SuperSecretKeyThatIsAtLeast32BytesLongForJWTSigning123456789",
    "Issuer": "TailoringApp",
    "Audience": "TailoringAppClient"
  }
}
```

#### Frontend (`.env.local`)
```env
VITE_API_BASE_URL=http://localhost:52244
```

## 📁 Project Structure

```
TailoringManagement2-Elkhaoua/
├── backend/
│   ├── src/
│   │   ├── TailoringApp.API/          # Web API layer
│   │   ├── TailoringApp.Application/  # Application services & CQRS
│   │   ├── TailoringApp.Domain/       # Domain entities & business logic
│   │   └── TailoringApp.Infrastructure/ # Data access & external services
│   └── tests/                         # Unit and integration tests
├── frontend/
│   ├── src/
│   │   ├── api/                       # API client and types
│   │   ├── components/                # Reusable React components
│   │   ├── pages/                     # Page components
│   │   ├── state/                     # Context providers
│   │   └── styles.css                 # Global styles
│   ├── Dockerfile                     # Frontend container setup
│   └── nginx.conf                     # Production web server config
├── docker-compose.yml                 # Multi-container setup
└── README.md                          # This file
```

### Backend Architecture (Clean Architecture)

```
┌─────────────────────────────────────────────────────────────┐
│                     Presentation Layer                     │
│                   (TailoringApp.API)                      │
│  Controllers, JWT Auth, Swagger, Global Exception Handling │
└─────────────────────────────────────────────────────────────┘
                              │
┌─────────────────────────────────────────────────────────────┐
│                    Application Layer                       │
│                (TailoringApp.Application)                 │
│     Commands, Queries, Handlers, DTOs, FluentValidation   │
└─────────────────────────────────────────────────────────────┘
                              │
┌─────────────────────────────────────────────────────────────┐
│                      Domain Layer                          │
│                   (TailoringApp.Domain)                   │
│      Entities, Value Objects, Domain Services, Rules      │
└─────────────────────────────────────────────────────────────┘
                              │
┌─────────────────────────────────────────────────────────────┐
│                  Infrastructure Layer                      │
│               (TailoringApp.Infrastructure)               │
│     EF Core, Repositories, External Services, Database    │
└─────────────────────────────────────────────────────────────┘
```

## 🔐 Authentication & Authorization

The application uses JWT (JSON Web Tokens) for authentication:

### Login Process
```http
POST /api/auth/login
Content-Type: application/json

{
  "username": "admin",
  "password": "admin123"
}
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": "guid",
    "username": "admin",
    "role": "Admin"
  }
}
```

### Using the Token
Include the token in the Authorization header for protected endpoints:
```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

## 📚 API Documentation

### Customer Endpoints
```http
# Create customer
POST /api/customers
{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@example.com",
  "phone": "+1234567890"
}

# List customers (paginated)
GET /api/customers?page=1&pageSize=10&search=john

# Get customer details
GET /api/customers/{customerId}
```

### Work Order Endpoints
```http
# Create work order
POST /api/customers/{customerId}/workorders
{
  "currency": "USD",
  "priority": "medium",
  "dueDate": "2025-09-01T00:00:00Z",
  "notes": "Custom suit order"
}

# List work orders
GET /api/customers/{customerId}/workorders?status=Draft&page=1&pageSize=20

# Get work order details
GET /api/workorders/{workOrderId}

# Update work order status
PATCH /api/workorders/{workOrderId}/status
{
  "status": "InProgress"
}
```

### Item Management
```http
# Add item to work order
POST /api/customers/{customerId}/workorders/{workOrderId}/items
{
  "description": "Custom three-piece suit",
  "currency": "USD",
  "garmentType": "Suit",
  "quantity": 1,
  "unitPrice": 500.00,
  "total": 500.00,
  "chestMeasurement": 42,
  "waistMeasurement": 34,
  "hipsMeasurement": 40,
  "sleeveMeasurement": 25,
  "measurementNotes": "Slim fit preferred"
}
```

### Response Format
All list endpoints return paginated responses:
```json
{
  "items": [...],
  "total": 42,
  "page": 1,
  "pageSize": 10,
  "totalPages": 5,
  "hasNext": true,
  "hasPrevious": false
}
```

## 🔄 Work Order Lifecycle

```
Draft → InProgress → Completed
  ↓         ↓
Cancelled  Cancelled
```

### Status Rules
- **Draft**: Items can be added, modified, or removed
- **InProgress**: Items cannot be modified, but work order can be completed or cancelled
- **Completed**: No modifications allowed, work order is finalized
- **Cancelled**: No modifications allowed, work order is terminated

### State Transitions
- `Draft` → `InProgress`: Start work on the order
- `Draft` → `Cancelled`: Cancel before starting work
- `InProgress` → `Completed`: Finish the work order
- `InProgress` → `Cancelled`: Cancel work in progress

## 🎨 Frontend Features

### Modern UI/UX
- **Responsive Design**: Works on desktop, tablet, and mobile devices
- **Real-time Updates**: Immediate feedback for user actions
- **Form Validation**: Client-side validation with helpful error messages
- **Modal Dialogs**: Smooth interactions for creating and editing items
- **Status Indicators**: Color-coded badges for different work order states

### Key Components
- **Work Order List**: Paginated list with filtering and search
- **Work Order Details**: Comprehensive view with tabbed interface
- **Customer Management**: Full CRUD operations for customer data
- **Item Management**: Add/edit items with measurement tracking
- **Authentication**: Login/logout with persistent sessions

### Styling System
- **CSS Variables**: Consistent color scheme and theming
- **Component Classes**: Reusable styling patterns
- **Responsive Grid**: Flexible layouts that adapt to screen size
- **Form Styling**: Professional form layouts with proper spacing

## 🧪 Testing

### Backend Testing
```bash
# Run all tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test tests/TailoringApp.UnitTests/
```

### Test Structure
- **Unit Tests**: Domain logic and business rules
- **Integration Tests**: API endpoints and database operations
- **Architecture Tests**: Ensure clean architecture compliance

### Frontend Testing
```bash
# Install test dependencies
npm install --save-dev @testing-library/react @testing-library/jest-dom

# Run tests (when implemented)
npm test
```

## 🚀 Deployment

### Production with Docker
```bash
# Build and start production containers
docker compose -f docker-compose.yml up --build -d

# View logs
docker compose logs -f

# Stop services
docker compose down
```

### Manual Deployment

#### Backend
```bash
# Publish the application
dotnet publish -c Release -o ./publish

# Run the published application
cd publish
dotnet TailoringApp.API.dll
```

#### Frontend
```bash
# Build for production
npm run build

# Serve with nginx or any static file server
# Files will be in the 'dist' directory
```

### Environment Variables
Ensure these environment variables are set in production:

```env
# Backend
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__Default=YourProductionConnectionString
Jwt__Key=YourProductionJWTKey32BytesOrMore

# Frontend
VITE_API_BASE_URL=https://your-api-domain.com
```

## 🤝 Contributing

### Getting Started
1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

### Development Guidelines
- Follow Clean Architecture principles
- Write unit tests for new features
- Use TypeScript for frontend development
- Follow C# coding conventions for backend
- Update documentation for new features

### Code Style
- **Backend**: Follow Microsoft C# coding conventions
- **Frontend**: Use ESLint and Prettier for consistent formatting
- **Commits**: Use conventional commit messages

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- Clean Architecture by Robert C. Martin
- Domain-Driven Design principles
- CQRS pattern implementation with MediatR
- Modern React development patterns

---

**Built with ❤️ for the tailoring industry**

**Repository**: https://github.com/redoneelkhaoua/TailoringManagement2-Elkhaoua

For questions or support, please open an issue on GitHub.


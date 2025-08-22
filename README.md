# Custom Tailoring Management System (TailoringApp)

Production-quality sample showing Clean Architecture, DDD, and CQRS with .NET 9 and React.

## Stack
- Backend: .NET 9, ASP.NET Core, EF Core (SQL Server), MediatR, FluentValidation, xUnit
- Frontend: React + TypeScript (Vite)
- Infra: Docker Compose (API, SQL Server, Frontend)

## Run with Docker
```bash
docker compose up --build
```
API: http://localhost:5000/swagger
Frontend: http://localhost:3000

## Dev Setup
- Backend
```bash
# from SplitIt/backend
dotnet ef migrations add InitialCreate -p src/TailoringApp.Infrastructure/TailoringApp.Infrastructure.csproj -s src/TailoringApp.API/TailoringApp.API.csproj
dotnet ef database update -p src/TailoringApp.Infrastructure/TailoringApp.Infrastructure.csproj -s src/TailoringApp.API/TailoringApp.API.csproj
```
- Frontend
```bash
# from SplitIt/frontend/splitit-web
npm i
npm run dev
```

## Architecture
- Domain: aggregates, value objects, domain events
- Application: CQRS, validators, behaviors
- Infrastructure: EF Core, repositories, event dispatch after SaveChanges
- API: controllers, Swagger, ProblemDetails + validation

## Decisions/Tradeoffs
- Field-backed collections with EF PropertyAccessMode.Field
- LocalDB for dev; container SQL for Docker
- Minimal read models; can expand to projections later

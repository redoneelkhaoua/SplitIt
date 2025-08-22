# Custom Tailoring Management System (TailoringApp)

Clean Architecture, DDD, CQRS sample (Backend: .NET 8) with React frontend.

## Stack
- Backend: .NET 8, ASP.NET Core Web API, EF Core, MediatR (CQRS), FluentValidation, JWT Auth, xUnit + FluentAssertions
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
- Domain: Aggregates (Customer, WorkOrder), value objects (Money), lifecycle rules
- Application: CQRS via MediatR (queries/commands + handlers), validators with FluentValidation
- Infrastructure: EF Core (repositories, migrations), persistence events
- API: Controllers, JWT auth (HS256), ProblemDetails for validation errors, global enum -> string JSON output

## Authentication
Login to obtain a JWT then send `Authorization: Bearer <token>` header.

```
POST /api/auth/login
{ "username": "admin", "password": "admin123" }

=> { "token": "<jwt>" }
```

Seed users (created on first run):
| Username | Password   | Role  |
|----------|------------|-------|
| admin    | admin123   | Admin |
| staff    | staff123   | Staff |

All mutating endpoints require authentication. Roles can be extended (currently Admin / Staff behavior mostly identical).

## Work Order Lifecycle
Draft → InProgress (start) → Completed OR Cancelled
- Add / update / remove items only while Draft
- Discount can be applied while not Completed/Cancelled (enforced in domain)

## Core Endpoints (simplified)
Customers:
- `POST /api/customers` create
- `GET /api/customers?page=1&pageSize=10&search=...` list (paged)

Work Orders (per customer):
- `POST /api/customers/{customerId}/workorders` create (body: `{ currency, appointmentId? }`)
- `GET /api/customers/{customerId}/workorders?status=Completed&sortBy=created&desc=true&page=1&pageSize=20` list (summary)
- `GET /api/customers/{customerId}/workorders/{workOrderId}` details (includes items)
- `POST /api/customers/{customerId}/workorders/{workOrderId}/items` add item
- `PUT /api/customers/{customerId}/workorders/{workOrderId}/items/{description}` update quantity `{ quantity }`
- `DELETE /api/customers/{customerId}/workorders/{workOrderId}/items/{description}` remove item
- `POST /api/customers/{customerId}/workorders/{workOrderId}/start` start
- `POST /api/customers/{customerId}/workorders/{workOrderId}/complete` complete
- `POST /api/customers/{customerId}/workorders/{workOrderId}/cancel` cancel (if implemented)
- (Discount endpoints follow similar pattern if exposed: set / clear)

## Pagination & Filtering
List endpoints return a unified envelope:
```
{
	"items": [ ... ],
	"total": 42,
	"page": 1,
	"pageSize": 10,
	"totalPages": 5,
	"hasNext": true,
	"hasPrevious": false
}
```
Query params (work orders): `page`, `pageSize`, `status`, `search`, `from`, `to`, `sortBy` (e.g. `created`), `desc` (true/false).

## Enum Serialization
All enums are serialized as strings (e.g. `"Completed"`). Work order status values: `Draft | InProgress | Completed | Cancelled`.

## Error Format (Validation)
Validation failures return HTTP 400:
```
{
	"title": "Validation failed",
	"status": 400,
	"detail": "One or more validation errors occurred.",
	"type": "https://httpstatuses.com/400",
	"errors": {
		"PageSize": ["PageSize must be between 1 and 100"]
	}
}
```

## Configuration
`appsettings.*.json` contains:
```
"Jwt": {
	"Key": "<>=32 byte secret>",
	"Issuer": "TailoringApp",
	"Audience": "TailoringAppClient"
}
```
Ensure the key is at least 32 bytes (guard enforced at startup).

## Running Tests
```
dotnet test
```

## Frontend Integration Tips
1. Authenticate once → store token (memory or secure storage).
2. Central fetch wrapper to inject Authorization header.
3. Map status to badge colors (Draft=gray, InProgress=blue, Completed=green, Cancelled=red).
4. Recalculate pages using envelope rather than deriving from items length.
5. Optimistic UI: After item add/remove adjust subtotal locally; refresh details for authoritative totals if needed.

## Future Enhancements (Ideas)
- Soft delete & audit trail
- Appointments module expansion
- Outbox pattern for integration events
- Caching layer for heavy read queries

## Decisions / Trade-offs
- Kept read models minimal (reuse aggregate projection) for speed; can introduce dedicated projections later.
- Enum-as-string chosen early to avoid breaking contract when enum evolves.
- Single discount concept (no taxes) per requirements.


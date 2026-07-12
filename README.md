# Transaction Approval Simulator

Production-quality take-home assignment implementation with:
- Frontend: React + TypeScript + Vite + CSS Modules
- Backend: ASP.NET Core 8 Web API (C#)
- Database: SQL Server (MSSQL)
- ORM: EF Core 8
- Validation: FluentValidation
- API docs: Swagger
- Bonus features included: Docker Compose, English/Hebrew i18n with RTL/LTR, JWT auth (signup/login)

## Project Overview
The app simulates whether a transaction is approved based on banking hours in the selected region.

Approval rule:
- Approved only when region-local time is in 08:00 (inclusive) to 18:00 (exclusive)
- Rejected otherwise

The backend is the source of truth for time-zone conversion and approval decisions.

## Repository Structure

```text
transaction-approval-simulator/
  backend/
    TransactionApproval.slnx
    src/
      TransactionApproval.Api/
      TransactionApproval.Application/
      TransactionApproval.Domain/
      TransactionApproval.Infrastructure/
    tests/
      TransactionApproval.Tests.Unit/
  frontend/
  docker-compose.yml
```

## Architecture

### Backend Layers
- Api: controllers, middleware, DI composition, auth, Swagger, CORS
- Application: use-case services, DTOs, validators, approval evaluator
- Domain: entities and enums
- Infrastructure: EF Core DbContext/configurations/repositories, JWT/password hashing, clock, migrations

### Frontend Structure
- app: root composition and page orchestration
- layout: top header and language toggle
- features/auth: auth context and login/signup UI
- features/transactions: simulator UI, result panel, approved list, hooks
- services/api: typed API client functions
- i18n: EN/HE translation resources and configuration

## API Endpoints

### Public
- `GET /api/regions`
- `POST /api/auth/signup`
- `POST /api/auth/login`

### Protected (JWT)
- `POST /api/transactions/simulate`
- `GET /api/transactions/approved?page=1&pageSize=20`

## Data Model
- Region: `Code`, `Name`, `TimeZoneId`
- Transaction: `Id`, `RegionCode`, `RegionName`, `TimeZoneId`, `SubmittedUtc`, `LocalTransactionTime`, `Status`, `CreatedAtUtc`
- User: `Id`, `Username`, `PasswordHash`, `CreatedAtUtc`

## Timezone Strategy
- Supported regions are fixed and seeded in DB (`RegionCatalog`)
- Each region maps to a stable IANA timezone ID
- Submitted transaction instant is stored in UTC
- Backend converts UTC to region local time with `TimeZoneInfo`
- DST is handled automatically by timezone data (no hardcoded offsets)

USA ambiguity is resolved by explicit sub-regions:
- `US-East` -> `America/New_York`
- `US-West` -> `America/Los_Angeles`

## Setup: Local Development

## 1) Prerequisites
- .NET SDK 8+ (SDK 10 works too)
- Node.js 20+
- SQL Server running locally on `localhost:1433`

## 2) Backend
```bash
cd backend
# optional: inspect settings
# src/TransactionApproval.Api/appsettings.Development.json

# run
 dotnet run --project src/TransactionApproval.Api
```

What happens on startup:
- API starts
- pending EF migrations are applied automatically
- region catalog is seeded via migration

Swagger:
- http://localhost:5000/swagger (or the URL printed by ASP.NET)

## 3) Frontend
```bash
cd frontend
npm install
npm run dev
```

Frontend URL:
- http://localhost:5173

Configure backend URL in frontend with:
- `VITE_API_BASE_URL` (defaults to `http://localhost:5000`)

## Setup: Docker Compose
```bash
docker compose up --build
```

Services:
- Frontend: http://localhost:8081
- Backend: http://localhost:8080
- SQL Server: localhost:1433

## Example API Requests

### Signup
```http
POST /api/auth/signup
Content-Type: application/json

{
  "username": "demo_user",
  "password": "secret123"
}
```

### Login
```http
POST /api/auth/login
Content-Type: application/json

{
  "username": "demo_user",
  "password": "secret123"
}
```

### Simulate (protected)
```http
POST /api/transactions/simulate
Authorization: Bearer <token>
Content-Type: application/json

{
  "regionCode": "IL",
  "submittedAt": "2026-07-12T09:15:00.000Z"
}
```

### Approved (protected)
```http
GET /api/transactions/approved?page=1&pageSize=20
Authorization: Bearer <token>
```

## Figma Alignment Notes
The UI is implemented to mirror the provided interview design direction:
- top brand header
- ENG/Hebrew toggle
- centered simulator title and badge
- searchable region selector
- prominent purple time card with hour/minute controls
- result panel and visual card
- bottom horizontal approved-transactions card list with arrows

## Architecture Decisions
- Thin controllers, business logic in application services
- UTC persistence for auditability and consistency
- Explicit region catalog instead of free-text countries
- Separate approved-transactions query endpoint for read clarity and easy paging
- JWT auth kept minimal and pragmatic for interview scope

## Tradeoffs
- Frontend uses native date/time controls and custom styling instead of a heavy date-time library
- No refresh tokens (short-lived access token only) to keep auth concise
- Approved list paging implemented server-side but frontend currently shows first page only
- Region catalog is static (best for deterministic business rules) rather than dynamic geopolitical coverage

## What I Would Improve With More Time
- Add integration tests for API + database (Testcontainers)
- Add refresh token flow and role-based claims
- Add audit trail metadata (request id, actor id)
- Add accessibility pass (keyboard flow, ARIA audit, contrast review)
- Add virtualization and infinite loading for large approved lists
- Add CI pipeline (build, test, lint, Docker build)

## Demo Script (Loom, 3-5 minutes)
1. Open app and briefly explain stack and architecture.
2. Signup and login.
3. Show region search and language toggle (ENG/HE), including RTL behavior in Hebrew.
4. Run a simulation for a time inside banking hours and show Approved result.
5. Run another simulation outside banking hours and show Rejected result.
6. Scroll the Approved Transactions list and show newest approved entries.
7. Open Swagger and show endpoints + JWT-protected routes.
8. Show one backend code snippet: `TransactionApprovalEvaluator` and explain timezone/DST handling.
9. Show Docker Compose up command and running services.

## Submission Checklist
- [ ] Backend builds successfully
- [ ] Unit tests pass
- [ ] Frontend builds successfully
- [ ] Database migrations apply on startup
- [ ] Regions endpoint returns supported regions
- [ ] Simulate endpoint enforces approval window correctly
- [ ] Approved endpoint returns approved only, newest-first
- [ ] ENG/HE toggle works and switches RTL/LTR
- [ ] Login/signup + JWT protection works
- [ ] Docker Compose runs all services
- [ ] README instructions validated on a clean machine

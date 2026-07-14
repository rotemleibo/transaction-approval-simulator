## Quick Start (One Command)

Run everything with Docker and open the UI automatically:

```powershell
docker compose up -d --build; Start-Process "http://localhost:8081"
```

## Demo

https://www.loom.com/share/b0344775f0794b61a4f2051781f4689f

# Transaction Approval Simulator

Full-stack simulator for transaction approval by region-local banking hours.

## Stack

- Frontend: React 19, TypeScript, Vite, React Query, React Hook Form, Zod, i18next (EN/HE + RTL)
- Backend: ASP.NET Core 8 Web API, FluentValidation, JWT auth, Swagger
- Data: SQL Server + EF Core 8
- Reliability: Transactional outbox with leasing, retry backoff, and dead-lettering
- DevOps: Docker Compose for end-to-end local environment

## Recent Changes

- Added transactional outbox processing with lease-based claiming, exponential backoff retries, and dead-letter handling.
- Added authentication flow (signup/login) with JWT-protected transaction endpoints.
- Added frontend auto-logout behavior when API returns 401.
- Added paged approved-transactions API + frontend infinite loading (carousel load more).
- Added EN/HE localization with persisted language selection and automatic RTL/LTR switching.
- Added startup database migration execution for smoother local/docker startup.

## Business Rule

Transaction is approved only when region-local time is:

- greater than or equal to 08:00
- less than 18:00

Otherwise the transaction is rejected.

The backend is the source of truth for timezone conversion and approval decisions.

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

## Backend Architecture

- Api: controllers, middleware, Swagger, JWT, CORS, DI composition
- Application: services, DTOs, validators, approval evaluator, application events
- Domain: entities and enums
- Infrastructure: EF Core persistence, repositories, password hashing, JWT issuing, system clock, outbox dispatcher/publisher

## Frontend Architecture

- app: top-level page composition
- layout: header and language switch
- features/auth: signup/login and auth context
- features/transactions: region selection, local date/time input, simulation, approved carousel
- services/api: typed API calls via axios instance
- i18n: translation resources and initialization

## API Endpoints

Public:

- GET /api/regions
- POST /api/auth/signup
- POST /api/auth/login

Protected (JWT):

- POST /api/transactions/simulate
- GET /api/transactions/approved?page=1&pageSize=20

## Data Model

- Region: Code, Name, TimeZoneId
- Transaction: Id, RegionCode, RegionName, TimeZoneId, SubmittedUtc, LocalTransactionTime, Status, CreatedAtUtc
- User: Id, Username, PasswordHash, CreatedAtUtc
- OutboxMessage: Id, Type, Payload, OccurredOnUtc, ProcessedOnUtc, DeadLetteredAtUtc, Attempts, AvailableAtUtc, LeasedUntilUtc, LastError

## Timezone Strategy

- Regions are fixed and seeded in the database.
- Each region maps to a stable IANA timezone.
- submittedAt is treated as an absolute UTC instant.
- Backend converts UTC to region-local time using TimeZoneInfo.
- DST is handled by timezone data (no hardcoded offsets).

US regions are explicitly split:

- US-East -> America/New_York
- US-West -> America/Los_Angeles

## Local Development

### Prerequisites

- .NET SDK 8+
- Node.js 20+
- SQL Server on localhost:1433

### Backend

```bash
cd backend
dotnet run --project src/TransactionApproval.Api
```

Default development URL:

- http://localhost:8080

Swagger:

- http://localhost:8080/swagger

On startup:

- pending EF migrations are applied automatically
- seeded region catalog is available

### Frontend

```bash
cd frontend
npm install
npm run dev
```

Frontend URL:

- http://localhost:5173

Environment variable:

- VITE_API_BASE_URL (default: http://localhost:5000)

Recommended local value for this backend setup:

- http://localhost:8080

## Docker Compose

```bash
docker compose up --build
```

Services:

- Frontend: http://localhost:8081
- Backend: http://localhost:8080
- SQL Server: localhost:1433

Compose also injects backend JWT/CORS/connection settings.

## Quality Commands

Backend tests:

```bash
cd backend
dotnet test
```

Frontend lint/build:

```bash
cd frontend
npm run lint
npm run build
```

## Example Requests

Signup:

```http
POST /api/auth/signup
Content-Type: application/json

{
  "username": "demo_user",
  "password": "secret123"
}
```

Login:

```http
POST /api/auth/login
Content-Type: application/json

{
  "username": "demo_user",
  "password": "secret123"
}
```

Simulate (JWT required):

```http
POST /api/transactions/simulate
Authorization: Bearer <token>
Content-Type: application/json

{
  "regionCode": "IL",
  "submittedAt": "2026-07-12T09:15:00.000Z"
}
```

Approved list (JWT required):

```http
GET /api/transactions/approved?page=1&pageSize=20
Authorization: Bearer <token>
```

## Outbox Flow

1. Transaction + outbox row are saved in one DB transaction.
2. Background dispatcher claims pending rows with a lease window.
3. Event payload is deserialized by type.
4. Publisher emits event (default implementation: structured logs).
5. Failures are retried with exponential backoff.
6. Messages that exceed max retries are dead-lettered.

Outbox settings are configured under Outbox in appsettings.

## Tradeoffs

- Uses short-lived access tokens only (no refresh tokens).
- Region catalog is static and intentionally bounded.
- Default event publisher logs events; broker integration can be added behind IEventPublisher.

## Possible Next Improvements

- Add integration tests (API + DB via Testcontainers).
- Add refresh tokens and richer claims/authorization.
- Add broker-backed event publisher (Kafka/Service Bus/RabbitMQ).
- Add CI pipeline for build/test/lint/docker.
- Add cahced region catalog to reduce DB roundtrips.
- Add frontend unit tests and Cypress E2E tests.


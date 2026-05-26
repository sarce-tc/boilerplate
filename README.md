# Microservice — .NET 10 Boilerplate

Production-ready microservice boilerplate built on **.NET 10 / C# 14** with Clean Architecture, CQRS, and a dual ORM strategy (EF Core for writes, Dapper for reads).

---

## Stack

| Layer | Technology | Version |
|---|---|---|
| Runtime | .NET | 10 |
| Language | C# | 14 |
| ORM (writes) | EF Core | 10 |
| Micro-ORM (reads) | Dapper | 2.1.x |
| RDBMS | PostgreSQL | 16 |
| Mediator | MediatR | 14.1 |
| Authentication | JWT Bearer | 10.0.x |
| OpenAPI | Swashbuckle + Microsoft.OpenApi | 10.x / 2.x |
| Observability | OpenTelemetry | 1.15.x |
| Logging | Serilog | latest |
| Tests | xUnit + Moq + FluentAssertions | latest |

---

## Architecture

```
Microservice.Domain          ← no external dependencies
Microservice.Application     ← depends on Domain
Microservice.Infrastructure  ← depends on Application
Microservice.API             ← depends on Application (not Infrastructure)
Microservice.Test            ← tests for all layers
```

**Domain** — entities, value objects, `DomainException`, domain invariants.  
**Application** — CQRS handlers, DTOs, contracts (`IUnitOfWork`, `IXReadRepository`), Result pattern, FluentValidation behaviors.  
**Infrastructure** — EF Core `DbContext`, Dapper read repositories, `UnitOfWork`, background jobs.  
**API** — controllers, `GlobalExceptionHandler`, JWT middleware, rate limiting, OpenTelemetry setup.

### Key patterns

- **CQRS via MediatR** — commands mutate state; queries only read. One handler per use case.
- **Result pattern** — `Result<T>` / `Result` propagate success/failure without exceptions. `GlobalExceptionHandler` is the only exception boundary.
- **Dual ORM** — EF Core + `UnitOfWork` for transactional writes; Dapper for all read queries (performance-sensitive paths).
- **Domain invariants** — enforced inside the aggregate root. Violations throw `DomainException` → HTTP 409.

---

## Project structure

```
Microservice.Domain/
  Entities/         Order.cs · Customer.cs · OrderItem.cs · Example.cs
  Exceptions/       DomainException.cs
  Common/           BaseDomainModel.cs

Microservice.Application/
  Contracts/
    Persistence/
      Dapper/       IUnitOfWork.cs · IOrderReadRepository.cs · ICustomerReadRepository.cs …
  DTOs/             Orders/ · Customers/
  Features/
    Orders/         Commands/ · Queries/
    Customers/      Commands/ · Queries/
  Common/
    Results/        Result.cs · Error.cs · PagedResult.cs

Microservice.Infrastructure/
  Repositories/
    Dapper/         UnitOfWork.cs · OrderReadRepository.cs · OrderWriteRepository.cs
                    CustomerReadRepository.cs · CustomerWriteRepository.cs
  Persistence/      ExampleDbContext.cs (migrations)

Microservice.API/
  Controllers/      OrdersController.cs · CustomersController.cs
  ExceptionHandling/GlobalExceptionHandler.cs
  Extensions/       ResultExtensions.cs

Microservice.Test/
  (mirrors source structure)
```

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)

---

## Getting started

### 1. Start the database

```bash
docker-compose up -d
```

Starts a `postgres:16` container on port **5432**, database `example_db`, user `postgres`.

### 2. Apply migrations

```bash
dotnet ef database update --project Microservice.Infrastructure --startup-project Microservice.API
```

### 3. Run the API

```bash
dotnet run --project Microservice.API
```

Swagger UI: `https://localhost:{port}/swagger`

---

## Configuration

`appsettings.json` (development overrides go in `appsettings.Development.json`):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=example_db;Username=postgres;Password=postgres"
  },
  "Jwt": {
    "Key": "<secret>",
    "Issuer": "Microservice",
    "Audience": "Microservice",
    "ExpirationMinutes": 60
  },
  "RateLimiting": {
    "PermitLimit": 100,
    "WindowSeconds": 60
  },
  "OpenTelemetry": {
    "ServiceName": "Microservice",
    "OtlpEndpoint": "http://localhost:4317"
  }
}
```

---

## Reference implementations

Two aggregates serve as canonical templates — **read the AGENT ENTRY POINT comments** at the top of each file.

| Aggregate | ORM path | Reference for |
|---|---|---|
| `Order` | EF write + Dapper read | All patterns: UoW, command/query handlers, domain invariants, controller binding |
| `Example` | EF Core only | Pure EF Core path without Dapper |

`Order` is the **permanent reference implementation** for this project. When adding a new aggregate, follow the Order patterns exactly — do not diverge without a documented reason.

---

## Infrastructure features

| Feature | Implementation |
|---|---|
| Exception handling | `GlobalExceptionHandler` — sole error boundary; maps `DomainException`→409, `ArgumentException`→400 |
| Authentication | JWT Bearer (HS256); `[AllowAnonymous]` where public access is intentional |
| Rate limiting | Sliding window, 100 req/min per IP, configurable in `appsettings.json` |
| Idempotency | `Idempotency-Key` header on POST/PUT/PATCH |
| Correlation ID | Propagated through request pipeline and logs |
| Observability | OpenTelemetry traces + metrics, OTLP export, console fallback |
| Logging | Serilog structured logging, enriched with correlation ID |
| Background jobs | `Channel<T>` + `BackgroundService`; scoped DI per work item |
| API versioning | `Asp.Versioning` — URL segment strategy (`/api/v{version}/`) |
| Pagination | `PagedResult<T>` on all collection endpoints |

---

## Running tests

```bash
dotnet test
```

Unit tests cover handlers, validators, and domain logic. Naming convention: `MethodUnderTest_Scenario_ExpectedResult`.

---

## AI agent navigation

This codebase is structured for AI-assisted development. Two slash commands are available in Claude Code:

- **`/arquitectura`** — task-to-files router. Maps any development task to the exact reference files to read before coding.
- **`/arq-new`** — 13-step checklist for adding a new aggregate end-to-end.

Key files carry `// AGENT ENTRY POINT` and `// REFERENCE IMPLEMENTATION` comments so agents can orient from any cold-start entry point.

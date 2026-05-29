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

This codebase is designed for **AI-assisted development**. The navigation system solves the core problem of AI agents in large codebases: too much context, wrong files read, inconsistent output.

### The problem it solves

Without structure, an AI agent starting a new task either reads too many files (slow, expensive, dilutes focus) or too few (misses the established pattern and generates inconsistent code). The result is code that works but doesn't match the project's conventions, requiring human review and correction on every task.

### How it works

Three complementary layers keep agents oriented and consistent:

**1. Task-to-files router (`/arquitectura`)**  
A slash command that maps any development task directly to the exact files to read — no exploration, no guessing. The agent reads only what's relevant and starts from the correct pattern immediately.

```
Task: "add a query handler"
→ read GetOrderByIdQueryHandler.cs · GetOrdersQueryHandler.cs
→ implement following established pattern
```

**2. Reference implementation (Order aggregate)**  
`Order` is the permanent canonical template for all patterns in this project: UoW, CQRS handlers, domain invariants, controller binding, Dapper read/write. New aggregates are built *from* Order, not from scratch. The pattern stays consistent across all aggregates regardless of who (human or agent) adds them.

**3. AGENT ENTRY POINT comments**  
Key reference files carry structured comments that surface non-obvious rules at the point of reading — without requiring the agent to have read other files first:

```csharp
// ═══════════════════════════════════════════════════════════════
// AGENT ENTRY POINT — Reference query handler (single entity)
// REFERENCE IMPLEMENTATION — plantilla para query handlers con join.
//
// Rules:
//   - Inject IXReadRepository — never IUnitOfWork (read-only, no TX)
//   - No try-catch — exceptions propagate to GlobalExceptionHandler
//   - Collections always use PagedResult<T>
//   - Project to DTO here, not in the repository
```

This means an agent can enter from any file and still discover the governing rules — no dependency on a separate docs file being in context.

### Benefits

| Without this system | With this system |
|---|---|
| Agent reads 15+ files to orient | Agent reads 2–4 targeted files |
| Rules re-explained every session | Rules embedded in code, always in context |
| Inconsistent output across agents/sessions | Output mirrors the reference implementation |
| Human must review pattern compliance | Pattern is enforced at the reading stage |
| Navigation skill grows with the codebase | Router is O(1) — points to patterns, not entities |

### Commands

- **`/arquitectura`** — task-to-files router. Run this at the start of any development task in a cold session.
- **`/arq-new`** — 13-step end-to-end checklist for adding a new aggregate (domain entity → repository → handlers → controller → registration).

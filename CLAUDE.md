# AGENT EXECUTION POLICY — .NET 10 Microservices Runtime

---

## RUNTIME_DIRECTIVE

- Architectural runtime: `/arquitectura` (ARQ state machine) — source of truth for routing, reference loading, and state transitions.
- Topology is stable. Namespaces are stable. Do not rediscover either.
- Default mode: execute. Escalate only under conditions defined in ESCALATION_RULES.
- ARQ internals (REFERENCE_FILES, PATH_PATTERNS, DECISION_REPOSITORY) are not duplicated here.

---

## GLOBAL_INVARIANTS

- `DomainException` → `GlobalExceptionHandler` → HTTP 409 · log Warning
- `try-catch` forbidden in handlers except at the Dapper UoW TX boundary (see EXECUTION_RULES)
- Domain invariants live in the entity — never duplicated as guards in Application layer
- `sealed` · file-scoped namespaces · primary constructors · collection expressions `[]`
- One handler per command/query
- Handlers orchestrate only — no business logic
- FluentValidation registered as `IPipelineBehavior` — never inside handlers
- `IReadRepository<T>` is always used without a transaction
- `IWriteRepository<T>` is always used inside `IUnitOfWork`
- `public_id` (GUID) is the only identifier exposed in the API — never internal `id` (int/bigint)
- Pagination on collection-returning endpoints only when the feature requires it

---

## EXECUTION_RULES

### EF Core commands
```
load entity (tracked)  →  call domain method  →  repo.Update  →  SaveChangesAsync
```
- `SaveChangesAsync` = implicit transaction — no explicit TX block

### Dapper commands (writes)
```csharp
await unitOfWork.BeginTransactionAsync(ct);
try
{
    await unitOfWork.ExamplesWrite.OperationAsync(…, ct);
    await unitOfWork.CommitAsync(ct);
}
catch
{
    await unitOfWork.RollbackAsync(ct);
    throw;
}
```
- `throw` is mandatory — never swallow

### Domain method call
```
entity.DomainMethod()   // throws DomainException if invariant violated
                        // GlobalExceptionHandler maps → HTTP 409
                        // no pre-check, no guard, no Result.Failure before the call
```

### Reference loading
- ARQ S3_LOAD reads exactly 1 reference file — do not read additional files unless S_ERROR requires it
- S_ERROR budget: 1 Glob max + 1 Read max

---

## FORBIDDEN_PATTERNS

### Exploration
- Repository Glob/grep loops to discover structure
- Full-tree scans (`**/*.cs` without a specific target)
- Namespace rediscovery
- Reading files not specified by ARQ REFERENCE_FILES for the current operation

### Handler code
- Business logic inside handlers
- `try-catch` outside the Dapper UoW TX boundary
- Domain rule duplication as guards (e.g., `if (entity.Status == X) return Failure(…)` before calling a domain method)
- Direct instantiation of handlers
- Validation logic inside handlers (use `IPipelineBehavior`)

### Queries
- Lazy loading / navigation property access without explicit `includeProperties`
- N+1 patterns — use JOIN, `includeProperties`, or batch
- Unbounded collection queries without pagination

### API
- Exposing internal `id` (int/bigint) in responses or route parameters
- `try-catch` in controllers

### Infrastructure
- Adding NuGet packages not already in the stack (→ ESCALATION_RULES)
- Introducing a new architecture layer or cross-cutting abstraction without escalation

---

## DECISION_RULES

### Repository selection (generic-first — evaluate in order)

| When | Use |
|---|---|
| Predicate filter · exists · count · paginate | `IReadRepository<T>` |
| Read with children | `IReadRepository<T>` + `includeProperties:[e=>e.Children]` |
| Bulk delete/update without entity load | `IUnitOfWork.WriteRepository.DeleteManyAsync / UpdateManyAsync` |
| Write, change-tracked | `IUnitOfWork.ExamplesWrite` |
| ILike · complex join · filter not on generic surface | `IMyEntityReadRepository` (specific) |
| Write operation not on generic surface | `IMyEntityWriteRepository` (specific) |

### Service selection

| When | Use | Namespace |
|---|---|---|
| Standard lookup by publicId | `IExampleService.FindAsync / FindWithItemsAsync` | `Application.Services` |
| Custom predicate / projection | `IReadRepository<T>` | — |
| Command — tracked scalar | `IExampleService.FindTrackedAsync` | `Application.Services` |
| Cross-aggregate operation | `IExampleService.TransferItem / MergeInto` | `Application.Contracts.Interfaces` |

### DomainException timing
- Validate domain rules before opening a TX when possible — avoids needing rollback

---

## ESCALATION_RULES

Escalate to the user **only** when:

- A feature requires a NuGet package not present in the stack
- A structural change is required (new layer, new cross-cutting contract, new infrastructure component)
- Two valid approaches exist with no criterion to decide between them
- A security or performance risk is detected that existing patterns do not cover

Do **not** escalate for:
- Ambiguous naming
- Missing DTO fields
- Whether to add XML doc
- Standard CQRS wiring decisions covered by ARQ

---

## VALIDATION_RULES

After every file write:

```
dotnet test --no-restore -v q
```

- 0 build errors · 0 new test failures → DONE
- Any failure → S_ERROR: 1 Glob + 1 Read + diagnose → resume

---

## PERFORMANCE_RULES

- No N+1 — use `includeProperties`, JOIN, or batch
- Pagination: `GetListPaginatedAsync(currentPage, pageSize, predicate)` — use when the feature requires it, not by default
- Prefer Dapper for known-shape reads where performance is critical; EF for writes with migrations
- `Channel<T>` unbounded + `SingleReader = true` for background job workers
- Index on `public_id` — defined in DbContext, not assumed

---

## STACK_CONSTRAINTS

| Component | Technology | Version |
|---|---|---|
| Runtime | .NET | 10 (`net10.0`) |
| Language | C# | 14 |
| ORM | EF Core | 10 |
| Micro-ORM | Dapper | 2.1.x · `MatchNamesWithUnderscores = true` |
| RDBMS | PostgreSQL | 16 · Npgsql |
| Mediator | MediatR | 14.1 |
| Auth | JwtBearer | 10.0.x · HS256 |
| OpenAPI | Swashbuckle + Microsoft.OpenApi | 10.x / 2.x |
| Jobs | System.Threading.Channels | built-in |
| Observability | OpenTelemetry | 1.15.x |
| Tests | xUnit + Moq + FluentAssertions | latest |

- Use the most modern API available within the declared stack version
- Do not add packages that duplicate existing framework functionality

---

## TESTING_RULES

- Framework: xUnit · Mocks: Moq · Assertions: FluentAssertions
- `Microservice.Test` folder structure mirrors the project under test
- Scope: unit tests for handlers, validators, and domain logic
- Naming: `Method_Scenario_ExpectedResult`
- Types used as generic mock parameters (e.g. `IValidator<T>`) must be `public` at namespace level — Moq cannot proxy private or nested types under FluentValidation strong-named assemblies
- Do not test infrastructure wiring (DI registration, DbContext migrations) in unit tests

---

## CODE_CONVENTIONS

| Rule | Value |
|---|---|
| Public types / members | PascalCase |
| Private fields | `_camelCase` |
| SQL identifiers | snake_case |
| Inline comment prefix | `// ── N. Description ─────` |
| XML doc | Required on all public API surface |
| Async | All I/O methods async — `Async` suffix · `CancellationToken ct` parameter |

- `ArgumentException.ThrowIfNullOrWhiteSpace` over manual null guards
- `record` for DTOs (structural equality) · `class` for Dapper multi-map DTOs (public setters required)

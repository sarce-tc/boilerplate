# ARQ_NEW — Aggregate scaffold state machine

> Paths: inherit PROJECT_ROOT / APP_SRC / INFRA_SRC / API_SRC / DOMAIN_SRC from /arquitectura

---

## GLOBAL_INVARIANTS
- generic-first at every step — evaluate before creating specific contracts
- `DomainException` enforces invariants — no guard duplication in Application layer
- `sealed` · file-scoped namespaces · `[]` · primary constructors
- `SaveChangesAsync` = implicit EF TX — no explicit TX block in EF handlers
- Explicit TX (try/catch/rollback) ONLY in Dapper command handlers

---

## STATE: S0_INIT
GUARD:  aggregate name + tech (EF|Dapper) provided
ACTION: resolve MyEntity = provided name
NEXT:   → S1_EF      [tech == EF]
        → S1_DAPPER  [tech == Dapper]

---

# ── EF CORE PATH ─────────────────────────────────────────────────────────────

## STATE: S1_EF
ACTION: Read cmd.create + cmd.update from /arquitectura REFERENCE_FILES
NEXT:   → S2_DOMAIN

## STATE: S2_DOMAIN
ACTION: create `{DOMAIN_SRC}\Entities\MyEntity.cs`
        [if children] create `{DOMAIN_SRC}\Entities\MyEntityItem.cs`
RULES:  sealed · private setters · factory constructor · DomainException
        children: `_items = []` · `IReadOnlyList<TItem> Items => _items.AsReadOnly()`
        item: internal constructor · private setters · internal state methods
NEXT:   → S3_DB_CONTEXT

## STATE: S3_DB_CONTEXT
ACTION: edit `{INFRA_SRC}\Persistence\ExampleDbContext.cs`
        + `DbSet<MyEntity>` [+ `DbSet<MyEntityItem>`]
        + `HasIndex(e=>e.PublicId).IsUnique()` · `HasMaxLength` · `IsRequired`
        [if children] + `Navigation(e=>e.Items).HasField("_items").UsePropertyAccessMode(Field)`
        run: dotnet ef migrations add Add_MyEntity --project Microservice.Infrastructure --startup-project Microservice.API
             dotnet ef database update --project Microservice.Infrastructure --startup-project Microservice.API
NEXT:   → S4_CONTRACTS

## STATE: S4_CONTRACTS
GUARD:  domain entity created
ACTION: evaluate each independently:
        `IMyEntityReadRepository`:  ONLY IF → ILike · complex join · filter not in `IReadRepository<T>`
        `IMyEntityWriteRepository`: ONLY IF → write operation not in `IWriteRepository<T>`
        `IUnitOfWork`:              ADD lazy property ONLY IF IMyEntityWriteRepository created
NEXT:   → S5_APPLICATION

## STATE: S5_APPLICATION
ACTION: create DTOs — `record` (structural equality) | `class` (Dapper multi-map)
        edit `{APP_SRC}\Mapping\MappingProfile.cs` → `CreateMap<Command, Entity>().ConstructUsing(…)`
        create Commands per COMMAND_PATTERNS
        create Queries per QUERY_PATTERNS
        create Validators (FluentValidation · auto-registered via IPipelineBehavior scan)
NEXT:   → S6_INFRA_REPOS

## STATE: S6_INFRA_REPOS
GUARD:  S4_CONTRACTS evaluated
ACTION: [if IMyEntityReadRepository created]  → create `MyEntityReadRepository : LINQRepository<MyEntity>`
        [if IMyEntityWriteRepository created] → create `MyEntityWriteRepository : LINQRepository<MyEntity>`
        [if none created]                     → skip
NEXT:   → S7_DI

## STATE: S7_DI
ACTION: edit `{INFRA_SRC}\InfrastuctureServiceRegistration.cs`
        [if ReadRepository created]  + `AddScoped<IMyEntityReadRepository, MyEntityReadRepository>()`
        [if Service created]         + `AddScoped<I{Service}, {Service}>()`
        WriteRepository: instantiated inside UoW only — not registered directly
NEXT:   → S8_API

## STATE: S8_API
ACTION: create `{API_SRC}\Controllers\MyEntityController.cs`
        thin · `MediatR.Send` · `result.ToActionResult()`
NEXT:   → S9_VALIDATE

---

# ── DAPPER PATH ──────────────────────────────────────────────────────────────

## STATE: S1_DAPPER
ACTION: Read dapper.base-read + dapper.base-write + dapper.uow-concrete + dapper.cmd.create
        from /arquitectura REFERENCE_FILES (dapper.cmd.create = canonical TX handler shape)
NEXT:   → S2_DOMAIN [same as EF]

## STATE: S3_DAPPER_CONTRACTS
GUARD:  domain entity created
ACTION: create `IMyEntityReadRepository` + `IMyEntityWriteRepository` (always required — no generic surface)
        edit `IUnitOfWork` → add `IMyEntityWriteRepository MyEntityWrite { get; }`
NEXT:   → S4_DAPPER_APPLICATION

## STATE: S4_DAPPER_APPLICATION
ACTION: create DTOs with public setters · Commands · Queries · Validators
TX_RULE (commands with writes):
        await unitOfWork.BeginTransactionAsync(ct);
        try { /* ... */ await unitOfWork.CommitAsync(ct); }
        catch { await unitOfWork.RollbackAsync(ct); throw; }
NEXT:   → S5_DAPPER_REPOS

## STATE: S5_DAPPER_REPOS
ACTION: create ReadRepository — SQL SELECT · `MatchNamesWithUnderscores=true` · multi-map for joins
        create WriteRepository — TWO constructors:
          · DI: no TX
          · UoW: `(IDbConnection, IDbTransaction)` — pass `_transaction` on every SQL call
NEXT:   → S6_DAPPER_UOW

## STATE: S6_DAPPER_UOW
ACTION: edit `{INFRA_SRC}\Repositories\Dapper\UnitOfWork.cs`
        + lazy: `_myEntity ??= new MyEntityWriteRepository(_connection!, _transaction!)`
NEXT:   → S7_DAPPER_DI

## STATE: S7_DAPPER_DI
ACTION: edit `{INFRA_SRC}\InfrastuctureServiceRegistration.cs`
        + `AddScoped<IMyEntityReadRepository, MyEntityReadRepository>()`
        + `AddScoped<IMyEntityWriteRepository, MyEntityWriteRepository>()`
NEXT:   → S8_API [same as EF]

---

# ── SHARED STATES ────────────────────────────────────────────────────────────

## STATE: S9_VALIDATE
GUARD:  files written
ACTION: dotnet test --no-restore -v q
NEXT:   → DONE    [0 errors · 0 new failures]
        → S_ERROR  [compile error | test failure]

## STATE: S_ERROR
GUARD:  compile error | test failure | unknown pattern
ACTION: Glob(target_directory) — 1 glob max
        Read(1 corrective file) — 1 read max
        diagnose → resume failed state
NEXT:   → failed state

## STATE: DONE [terminal]

---

## COMMAND_PATTERNS

| Type | Load | Domain | Persist |
|---|---|---|---|
| Create | — | `new MyEntity(…)` · `AddItem()` per child | `AddAsync` → `SaveChanges` |
| Update full (PUT) | `GetEntityAsync(includeProperties, disableTracking:false)` | `UpdateName / AddItem / RemoveItem` | `Update` → `SaveChanges` |
| Update partial (PATCH) | `GetEntityAsync(disableTracking:true)` | domain methods | `UpdateFields([x=>x.Prop])` → `SaveChanges` |
| Delete | `GetEntityAsync` | — | `Delete` → `SaveChanges` |
| Delete bulk | — | — | `WriteRepository.DeleteManyAsync(predicate)` → `SaveChanges` |
| Update bulk | — | — | `WriteRepository.UpdateManyAsync(predicate, values)` → `SaveChanges` |
| Domain service | load N aggregates (tracked) | `IDomainService.Method()` | `Update × N` → `SaveChanges` |

## QUERY_PATTERNS

| Type | Inject | Load |
|---|---|---|
| By publicId | `IReadRepository<T>` or `IExampleService` | `GetEntityAsync(x=>x.PublicId==id)` |
| By predicate | `IReadRepository<T>` | `GetListAsync(predicate)` |
| Paginated | `IReadRepository<T>` | `GetListPaginatedAsync(page, size, predicate)` |
| With children | `IReadRepository<T>` or `IExampleService` | `GetEntityAsync(predicate, includeProperties:[e=>e.Children])` |
| Child collection | `IReadRepository<T>` | `GetEntityAsync(includeProperties)` → map `entity.Items` |
| Single child | `IReadRepository<T>` | `GetEntityAsync(includeProperties)` → `Items.FirstOrDefault(i=>i.PublicId==id)` |
| Computed summary | `IExampleService` | `FindWithItemsAsync` → construct DTO manually (skip mapper) |

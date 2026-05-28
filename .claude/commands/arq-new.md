# ARQ_NEW — Aggregate scaffold protocol

## SKILL_IDENTITY
PURPOSE: Deterministic end-to-end scaffold for a new aggregate
REFERENCE: /arquitectura (routing) · CLAUDE.md (invariants)
DEFAULT: no exploration · expand templates · evaluate conditionals · execute

---

## GLOBAL_INVARIANTS
- generic-first at every step — evaluate before creating specific contracts
- `DomainException` enforces invariants — no guard duplication in Application layer
- `sealed` · file-scoped namespaces · collection expressions `[]` · primary constructors
- `SaveChangesAsync` = implicit EF TX — no explicit TX block in EF handlers
- Explicit TX (try/catch/rollback) ONLY in Dapper command handlers

---

## MODE: EF_CORE

ARCHETYPE: Example + ExampleItem
READ_FIRST: `CreateExampleCommandHandler.cs` · `UpdateExampleCommandHandler.cs`

### EXECUTION_SEQUENCE

```
1. DOMAIN
   Entity:   sealed · private setters · factory constructor · DomainException
   Children: _items = [] · IReadOnlyList<TItem> Items => _items.AsReadOnly()
   Item:     sealed · internal constructor · private setters · internal state methods

2. DB_CONTEXT
   Add:      DbSet<MyEntity> [+ DbSet<MyEntityItem>]
   Config:   HasIndex(e=>e.PublicId).IsUnique() · HasMaxLength · IsRequired
   Children: Navigation(e=>e.Items).HasField("_items").UsePropertyAccessMode(Field)
   Migrate:  dotnet ef migrations add Add_MyEntity --project Infrastructure --startup API
             dotnet ef database update --project Infrastructure --startup API

3. CONTRACTS  [conditional — evaluate each independently]
   IMyEntityReadRepository:  ONLY IF → ILike · complex join · filter not in IReadRepository<T>
   IMyEntityWriteRepository: ONLY IF → write operation not in IWriteRepository<T>
   IUnitOfWork:              ADD property ONLY IF IMyEntityWriteRepository created

4. APPLICATION
   DTOs:       record (structural equality) | class (Dapper multi-map)
   Mapping:    MappingProfile → CreateMap<Command, Entity>().ConstructUsing(…)
   Commands:   [see COMMAND_PATTERNS]
   Queries:    [see QUERY_PATTERNS]
   Validators: FluentValidation · auto-registered via IPipelineBehavior scan

5. INFRA_REPOS  [conditional — create only if contract created in §3]
   ReadRepository:  sealed · LINQRepository<MyEntity> · IMyEntityReadRepository
   WriteRepository: sealed · LINQRepository<MyEntity> · IMyEntityWriteRepository

6. DI
   ReadRepository:  1 line InfrastructureServiceRegistration (if created)
   WriteRepository: instantiated inside UoW only — not registered directly
   Service:         1 line InfrastructureServiceRegistration (if I{Service} created)

7. API
   Controller: thin · MediatR.Send · result.ToActionResult()
```

### COMMAND_PATTERNS

| Type | Load | Domain | Persist |
|---|---|---|---|
| Create | — | `new MyEntity(…)` · `AddItem()` per child | `AddAsync` → `SaveChanges` |
| Update full (PUT) | `GetEntityAsync(includeProperties, disableTracking:false)` | `UpdateName / AddItem / RemoveItem` | `Update` → `SaveChanges` |
| Update partial (PATCH) | `GetEntityAsync(disableTracking:true)` | domain methods | `UpdateFields([x=>x.Prop])` → `SaveChanges` |
| Delete | `GetEntityAsync` | — | `Delete` → `SaveChanges` |
| Delete bulk | — | — | `WriteRepository.DeleteManyAsync(predicate)` → `SaveChanges` |
| Update bulk | — | — | `WriteRepository.UpdateManyAsync(predicate, values)` → `SaveChanges` |
| Domain service op | load both aggregates (tracked) | `IDomainService.Method()` | `Update × N` → `SaveChanges` |

### QUERY_PATTERNS

| Type | Inject | Load |
|---|---|---|
| By publicId (scalar) | `IReadRepository<T>` or `IExampleService` | `GetEntityAsync(x=>x.PublicId==id)` |
| Paginated | `IReadRepository<T>` | `GetListPaginatedAsync(page, size, predicate)` |
| With children | `IReadRepository<T>` or `IExampleService` | `GetEntityAsync(predicate, includeProperties:[e=>e.Children])` |
| Child collection | `IReadRepository<T>` | `GetEntityAsync(includeProperties)` → map `entity.Items` |
| Single child | `IReadRepository<T>` | `GetEntityAsync(includeProperties)` → `Items.FirstOrDefault(i=>i.PublicId==id)` |
| Computed summary | `IExampleService` | `FindWithItemsAsync` → construct DTO manually (skip mapper if computed fields) |

---

## MODE: DAPPER

ARCHETYPE: Example (Dapper) — target path Features/ExamplesDapper/
READ_FIRST: `Infrastructure/Repositories/Dapper/ReadRepository.cs` · `WriteRepository.cs` · `UnitOfWork.cs`

### EXECUTION_SEQUENCE

```
1. DOMAIN      — same as EF §1
2. CONTRACTS   — IMyEntityReadRepository + IMyEntityWriteRepository (always required — no generic surface)
                 IUnitOfWork → add: IMyEntityWriteRepository MyEntityWrite { get; }
3. APPLICATION — DTOs with public setters · Commands · Queries · Validators
4. REPOS
   Read:  SQL SELECT · MatchNamesWithUnderscores=true · multi-map for joins
   Write: TWO constructors — DI (no TX) | UoW (IDbConnection + IDbTransaction)
          pass _transaction explicitly on every SQL call
5. UoW:  lazy: _myEntity ??= new MyEntityWriteRepository(_connection!, _transaction!)
6. DI:   2 lines InfrastructureServiceRegistration
7. API:  same as EF §7
```

### TX_PATTERN (Dapper only)

```csharp
await unitOfWork.BeginTransactionAsync(ct);
try { /* ... */ await unitOfWork.CommitAsync(ct); }
catch { await unitOfWork.RollbackAsync(ct); throw; }
```

---

## EXPLORATION_PROTOCOL

DEFAULT: disabled

TRIGGER_IF:
- Step requires pattern not in COMMAND_PATTERNS / QUERY_PATTERNS
- Compile error indicates contract divergence

BOUND: Read 1 file from /arquitectura REFERENCE_FILES matching task label

# ARQ — Routing protocol

## SKILL_IDENTITY
PURPOSE: Route agent to correct reference file without codebase exploration
CACHES: namespace topology · repository contracts · CQRS archetype locations · DI registration
DEFAULT: no exploration · no grep loops · no pattern discovery

---

## GLOBAL_INVARIANTS
- Namespaces stable — do not rediscover
- generic-first always: `IReadRepository<T>` → `IUnitOfWork.WriteRepository` → specific contracts
- `Example` (EF) + `Example` (Dapper) = canonical archetypes — do not degrade
- `DomainException` → `GlobalExceptionHandler` → HTTP 409 — no try/catch in handlers
- `sealed` on all handlers · validators · repos · exceptions

---

## DECISION: Repository

| Scenario | Use |
|---|---|
| Read by predicate · paginate · exists · count | `IReadRepository<T>` |
| Read with children | `IReadRepository<T>` + `includeProperties:[e=>e.Children]` |
| Write, change-tracked | `IUnitOfWork.ExamplesWrite` or `WriteRepository` |
| Bulk delete/update without loading | `IUnitOfWork.WriteRepository.DeleteManyAsync / UpdateManyAsync` |
| ILike · complex join · business filter not in generic surface | `IMyEntityReadRepository` |
| Custom write not in generic surface | `IMyEntityWriteRepository` |

## DECISION: Service injection

| Context | Inject |
|---|---|
| Query — standard lookup by publicId | `IExampleService` → `FindAsync` / `FindWithItemsAsync` |
| Query — custom predicate / projection | `IReadRepository<T>` |
| Command — tracked scalar (no children) | `IExampleService.FindTrackedAsync` |
| Command — tracked + children | `IReadRepository<T>` (`includeProperties`, `disableTracking:false`) |

---

## PATH_PATTERNS

```
Commands:     Application/Features/{Aggregate}/Commands/{Action}/{Action}Command(Handler|Validator).cs
Queries:      Application/Features/{Aggregate}/Queries/{Action}/{Action}Query(Handler).cs
DTOs:         Application/DTOs/{Dto}Dto.cs
Repos EF:     Infrastructure/Repositories/EF/{Entity}(Read|Write)Repository.cs
Repos Dapper: Infrastructure/Repositories/Dapper/{Entity}(Read|Write)Repository.cs
Services:     Application/Contracts/Interfaces/I{Service}.cs  ·  Infrastructure/Services/{Service}.cs
Controllers:  API/Controllers/{Aggregate}Controller.cs
```

---

## REFERENCE_FILES (read only when task label matches)

| Task | File |
|---|---|
| EF entity + children | `Domain/Entities/Example.cs` · `Domain/Entities/ExampleItem.cs` |
| EF generic contracts | `Application/Contracts/Persistence/EF/ILINQRepository.cs` |
| EF UoW contract | `Application/Contracts/Persistence/EF/IUnitOfWork.cs` |
| EF specific write contract | `Application/Contracts/Persistence/EF/IExampleWriteRepository.cs` |
| EF specific read contract | `Application/Contracts/Persistence/EF/IExampleReadRepository.cs` |
| EF base repo | `Infrastructure/Repositories/EF/LINQRepository.cs` |
| EF write repo | `Infrastructure/Repositories/EF/ExampleWriteRepository.cs` |
| EF read repo | `Infrastructure/Repositories/EF/ExampleReadRepository.cs` |
| EF UoW concrete | `Infrastructure/Repositories/EF/UnitOfWork.cs` |
| DbContext | `Infrastructure/Persistence/ExampleDbContext.cs` |
| Command: Create | `Features/ExamplesEF/Commands/CreateExample/CreateExampleCommandHandler.cs` |
| Command: Update full | `Features/ExamplesEF/Commands/UpdateExample/UpdateExampleCommandHandler.cs` |
| Command: Update partial | `Features/ExamplesEF/Commands/UpdateExampleFields/UpdateExampleFieldsCommandHandler.cs` |
| Command: Delete | `Features/ExamplesEF/Commands/DeleteExample/DeleteExampleCommandHandler.cs` |
| Command: Delete bulk | `Features/ExamplesEF/Commands/DeleteManyExamples/DeleteManyExamplesCommandHandler.cs` |
| Command: Update bulk | `Features/ExamplesEF/Commands/UpdateManyExamples/UpdateManyExamplesCommandHandler.cs` |
| Query: by predicate | `Features/ExamplesEF/Queries/GetExampleByPredicate/GetExampleByPredicateQueryHandler.cs` |
| Query: paginated | `Features/ExamplesEF/Queries/GetExamplesPaginated/GetExamplesPaginatedQueryHandler.cs` |
| Query: aggregate + children | `Features/ExamplesEF/Queries/GetExampleWithItems/GetExampleWithItemsQueryHandler.cs` |
| Query: child collection | `Features/ExamplesEF/Queries/GetExampleItems/GetExampleItemsQueryHandler.cs` |
| Query: single child | `Features/ExamplesEF/Queries/GetExampleItemByPublicId/GetExampleItemByPublicIdQueryHandler.cs` |
| Mapping | `Application/Mapping/MappingProfile.cs` |
| Dapper UoW contract | `Application/Contracts/Persistence/Dapper/IUnitOfWork.cs` |
| Dapper repo base | `Infrastructure/Repositories/Dapper/ReadRepository.cs` · `WriteRepository.cs` |
| Dapper UoW concrete | `Infrastructure/Repositories/Dapper/UnitOfWork.cs` |
| Controller + result mapping | `API/Controllers/ExamplesEFController.cs` · `API/Extensions/ResultExtensions.cs` |
| Error handling | `API/ExceptionHandling/GlobalExceptionHandler.cs` |
| DI registration | `Infrastructure/InfrastuctureServiceRegistration.cs` |
| App services (name given at runtime) | Glob `Infrastructure/Services/` → read matching file |

---

## DIRECTORY (compact)

```
Domain/
  Entities/    Example.cs ExampleItem.cs ExampleStatus.cs ExampleItemStatus.cs
  Exceptions/  DomainException.cs
  Services/    IExampleDomainService.cs
  ValueObjects/ BaseDomainModel.cs

Application/
  Contracts/
    Interfaces/          I{Service}.cs
    Persistence/EF/      ILINQRepository.cs IUnitOfWork.cs ISqlRepository.cs
                         IExampleWriteRepository.cs IExampleReadRepository.cs
    Persistence/Dapper/  IUnitOfWork.cs IReadRepository.cs IWriteRepository.cs
  Features/
    ExamplesEF/      Commands/ Queries/
    ExamplesDapper/  Commands/ Queries/  ← planned
  Mapping/  MappingProfile.cs

Infrastructure/
  Persistence/      ExampleDbContext.cs Migrations/
  Repositories/
    EF/     LINQRepository.cs SqlRepository.cs
            ExampleWriteRepository.cs ExampleReadRepository.cs UnitOfWork.cs
    Dapper/ ReadRepository.cs WriteRepository.cs UnitOfWork.cs
            ExampleReadRepository.cs ExampleWriteRepository.cs  ← planned
  Services/  {Service}.cs

API/
  Controllers/       ExamplesEFController.cs ExamplesDapperController.cs  ← planned
  ExceptionHandling/ GlobalExceptionHandler.cs
  Extensions/        ResultExtensions.cs
```

---

## EXPLORATION_PROTOCOL

DEFAULT: disabled

TRIGGER_IF:
- Compile error indicates contract divergence
- Task requires pattern not covered by DECISION tables
- Bounded context boundary is ambiguous
- File path doesn't match PATH_PATTERNS

BOUND: Glob target directory only · Read 1 file per task label · No grep loops · No full-tree scan

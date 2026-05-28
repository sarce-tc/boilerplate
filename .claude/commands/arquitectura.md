# ARQ — Architectural State Machine

PROJECT_ROOT: C:\Users\SantiagoArce\source\repos\boilerplate
APP_SRC:      {PROJECT_ROOT}\Microservice.Application
INFRA_SRC:    {PROJECT_ROOT}\Microservice.Infrastructure
API_SRC:      {PROJECT_ROOT}\Microservice.API
DOMAIN_SRC:   {PROJECT_ROOT}\Microservice.Domain

---

## GLOBAL_INVARIANTS
- Namespaces stable — do not rediscover
- generic-first: `IReadRepository<T>` → `IUnitOfWork.WriteRepository` → specific contracts
- `Example` (EF) = canonical archetype — do not degrade
- `DomainException` → `GlobalExceptionHandler` → HTTP 409 — no try/catch in handlers
- `sealed` · file-scoped namespaces · `[]` · primary constructors

---

## STATE: S0_INIT
GUARD:  task received
ACTION: parse → resolve tech (EF|Dapper) × operation (Command|Query|Service|DI)
        detect session_temperature: warm = reference files in context | cold
NEXT:   → S1_CLASSIFY

## STATE: S1_CLASSIFY
GUARD:  tech + operation resolved
ACTION: apply DECISION_REPOSITORY → emit repository_contract
        apply DECISION_SERVICE    → emit service_contract
        lookup REFERENCE_FILES[operation] → emit reference_path
NEXT:   → S2_WARM  [session_temperature == warm]
        → S3_LOAD   [session_temperature == cold]

### DECISION_REPOSITORY
| Scenario | Contract |
|---|---|
| Read by predicate · paginate · exists · count | `IReadRepository<T>` |
| Read with children | `IReadRepository<T>` + `includeProperties:[e=>e.Children]` |
| Write, change-tracked | `IUnitOfWork.ExamplesWrite` or `WriteRepository` |
| Bulk delete/update without loading | `IUnitOfWork.WriteRepository.DeleteManyAsync / UpdateManyAsync` |
| ILike · complex join · business filter not in generic surface | `IMyEntityReadRepository` |
| Custom write not in generic surface | `IMyEntityWriteRepository` |

### DECISION_SERVICE
| Context | Contract | Namespace |
|---|---|---|
| Query — standard lookup by publicId | `IExampleService` → `FindAsync` / `FindWithItemsAsync` | `Application.Services` |
| Query — custom predicate / projection | `IReadRepository<T>` | — |
| Command — tracked scalar | `IExampleService.FindTrackedAsync` | `Application.Services` |
| Command — tracked + children | `IReadRepository<T>` (`includeProperties`, `disableTracking:false`) | — |
| Cross-aggregate operation | `IExampleService` → `TransferItem` / `MergeInto` | `Application.Contracts.Interfaces` |

### REFERENCE_FILES
| operation | path |
|---|---|
| ef.contracts | `{APP_SRC}\Contracts\Persistence\EF\ILINQRepository.cs` |
| ef.uow | `{APP_SRC}\Contracts\Persistence\EF\IUnitOfWork.cs` |
| ef.read-contract | `{APP_SRC}\Contracts\Persistence\EF\IExampleReadRepository.cs` |
| ef.write-contract | `{APP_SRC}\Contracts\Persistence\EF\IExampleWriteRepository.cs` |
| ef.base-repo | `{INFRA_SRC}\Repositories\EF\LINQRepository.cs` |
| ef.uow-concrete | `{INFRA_SRC}\Repositories\EF\UnitOfWork.cs` |
| ef.dbcontext | `{INFRA_SRC}\Persistence\ExampleDbContext.cs` |
| ef.entity | `{DOMAIN_SRC}\Entities\Example.cs` |
| ef.entity-child | `{DOMAIN_SRC}\Entities\ExampleItem.cs` |
| cmd.create | `{APP_SRC}\Features\ExamplesEF\Commands\CreateExample\CreateExampleCommandHandler.cs` |
| cmd.update | `{APP_SRC}\Features\ExamplesEF\Commands\UpdateExample\UpdateExampleCommandHandler.cs` |
| cmd.update-fields | `{APP_SRC}\Features\ExamplesEF\Commands\UpdateExampleFields\UpdateExampleFieldsCommandHandler.cs` |
| cmd.delete | `{APP_SRC}\Features\ExamplesEF\Commands\DeleteExample\DeleteExampleCommandHandler.cs` |
| cmd.delete-bulk | `{APP_SRC}\Features\ExamplesEF\Commands\DeleteManyExamples\DeleteManyExamplesCommandHandler.cs` |
| cmd.update-bulk | `{APP_SRC}\Features\ExamplesEF\Commands\UpdateManyExamples\UpdateManyExamplesCommandHandler.cs` |
| query.predicate | `{APP_SRC}\Features\ExamplesEF\Queries\GetExampleByPredicate\GetExampleByPredicateQueryHandler.cs` |
| query.paginated | `{APP_SRC}\Features\ExamplesEF\Queries\GetExamplesPaginated\GetExamplesPaginatedQueryHandler.cs` |
| query.with-children | `{APP_SRC}\Features\ExamplesEF\Queries\GetExampleWithItems\GetExampleWithItemsQueryHandler.cs` |
| query.child-list | `{APP_SRC}\Features\ExamplesEF\Queries\GetExampleItems\GetExampleItemsQueryHandler.cs` |
| query.child-single | `{APP_SRC}\Features\ExamplesEF\Queries\GetExampleItemByPublicId\GetExampleItemByPublicIdQueryHandler.cs` |
| mapping | `{APP_SRC}\Mapping\MappingProfile.cs` |
| di | `{INFRA_SRC}\InfrastuctureServiceRegistration.cs` |
| controller | `{API_SRC}\Controllers\ExamplesEFController.cs` |
| result-ext | `{API_SRC}\Extensions\ResultExtensions.cs` |
| error-handler | `{API_SRC}\ExceptionHandling\GlobalExceptionHandler.cs` |
| dapper.uow | `{APP_SRC}\Contracts\Persistence\Dapper\IUnitOfWork.cs` |
| dapper.base-read | `{INFRA_SRC}\Repositories\Dapper\ReadRepository.cs` |
| dapper.base-write | `{INFRA_SRC}\Repositories\Dapper\WriteRepository.cs` |
| dapper.uow-concrete | `{INFRA_SRC}\Repositories\Dapper\UnitOfWork.cs` |
| svc.lookup | `{APP_SRC}\Services\IExampleService.cs` · `{APP_SRC}\Services\ExampleService.cs` |
| svc.domain | Glob `{INFRA_SRC}\Services\` → read matching file |

---

## STATE: S2_WARM
GUARD:  REFERENCE_FILES[operation] already in context
ACTION: expand template from memory · write files per PATH_PATTERNS · apply GLOBAL_INVARIANTS
NEXT:   → S5_VALIDATE

## STATE: S3_LOAD
GUARD:  operation ∈ REFERENCE_FILES
ACTION: Read(REFERENCE_FILES[operation]) — exactly 1 file
NEXT:   → S4_EXECUTE

## STATE: S4_EXECUTE
GUARD:  reference loaded · GLOBAL_INVARIANTS verified
ACTION: create/edit files per PATH_PATTERNS
NEXT:   → S5_VALIDATE

## STATE: S5_VALIDATE
GUARD:  files written
ACTION: dotnet test --no-restore -v q
NEXT:   → DONE    [0 errors · 0 new failures]
        → S_ERROR  [compile error | test failure]

## STATE: S_ERROR
GUARD:  unknown pattern | path mismatch | contract divergence | test failure
ACTION: Glob(target_directory) — 1 glob max
        Read(1 corrective file) — 1 read max
        diagnose root cause
NEXT:   → S4_EXECUTE [diagnosis complete]
        → S3_LOAD    [different reference needed]

## STATE: DONE [terminal]

---

## PATH_PATTERNS
```
Commands:     {APP_SRC}\Features\{Aggregate}\Commands\{Action}\{Action}Command(Handler|Validator).cs
Queries:      {APP_SRC}\Features\{Aggregate}\Queries\{Action}\{Action}Query(Handler).cs
DTOs:         {APP_SRC}\DTOs\{Dto}Dto.cs
Repos EF:     {INFRA_SRC}\Repositories\EF\{Entity}(Read|Write)Repository.cs
Repos Dapper: {INFRA_SRC}\Repositories\Dapper\{Entity}(Read|Write)Repository.cs
Services:     {APP_SRC}\Contracts\Interfaces\I{Service}.cs · {INFRA_SRC}\Services\{Service}.cs
Controllers:  {API_SRC}\Controllers\{Aggregate}Controller.cs
```

## DIRECTORY (compact)
```
{DOMAIN_SRC}\
  Entities\    Example.cs ExampleItem.cs ExampleStatus.cs ExampleItemStatus.cs
  Exceptions\  DomainException.cs
  Services\    IExampleDomainService.cs
  ValueObjects\ BaseDomainModel.cs

{APP_SRC}\
  Contracts\
    Interfaces\          I{Service}.cs  ← domain services (cross-aggregate; implemented in Infrastructure)
    Persistence\EF\      ILINQRepository.cs IUnitOfWork.cs ISqlRepository.cs
                         IExampleWriteRepository.cs IExampleReadRepository.cs
    Persistence\Dapper\  IUnitOfWork.cs IReadRepository.cs IWriteRepository.cs
  Features\
    ExamplesEF\      Commands\ Queries\
    ExamplesDapper\  Commands\ Queries\  ← planned
  Mapping\  MappingProfile.cs
  Services\  IExampleService.cs ExampleService.cs  ← lookup services (query helpers; implemented in Application)

{INFRA_SRC}\
  Persistence\      ExampleDbContext.cs Migrations\
  Repositories\
    EF\     LINQRepository.cs SqlRepository.cs
            ExampleWriteRepository.cs ExampleReadRepository.cs UnitOfWork.cs
    Dapper\ ReadRepository.cs WriteRepository.cs UnitOfWork.cs
            ExampleReadRepository.cs ExampleWriteRepository.cs  ← planned
  Services\  {Service}.cs

{API_SRC}\
  Controllers\       ExamplesEFController.cs ExamplesDapperController.cs  ← planned
  ExceptionHandling\ GlobalExceptionHandler.cs
  Extensions\        ResultExtensions.cs
```

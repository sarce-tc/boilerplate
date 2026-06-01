# ARQ — Architectural State Machine

PROJECT_ROOT: the repository root = current working directory.
              Do NOT hard-code an absolute path — it varies per machine/user.
              All paths below are relative to PROJECT_ROOT and resolve from the CWD.
APP_SRC:      Microservice.Application
INFRA_SRC:    Microservice.Infrastructure
API_SRC:      Microservice.API
DOMAIN_SRC:   Microservice.Domain

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
        detect task_scope:
          · SINGLE  = una sola operación (1 command/query/service/DI)
          · PROGRAM = ≥2 aggregates, o un vertical completo (entity + CRUD + queries + API),
                      o ≥2 operaciones que comparten el mismo archetype
NEXT:   → S_WARMUP    [task_scope == PROGRAM · session cold]
        → S1_CLASSIFY  [otherwise]

## STATE: S_WARMUP   [program preflight — corre A LO SUMO UNA VEZ por sesión]
GUARD:  task_scope == PROGRAM · session cold
ACTION: Read del ARCHETYPE_SET en UN solo lote. Es carga determinista de referencias, NO exploración:
        el set está enumerado y es finito — sin Glob, sin full-tree scan, sin rediscovery.
        Tras el lote: session_temperature := warm para TODOS los aggregates del programa.
        El loop por aggregate pasa a ser S1_CLASSIFY → S2_WARM (cero re-lecturas).
        Bajar a S3_LOAD solo para un contract que el ARCHETYPE_SET no cubra (p.ej. specific-filter).
NEXT:   → S1_CLASSIFY

### ARCHETYPE_SET — el vertical canónico, leído UNA vez para un programa
EF:     ef.entity · ef.entity-child · ef.dbcontext · ef.uow · ef.uow-concrete · ef.contracts ·
        ef.base-repo · cmd.create · cmd.update · query.predicate · query.with-children ·
        query.paginated · mapping · controller · di
Dapper: + dapper.base-read · dapper.base-write · dapper.uow-concrete · dapper.cmd.create
(Los paths salen de REFERENCE_FILES. Una vez en contexto, NO se vuelven a leer en el programa.)

## STATE: S1_CLASSIFY
GUARD:  tech + operation resolved
ACTION: La decisión va ANTES de cargar el ejemplar. El orden es obligatorio.
        STEP 1 — DECIDIR repository_contract vía DECISION_REPOSITORY (es un BRANCH, no una nota).
                 La superficie genérica es AMPLIA — agótala antes de considerar SPECIFIC:
                   · Lecturas: CUALQUIER predicado, incluido `EF.Functions.ILike(e.Prop, x)` dentro
                     del Expression; Include/eager-load; proyección; paginación; Exists; Count.
                   · Escrituras: Add · Update · UpdateFields(PATCH) · Delete ·
                     DeleteManyAsync(predicate) · UpdateManyAsync(filter, ExecuteUpdateAsync).
                     → un UPDATE/DELETE set-based con filtro ILike ES GENÉRICO (UpdateManyAsync).
                 Pregunta discriminante (responde SÍ solo si NO cabe en lo de arriba):
                 «¿requiere SQL crudo · cláusula RETURNING · JOIN/subconsulta no expresable como
                   predicate+Include · lógica de dominio multi-statement?»
                   · no  → repository_contract = GENÉRICO  (`IReadRepository<T>` / `WriteRepository`)
                   · sí  → repository_contract = ESPECÍFICO (`IMyEntityReadRepository` / `IMyEntityWriteRepository`)
                 Excepción de CONVENCIÓN (opcional, NO necesidad): encapsular una consulta ILike
                 reutilizable como método nombrado en el repo específico — es calidad, no capacidad.
        STEP 2 — DECIDIR service_contract vía DECISION_SERVICE.
        STEP 3 — SELECCIONAR reference_path = REFERENCE_FILES[operation, repository_contract].
                 El ejemplar cargado DEBE coincidir con el contract de STEP 1:
                 genérico → ejemplar genérico · específico → ejemplar específico.
                 Cuando una operación tiene rama genérica y específica, elegir la fila que
                 corresponde a la decisión — no la primera.
NEXT:   → S2_WARM  [session_temperature == warm]
        → S3_LOAD   [session_temperature == cold]

### DECISION_REPOSITORY
GUARD aplicado en S1_CLASSIFY STEP 1 — NO es tabla de consulta. Cada fila decide el `contract`
Y la fila de REFERENCE_FILES que se cargará. Genérico y específico cargan ejemplares DISTINTOS.
La superficie genérica es amplia: la mayoría de casos (incluido ILike y set-based) son GENÉRICOS.

| Scenario | Contract | Ejemplar (REFERENCE_FILES) |
|---|---|---|
| Read por predicado (incl. `EF.Functions.ILike`) · paginate · exists · count | `IReadRepository<T>` (genérico) | `query.predicate` |
| Read con hijos (eager-load) | `IReadRepository<T>` + `includeProperties:[e=>e.Children]` | `query.with-children` |
| Write change-tracked (Add · Update · UpdateFields · Delete) | `IUnitOfWork.ExamplesWrite` / `WriteRepository` (genérico) | `cmd.create` / `cmd.update` |
| Bulk set-based UPDATE/DELETE (incl. filtro ILike + ExecuteUpdate) | `WriteRepository.DeleteManyAsync / UpdateManyAsync` (genérico) | `cmd.delete-bulk` / `cmd.update-bulk` |
| Read con **JOIN/subconsulta NO expresable** como predicate+Include | `IMyEntityReadRepository` (específico) | `query.specific-filter` |
| Write con **SQL crudo · RETURNING · lógica multi-statement** de dominio | `IMyEntityWriteRepository` (específico) | sin arquetipo — derivar de `ef.base-repo` |
| (convención OPCIONAL) encapsular ILike reutilizable como método nombrado | `IMyEntityReadRepository` | `query.specific-filter` |

### DECISION_SERVICE
Discriminador (responder EN ORDEN; la primera que aplique decide):
  1. ¿La operación hace **I/O o toca un sistema externo** (red, SOAP/HTTP, impresora, email, SMS,
     pago, reloj/now como dependencia, cache)? → **PUERTO DE INFRAESTRUCTURA** (fila 6).
  2. ¿Es **lógica de dominio pura, sin I/O**, que coordina ≥2 aggregates ya cargados? → **DOMAIN SERVICE** (fila 5).
  3. ¿Es un **helper de consulta** (lookup por publicId)? → **LOOKUP SERVICE** (`Application.Services`).
  4. Si no, no es servicio: usar repos genéricos.

| Context | Contract | Namespace |
|---|---|---|
| Query — standard lookup by publicId | `IExampleService` → `FindAsync` / `FindWithItemsAsync` | `Application.Services` |
| Query — custom predicate / projection | `IReadRepository<T>` | — |
| Command — tracked scalar | `IExampleService.FindTrackedAsync` | `Application.Services` |
| Command — tracked + children | `IReadRepository<T>` (`includeProperties`, `disableTracking:false`) | — |
| Cross-aggregate operation — **lógica PURA, sin I/O** | `IExampleService` → `TransferItem` / `MergeInto` | `Application.Contracts.Interfaces` (impl en `Infrastructure\Services`) |
| **Side-effect / sistema externo** (gateway AFIP, impresora, email, pago, SMS, cache) — **hace I/O** | `I{Port}` (puerto) + records request/result al lado | `Application.Contracts.Infrastructure` (impl en `Infrastructure\Services`, registrar en DI) |

> Puerto de infraestructura: la interfaz + sus DTOs (request/result) viven en `Application.Contracts.Infrastructure`
> (junto a `ICacheService`); la implementación concreta (incl. STUB) y su `AddScoped` van en Infrastructure.
> El handler que lo consume lo inyecta como cualquier dependencia. Archetype: `port.contract` / `port.impl`.

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
| query.predicate (genérico) | `{APP_SRC}\Features\ExamplesEF\Queries\GetExampleByPredicate\GetExampleByPredicateQueryHandler.cs` |
| query.specific-filter (JOIN no-genérico · o convención: encapsular ILike nombrado) | `{INFRA_SRC}\Repositories\EF\ExampleReadRepository.cs` |
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
| dapper.read-contract | `{APP_SRC}\Contracts\Persistence\Dapper\IExampleReadRepository.cs` |
| dapper.write-contract | `{APP_SRC}\Contracts\Persistence\Dapper\IExampleWriteRepository.cs` |
| dapper.base-read | `{INFRA_SRC}\Repositories\Dapper\ReadRepository.cs` |
| dapper.base-write | `{INFRA_SRC}\Repositories\Dapper\WriteRepository.cs` |
| dapper.uow-concrete | `{INFRA_SRC}\Repositories\Dapper\UnitOfWork.cs` |
| dapper.read-repo | `{INFRA_SRC}\Repositories\Dapper\ExampleReadRepository.cs` |
| dapper.write-repo | `{INFRA_SRC}\Repositories\Dapper\ExampleWriteRepository.cs` |
| dapper.cmd.create | `{APP_SRC}\Features\ExamplesDapper\Commands\CreateExampleDapper\CreateExampleDapperCommandHandler.cs` |
| dapper.cmd.update | `{APP_SRC}\Features\ExamplesDapper\Commands\UpdateExampleDapper\UpdateExampleDapperCommandHandler.cs` |
| dapper.cmd.delete | `{APP_SRC}\Features\ExamplesDapper\Commands\DeleteExampleDapper\DeleteExampleDapperCommandHandler.cs` |
| dapper.query.by-id | `{APP_SRC}\Features\ExamplesDapper\Queries\GetExampleByPublicIdDapper\GetExampleByPublicIdDapperQueryHandler.cs` |
| dapper.query.paginated | `{APP_SRC}\Features\ExamplesDapper\Queries\GetExamplesPaginatedDapper\GetExamplesPaginatedDapperQueryHandler.cs` |
| dapper.query.search | `{APP_SRC}\Features\ExamplesDapper\Queries\SearchExamplesByNameDapper\SearchExamplesByNameDapperQueryHandler.cs` |
| dapper.controller | `{API_SRC}\Controllers\ExamplesDapperController.cs` |
| svc.lookup | `{APP_SRC}\Services\IExampleService.cs` · `{APP_SRC}\Services\ExampleService.cs` |
| svc.domain | Glob `{INFRA_SRC}\Services\` → read matching file |
| port.contract | `{APP_SRC}\Contracts\Infrastructure\ICacheService.cs` |
| port.impl | `{INFRA_SRC}\Cache\MemoryCacheService.cs` |

---

## STATE: S2_WARM
GUARD:  REFERENCE_FILES[operation] already in context
        (warm de origen, O sesión calentada por S_WARMUP en un PROGRAM)
ACTION: expand template from memory · write files per PATH_PATTERNS · apply GLOBAL_INVARIANTS
NEXT:   → S5_VALIDATE

## STATE: S3_LOAD
GUARD:  reference_path resuelto en S1_CLASSIFY (operation + repository_contract)
ACTION: Read(reference_path) — exactly 1 file, el ejemplar que coincide con repository_contract.
        Si contract == específico y el ejemplar es el repo concreto (p.ej. `query.specific-filter`),
        la declaración en el contrato (`IMyEntityReadRepository`) y la inyección en el handler
        son espejo mecánico del ejemplar — no requieren leer otro archivo.
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
Ports (infra):{APP_SRC}\Contracts\Infrastructure\I{Port}.cs (+ records request/result) · {INFRA_SRC}\Services\{Port}.cs · DI
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
    Interfaces\          I{Service}.cs  ← domain services (cross-aggregate, lógica PURA sin I/O; impl en Infrastructure)
    Infrastructure\      ICacheService.cs I{Port}.cs  ← puertos de infra (I/O / sistema externo; impl en Infrastructure + DI)
    Persistence\EF\      ILINQRepository.cs IUnitOfWork.cs ISqlRepository.cs
                         IExampleWriteRepository.cs IExampleReadRepository.cs
    Persistence\Dapper\  IUnitOfWork.cs IReadRepository.cs IWriteRepository.cs
  Features\
    ExamplesEF\      Commands\ Queries\
    ExamplesDapper\  Commands\ Queries\
  Mapping\  MappingProfile.cs
  Services\  IExampleService.cs ExampleService.cs  ← lookup services (query helpers; implemented in Application)

{INFRA_SRC}\
  Persistence\      ExampleDbContext.cs Migrations\
  Repositories\
    EF\     LINQRepository.cs SqlRepository.cs
            ExampleWriteRepository.cs ExampleReadRepository.cs UnitOfWork.cs
    Dapper\ ReadRepository.cs WriteRepository.cs UnitOfWork.cs
            ExampleReadRepository.cs ExampleWriteRepository.cs
  Services\  {Service}.cs

{API_SRC}\
  Controllers\       ExamplesEFController.cs ExamplesDapperController.cs
  ExceptionHandling\ GlobalExceptionHandler.cs
  Extensions\        ResultExtensions.cs
```

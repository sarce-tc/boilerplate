# /arquitectura — Navegación del proyecto

**Sesión cálida** (archivos ya en contexto): trabaja directo, omite lecturas.
**Sesión fría**: lee los archivos de tu fila antes de escribir código.

---

## Implementaciones de referencia

| Aggregate | Path | Propósito |
|---|---|---|
| `Order` | **Dapper** | Patrón de referencia para lectura performante con SQL explícito y transacciones manuales. |
| `Example` + `ExampleItem` | **EF Core** | Patrón de referencia para escritura con LINQ, change-tracking y aggregate con hijos. Leer antes de implementar una nueva entidad EF. |

> Nunca degradar la claridad de estos dos aggregates al refactorizar — son la documentación viva del stack.

---

## Principio generic-first (EF Core)

El agente debe seguir este orden de decisión para cada nueva entidad:

1. **Inyectar `IReadRepository<T>`** en query handlers y validators — cubre el 80 % de los casos (FindAsync, GetEntityAsync, GetListPaginatedAsync, ExistsAsync, etc.).
2. **Usar `IUnitOfWork.WriteRepository`** en command handlers para operaciones bulk (DeleteManyAsync, UpdateManyAsync).
3. **Crear `IMyEntityWriteRepository : IWriteRepository<T>`** solo si el aggregate necesita métodos de escritura que no existen en la superficie genérica.
4. **Crear `IMyEntityReadRepository : IReadRepository<T>`** solo si el handler necesita queries que no existen en la superficie genérica (eager-loading de hijos, filtros con lógica de negocio).

---

## Archivos por tarea

### Path EF Core — leer para nueva entidad

| Tarea | Archivo de referencia |
|---|---|
| Entidad raíz con hijos | `Domain/Entities/Example.cs` · `Domain/Entities/ExampleItem.cs` |
| Contratos genéricos EF | `Application/Contracts/Persistence/EF/ILINQRepository.cs` |
| Contrato UoW EF | `Application/Contracts/Persistence/EF/IUnitOfWork.cs` |
| Contrato write específico | `Application/Contracts/Persistence/EF/IExampleWriteRepository.cs` |
| Contrato read específico | `Application/Contracts/Persistence/EF/IExampleReadRepository.cs` |
| LINQRepository base | `Infrastructure/Repositories/EF/LINQRepository.cs` |
| Write repo concreto | `Infrastructure/Repositories/EF/ExampleWriteRepository.cs` |
| Read repo concreto | `Infrastructure/Repositories/EF/ExampleReadRepository.cs` |
| UoW concreto | `Infrastructure/Repositories/EF/UnitOfWork.cs` |
| DbContext + migraciones | `Infrastructure/Persistence/ExampleDbContext.cs` |
| Create (aggregate + hijos) | `Features/ExamplesEF/Commands/CreateExample/CreateExampleCommandHandler.cs` |
| Update (scalar + hijos) | `Features/ExamplesEF/Commands/UpdateExample/UpdateExampleCommandHandler.cs` |
| Update (PATCH scalar) | `Features/ExamplesEF/Commands/UpdateExampleFields/UpdateExampleFieldsCommandHandler.cs` |
| Delete individual | `Features/ExamplesEF/Commands/DeleteExample/DeleteExampleCommandHandler.cs` |
| Delete bulk sin carga | `Features/ExamplesEF/Commands/DeleteManyExamples/DeleteManyExamplesCommandHandler.cs` |
| Update bulk sin carga | `Features/ExamplesEF/Commands/UpdateManyExamples/UpdateManyExamplesCommandHandler.cs` |
| Query por PK | `Features/ExamplesEF/Queries/GetExampleById/GetExampleByIdQueryHandler.cs` |
| Query paginada | `Features/ExamplesEF/Queries/GetExamplesPaginated/GetExamplesPaginatedQueryHandler.cs` |
| Query aggregate + hijos (includeProperties) | `Features/ExamplesEF/Queries/GetExampleWithItems/GetExampleWithItemsQueryHandler.cs` |
| Query colección hija (includeProperties) | `Features/ExamplesEF/Queries/GetExampleItems/GetExampleItemsQueryHandler.cs` |
| Query hijo individual (includeProperties + in-memory filter) | `Features/ExamplesEF/Queries/GetExampleItemByPublicId/GetExampleItemByPublicIdQueryHandler.cs` |
| Mapping | `Application/Mapping/MappingProfile.cs` |

### Path Dapper — leer para nueva entidad

| Tarea | Archivo de referencia |
|---|---|
| Repositorio lectura | `Infrastructure/Repositories/Dapper/OrderReadRepository.cs` |
| Repositorio escritura | `Infrastructure/Repositories/Dapper/OrderWriteRepository.cs` |
| UoW Dapper | `Infrastructure/Repositories/Dapper/UnitOfWork.cs` · `Application/Contracts/Persistence/Dapper/IUnitOfWork.cs` |
| Command handler | `Features/Orders/Commands/CancelOrder/CancelOrderCommandHandler.cs` |
| Query handler | `Features/Orders/Queries/GetOrderById/GetOrderByIdQueryHandler.cs` |
| Controller | `API/Controllers/OrdersController.cs` · `API/Extensions/ResultExtensions.cs` |

### Compartido

| Tarea | Archivo |
|---|---|
| Error handling | `API/ExceptionHandling/GlobalExceptionHandler.cs` · `API/Extensions/ResultExtensions.cs` |
| DI de todos los repos | `Infrastructure/InfrastuctureServiceRegistration.cs` |
| Application services | contratos: `Application/Contracts/Interfaces/` · implementaciones: `Infrastructure/Services/` |
| Nuevo aggregate end-to-end | `/arq-new` |
| Convenciones | `CLAUDE.md` (siempre en contexto) |

---

## Estructura de carpetas

```
Microservice.Domain/
  Entities/    Example.cs · ExampleItem.cs · ExampleStatus.cs · ExampleItemStatus.cs
               Order.cs · OrderItem.cs · OrderStatus.cs
  Exceptions/  DomainException.cs
  Services/    IExampleDomainService.cs · ExampleDomainService.cs
  ValueObjects/ BaseDomainModel.cs

Microservice.Application/
  Contracts/Persistence/
    EF/      ILINQRepository.cs (IReadRepository<T> · IWriteRepository<T> · IQueryRepository<T>)
             IUnitOfWork.cs · ISqlRepository.cs
             IExampleWriteRepository.cs · IExampleReadRepository.cs
    Dapper/  IUnitOfWork.cs · IReadRepository.cs · IWriteRepository.cs
             IExampleReadRepository.cs · IExampleWriteRepository.cs
  Features/
    ExamplesEF/  Commands/ · Queries/
    Orders/      Commands/ · Queries/
  Mapping/     MappingProfile.cs

Microservice.Infrastructure/
  Persistence/     ExampleDbContext.cs · Migrations/
  Repositories/
    EF/     LINQRepository.cs · SqlRepository.cs
            ExampleWriteRepository.cs · ExampleReadRepository.cs · UnitOfWork.cs
    Dapper/ ReadRepository.cs · WriteRepository.cs
            OrderReadRepository.cs · OrderWriteRepository.cs · UnitOfWork.cs

Microservice.API/
  Controllers/       ExamplesController.cs · OrdersController.cs
  ExceptionHandling/ GlobalExceptionHandler.cs
  Extensions/        ResultExtensions.cs
```

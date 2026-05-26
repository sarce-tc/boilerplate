# /arquitectura — Navegación del proyecto

**Sesión cálida** (archivos ya en contexto): trabaja directo, omite lecturas.
**Sesión fría**: lee los archivos de tu fila — las reglas están en sus AGENT ENTRY POINT.

> **Principio:** los archivos de referencia apuntan siempre al aggregate `Order`.
> `Order` es la implementación de referencia permanente del proyecto — los patrones se aprenden de él, no se reescriben por aggregate. Nunca degradar su claridad al refactorizar.

## Stack
.NET 10 / C# 14 · MediatR 14.1 · EF Core 10 + Dapper 2.1 · PostgreSQL 16 · Serilog · OpenTelemetry 1.15
`Domain → Application → Infrastructure → API`

## Archivos por tarea

| Tarea | Leer |
|---|---|
| Command handler | `Features/Orders/Commands/CancelOrder/CancelOrderCommandHandler.cs` · `Features/Orders/Commands/CreateOrder/CreateOrderCommandHandler.cs` · `API/Extensions/ResultExtensions.cs` |
| Query handler | `Features/Orders/Queries/GetOrderById/GetOrderByIdQueryHandler.cs` · `Features/Orders/Queries/GetOrders/GetOrdersQueryHandler.cs` |
| Controller / endpoint | `API/Controllers/OrdersController.cs` |
| Error handling | `API/ExceptionHandling/GlobalExceptionHandler.cs` · `API/Extensions/ResultExtensions.cs` |
| Repositorio Dapper | `Infrastructure/Repositories/Dapper/OrderReadRepository.cs` · `Infrastructure/Repositories/Dapper/OrderWriteRepository.cs` · `Infrastructure/Repositories/Dapper/UnitOfWork.cs` · `Application/Contracts/Persistence/Dapper/IUnitOfWork.cs` |
| Dominio / entidad | `Domain/Entities/Order.cs` |
| Schema SQL | `Infrastructure/Persistence/ExampleDbContext.cs` |
| Estructura actual | `Infrastructure/InfrastuctureServiceRegistration.cs` |
| Nuevo aggregate | `/arq-new` · todas las filas anteriores |
| Convenciones | CLAUDE.md (siempre en contexto) |

## Estructura de carpetas

```
Microservice.Domain/         Entities/ Order.cs · Customer.cs · OrderItem.cs · Example.cs
                             Exceptions/DomainException.cs · Common/BaseDomainModel.cs
Microservice.Application/    Contracts/Persistence/Dapper/ · DTOs/ · Features/ · Common/Results/
Microservice.Infrastructure/ Repositories/Dapper/ · Persistence/ExampleDbContext.cs
Microservice.API/            Controllers/ · ExceptionHandling/ · Extensions/ResultExtensions.cs
```

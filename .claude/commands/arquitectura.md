# /arquitectura — Mapa de arquitectura del boilerplate

Usa este comando cuando necesites implementar o modificar funcionalidad.
Lee este documento COMPLETO antes de tocar código; te permite tomar decisiones sin explorar el filesystem.

---

## Stack

.NET 10 / C# 14 · MediatR 14.1 · EF Core 10 + Dapper 2.1 · PostgreSQL 16 · Serilog · OpenTelemetry 1.15

---

## Capas y reglas de dependencia

```
Domain          → sin dependencias externas
Application     → depende de Domain
Infrastructure  → depende de Application
API             → depende de Application
```

---

## Estructura de carpetas clave

```
Microservice.Domain/
  Entities/         Order.cs · OrderItem.cs · Example.cs · Product.cs
  Exceptions/       DomainException.cs          ← lanza → GlobalExceptionHandler → 409
  Common/           BaseDomainModel.cs           ← Id, PublicId, CreatedAt, UpdatedAt

Microservice.Application/
  Common/
    Results/        Result.cs · Error.cs         ← Result<T> / Result para todos los handlers
    PagedResult.cs                               ← genérico para listas paginadas
  Contracts/
    Persistence/
      Dapper/       IUnitOfWork.cs               ← ENTRY POINT escritura Dapper
                    IOrderReadRepository.cs      ← ENTRY POINT lectura Dapper Orders
                    IOrderWriteRepository.cs
                    IReadRepository.cs           ← GetByIdAsync / GetByPublicIdAsync / GetAllAsync
                    IWriteRepository.cs          ← AddAsync / UpdateAsync / DeleteAsync
  DTOs/
    Orders/         OrderDto · OrderSummaryDto · OrderItemDto · CreateOrderItemDto
  Features/
    Orders/
      Commands/
        CreateOrder/      CreateOrderCommand + Handler + Validator
        CancelOrder/      CancelOrderCommand + Handler
        CompleteOrder/    CompleteOrderCommand + Handler
        UpdateOrder/      UpdateOrderCommand + Handler + Validator
        AddOrderItem/     AddOrderItemCommand + Handler + Validator
        RemoveOrderItem/  RemoveOrderItemCommand + Handler

      Queries/
        GetOrderById/   GetOrderByIdQuery + Handler
        GetOrders/      GetOrdersQuery + Handler + Validator

Microservice.Infrastructure/
  Repositories/Dapper/
    ReadRepository.cs    ← base: GetByIdAsync, GetByPublicIdAsync, GetAllAsync, CountAsync
    WriteRepository.cs   ← base: AddAsync(abstract), UpdateAsync(abstract), DeleteAsync
    OrderReadRepository  ← override TableName="orders"; GetWithItemsAsync; GetPagedAsync
    OrderWriteRepository ← override AddAsync/UpdateAsync; AddItemAsync; RemoveItemAsync
    UnitOfWork.cs        ← gestiona NpgsqlConnection + NpgsqlTransaction; lazy repos

Microservice.API/
  Controllers/
    OrdersController.cs  ← 8 endpoints (ver tabla abajo)
    AuthController.cs    ← POST /api/v1/auth/token (dev only)
    JobsController.cs    ← GET /api/v1/jobs/{jobId}
  ExceptionHandling/
    GlobalExceptionHandler.cs  ← ÚNICO manejador de excepciones; mappings abajo
  Extensions/
    AuthExtensions.cs
    ObservabilityExtensions.cs
    RateLimitingExtensions.cs
    HealthCheckExtensions.cs
    ResultExtensions.cs  ← ToActionResult() convierte Result → IActionResult
  Middleware/
    CorrelationIdMiddleware.cs
    IdempotencyMiddleware.cs
```

---

## Endpoints Orders — mapa completo

| Método | Ruta | Command/Query | Respuestas |
|--------|------|---------------|------------|
| GET | `/api/v1/orders` | `GetOrdersQuery(page, pageSize)` | 200 `PagedResult<OrderSummaryDto>` · 400 |
| POST | `/api/v1/orders` | `CreateOrderCommand` | 201 `Guid` · 400 |
| GET | `/api/v1/orders/{id}` | `GetOrderByIdQuery` | 200 `OrderDto` · 404 |
| PUT | `/api/v1/orders/{id}` | `UpdateOrderCommand` | 200 · 400 · 404 · 409 |
| DELETE | `/api/v1/orders/{id}` | `CancelOrderCommand` | 200 · 404 · 409 |
| POST | `/api/v1/orders/{id}/complete` | `CompleteOrderCommand` | 200 · 404 · 409 |
| POST | `/api/v1/orders/{id}/items` | `AddOrderItemCommand` | 201 `Guid` · 400 · 404 · 409 |
| DELETE | `/api/v1/orders/{id}/items/{itemId}` | `RemoveOrderItemCommand` | 200 · 404 · 409 |

---

## Patrón de un command handler (plantilla mental)

```csharp
// 1. LEER (sin transacción)
var order = await orderReadRepo.GetByPublicIdAsync(request.PublicId, ct);
if (order is null) return Result.Failure(Error.NotFound("..."));

// 2. DOMINIO (antes de abrir TX — DomainException → GlobalExceptionHandler → 409)
order.SomeDomainMethod();

// 3. PERSISTIR (única razón válida para try-catch)
await unitOfWork.BeginTransactionAsync(ct);
try
{
    await unitOfWork.OrdersWrite.UpdateAsync(order, ct);
    await unitOfWork.CommitAsync(ct);
}
catch
{
    await unitOfWork.RollbackAsync(ct);
    throw;
}
return Result.Success();
```

---

## Patrón de un query handler

```csharp
// Sin transacción, sin try-catch, solo lectura
var (order, items) = await orderReadRepo.GetWithItemsAsync(request.PublicId, ct);
if (order is null) return Result<OrderDto>.Failure(Error.NotFound("..."));
return Result<OrderDto>.Success(new OrderDto(...));
```

---

## GlobalExceptionHandler — mappings

| Excepción | HTTP | Log level |
|-----------|------|-----------|
| `ValidationException` | 400 Bad Request | Warning |
| `ArgumentException` | 400 Bad Request | Warning |
| `KeyNotFoundException` | 404 Not Found | Information |
| `DomainException` | 409 Conflict | Warning |
| `InvalidOperationException` | 409 Conflict | Error |
| `UnauthorizedAccessException` | 401 Unauthorized | Error |
| `NotImplementedException` | 501 Not Implemented | Error |
| Cualquier otra | 500 Internal Server Error | Error |

Archivo: `Microservice.API/ExceptionHandling/GlobalExceptionHandler.cs`

---

## Reglas críticas (resumen de CLAUDE.md)

1. **`try-catch` solo para `RollbackAsync`** — todo lo demás lo maneja `GlobalExceptionHandler`.
2. **No duplicar reglas de dominio** — la entidad es la única fuente de verdad; no añadir guards en handlers si el dominio ya valida.
3. **`DomainException` antes de `BeginTransactionAsync`** — así si lanza, no hay rollback que gestionar.
4. **Genéricos first** — usar `GetByPublicIdAsync`, `AddAsync`, `UpdateAsync` antes de crear métodos específicos; añadir métodos nuevos en el repo solo si el genérico no cubre el caso.
5. **snake_case en SQL** — `DefaultTypeMap.MatchNamesWithUnderscores = true` mapea automáticamente.
6. **Dos constructores en write repos** — DI (IDbConnectionFactory) + UoW (NpgsqlConnection, NpgsqlTransaction).
7. **Paginación obligatoria** en endpoints de colecciones — usar `GetPagedAsync` + `PagedResult<T>`.
8. **Antes de implementar algo no cubierto aquí — preguntar al piloto.**

---

## Cómo añadir un nuevo aggregate (checklist)

- [ ] `Domain/Entities/MyEntity.cs` — hereda `BaseDomainModel`, factory `Create()`, métodos de dominio que lanzan `DomainException`
- [ ] EF config en `ExampleDbContext` — snake_case column names explícitos, migración
- [ ] `Application/Contracts/Persistence/Dapper/IMyEntityReadRepository.cs`
- [ ] `Application/Contracts/Persistence/Dapper/IMyEntityWriteRepository.cs`
- [ ] Añadir `IMyEntityWriteRepository MyEntityWrite { get; }` en `IUnitOfWork`
- [ ] `Application/DTOs/MyEntity/MyEntityDto.cs` (+ summary si hay lista)
- [ ] `Application/Features/MyEntity/Commands/` — un directorio por command; `Command + Handler + Validator`
- [ ] `Application/Features/MyEntity/Queries/` — un directorio por query; `Query + Handler + Validator`
- [ ] `Infrastructure/Repositories/Dapper/MyEntityReadRepository.cs` — hereda `ReadRepository<MyEntity>`
- [ ] `Infrastructure/Repositories/Dapper/MyEntityWriteRepository.cs` — hereda `WriteRepository<MyEntity>`; dos constructores
- [ ] Registrar lazy en `UnitOfWork.cs` (propiedad `MyEntityWrite`)
- [ ] Registrar repos en `InfrastructureServiceRegistration.cs`
- [ ] `API/Controllers/MyEntityController.cs` — delgado, solo `mediator.Send + ToActionResult`

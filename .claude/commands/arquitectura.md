# /arquitectura — Mapa de arquitectura del boilerplate

Lee este documento antes de implementar o modificar cualquier funcionalidad.
Cubre los patrones establecidos del proyecto. Para casos fuera del patrón, aplica las reglas de la sección **"Casos fuera del patrón"** al final del documento.

---

## Stack

.NET 10 / C# 14 · MediatR 14.1 · EF Core 10 + Dapper 2.1 · PostgreSQL 16 · Serilog · OpenTelemetry 1.15

---

## Capas y dependencias

```
Domain          → sin dependencias externas
Application     → depende de Domain
Infrastructure  → depende de Application
API             → depende de Application
```

---

## Estructura de carpetas

```
Microservice.Domain/
  Entities/         Order.cs · OrderItem.cs · Example.cs · Product.cs
  Exceptions/       DomainException.cs        ← lanza → GlobalExceptionHandler → 409
  Common/           BaseDomainModel.cs         ← Id(int) · PublicId(Guid) · CreatedAt · UpdatedAt

Microservice.Application/
  Common/
    Results/        Result.cs · Error.cs       ← Result<T> y Result para todos los handlers
    PagedResult.cs                             ← PagedResult<T>(Items, TotalCount, Page, PageSize)
  Contracts/Persistence/Dapper/
    IUnitOfWork.cs                             ← ExamplesWrite · ProductWrite · OrdersWrite
    IReadRepository.cs                         ← GetByIdAsync · GetByPublicIdAsync · GetAllAsync · ExistsAsync · CountAsync
    IWriteRepository.cs                        ← AddAsync · UpdateAsync · DeleteAsync
    IOrderReadRepository.cs                    ← + GetWithItemsAsync · GetPagedAsync
    IOrderWriteRepository.cs                   ← + AddItemAsync · RemoveItemAsync
  DTOs/Orders/
    OrderDto.cs          (PublicId, CustomerName, Status, TotalAmount, CreatedAt, Items)
    OrderSummaryDto.cs   (PublicId, CustomerName, Status, TotalAmount, ItemCount, CreatedAt) ← Dapper direct-map, public setters
    OrderItemDto.cs      (PublicId, ProductName, Quantity, UnitPrice, LineTotal)
    CreateOrderItemDto.cs(ProductName, Quantity, UnitPrice)
  Features/Orders/
    Commands/  CreateOrder · CancelOrder · CompleteOrder · UpdateOrder · AddOrderItem · RemoveOrderItem
    Queries/   GetOrderById · GetOrders

Microservice.Infrastructure/
  Repositories/Dapper/
    ReadRepository.cs    ← base genérica (override TableName)
    WriteRepository.cs   ← base genérica (2 constructores DI/UoW; override TableName, AddAsync, UpdateAsync)
    OrderReadRepository  ← TableName="orders" · GetWithItemsAsync · GetPagedAsync
    OrderWriteRepository ← AddAsync · UpdateAsync · AddItemAsync · RemoveItemAsync
    UnitOfWork.cs        ← lazy repos con NpgsqlConnection + NpgsqlTransaction compartidos
  InfrastuctureServiceRegistration.cs ← registro de todos los servicios

Microservice.API/
  Controllers/     OrdersController.cs · AuthController.cs · JobsController.cs
  ExceptionHandling/GlobalExceptionHandler.cs   ← ÚNICO manejador de excepciones del pipeline
  Extensions/      ResultExtensions.cs           ← ToActionResult() convierte Result → IActionResult
  Middleware/      CorrelationIdMiddleware · IdempotencyMiddleware
```

---

## Endpoints Orders — tabla completa

| Método   | Ruta                                    | Command/Query          | Success | Errors       |
|----------|-----------------------------------------|------------------------|---------|--------------|
| GET      | `/api/v1/orders?page=1&pageSize=20`     | `GetOrdersQuery`       | 200     | 400          |
| POST     | `/api/v1/orders`                        | `CreateOrderCommand`   | 201     | 400          |
| GET      | `/api/v1/orders/{id}`                   | `GetOrderByIdQuery`    | 200     | 404          |
| PUT      | `/api/v1/orders/{id}`                   | `UpdateOrderCommand`   | 200     | 400·404·409  |
| DELETE   | `/api/v1/orders/{id}`                   | `CancelOrderCommand`   | 200     | 404·409      |
| POST     | `/api/v1/orders/{id}/complete`          | `CompleteOrderCommand` | 200     | 404·409      |
| POST     | `/api/v1/orders/{id}/items`             | `AddOrderItemCommand`  | 201     | 400·404·409  |
| DELETE   | `/api/v1/orders/{id}/items/{itemId}`    | `RemoveOrderItemCommand`| 200    | 404·409      |

---

## Schema SQL — columnas exactas

### Tabla `orders`
```
id            BIGINT PK
public_id     UUID   UNIQUE
customer_name TEXT
status        TEXT   ('Pending' | 'Completed' | 'Cancelled')
total_amount  NUMERIC(18,2)
created_at    TIMESTAMPTZ
updated_at    TIMESTAMPTZ
```

### Tabla `order_items`
```
id            BIGINT PK
public_id     UUID   UNIQUE
order_id      BIGINT FK → orders.id (CASCADE DELETE)
product_name  TEXT
quantity      INT
unit_price    NUMERIC(18,2)
line_total    NUMERIC(18,2)
created_at    TIMESTAMPTZ
updated_at    TIMESTAMPTZ
```

`MatchNamesWithUnderscores = true` mapea automáticamente: `customer_name → CustomerName`, `order_id → OrderId`, `item_count → ItemCount`.

---

## Firmas exactas — métodos no obvios

```csharp
// ── Domain ────────────────────────────────────────────────────────────────
Order.Create(string customerName)                              // factory
order.Cancel()                                                 // DomainException si Completed
order.Complete()                                               // DomainException si Cancelled
order.EnsureModifiable()                                       // DomainException si no Pending
order.UpdateCustomerName(string customerName)                  // llama EnsureModifiable
order.AddItemForDapper(string name, int qty, decimal price)    // → OrderItem (TotalAmount += LineTotal)
order.RemoveItemForDapper(Guid itemPublicId, IReadOnlyList<OrderItem> currentItems) // → OrderItem (TotalAmount -= LineTotal)
order.RecalculateTotal(IEnumerable<OrderItem> items)           // sync total tras carga Dapper

OrderItem.CreateForOrder(int orderId, string productName, int quantity, decimal unitPrice) // → OrderItem con PublicId=NewGuid()

// ── IOrderReadRepository ─────────────────────────────────────────────────
Task<(Order? Order, IReadOnlyList<OrderItem> Items)> GetWithItemsAsync(Guid publicId, CancellationToken ct)
Task<(IReadOnlyList<OrderSummaryDto> Orders, int TotalCount)> GetPagedAsync(int page, int pageSize, CancellationToken ct)

// ── IOrderWriteRepository ────────────────────────────────────────────────
Task<Order>     AddAsync(Order entity, CancellationToken ct)      // INSERT RETURNING *
Task<Order>     UpdateAsync(Order entity, CancellationToken ct)   // UPDATE customer_name, status, total_amount
Task<OrderItem> AddItemAsync(OrderItem item, CancellationToken ct) // INSERT RETURNING *
Task            RemoveItemAsync(int itemId, CancellationToken ct)  // DELETE WHERE id = @Id

// ── WriteRepository constructores (implementar SIEMPRE los dos) ───────────
public MyEntityWriteRepository(IDbConnectionFactory connectionFactory)              // DI standalone
public MyEntityWriteRepository(NpgsqlConnection connection, NpgsqlTransaction tx)   // UoW compartido
```

---

## Patrón command handler — plantilla exacta

```csharp
// Referencia: Features/Orders/Commands/CancelOrder/CancelOrderCommandHandler.cs

public sealed class XCommandHandler(
    IOrderReadRepository orderReadRepo,   // lectura sin TX
    IUnitOfWork          unitOfWork       // escritura con TX
) : IRequestHandler<XCommand, Result>
{
    public async Task<Result> Handle(XCommand request, CancellationToken ct)
    {
        // 1. Leer (sin transacción)
        var order = await orderReadRepo.GetByPublicIdAsync(request.PublicId, ct);
        if (order is null)
            return Result.Failure(Error.NotFound($"Order '{request.PublicId}' was not found."));

        // 2. Dominio (ANTES de BeginTransactionAsync → DomainException no necesita rollback)
        order.SomeDomainMethod();   // lanza DomainException → GlobalExceptionHandler → 409

        // 3. Persistir (único try-catch permitido en handlers)
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
    }
}
```

---

## Patrón query handler — plantilla exacta

```csharp
// Referencia: Features/Orders/Queries/GetOrderById/GetOrderByIdQueryHandler.cs

public sealed class XQueryHandler(
    IOrderReadRepository orderReadRepo
) : IRequestHandler<XQuery, Result<OrderDto>>
{
    public async Task<Result<OrderDto>> Handle(XQuery request, CancellationToken ct)
    {
        var (order, items) = await orderReadRepo.GetWithItemsAsync(request.PublicId, ct);
        if (order is null)
            return Result<OrderDto>.Failure(Error.NotFound($"Order '{request.PublicId}' was not found."));

        return Result<OrderDto>.Success(new OrderDto(
            order.PublicId, order.CustomerName, order.Status, order.TotalAmount, order.CreatedAt,
            items.Select(i => new OrderItemDto(i.PublicId, i.ProductName, i.Quantity, i.UnitPrice, i.LineTotal))
                 .ToList().AsReadOnly()));
    }
}
```

---

## Patrón controller — dos variantes de binding

```csharp
// Variante A: solo ruta (sin body)
[HttpPost("{publicId:guid}/complete")]
public async Task<IActionResult> CompleteOrder(Guid publicId, CancellationToken ct)
{
    var result = await mediator.Send(new CompleteOrderCommand(publicId), ct);
    return result.ToActionResult();
}

// Variante B: ruta + body (usar `with` para fusionar)
[HttpPut("{publicId:guid}")]
public async Task<IActionResult> UpdateOrder(
    Guid publicId,
    [FromBody] UpdateOrderCommand command,    // body sin publicId
    CancellationToken ct)
{
    var result = await mediator.Send(command with { PublicId = publicId }, ct);
    return result.ToActionResult();
}

// Variante C: 201 Created con payload
[HttpPost]
public async Task<IActionResult> CreateOrder([FromBody] CreateOrderCommand command, CancellationToken ct)
{
    var result = await mediator.Send(command, ct);
    return result.ToActionResult(StatusCodes.Status201Created);
}
```

---

## GlobalExceptionHandler — mappings

| Excepción                   | HTTP | Log      | Cuándo usar                            |
|-----------------------------|------|----------|----------------------------------------|
| `DomainException`           | 409  | Warning  | Invariantes de dominio (Cancel, etc.)  |
| `ValidationException`       | 400  | Warning  | FluentValidation pipeline              |
| `ArgumentException`         | 400  | Warning  | Guardia de parámetros de dominio       |
| `KeyNotFoundException`      | 404  | Info     | Entidad no encontrada (alternativa)    |
| `InvalidOperationException` | 409  | Error    | Fallback (preferir DomainException)    |
| `UnauthorizedAccessException`| 401 | Error    | Acceso denegado                        |
| `NotImplementedException`   | 501  | Error    | Funcionalidad pendiente                |
| Cualquier otra              | 500  | Error    | Error inesperado del sistema           |

---

## GlobalExceptionHandler — ProblemDetails response

```json
{
  "type": "https://httpstatuses.com/409",
  "title": "Conflict",
  "detail": "Cannot cancel a completed order.",
  "status": 409,
  "instance": "/api/v1/orders/abc123",
  "traceId": "...",
  "correlationId": "...",
  "timestamp": "...",
  "exceptionType": "DomainException"   // solo en Development
}
```

---

## Reglas críticas (resumen CLAUDE.md)

1. **`try-catch` solo para `RollbackAsync`** — todo lo demás va a GlobalExceptionHandler.
2. **No duplicar reglas de dominio** — si `Order.Cancel()` ya valida, no añadir guard en el handler.
3. **`DomainException` antes de `BeginTransactionAsync`** — así no hay rollback que gestionar.
4. **Genéricos first** — usar `GetByPublicIdAsync`, `UpdateAsync` antes de crear métodos específicos.
5. **snake_case en SQL** — MatchNamesWithUnderscores mapea automáticamente.
6. **Dos constructores en write repos** — DI + UoW; implementar siempre los dos.
7. **Paginación obligatoria** en colecciones — `GetPagedAsync` + `PagedResult<T>`.
8. **Ante duda → preguntar al piloto.**

---

## Casos fuera del patrón establecido

Cuando una feature no encaja exactamente en los patrones de esta guía, el agente **no se bloquea** — toma el camino de desarrollo siguiendo estas reglas de decisión en orden:

### 1. Prioridad de decisión

```
¿Existe un patrón similar en el proyecto?
  → Sí: úsalo como base, adaptando lo mínimo necesario.
  → No: diseña desde cero respetando las convenciones del stack.
```

### 2. Convenciones que aplican siempre (con o sin patrón previo)

| Área | Regla |
|---|---|
| **Lenguaje** | C# 14: primary constructors, `record`, `sealed`, collection expressions `[]` |
| **Naming** | PascalCase público, `_camelCase` privado, snake_case en SQL |
| **Error handling** | `DomainException` → 409 · `Result<T>` en handlers · `GlobalExceptionHandler` central |
| **Async** | Siempre `async/await` con `CancellationToken ct` propagado |
| **Inyección** | Constructor injection vía primary constructors; interfaces, nunca concretos |
| **Logging** | Serilog structured logging; nunca `Console.Write` ni `Debug.Print` |
| **Performance** | No N+1 · Dapper para lecturas complejas · paginación en colecciones |

### 3. Librerías — usar solo las ya presentes en el stack

| Necesidad | Librería a usar |
|---|---|
| Mediator / CQRS | MediatR |
| Validación | FluentValidation |
| ORM / migrations | EF Core 10 |
| Micro-ORM / queries | Dapper |
| HTTP client resilience | Polly (si se agrega un cliente HTTP externo) |
| Background jobs | System.Threading.Channels + BackgroundService |
| Observabilidad | OpenTelemetry + Serilog |
| Tests | xUnit + Moq + FluentAssertions |

**Nunca introducir una librería nueva sin consultar al piloto.**

### 4. Cuándo preguntar al piloto

- La feature requiere una librería que no está en el stack.
- El diseño implica un cambio estructural (nueva capa, nuevo patrón de comunicación, cambio en el contrato de IUnitOfWork).
- Hay dos enfoques válidos y no está claro cuál priorizar.
- Se detecta un riesgo de seguridad o de rendimiento que el patrón existente no cubre.

### 5. Documentar la decisión

Cuando se implementa un patrón nuevo, agregar un bloque `AGENT ENTRY POINT` en el archivo más representativo explicando el enfoque elegido y por qué, para que los siguientes agentes lo encuentren.

---

## Checklist para nuevo aggregate

- [ ] `Domain/Entities/MyEntity.cs` — hereda `BaseDomainModel`; factory `Create()`; métodos que lanzan `DomainException`
- [ ] EF config en `ExampleDbContext` — `ToTable("my_entities")`, `HasColumnName` snake_case para cada propiedad
- [ ] Migración EF + `dotnet ef database update`
- [ ] `Application/Contracts/Persistence/Dapper/IMyEntityReadRepository.cs`
- [ ] `Application/Contracts/Persistence/Dapper/IMyEntityWriteRepository.cs`
- [ ] Añadir `IMyEntityWriteRepository MyEntityWrite { get; }` en `IUnitOfWork`
- [ ] `Application/DTOs/MyEntity/MyEntityDto.cs` (+ `MyEntitySummaryDto` para lista)
- [ ] Features: Commands + Queries en `Application/Features/MyEntity/`
- [ ] `Infrastructure/Repositories/Dapper/MyEntityReadRepository.cs` — hereda `ReadRepository<MyEntity>`
- [ ] `Infrastructure/Repositories/Dapper/MyEntityWriteRepository.cs` — hereda `WriteRepository<MyEntity>`; 2 constructores
- [ ] Añadir lazy prop en `UnitOfWork.cs`: `_myEntityWrite ??= new MyEntityWriteRepository(_connection!, _transaction!)`
- [ ] Registrar en `InfrastuctureServiceRegistration.cs`
- [ ] `API/Controllers/MyEntityController.cs` — delgado; `mediator.Send + ToActionResult`

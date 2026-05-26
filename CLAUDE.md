# Guía del agente — Boilerplate .NET 10 Microservices

Instrucciones que el agente **debe seguir siempre** en este repositorio.

Cuando una feature no encaja en los patrones documentados, el agente **no se bloquea**:
toma el camino de desarrollo siguiendo las convenciones, patrones y librerías del stack.
Solo pregunta al piloto si hay un cambio estructural, una librería nueva o dos enfoques
válidos sin criterio claro para elegir. Ver sección **"Casos fuera del patrón"** en `/arquitectura`.

---

## 1. Manejo de excepciones

- **`GlobalExceptionHandler` es el único punto de manejo de excepciones** en el pipeline.
- **`try-catch` está prohibido en lógica de negocio**, con una única excepción justificada:

```csharp
// CORRECTO — único try-catch permitido en handlers
await unitOfWork.BeginTransactionAsync(ct);
try
{
    await unitOfWork.Orders.UpdateAsync(order, ct);
    await unitOfWork.CommitAsync(ct);
}
catch
{
    await unitOfWork.RollbackAsync(ct);
    throw;   // siempre re-lanzar
}
```

- Si durante el desarrollo surge un caso donde parece necesario un `try-catch` fuera del patrón UoW, **el agente debe preguntar al piloto** antes de implementarlo.

---

## 2. DDD (Domain-Driven Design)

- Las **invariantes de dominio** viven en la entidad/aggregate root, nunca en el application layer.
- Los métodos de dominio lanzan **`DomainException`** (definida en `Microservice.Domain/Exceptions/`) cuando se viola una regla de negocio.
- `GlobalExceptionHandler` mapea `DomainException` → **HTTP 409 Conflict**, log en `Warning`.
- **No duplicar reglas de dominio** en el application layer como guards previos. La entidad es la única fuente de verdad.

```csharp
// INCORRECTO — guard en el handler duplica la regla del dominio
if (order.Status == OrderStatus.Completed)
    return Result.Failure(Error.Conflict("..."));
order.Cancel();

// CORRECTO — el dominio lanza DomainException, GlobalExceptionHandler la gestiona
order.Cancel();  // lanza DomainException si la regla no se cumple
```

- `DomainException` se llama **antes** de abrir una transacción UoW cuando sea posible, así no se necesita rollback.

---

## 3. Principios SOLID

- **S** — cada clase tiene una única responsabilidad. Handlers solo orquestan; la lógica de negocio está en el dominio.
- **O** — extender comportamiento mediante nuevas clases / implementaciones, no modificando las existentes.
- **L** — los repositorios concretos son sustituibles por sus interfaces sin cambiar el comportamiento del caller.
- **I** — interfaces pequeñas y específicas (`IOrderReadRepository`, `IOrderWriteRepository`, no un `IRepository` monolítico).
- **D** — depender de abstracciones (`IUnitOfWork`, `IOrderReadRepository`), nunca de implementaciones concretas en Application.

---

## 4. Convención de código

- **C# 14 / .NET 10**: usar primary constructors, `record`, `required`, collection expressions `[]`, `nameof`, `ArgumentException.ThrowIfNullOrWhiteSpace`.
- Nombres en **PascalCase** para tipos y miembros públicos; **camelCase** con `_` prefix para campos privados (`_items`, `_connection`).
- **`sealed`** en clases que no están diseñadas para heredar (handlers, repositorios concretos, excepciones).
- XML doc (`///`) en toda clase y método público que forme parte de la API pública del proyecto.
- **Snake_case en SQL** para compatibilidad con Dapper `MatchNamesWithUnderscores = true`.
- Prefijo de sección en comentarios inline: `// ── 1. Descripción ─────`.

---

## 5. Versiones y características modernas

| Capa | Tecnología | Versión | Notas |
|---|---|---|---|
| Runtime | .NET | 10 | TFM `net10.0` |
| Lenguaje | C# | 14 | Primary constructors, collection expressions |
| ORM | EF Core | 10 | Migraciones, DbContext, snake_case config explícita |
| Micro-ORM | Dapper | 2.1.x | `MatchNamesWithUnderscores`, multi-map, RETURNING |
| RDBMS | PostgreSQL | 16 | Docker `postgres:16`, Npgsql |
| Mediator | MediatR | 14.1 | `IRequestHandler<TRequest, TResponse>` |
| Autenticación | JwtBearer | 10.0.x | HS256, `OnChallenge` → ProblemDetails |
| OpenAPI | Swashbuckle + Microsoft.OpenApi | 10.x / 2.x | `OpenApiSecuritySchemeReference`, `AddSecurityRequirement(Func<>)` |
| Jobs | System.Threading.Channels | built-in | `Channel<T>`, `BackgroundService`, scoped DI por item |
| Observabilidad | OpenTelemetry | 1.15.x | Traces + metrics, OTLP/Console |
| Tests | xUnit + Moq + FluentAssertions | latest | — |

- Usar siempre las APIs más modernas disponibles en la versión del stack declarada.
- **No agregar paquetes obsoletos** o que dupliquen funcionalidad ya cubierta por el framework.

---

## 6. Arquitectura limpia de microservicios

Capas y reglas de dependencia estrictas:

```
Domain          ← sin dependencias externas
Application     ← depende de Domain
Infrastructure  ← depende de Application
API             ← depende de Application (no de Infrastructure directamente)
```

- **Domain**: entidades, value objects, `DomainException`, interfaces de dominio.
- **Application**: contratos (`IUnitOfWork`, `IOrderReadRepository`…), DTOs, handlers CQRS, Result pattern, validadores.
- **Infrastructure**: implementaciones de repos (EF + Dapper), UoW, jobs, DbContext, migraciones.
- **API**: controllers, middleware, extensiones de registro, `GlobalExceptionHandler`.

---

## 7. Escalabilidad y rendimiento

- **No N+1**: queries con JOIN o batch, nunca carga lazy.
- **Dapper para lectura** cuando la query es conocida y el rendimiento importa; EF para escritura con migraciones.
- **`Channel<T>` unbounded con `SingleReader = true`** para el job worker — evita contención.
- **Paginación obligatoria** en cualquier endpoint que devuelva colecciones (`pageSize` + `cursor` o `offset`).
- **Rate limiting** por IP configurado en `appsettings.json` (sliding window).
- **Idempotency-Key** en POST/PUT/PATCH para operaciones que deben ser seguras ante reintento.
- **Índices en `public_id`** (GUID externo) — nunca exponer el `id` interno (int/bigint) en la API.

---

## 8. CQRS

- **Commands** en `Features/{Aggregate}/Commands/{Action}/` — modifican estado, devuelven `Result` o `Result<T>`.
- **Queries** en `Features/{Aggregate}/Queries/{Action}/` — solo leen, nunca modifican estado.
- Un handler por command/query (`sealed class XCommandHandler : IRequestHandler<XCommand, Result>`).
- **Validación con FluentValidation** registrada como `IPipelineBehavior` — nunca dentro del handler.
- Los handlers no contienen lógica de negocio; orquestan llamadas al dominio, repos y UoW.

---

## 9. Repository y Unit of Work

- **Usar los métodos genéricos del repositorio base primero** (`GetByPublicIdAsync`, `AddAsync`, `UpdateAsync`).
- **Añadir métodos específicos solo cuando el caso de uso lo justifica** (e.g. `GetWithItemsAsync` para JOIN, `AddItemAsync` para `order_items`).
- **`IReadRepository<T>`** para queries — siempre sin transacción.
- **`IWriteRepository<T>`** dentro de `IUnitOfWork` — siempre dentro de transacción.
- **Dos constructores en `WriteRepository`**: uno para DI (sin TX) y uno para UoW (con `IDbConnection` + `IDbTransaction`).
- Dapper pasa `_transaction` explícitamente en cada llamada SQL.

---

## 10. MediatR

- Todo command/query se envía mediante `IMediator.Send()` desde el controller.
- Los controllers son **delgados**: reciben el request, envían a MediatR, mapean `Result` a `IActionResult`.
- Usar `IPipelineBehavior<,>` para cross-cutting concerns: logging, validación, performance.
- No instanciar handlers directamente; siempre a través del pipeline de MediatR.

---

## 11. Tests

- **xUnit** como framework; **Moq** para mocks; **FluentAssertions** para aserciones.
- Estructura de carpetas en `Microservice.Test` espeja la estructura del proyecto que prueba.
- Tests unitarios para handlers, validadores y lógica de dominio.
- Los tipos usados como parámetro genérico de mocks (`IValidator<T>`) deben ser **`public`** a nivel de namespace — Moq no puede crear proxies de tipos privados o anidados con FluentValidation strong-named.
- Naming convention: `MetodoQuePrueba_Escenario_ResultadoEsperado`.

---

## 12. Docker

- La base de datos de desarrollo corre en Docker (`postgres:16`).
- El `docker-compose.yml` define el servicio `postgres` con healthcheck.
- Las migraciones se aplican con `dotnet ef database update` apuntando al container.
- Las connection strings en `appsettings.Development.json` usan `localhost` + puerto mapeado del container.

---

## Regla general del agente

El agente opera en dos modos según el caso:

**Patrón conocido** (cubierto por esta guía o por `/arquitectura`):
→ Implementar directamente. No preguntar, no dudar.

**Patrón nuevo o ambiguo** (no documentado):
→ Tomar el camino de desarrollo de forma autónoma, respetando:
  - Las convenciones de código del stack (C# 14, naming, async, DI).
  - Las librerías ya presentes (nunca introducir una nueva sin consultar).
  - Rendimiento y buenas prácticas del stack actual.
  - El principio de menor sorpresa: el código nuevo debe leerse como el código existente.
→ Preguntar al piloto **solo si**:
  - La feature requiere una librería que no está en el stack.
  - El diseño implica un cambio estructural (nueva capa, nuevo contrato, cambio de arquitectura).
  - Hay dos enfoques válidos sin criterio claro para elegir entre ellos.
  - Se detecta un riesgo de seguridad o rendimiento que el patrón existente no cubre.

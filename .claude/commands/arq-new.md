# /arq-new — Checklist para nuevo aggregate (end-to-end)

> **Antes de empezar:** leer los archivos de referencia de la fila correspondiente en `/arquitectura`.
> El aggregate `Example` (EF) y `Order` (Dapper) son la implementación canónica — el código nuevo debe leerse igual que el existente.

---

## Principio generic-first

Aplicar en este orden antes de crear cualquier interfaz o clase nueva:

1. ¿Alcanza `IReadRepository<T>`? → **Inyectarlo directamente.** No crear `IMyEntityReadRepository`.
2. ¿Alcanza `IUnitOfWork.WriteRepository`? → **Usarlo directamente.** No crear `IMyEntityWriteRepository`.
3. Solo si la superficie genérica no alcanza → crear la interfaz específica del aggregate y justificarlo en un comentario.

---

## Path A — EF Core

Referencia: `Example` + `ExampleItem`. Leer `ExampleDbContext.cs`, `CreateExampleCommandHandler.cs` y `UpdateExampleCommandHandler.cs` antes de implementar.

### Checklist en orden

- [ ] **`Domain/Entities/MyEntity.cs`** — `sealed`, private setters, constructor factory, métodos de dominio que lanzan `DomainException`. Si tiene hijos: `private readonly List<MyEntityItem> _items = []` + `public IReadOnlyList<MyEntityItem> Items => _items.AsReadOnly()`.
- [ ] **`Domain/Entities/MyEntityItem.cs`** (si aplica) — `sealed`, constructor `internal` (solo el aggregate root puede instanciarla), `private set` en todo, método de transición de estado `internal`.
- [ ] **`ExampleDbContext.cs`** — añadir `DbSet<MyEntity>` (+ `DbSet<MyEntityItem>` si aplica). Configurar en `OnModelCreating`: `HasIndex(PublicId).IsUnique()`, `HasMaxLength`, `IsRequired`, `HasMany/WithOne/FK/Cascade`. Si tiene hijos: `Navigation(e => e.Items).HasField("_items").UsePropertyAccessMode(PropertyAccessMode.Field)` para que el snapshot change-tracker detecte Add/Remove via domain methods.
- [ ] **Migración** — `dotnet ef migrations add Add_MyEntity --project Microservice.Infrastructure --startup-project Microservice.API` → `dotnet ef database update ...`.
- [ ] **`Application/Contracts/Persistence/EF/IMyEntityReadRepository.cs`** — **solo si** el handler necesita queries que no existen en `IReadRepository<T>` (ej. `ExistsByNameAsync` con ILike case-insensitive). El eager-loading de hijos **no** justifica esta interfaz — usar `GetEntityAsync` con `includeProperties:[e => e.Children]`. Si no hay lógica específica del aggregate: omitir.
- [ ] **`Application/Contracts/Persistence/EF/IMyEntityWriteRepository.cs`** — **solo si** el aggregate necesita métodos de escritura fuera de la superficie genérica. Si no: omitir. Body vacío es válido si solo necesitas la distinción de tipo.
- [ ] **`Application/Contracts/Persistence/EF/IUnitOfWork.cs`** — añadir `IMyEntityWriteRepository MyEntityWrite { get; }` **solo si** se creó `IMyEntityWriteRepository`. Si la superficie genérica alcanza, el handler usa `WriteRepository` directamente.
- [ ] **`Application/DTOs/MyEntity/`** — DTOs de salida. Usar `record` o `class` según si se necesita igualdad estructural.
- [ ] **`Application/Mapping/MappingProfile.cs`** — `CreateMap<CreateMyEntityCommand, MyEntity>().ConstructUsing(src => new MyEntity(src.Name, ...))`.
- [ ] **`Application/Features/MyEntityEF/Commands/`** — un handler por command. Seguir los patrones de `Example`:
  - **Create**: mapper → `AddItem()` por hijo → `AddAsync` → `SaveChanges`.
  - **Update (PUT)**: `GetEntityAsync(includeProperties:[e=>e.Items], disableTracking:false)` → domain methods → `Update` → `SaveChanges`.
  - **Update (PATCH)**: `GetEntityAsync(disableTracking:true)` → domain methods → `UpdateFields([x=>x.Campo])` → `SaveChanges`.
  - **Delete individual**: `GetEntityAsync` → `Delete` → `SaveChanges` (cascade en DB elimina hijos).
  - **Delete bulk**: `WriteRepository.DeleteManyAsync(predicado)` → `SaveChanges` (sin cargar entidades).
- [ ] **`Application/Features/MyEntityEF/Queries/`** — inyectar `IReadRepository<MyEntity>` directamente. Para cargar hijos usar `GetEntityAsync(predicate, includeProperties:[e => e.Children])` — no crear método específico en el repositorio. Solo inyectar `IMyEntityReadRepository` si el query necesita lógica que no existe en la superficie genérica (ej. ILike, joins complejos).
- [ ] **`Infrastructure/Repositories/EF/MyEntityWriteRepository.cs`** — `sealed`, hereda `LINQRepository<MyEntity>`, implementa `IMyEntityWriteRepository` (si fue creada). Agregar métodos solo si son necesarios.
- [ ] **`Infrastructure/Repositories/EF/MyEntityReadRepository.cs`** — `sealed`, hereda `LINQRepository<MyEntity>`, implementa `IMyEntityReadRepository` (si fue creada). `GetWithChildrenAsync` usa `.Include(e => e.Children)`. `ExistsByNameAsync` usa `Microsoft.EntityFrameworkCore.EF.Functions.ILike` (fully qualified para evitar conflicto de namespace).
- [ ] **`Infrastructure/Repositories/EF/UnitOfWork.cs`** — si se creó `IMyEntityWriteRepository`: añadir `private MyEntityWriteRepository? _myEntity;` + lazy property. Exponer por ambas interfaces apuntando a la misma instancia.
- [ ] **`InfrastuctureServiceRegistration.cs`** — registrar `IMyEntityReadRepository → MyEntityReadRepository` (si fue creada). El UoW EF ya está registrado globalmente.
- [ ] **`API/Controllers/MyEntityController.cs`** — controller delgado: recibe request, envía a MediatR, mapea `Result` → `IActionResult` con `ResultExtensions`.

### Comentarios obligatorios en cada clase

Cada clase del end-to-end debe tener un comentario que indique:
- Qué patrón demuestra (`// PATRÓN —`).
- La regla generic-first aplicada (por qué se creó o no una interfaz específica).
- Cuándo el agente debe replicar esta clase para una nueva entidad vs cuándo omitirla.

---

## Path B — Dapper

Referencia: `Order`. Leer `OrderReadRepository.cs`, `OrderWriteRepository.cs` y `UnitOfWork.cs` (Dapper) antes de implementar.

### Checklist en orden

- [ ] `Domain/Entities/MyEntity.cs` — constructor factory, métodos que lanzan `DomainException`.
- [ ] `Application/Contracts/Persistence/Dapper/IMyEntityReadRepository.cs`
- [ ] `Application/Contracts/Persistence/Dapper/IMyEntityWriteRepository.cs`
- [ ] Añadir `IMyEntityWriteRepository MyEntityWrite { get; }` en `Application/Contracts/Persistence/Dapper/IUnitOfWork.cs`.
- [ ] `Application/DTOs/MyEntity/` — `class` con public setters para Dapper multi-map.
- [ ] `Application/Features/MyEntity/Commands/` y `Queries/`.
- [ ] `Infrastructure/Repositories/Dapper/MyEntityReadRepository.cs`.
- [ ] `Infrastructure/Repositories/Dapper/MyEntityWriteRepository.cs` — dos constructores: DI sin TX / UoW con `IDbConnection + IDbTransaction`.
- [ ] Lazy property en `Infrastructure/Repositories/Dapper/UnitOfWork.cs`: `_myEntity ??= new MyEntityWriteRepository(_connection!, _transaction!)`.
- [ ] Registrar en `InfrastuctureServiceRegistration.cs` — 2 líneas.
- [ ] `API/Controllers/MyEntityController.cs`.

# /arq-new — Checklist para nuevo aggregate (end-to-end)

## 13 pasos en orden

- [ ] `Domain/Entities/MyEntity.cs` — hereda `BaseDomainModel`; factory `Create()`; métodos que lanzan `DomainException`
- [ ] EF config en `ExampleDbContext` — `ToTable("my_entities")`, `HasColumnName` snake_case para cada propiedad
- [ ] Migración EF + `dotnet ef database update`
- [ ] `Application/Contracts/Persistence/Dapper/IMyEntityReadRepository.cs`
- [ ] `Application/Contracts/Persistence/Dapper/IMyEntityWriteRepository.cs`
- [ ] Añadir `IMyEntityWriteRepository MyEntityWrite { get; }` en `IUnitOfWork`
- [ ] `Application/DTOs/MyEntity/MyEntityDto.cs` + `MyEntitySummaryDto` (class con public setters para Dapper)
- [ ] Features: Commands + Queries en `Application/Features/MyEntity/`
- [ ] `Infrastructure/Repositories/Dapper/MyEntityReadRepository.cs`
- [ ] `Infrastructure/Repositories/Dapper/MyEntityWriteRepository.cs` — 2 constructores
- [ ] Lazy prop en `UnitOfWork.cs`: `_myEntityWrite ??= new MyEntityWriteRepository(_connection!, _transaction!)`
- [ ] Registrar en `InfrastuctureServiceRegistration.cs` (2 líneas)
- [ ] `API/Controllers/MyEntityController.cs`

## Archivos de referencia a leer

| Pieza | Archivo |
|---|---|
| Entidad | `Domain/Entities/Order.cs` |
| Read repo | `Infrastructure/Repositories/Dapper/OrderReadRepository.cs` |
| Write repo | `Infrastructure/Repositories/Dapper/OrderWriteRepository.cs` |
| UoW | `Infrastructure/Repositories/Dapper/UnitOfWork.cs` |
| Command handler | `Features/Orders/Commands/CancelOrder/CancelOrderCommandHandler.cs` |
| Query handler | `Features/Orders/Queries/GetOrderById/GetOrderByIdQueryHandler.cs` |
| Controller | `API/Controllers/OrdersController.cs` |

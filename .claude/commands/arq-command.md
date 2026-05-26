# /arq-command — Patrón command handler

Leer: `Features/Orders/Commands/CancelOrder/CancelOrderCommandHandler.cs`
Para `Result<Guid>`: `Features/Orders/Commands/CreateOrder/CreateOrderCommandHandler.cs`

**Regla no obvia:** llamar métodos de dominio (`order.Cancel()`) ANTES de `BeginTransactionAsync`.
Si lanzan `DomainException` no hay TX abierta → no hay rollback que gestionar.

Errores: `Error.NotFound`→404 · `Error.Conflict`→409 · `Error.Validation`→400 · `Error.Unauthorized`→401

# /arq-errors — Error handling

Leer: `API/ExceptionHandling/GlobalExceptionHandler.cs`

| Excepción | HTTP | Cuándo |
|---|---|---|
| `DomainException` | 409 | Invariantes de dominio |
| `ValidationException` | 400 | FluentValidation pipeline |
| `ArgumentException` | 400 | Factory guards |
| `KeyNotFoundException` | 404 | Entidad no encontrada |
| Cualquier otra | 500 | Error inesperado |

Result: `Error.NotFound`→404 · `Error.Conflict`→409 · `Error.Validation`→400 · `Error.Unauthorized`→401 · `Error.Forbidden`→403

`try-catch` solo para RollbackAsync — siempre re-lanzar con `throw`.

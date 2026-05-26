# /arq-rules — Reglas críticas, convenciones y autonomía

## 8 reglas críticas

1. **`try-catch` solo para `RollbackAsync`** — todo lo demás va a GlobalExceptionHandler.
2. **No duplicar reglas de dominio** — si `Order.Cancel()` ya valida, no añadir guard en el handler.
3. **`DomainException` antes de `BeginTransactionAsync`** — así no hay rollback que gestionar.
4. **Genéricos first** — usar `GetByPublicIdAsync`, `UpdateAsync` antes de crear métodos específicos.
5. **snake_case en SQL** — `MatchNamesWithUnderscores` mapea automáticamente a PascalCase en C#.
6. **Dos constructores en write repos** — DI standalone + UoW compartido; implementar siempre los dos.
7. **Paginación obligatoria** en colecciones — `GetPagedAsync` + `PagedResult<T>`.
8. **Ante duda estructural → preguntar al piloto.**

## Convenciones de código

| Área | Regla |
|---|---|
| **Lenguaje** | C# 14: primary constructors, `record`, `sealed`, collection expressions `[]` |
| **Naming** | PascalCase público · `_camelCase` privado · snake_case en SQL |
| **Async** | Siempre `async/await` con `CancellationToken ct` propagado hasta el repo |
| **Inyección** | Constructor injection vía primary constructors; interfaces, nunca concretos |

## Cuándo preguntar al piloto

- La feature requiere una librería que no está en el stack.
- El diseño implica un cambio estructural (nueva capa, nuevo contrato, cambio en `IUnitOfWork`).
- Hay dos enfoques válidos sin criterio claro para elegir entre ellos.

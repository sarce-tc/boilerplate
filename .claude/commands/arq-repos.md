# /arq-repos — Repositorios Dapper y UnitOfWork

Leer: `Infrastructure/Repositories/Dapper/OrderReadRepository.cs`  ← AGENT ENTRY POINT
Leer: `Infrastructure/Repositories/Dapper/OrderWriteRepository.cs` ← AGENT ENTRY POINT
Leer: `Infrastructure/Repositories/Dapper/UnitOfWork.cs`           ← cómo añadir nuevo repo
Leer: `Application/Contracts/Persistence/Dapper/IUnitOfWork.cs`   ← AGENT ENTRY POINT

**Regla no obvia:** dos constructores siempre en write repos — DI standalone (`IDbConnectionFactory`) + UoW compartido (`NpgsqlConnection` + `NpgsqlTransaction`).

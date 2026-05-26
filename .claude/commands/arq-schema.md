# /arq-schema — Schema SQL

Leer: `Infrastructure/Persistence/ExampleDbContext.cs` ← columnas exactas con `HasColumnName`

`MatchNamesWithUnderscores = true` → `public_id`→`PublicId` · `customer_name`→`CustomerName` · `item_count`→`ItemCount`

Convenciones: `RETURNING *` en INSERT/UPDATE · snake_case en SQL · nunca exponer `id` (int) en la API, solo `public_id` (Guid).

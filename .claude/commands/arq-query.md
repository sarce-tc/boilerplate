# /arq-query — Patrón query handler

Leer (single):    `Features/Orders/Queries/GetOrderById/GetOrderByIdQueryHandler.cs`
Leer (paginado):  `Features/Orders/Queries/GetOrders/GetOrdersQueryHandler.cs`

**Reglas no obvias:** nunca `IUnitOfWork` en queries — solo `IXReadRepository`. Nunca `try-catch`. Siempre `PagedResult<T>` en colecciones.

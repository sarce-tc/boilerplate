namespace Microservice.Application.DTOs.EF;
// Contrato de salida de la query GetExamplesPaginated.
// AutoMapper hidrata este record desde cada entidad Example de la página devuelta por GetListPaginatedAsync.
public record GetExamplesPaginatedDto(Guid PublicId, string Name, string? Description, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);

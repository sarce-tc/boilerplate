namespace Microservice.Application.DTOs.EF;
// Contrato de salida de la query GetExamplesPaginated, con sus hijos.
// AutoMapper hidrata este record desde cada entidad Example de la página (con Items via Include).
public record GetExamplesPaginatedDto(
    Guid                            PublicId,
    string                          Name,
    string?                         Description,
    DateTimeOffset                  CreatedAt,
    DateTimeOffset                  UpdatedAt,
    IReadOnlyList<GetExampleItemDto> Items);

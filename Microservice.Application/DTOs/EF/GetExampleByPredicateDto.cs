namespace Microservice.Application.DTOs.EF;
// Contrato de salida de la query GetExampleByPredicate, con sus hijos.
// AutoMapper hidrata este record desde la entidad Example (con Items cargados via Include).
public record GetExampleByPredicateDto(
    Guid                            PublicId,
    string                          Name,
    string?                         Description,
    DateTimeOffset                  CreatedAt,
    DateTimeOffset                  UpdatedAt,
    IReadOnlyList<GetExampleItemDto> Items);

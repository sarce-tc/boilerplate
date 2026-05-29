namespace Microservice.Application.DTOs.EF;
// Contrato de salida de la query GetAllExamples (EF), con sus hijos.
// AutoMapper hidrata este record desde las entidades Example (con Items cargados via Include).
public record GetAllExamplesDto(
    Guid                            PublicId,
    string                          Name,
    string?                         Description,
    DateTimeOffset                  CreatedAt,
    DateTimeOffset                  UpdatedAt,
    IReadOnlyList<GetExampleItemDto> Items);

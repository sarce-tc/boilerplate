namespace Microservice.Application.DTOs.Dapper;
// Contrato de salida de la query GetAllExamplesDapper (con hijos).
// Se hidrata por multi-mapping (JOIN a dapper.example_items) en ExampleReadRepository.
public record GetAllExamplesDto(
    Guid                            PublicId,
    string                          Name,
    string?                         Description,
    DateTimeOffset                  CreatedAt,
    DateTimeOffset                  UpdatedAt,
    IReadOnlyList<GetExampleItemDto> Items
);

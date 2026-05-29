namespace Microservice.Application.DTOs.Dapper;
// Contrato de salida de la query GetExamplesPaginatedDapper (con hijos).
// Se hidrata por multi-mapping (JOIN a dapper.example_items) en ExampleReadRepository.
public record GetExamplesPaginatedDto(
    Guid                            PublicId,
    string                          Name,
    string?                         Description,
    DateTimeOffset                  CreatedAt,
    DateTimeOffset                  UpdatedAt,
    IReadOnlyList<GetExampleItemDto> Items
);

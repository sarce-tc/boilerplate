using Microservice.Domain.Entities;

namespace Microservice.Application.DTOs.EF;
// Contrato de salida de la query GetExampleWithItems.
// AutoMapper hidrata este record desde la entidad Example con su colección Items cargada via eager-loading.
public record GetExampleWithItemsDto(
    Guid                          PublicId,
    string                        Name,
    string?                       Description,
    ExampleStatus                 Status,
    IEnumerable<GetExampleItemDto> Items,
    DateTimeOffset                CreatedAt,
    DateTimeOffset                UpdatedAt
);

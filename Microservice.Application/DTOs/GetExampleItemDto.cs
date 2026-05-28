using Microservice.Domain.Entities;

namespace Microservice.Application.DTOs;
// Contrato de salida de las queries GetExampleItems y GetExampleItemByPublicId.
// AutoMapper hidrata este record desde una entidad ExampleItem cargada como hijo del aggregate Example.
public record GetExampleItemDto(
    Guid            PublicId,
    string          Label,
    int             Quantity,
    ExampleItemStatus Status,
    DateTimeOffset  CreatedAt,
    DateTimeOffset  UpdatedAt
);

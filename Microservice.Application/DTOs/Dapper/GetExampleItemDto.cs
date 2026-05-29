using Microservice.Domain.Entities;

namespace Microservice.Application.DTOs.Dapper;
// Contrato de salida de un hijo ExampleItem en las queries Dapper con items.
// Se hidrata por multi-mapping desde la tabla dapper.example_items (no via AutoMapper).
public record GetExampleItemDto(
    Guid              PublicId,
    string            Label,
    int               Quantity,
    ExampleItemStatus Status,
    DateTimeOffset    CreatedAt,
    DateTimeOffset    UpdatedAt
);

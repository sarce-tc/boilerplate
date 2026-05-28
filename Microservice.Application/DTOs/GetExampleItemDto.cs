using Microservice.Domain.Entities;

namespace Microservice.Application.DTOs;
public record GetExampleItemDto(
    Guid            PublicId,
    string          Label,
    int             Quantity,
    ExampleItemStatus Status,
    DateTimeOffset  CreatedAt,
    DateTimeOffset  UpdatedAt
);

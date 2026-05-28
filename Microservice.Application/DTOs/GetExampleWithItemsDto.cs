using Microservice.Domain.Entities;

namespace Microservice.Application.DTOs;
public record GetExampleWithItemsDto(
    Guid                          PublicId,
    string                        Name,
    string?                       Description,
    ExampleStatus                 Status,
    IEnumerable<GetExampleItemDto> Items,
    DateTimeOffset                CreatedAt,
    DateTimeOffset                UpdatedAt
);

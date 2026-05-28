namespace Microservice.Application.DTOs;
public record GetExampleByPredicateDto(Guid PublicId, string Name, string? Description, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);

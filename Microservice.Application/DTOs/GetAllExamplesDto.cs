namespace Microservice.Application.DTOs;
public record GetAllExamplesDto(Guid PublicId, string Name, string? Description, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);

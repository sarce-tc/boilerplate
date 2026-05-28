namespace Microservice.Application.DTOs;
public record GetExamplesPaginatedDto(Guid PublicId, string Name, string? Description, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);

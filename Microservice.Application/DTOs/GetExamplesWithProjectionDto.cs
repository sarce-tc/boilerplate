namespace Microservice.Application.DTOs;
public record GetExamplesWithProjectionDto(Guid PublicId, string Name, string? Description);

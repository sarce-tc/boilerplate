namespace Microservice.Application.DTOs.EF;
// Contrato de salida de la query GetExamplesWithProjection.
// El handler lo construye directamente en el selector lambda de IQueryRepository, sin pasar por AutoMapper.
public record GetExamplesWithProjectionDto(Guid PublicId, string Name, string? Description);

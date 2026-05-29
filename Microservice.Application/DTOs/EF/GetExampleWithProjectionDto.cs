namespace Microservice.Application.DTOs.EF;
// Contrato de salida de la query GetExampleWithProjection.
// El handler lo construye directamente en el selector lambda de IQueryRepository, sin pasar por AutoMapper.
public record GetExampleWithProjectionDto(Guid PublicId, string Name, string? Description);

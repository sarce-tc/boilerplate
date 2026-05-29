namespace Microservice.Application.DTOs.EF;
// Contrato de salida de la query GetExampleById.
// AutoMapper hidrata este record desde la entidad Example recuperada por publicId en el handler.
public record GetExampleByIdDto(Guid PublicId, string Name, string? Description, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);

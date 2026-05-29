namespace Microservice.Application.DTOs.EF;
// Contrato de salida de la query GetAllExamples (EF).
// AutoMapper hidrata este record desde las entidades Example devueltas por GetListAsync en el handler.
public record GetAllExamplesDto(Guid PublicId, string Name, string? Description, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);

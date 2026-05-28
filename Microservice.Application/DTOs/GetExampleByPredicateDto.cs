namespace Microservice.Application.DTOs;
// Contrato de salida de la query GetExampleByPredicate.
// AutoMapper hidrata este record desde la entidad Example recuperada por predicado publicId en el handler.
public record GetExampleByPredicateDto(Guid PublicId, string Name, string? Description, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);

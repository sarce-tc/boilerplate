namespace Microservice.Application.DTOs.EF;
// Contrato de salida de la query GetExamplesFromSql.
// AutoMapper hidrata este record desde las entidades Example materializadas por el SQL hardcoded en el handler.
public record GetExamplesFromSqlDto(Guid PublicId, string Name, string? Description, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);

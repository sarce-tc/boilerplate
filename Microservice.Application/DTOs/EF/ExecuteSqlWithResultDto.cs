namespace Microservice.Application.DTOs.EF;
// Contrato de salida de la query ExecuteSqlWithResult.
// AutoMapper hidrata este record desde las entidades Example materializadas por FromSqlAsync en el handler.
public record ExecuteSqlWithResultDto(Guid PublicId, string Name, string? Description, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);

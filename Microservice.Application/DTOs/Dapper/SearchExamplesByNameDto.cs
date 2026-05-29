namespace Microservice.Application.DTOs.Dapper;
// Contrato de salida de la query SearchExamplesByNameDapper.
// AutoMapper hidrata este record desde la entidad Example usando el perfil en MappingProfile.
public record SearchExamplesByNameDto(Guid PublicId, string Name, string? Description, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);

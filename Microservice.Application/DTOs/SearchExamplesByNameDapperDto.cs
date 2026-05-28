namespace Microservice.Application.DTOs;
// Contrato de salida de la query SearchExamplesByNameDapper.
// AutoMapper hidrata este record desde la entidad Example usando el perfil en MappingProfile.
public record SearchExamplesByNameDapperDto(Guid PublicId, string Name, string? Description, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);

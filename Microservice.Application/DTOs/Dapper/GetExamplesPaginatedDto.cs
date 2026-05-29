namespace Microservice.Application.DTOs.Dapper;
// Contrato de salida de la query GetExamplesPaginatedDapper.
// AutoMapper hidrata este record desde la entidad Example usando el perfil en MappingProfile.
public record GetExamplesPaginatedDto(Guid PublicId, string Name, string? Description, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);

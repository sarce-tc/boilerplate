namespace Microservice.Application.DTOs;
// Contrato de salida de la query GetExamplesPaginatedDapper.
// AutoMapper hidrata este record desde la entidad Example usando el perfil en MappingProfile.
public record GetExamplesPaginatedDapperDto(Guid PublicId, string Name, string? Description, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);

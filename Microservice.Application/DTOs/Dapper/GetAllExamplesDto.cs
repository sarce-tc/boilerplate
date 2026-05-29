namespace Microservice.Application.DTOs.Dapper;
// Contrato de salida de la query GetAllExamplesDapper.
// AutoMapper hidrata este record desde la entidad Example usando el perfil en MappingProfile;
// expone los campos de identificación pública, datos descriptivos y marcas de auditoría temporal.
public record GetAllExamplesDto(Guid PublicId, string Name, string? Description, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);

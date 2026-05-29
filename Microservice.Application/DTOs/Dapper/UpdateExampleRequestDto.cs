namespace Microservice.Application.DTOs.Dapper;
// Contrato de entrada del endpoint PUT /examples/{publicId}.
// El controller lo deserializa desde el body y construye UpdateExampleCommand con sus valores.
public record UpdateExampleRequestDto(string? Name, string? Description);

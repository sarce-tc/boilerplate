namespace Microservice.Application.DTOs;
// Contrato de entrada del endpoint PUT /examples/{publicId}/fields.
// El controller lo deserializa desde el body y construye UpdateExampleFieldsCommand con sus valores.
public record UpdateExampleFieldsRequestDto(string? Name, string? Description);

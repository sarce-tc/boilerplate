using Microservice.Application.Features.ExamplesDapper.Commands.UpdateExampleDapper;

namespace Microservice.Application.DTOs.Dapper;
// Contrato de entrada del endpoint PUT /examplesdapper/{publicId}.
// El controller lo deserializa desde el body y construye UpdateExampleDapperCommand con sus valores.
// Items (opcional) sigue la semántica replace-all del comando: null = no tocar, [] = vaciar.
public record UpdateExampleRequestDto(
    string? Name,
    string? Description,
    IReadOnlyList<UpdateExampleItemDapperRequest>? Items = null);

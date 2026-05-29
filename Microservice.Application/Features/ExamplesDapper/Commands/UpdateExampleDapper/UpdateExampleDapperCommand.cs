using MediatR;
using Microservice.Application.Common.Results;

namespace Microservice.Application.Features.ExamplesDapper.Commands.UpdateExampleDapper;
// PATRÓN — Mensaje que MediatR enruta hacia UpdateExampleDapperCommandHandler.
//   Transporta PublicId para localizar el aggregate y Name/Description opcionales para actualizar; aplica semántica PUT.
//   Items (opcional) reemplaza el conjunto de hijos del aggregate (estrategia replace-all):
//     · null  → no se tocan los items existentes.
//     · []    → se eliminan todos los items.
//     · [...] → se reemplazan por exactamente estos.
//   Result<Guid> retorna el PublicId del registro actualizado.
public sealed record UpdateExampleDapperCommand(
    Guid PublicId,
    string? Name,
    string? Description,
    IReadOnlyList<UpdateExampleItemDapperRequest>? Items = null
) : IRequest<Result<Guid>>;

// Hijo del conjunto de reemplazo. No lleva PublicId: replace-all regenera identidad
// en cada edición (si se requiere preservar identidad, migrar a diff/merge por PublicId).
public sealed record UpdateExampleItemDapperRequest(string Label, int Quantity);

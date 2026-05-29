using MediatR;
using Microservice.Application.Common.Results;

namespace Microservice.Application.Features.ExamplesDapper.Commands.CreateExampleDapper;
// PATRÓN — Crea un aggregate raíz Example (via Dapper) con una colección opcional de hijos en el mismo request.
//   Transporta Name/Description para construir el aggregate e Items para sus ExampleItem hijos.
//   Items es opcional; si se provee, cada elemento se añade vía domain method (example.AddItem),
//   que valida invariantes (aggregate activo, label único, quantity > 0).
//   Result<Guid> retorna el PublicId generado del nuevo registro.
public sealed record CreateExampleDapperCommand(
    string Name,
    string? Description,
    IReadOnlyList<CreateExampleItemDapperRequest>? Items = null
) : IRequest<Result<Guid>>;

// Hijo a crear junto al aggregate. Espejo de CreateExampleItemRequest (lado EF).
public sealed record CreateExampleItemDapperRequest(string Label, int Quantity);

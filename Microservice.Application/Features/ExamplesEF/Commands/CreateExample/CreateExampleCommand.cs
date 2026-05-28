using MediatR;
using Microservice.Application.Common.Results;

namespace Microservice.Application.Features.ExamplesEF.Commands.CreateExample;
// PATRÓN — Crea un aggregate raíz Example con una colección opcional de hijos en el mismo request.
//   PublicId del aggregate creado se devuelve como contrato de respuesta (Result<Guid>).
//   Items es opcional; si se provee, cada elemento se añade vía domain method del aggregate.
public record CreateExampleCommand(
    string Name,
    string? Description,
    IReadOnlyList<CreateExampleItemRequest>? Items = null
) : IRequest<Result<Guid>>;

public record CreateExampleItemRequest(string Label, int Quantity);

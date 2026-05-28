using MediatR;
using Microservice.Application.Common.Results;

namespace Microservice.Application.Features.ExamplesEF.Commands.UpdateExample;
// PATRÓN — Actualiza el aggregate Example (PUT): campos escalares y operaciones sobre hijos en un solo request.
//   PublicId identifica el aggregate; Name y Description son opcionales (null = sin cambio).
//   AddItems agrega nuevos hijos, RemoveItemIds elimina existentes, CompleteItemIds los transiciona a completado.
//   Contrato de respuesta: Result<Guid> con el PublicId del aggregate actualizado.
public record UpdateExampleCommand(
    Guid PublicId,
    string? Name,
    string? Description,
    IReadOnlyList<UpdateExampleItemRequest>? AddItems      = null,
    IReadOnlyList<Guid>?                    RemoveItemIds  = null,
    IReadOnlyList<Guid>?                    CompleteItemIds = null
) : IRequest<Result<Guid>>;

public record UpdateExampleItemRequest(string Label, int Quantity);

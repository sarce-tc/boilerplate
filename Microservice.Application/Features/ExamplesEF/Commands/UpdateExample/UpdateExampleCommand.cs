using MediatR;
using Microservice.Application.Common.Results;

namespace Microservice.Application.Features.ExamplesEF.Commands.UpdateExample;
public record UpdateExampleCommand(
    Guid PublicId,
    string? Name,
    string? Description,
    IReadOnlyList<UpdateExampleItemRequest>? AddItems      = null,
    IReadOnlyList<Guid>?                    RemoveItemIds  = null,
    IReadOnlyList<Guid>?                    CompleteItemIds = null
) : IRequest<Result<Guid>>;

public record UpdateExampleItemRequest(string Label, int Quantity);

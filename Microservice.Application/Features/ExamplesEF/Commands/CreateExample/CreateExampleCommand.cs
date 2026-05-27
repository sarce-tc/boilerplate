using MediatR;
using Microservice.Application.Common.Results;

namespace Microservice.Application.Features.ExamplesEF.Commands.CreateExample
{
    public record CreateExampleCommand(
        string Name,
        string? Description,
        IReadOnlyList<CreateExampleItemRequest>? Items = null
    ) : IRequest<Result<Guid>>;

    public record CreateExampleItemRequest(string Label, int Quantity);
}

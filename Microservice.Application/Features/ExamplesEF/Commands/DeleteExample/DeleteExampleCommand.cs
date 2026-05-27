using MediatR;
using Microservice.Application.Common.Results;

namespace Microservice.Application.Features.ExamplesEF.Commands.DeleteExample
{
    public record DeleteExampleCommand(
        Guid PublicId
    ) : IRequest<Result<Guid>>;
}

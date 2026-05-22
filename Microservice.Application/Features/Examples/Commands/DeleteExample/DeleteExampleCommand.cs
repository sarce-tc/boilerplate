using MediatR;
using Microservice.Application.Common.Results;

namespace Microservice.Application.Features.Examples.Commands.DeleteExample
{
    public record DeleteExampleCommand(
        Guid PublicId
    ) : IRequest<Result<Guid>>;
}

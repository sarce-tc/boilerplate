using MediatR;
using Microservice.Application.Common.Results;

namespace Microservice.Application.Features.Examples.Commands.DeleteManyExamples
{
    public record DeleteManyExamplesCommand(
        Guid[] PublicIds
    ) : IRequest<Result<int>>;
}

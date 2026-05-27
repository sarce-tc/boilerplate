using MediatR;
using Microservice.Application.Common.Results;

namespace Microservice.Application.Features.ExamplesEF.Commands.DeleteManyExamples
{
    public record DeleteManyExamplesCommand(
        Guid[] PublicIds
    ) : IRequest<Result<int>>;
}

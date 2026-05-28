using MediatR;
using Microservice.Application.Common.Results;

namespace Microservice.Application.Features.ExamplesEF.Commands.UpdateManyExamples;
public record UpdateManyExamplesCommand(
    Guid[] PublicIds
) : IRequest<Result<int>>;

using MediatR;
using Microservice.Application.Common.Results;

namespace Microservice.Application.Features.Examples.Commands.UpdateExampleFields
{
    public record UpdateExampleFieldsCommand(
        Guid PublicId,
        string? Name,
        string? Description
    ) : IRequest<Result<Guid>>;
}

using MediatR;
using Microservice.Application.Common.Results;

namespace Microservice.Application.Features.ExamplesEF.Commands.UpdateExampleFields;
public record UpdateExampleFieldsCommand(
    Guid PublicId,
    string? Name,
    string? Description
) : IRequest<Result<Guid>>;

using MediatR;
using Microservice.Application.Common.Results;

namespace Microservice.Application.Features.Examples.Queries.ExistsExample
{
    public record ExistsExampleQuery(
        Guid PublicId
    ) : IRequest<Result<bool>>;
}

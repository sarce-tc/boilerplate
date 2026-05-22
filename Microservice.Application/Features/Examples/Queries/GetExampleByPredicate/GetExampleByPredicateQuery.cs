using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.DTOs;

namespace Microservice.Application.Features.Examples.Queries.GetExampleByPredicate
{
    public record GetExampleByPredicateQuery(
        Guid PublicId
    ) : IRequest<Result<GetExampleByPredicateDto>>;
}

using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.DTOs;

namespace Microservice.Application.Features.ExamplesEF.Queries.GetExamplesFromSql
{
    public record GetExamplesFromSqlQuery(
        string Sql
    ) : IRequest<Result<IEnumerable<GetExamplesFromSqlDto>>>;
}

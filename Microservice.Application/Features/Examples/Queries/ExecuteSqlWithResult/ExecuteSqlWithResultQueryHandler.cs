using AutoMapper;
using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Application.DTOs;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.Examples.Queries.ExecuteSqlWithResult
{
    // EF raw SELECT (caller-supplied SQL): ISqlQueryRepository<T>.FromSqlAsync with FormattableString from the request
    public class ExecuteSqlWithResultQueryHandler(
        ISqlQueryRepository<Example> sqlQueryRepository,
        IMapper mapper
        ) : IRequestHandler<ExecuteSqlWithResultQuery, Result<IReadOnlyList<ExecuteSqlWithResultDto>>>
    {
        public async Task<Result<IReadOnlyList<ExecuteSqlWithResultDto>>> Handle(ExecuteSqlWithResultQuery request, CancellationToken cancellationToken)
        {
            var examples = await sqlQueryRepository.FromSqlAsync(request.Sql, cancellationToken);
            var data = mapper.Map<IReadOnlyList<ExecuteSqlWithResultDto>>(examples);
            return Result<IReadOnlyList<ExecuteSqlWithResultDto>>.Success(data);
        }
    }
}

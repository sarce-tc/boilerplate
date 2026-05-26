using AutoMapper;
using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Application.DTOs;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.Examples.Queries.GetExamplesFromSql
{
    // EF raw SELECT: ISqlQueryRepository<T>.FromSqlAsync — always use FormattableString ($"...{var}") for safe parameterization
    public class GetExamplesFromSqlQueryHandler(
        ISqlQueryRepository<Example> sqlQueryRepository,
        IMapper mapper
        ) : IRequestHandler<GetExamplesFromSqlQuery, Result<IEnumerable<GetExamplesFromSqlDto>>>
    {
        public async Task<Result<IEnumerable<GetExamplesFromSqlDto>>> Handle(
            GetExamplesFromSqlQuery request, 
            CancellationToken cancellationToken)
        {
            FormattableString sql = $"SELECT * FROM \"Examples\" WHERE \"Id\" > 0";

            var examples = await sqlQueryRepository.FromSqlAsync(sql, cancellationToken);
            var data = mapper.Map<IEnumerable<GetExamplesFromSqlDto>>(examples);

            return Result<IEnumerable<GetExamplesFromSqlDto>>.Success(data);
        }
    }
}

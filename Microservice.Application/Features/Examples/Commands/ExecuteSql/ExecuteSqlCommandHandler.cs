using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.Examples.Commands.ExecuteSql
{
    // EF raw command: ISqlCommandRepository<T>.ExecuteSqlAsync — always use FormattableString to prevent SQL injection
    public class ExecuteSqlCommandHandler(
        ISqlCommandRepository<Example> sqlCommandRepository
        ) : IRequestHandler<ExecuteSqlCommand, Result<int>>
    {
        public async Task<Result<int>> Handle(ExecuteSqlCommand request, CancellationToken cancellationToken)
        {
            var result = await sqlCommandRepository.ExecuteSqlAsync(request.Sql, cancellationToken);
            return Result<int>.Success(result);
        }
    }
}

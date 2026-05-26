using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.Examples.Commands.ExecuteInTransaction
{
    // EF atomic write: ISqlRepository<T>.ExecuteInTransactionAsync wraps multiple SQL statements in one transaction
    public class ExecuteInTransactionCommandHandler(
        ISqlRepository<Example> sqlRepository
        ) : IRequestHandler<ExecuteInTransactionCommand, Result<int>>
    {
        public async Task<Result<int>> Handle(ExecuteInTransactionCommand request, CancellationToken cancellationToken)
        {
            var result = await sqlRepository.ExecuteInTransactionAsync(
                async (repository) =>
                {
                    var execResult = await repository.ExecuteSqlAsync(
                        $"INSERT INTO Examples (Description) VALUES ({request.Description})", 
                        cancellationToken);
                    return execResult;
                },
                cancellationToken);

            return Result<int>.Success(result);
        }
    }
}

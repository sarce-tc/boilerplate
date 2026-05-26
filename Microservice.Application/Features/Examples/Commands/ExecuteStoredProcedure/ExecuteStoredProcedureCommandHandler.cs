using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.Examples.Commands.ExecuteStoredProcedure
{
    // EF stored procedure: ISqlCommandRepository<T>.ExecuteStoredProcedureAsync
    public class ExecuteStoredProcedureCommandHandler(
        ISqlCommandRepository<Example> sqlCommandRepository
        ) : IRequestHandler<ExecuteStoredProcedureCommand, Result<int>>
    {
        public async Task<Result<int>> Handle(ExecuteStoredProcedureCommand request, CancellationToken cancellationToken)
        {
            var result = await sqlCommandRepository.ExecuteStoredProcedureAsync(request.Sql, cancellationToken);
            return Result<int>.Success(result);
        }
    }
}

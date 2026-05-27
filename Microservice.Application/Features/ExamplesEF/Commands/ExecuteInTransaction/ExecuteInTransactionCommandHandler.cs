using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.ExamplesEF.Commands.ExecuteInTransaction
{
    // PATRÓN — Ejecutar múltiples operaciones SQL en una sola transacción atómica.
    // · ISqlRepository<T>.ExecuteInTransactionAsync envuelve el bloque en BEGIN/COMMIT/ROLLBACK.
    // · Usar cuando se necesitan varios ExecuteSqlAsync dentro de la misma transacción sin UoW.
    // · Siempre usar FormattableString ($"...{var}") para parametrizar — nunca concatenar strings SQL.
    public sealed class ExecuteInTransactionCommandHandler(
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

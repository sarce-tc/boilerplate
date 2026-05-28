using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.ExamplesEF.Commands.ExecuteInTransaction;
// PATRÓN — Ejecutar múltiples operaciones SQL en una sola transacción atómica.
// ── Parámetros ────────────────────────────────────────────────────────────
//   · sqlRepository — ISqlRepository<Example> (Application.Contracts.Persistence.EF): expone
//     ExecuteInTransactionAsync que envuelve el bloque delegado en BEGIN/COMMIT/ROLLBACK, permitiendo
//     ejecutar varios ExecuteSqlAsync dentro de la misma transacción sin depender del UoW de EF.
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

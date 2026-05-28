using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.ExamplesEF.Commands.ExecuteStoredProcedure;
// PATRÓN — Invocar un stored procedure de base de datos.
// ── Parámetros ────────────────────────────────────────────────────────────
//   · sqlCommandRepository — ISqlCommandRepository<Example> (Application.Contracts.Persistence.EF): delega la
//     ejecución del SP mediante ExecuteStoredProcedureAsync sin materializar entidades ni invocar el ChangeTracker,
//     apropiado cuando la lógica de negocio compleja reside en el SP y no se reescribe en el dominio.
public sealed class ExecuteStoredProcedureCommandHandler(
    ISqlCommandRepository<Example> sqlCommandRepository
    ) : IRequestHandler<ExecuteStoredProcedureCommand, Result<int>>
{
    public async Task<Result<int>> Handle(ExecuteStoredProcedureCommand request, CancellationToken cancellationToken)
    {
        var result = await sqlCommandRepository.ExecuteStoredProcedureAsync(request.Sql, cancellationToken);
        return Result<int>.Success(result);
    }
}

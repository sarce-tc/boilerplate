using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.ExamplesEF.Commands.ExecuteSql;
// PATRÓN — Ejecutar un comando SQL arbitrario (INSERT/UPDATE/DELETE) desde la capa de aplicación.
// ── Parámetros ────────────────────────────────────────────────────────────
//   · sqlCommandRepository — ISqlCommandRepository<Example> (Application.Contracts.Persistence.EF): ejecuta
//     el FormattableString parametrizado del command mediante ExecuteSqlAsync sin involucrar el ChangeTracker,
//     apropiado para operaciones de escritura donde el rendimiento importa y no hay invariantes de dominio.
public sealed class ExecuteSqlCommandHandler(
    ISqlCommandRepository<Example> sqlCommandRepository
    ) : IRequestHandler<ExecuteSqlCommand, Result<int>>
{
    public async Task<Result<int>> Handle(ExecuteSqlCommand request, CancellationToken cancellationToken)
    {
        var result = await sqlCommandRepository.ExecuteSqlAsync(request.Sql, cancellationToken);
        return Result<int>.Success(result);
    }
}

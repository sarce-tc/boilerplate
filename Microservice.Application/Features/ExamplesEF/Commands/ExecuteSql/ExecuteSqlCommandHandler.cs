using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.ExamplesEF.Commands.ExecuteSql
{
    // PATRÓN — Ejecutar un comando SQL arbitrario (INSERT/UPDATE/DELETE) desde la capa de aplicación.
    // · ISqlCommandRepository<T>.ExecuteSqlAsync ejecuta FormattableString parametrizado — nunca string crudo.
    // · Usar cuando EF change-tracking es innecesario y se necesita SQL directo por rendimiento o complejidad.
    // · Para operaciones de escritura con invariantes de dominio, usar UoW + entidades en lugar de SQL crudo.
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
}

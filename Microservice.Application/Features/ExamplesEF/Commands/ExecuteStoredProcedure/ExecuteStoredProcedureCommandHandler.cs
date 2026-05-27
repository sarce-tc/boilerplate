using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Domain.Entities;

namespace Microservice.Application.Features.ExamplesEF.Commands.ExecuteStoredProcedure
{
    // PATRÓN — Invocar un stored procedure de base de datos.
    // · ISqlCommandRepository<T>.ExecuteStoredProcedureAsync delega la ejecución al SP sin materializar entidades.
    // · Usar cuando la lógica de negocio compleja ya existe en el SP y no se puede/quiere reescribir en el dominio.
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
}

using MediatR;
using Microservice.Application.Common.Results;

namespace Microservice.Application.Features.ExamplesEF.Commands.ExecuteStoredProcedure;
// PATRÓN — Invoca un stored procedure de base de datos delegando la ejecución completa al SP.
//   Sql es el FormattableString con el nombre y parámetros del stored procedure.
//   Contrato de respuesta: Result<int> con el valor de retorno o filas afectadas por el SP.
public record ExecuteStoredProcedureCommand(
    FormattableString Sql
) : IRequest<Result<int>>;

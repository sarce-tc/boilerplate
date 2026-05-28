using MediatR;
using Microservice.Application.Common.Results;

namespace Microservice.Application.Features.ExamplesEF.Commands.ExecuteSql;
// PATRÓN — Ejecuta un comando SQL arbitrario (INSERT/UPDATE/DELETE) provisto por el caller.
//   Sql es el FormattableString parametrizado que se pasa directamente al repositorio SQL.
//   Contrato de respuesta: Result<int> con el número de filas afectadas.
public record ExecuteSqlCommand(
    FormattableString Sql
) : IRequest<Result<int>>;

using MediatR;
using Microservice.Application.Common.Results;

namespace Microservice.Application.Features.ExamplesEF.Commands.ExecuteInTransaction;
// PATRÓN — Ejecuta múltiples operaciones SQL dentro de una transacción atómica explícita.
//   Description es el valor de datos que se pasa al SQL parametrizado ejecutado dentro de la TX.
//   Contrato de respuesta: Result<int> con el número de filas afectadas por el bloque transaccional.
public record ExecuteInTransactionCommand(
    string Description
) : IRequest<Result<int>>;

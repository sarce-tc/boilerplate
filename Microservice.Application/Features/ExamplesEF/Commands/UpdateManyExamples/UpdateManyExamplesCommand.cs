using MediatR;
using Microservice.Application.Common.Results;

namespace Microservice.Application.Features.ExamplesEF.Commands.UpdateManyExamples;
// PATRÓN — Actualiza múltiples aggregates Example en batch sin materializar entidades individualmente.
//   PublicIds es el array de identificadores de los aggregates a actualizar.
//   Contrato de respuesta: Result<int> con el número de registros actualizados.
public record UpdateManyExamplesCommand(
    Guid[] PublicIds
) : IRequest<Result<int>>;

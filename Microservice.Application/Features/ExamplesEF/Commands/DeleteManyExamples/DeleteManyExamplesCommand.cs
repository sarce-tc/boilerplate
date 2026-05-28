using MediatR;
using Microservice.Application.Common.Results;

namespace Microservice.Application.Features.ExamplesEF.Commands.DeleteManyExamples;
// PATRÓN — Elimina múltiples aggregates Example por sus PublicIds en un solo DELETE SQL sin cargar entidades.
//   PublicIds es el array de identificadores a eliminar.
//   Contrato de respuesta: Result<int> con el número de registros eliminados.
public record DeleteManyExamplesCommand(
    Guid[] PublicIds
) : IRequest<Result<int>>;

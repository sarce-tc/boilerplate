using MediatR;
using Microservice.Application.Common.Results;

namespace Microservice.Application.Features.ExamplesEF.Commands.DeleteExample;
// PATRÓN — Elimina un aggregate Example por su PublicId con verificación de existencia previa.
//   Contrato de respuesta: Result<Guid> con el PublicId eliminado, o Result.Failure(NotFound).
public record DeleteExampleCommand(
    Guid PublicId
) : IRequest<Result<Guid>>;

using MediatR;
using Microservice.Application.Common.Results;

namespace Microservice.Application.Features.ExamplesEF.Commands.UpdateExampleFields;
// PATRÓN — Actualiza solo los campos escalares del aggregate Example con semántica PATCH.
//   PublicId identifica el aggregate; Name y Description son opcionales (null = campo no modificado).
//   Contrato de respuesta: Result<Guid> con el PublicId del aggregate actualizado.
public record UpdateExampleFieldsCommand(
    Guid PublicId,
    string? Name,
    string? Description
) : IRequest<Result<Guid>>;

using MediatR;
using Microservice.Application.Common.Results;

namespace Microservice.Application.Features.ExamplesEF.Commands.ActivateExample;

// PATRÓN — Transiciona un aggregate Example del estado Inactive al estado Active.
//   PublicId identifica el aggregate a reactivar.
//   Contrato de respuesta: Result<Guid> con el PublicId del aggregate activado, o HTTP 409 si ya está activo.
public record ActivateExampleCommand(Guid PublicId) : IRequest<Result<Guid>>;

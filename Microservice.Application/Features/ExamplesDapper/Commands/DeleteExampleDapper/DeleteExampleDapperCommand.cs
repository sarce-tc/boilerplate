using MediatR;
using Microservice.Application.Common.Results;

namespace Microservice.Application.Features.ExamplesDapper.Commands.DeleteExampleDapper;
// PATRÓN — Mensaje que MediatR enruta hacia DeleteExampleDapperCommandHandler.
//   Transporta el PublicId del Example a eliminar; representa la intención de borrar el aggregate via Dapper.
//   Result<Guid> retorna el PublicId del registro eliminado.
public sealed record DeleteExampleDapperCommand(Guid PublicId) : IRequest<Result<Guid>>;

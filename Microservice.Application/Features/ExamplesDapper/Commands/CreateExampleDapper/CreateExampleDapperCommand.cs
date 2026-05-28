using MediatR;
using Microservice.Application.Common.Results;

namespace Microservice.Application.Features.ExamplesDapper.Commands.CreateExampleDapper;
// PATRÓN — Mensaje que MediatR enruta hacia CreateExampleDapperCommandHandler.
//   Transporta Name y Description para construir el aggregate; representa la intención de crear un nuevo Example via Dapper.
//   Result<Guid> retorna el PublicId generado del nuevo registro.
public sealed record CreateExampleDapperCommand(string Name, string? Description) : IRequest<Result<Guid>>;

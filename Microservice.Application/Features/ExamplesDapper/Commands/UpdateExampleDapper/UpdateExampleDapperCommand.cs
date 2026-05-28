using MediatR;
using Microservice.Application.Common.Results;

namespace Microservice.Application.Features.ExamplesDapper.Commands.UpdateExampleDapper;
// PATRÓN — Mensaje que MediatR enruta hacia UpdateExampleDapperCommandHandler.
//   Transporta PublicId para localizar el aggregate y Name/Description opcionales para actualizar; aplica semántica PUT.
//   Result<Guid> retorna el PublicId del registro actualizado.
public sealed record UpdateExampleDapperCommand(Guid PublicId, string? Name, string? Description) : IRequest<Result<Guid>>;

using MediatR;
using Microservice.Application.Common.Results;

namespace Microservice.Application.Features.ExamplesDapper.Queries.ExistsExampleByNameDapper;
// PATRÓN — Mensaje que MediatR enruta hacia ExistsExampleByNameDapperQueryHandler.
//   Transporta el Name a verificar; representa la intención de comprobar si existe un Example con ese nombre.
//   Result<bool> es el contrato de respuesta del pipeline.
public sealed record ExistsExampleByNameDapperQuery(string Name) : IRequest<Result<bool>>;

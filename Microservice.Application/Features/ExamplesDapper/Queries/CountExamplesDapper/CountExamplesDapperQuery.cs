using MediatR;
using Microservice.Application.Common.Results;

namespace Microservice.Application.Features.ExamplesDapper.Queries.CountExamplesDapper;
// PATRÓN — Mensaje que MediatR enruta hacia CountExamplesDapperQueryHandler.
//   No transporta parámetros de entrada; representa la intención de contar el total de Examples.
//   Result<int> es el contrato de respuesta del pipeline.
public sealed record CountExamplesDapperQuery : IRequest<Result<int>>;

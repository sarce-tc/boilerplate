using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.DTOs;

namespace Microservice.Application.Features.ExamplesDapper.Queries.GetAllExamplesDapper;
// PATRÓN — Mensaje que MediatR enruta hacia GetAllExamplesDapperQueryHandler.
//   No transporta parámetros de entrada; representa la intención de obtener todos los Examples.
//   Result<IEnumerable<GetAllExamplesDapperDto>> es el contrato de respuesta del pipeline.
public sealed record GetAllExamplesDapperQuery : IRequest<Result<IEnumerable<GetAllExamplesDapperDto>>>;

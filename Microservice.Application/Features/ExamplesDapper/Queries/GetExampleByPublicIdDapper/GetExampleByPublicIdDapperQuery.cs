using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.DTOs;

namespace Microservice.Application.Features.ExamplesDapper.Queries.GetExampleByPublicIdDapper;
// PATRÓN — Mensaje que MediatR enruta hacia GetExampleByPublicIdDapperQueryHandler.
//   Transporta el PublicId del Example a recuperar; representa la intención de obtener un Example por su identificador público.
//   Result<GetExampleByPublicIdDapperDto> es el contrato de respuesta del pipeline.
public sealed record GetExampleByPublicIdDapperQuery(Guid PublicId) : IRequest<Result<GetExampleByPublicIdDapperDto>>;

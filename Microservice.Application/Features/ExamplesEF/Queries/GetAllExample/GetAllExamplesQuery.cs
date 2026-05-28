using MediatR;
using Microservice.Application.Common.Interfaces;
using Microservice.Application.Common.Results;
using Microservice.Application.DTOs;

namespace Microservice.Application.Features.ExamplesEF.Queries.GetAllExample;
// PATRÓN — Obtiene la colección completa de aggregates Example sin paginación, con soporte de caché.
//   Implementa ICacheableQuery para que el CachingBehavior del pipeline almacene la respuesta un día.
//   Contrato de respuesta: Result<IEnumerable<GetAllExamplesDto>> proyectado por AutoMapper.
public record GetAllExamplesQuery : IRequest<Result<IEnumerable<GetAllExamplesDto>>>, ICacheableQuery
{
    public string CacheKey => nameof(GetAllExamplesQuery);

    public TimeSpan? Expiration => TimeSpan.FromDays(1);
}

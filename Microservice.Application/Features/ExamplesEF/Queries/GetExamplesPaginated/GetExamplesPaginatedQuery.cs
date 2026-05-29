using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.DTOs.EF;
using Microservice.Application.Models;

namespace Microservice.Application.Features.ExamplesEF.Queries.GetExamplesPaginated;
// PATRÓN — Obtiene una página de aggregates Example con metadatos de navegación.
//   CurrentPage es el número de página (1-based); PageSize es el tamaño de cada página.
//   Contrato de respuesta: Result<PagedResult<GetExamplesPaginatedDto>> con RowsCount, CurrentPage y PageSize.
public record GetExamplesPaginatedQuery(
    int CurrentPage,
    int PageSize
) : IRequest<Result<PagedResult<GetExamplesPaginatedDto>>>;

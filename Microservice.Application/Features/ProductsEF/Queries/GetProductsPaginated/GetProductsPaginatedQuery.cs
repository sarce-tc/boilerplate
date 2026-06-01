using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.DTOs.EF;
using Microservice.Application.Models;

namespace Microservice.Application.Features.ProductsEF.Queries.GetProductsPaginated;
// PATRÓN — Listado paginado (offset) del catálogo con metadatos de navegación.
public record GetProductsPaginatedQuery(int CurrentPage = 1, int PageSize = 10)
    : IRequest<Result<PagedResult<GetProductsPaginatedDto>>>;

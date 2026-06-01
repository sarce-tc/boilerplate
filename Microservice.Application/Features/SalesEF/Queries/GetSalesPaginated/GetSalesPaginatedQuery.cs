using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.DTOs.EF;
using Microservice.Application.Models;

namespace Microservice.Application.Features.SalesEF.Queries.GetSalesPaginated;
// PATRÓN — Listado paginado de ventas (más recientes primero).
public record GetSalesPaginatedQuery(int CurrentPage = 1, int PageSize = 10)
    : IRequest<Result<PagedResult<SalesPaginatedDto>>>;

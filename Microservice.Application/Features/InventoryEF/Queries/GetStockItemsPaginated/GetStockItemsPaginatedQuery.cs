using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.DTOs.EF;
using Microservice.Application.Models;

namespace Microservice.Application.Features.InventoryEF.Queries.GetStockItemsPaginated;
// PATRÓN — Listado paginado de saldos de stock.
public record GetStockItemsPaginatedQuery(int CurrentPage = 1, int PageSize = 10)
    : IRequest<Result<PagedResult<StockItemDto>>>;

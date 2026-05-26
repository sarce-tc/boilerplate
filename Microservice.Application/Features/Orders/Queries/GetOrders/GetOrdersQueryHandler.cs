using MediatR;
using Microservice.Application.Common;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.Dapper;
using Microservice.Application.DTOs.Orders;

namespace Microservice.Application.Features.Orders.Queries.GetOrders;

/// <summary>
/// Returns a paginated list of order summaries (no items detail).
///
/// Uses a single <c>QueryMultipleAsync</c> call — one round-trip returns both
/// the page of rows and the total count.
/// Read path: no UoW, no transaction.
/// </summary>
public sealed class GetOrdersQueryHandler(
    IOrderReadRepository orderReadRepo
) : IRequestHandler<GetOrdersQuery, Result<PagedResult<OrderSummaryDto>>>
{
    public async Task<Result<PagedResult<OrderSummaryDto>>> Handle(
        GetOrdersQuery    request,
        CancellationToken cancellationToken)
    {
        var (orders, total) = await orderReadRepo.GetPagedAsync(
            request.Page,
            request.PageSize,
            cancellationToken);

        return Result<PagedResult<OrderSummaryDto>>.Success(
            new PagedResult<OrderSummaryDto>(orders, total, request.Page, request.PageSize));
    }
}

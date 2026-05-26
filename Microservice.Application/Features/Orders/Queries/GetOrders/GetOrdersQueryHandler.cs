using MediatR;
using Microservice.Application.Common;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.Dapper;
using Microservice.Application.DTOs.Orders;

namespace Microservice.Application.Features.Orders.Queries.GetOrders;

// AGENT ENTRY POINT — Reference paginated query handler
// Pattern: GetPagedAsync → (IReadOnlyList<TDto>, int totalCount)
//          → new PagedResult<T>(items, total, page, pageSize)
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

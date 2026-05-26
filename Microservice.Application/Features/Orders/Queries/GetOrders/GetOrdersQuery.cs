using MediatR;
using Microservice.Application.Common;
using Microservice.Application.Common.Results;
using Microservice.Application.DTOs.Orders;

namespace Microservice.Application.Features.Orders.Queries.GetOrders;

/// <param name="Page">1-based page number.</param>
/// <param name="PageSize">Records per page (1–100).</param>
public record GetOrdersQuery(int Page, int PageSize)
    : IRequest<Result<PagedResult<OrderSummaryDto>>>;

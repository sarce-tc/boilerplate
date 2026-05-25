using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.Dapper;
using Microservice.Application.DTOs.Orders;

namespace Microservice.Application.Features.Orders.Queries.GetOrderById;

/// <summary>
/// Fetches an Order with its items via a single Dapper multi-map JOIN query.
/// No EF context, no N+1 — one round-trip to the database.
/// </summary>
public sealed class GetOrderByIdQueryHandler(
    IOrderReadRepository orderReadRepo
) : IRequestHandler<GetOrderByIdQuery, Result<OrderDto>>
{
    public async Task<Result<OrderDto>> Handle(
        GetOrderByIdQuery request,
        CancellationToken cancellationToken)
    {
        var (order, items) = await orderReadRepo.GetWithItemsAsync(
            request.PublicId,
            cancellationToken);

        if (order is null)
            return Result<OrderDto>.Failure(
                Error.NotFound($"Order '{request.PublicId}' was not found."));

        var dto = new OrderDto(
            PublicId:      order.PublicId,
            CustomerName:  order.CustomerName,
            Status:        order.Status,
            TotalAmount:   order.TotalAmount,
            CreatedAt:     order.CreatedAt,
            Items:         items
                .Select(i => new OrderItemDto(
                    i.PublicId,
                    i.ProductName,
                    i.Quantity,
                    i.UnitPrice,
                    i.LineTotal))
                .ToList()
                .AsReadOnly());

        return Result<OrderDto>.Success(dto);
    }
}

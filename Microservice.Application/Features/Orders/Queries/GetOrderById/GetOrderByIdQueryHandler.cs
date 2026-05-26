using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.Dapper;
using Microservice.Application.DTOs.Orders;

namespace Microservice.Application.Features.Orders.Queries.GetOrderById;

// ═══════════════════════════════════════════════════════════════════════
// AGENT ENTRY POINT — Reference query handler (single entity with join)
// REFERENCE IMPLEMENTATION — plantilla para query handlers con join y proyección a DTO.
//
// Query handler rules:
//   - Inject IXReadRepository — never IUnitOfWork (queries are read-only, no TX)
//   - No try-catch — exceptions propagate to GlobalExceptionHandler
//   - Collections always use PagedResult<T> — see GetOrdersQueryHandler.cs
//   - Project to DTO here, not in the repository
// ═══════════════════════════════════════════════════════════════════════
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

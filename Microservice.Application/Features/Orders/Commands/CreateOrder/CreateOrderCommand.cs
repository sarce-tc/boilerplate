using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.DTOs.Orders;

namespace Microservice.Application.Features.Orders.Commands.CreateOrder;

/// <summary>
/// Creates an Order together with its line items in a single Dapper transaction.
/// Returns the new Order's PublicId on success.
/// </summary>
public sealed record CreateOrderCommand(
    string CustomerName,
    IReadOnlyList<CreateOrderItemDto> Items
) : IRequest<Result<Guid>>;

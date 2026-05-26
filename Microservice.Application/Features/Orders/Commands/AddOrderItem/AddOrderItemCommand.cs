using MediatR;
using Microservice.Application.Common.Results;

namespace Microservice.Application.Features.Orders.Commands.AddOrderItem;

/// <param name="OrderPublicId">The order to add the item to.</param>
/// <param name="ProductName">Product name (non-empty, max 200 chars).</param>
/// <param name="Quantity">Units ordered (> 0).</param>
/// <param name="UnitPrice">Price per unit (> 0).</param>
public record AddOrderItemCommand(
    Guid    OrderPublicId,
    string  ProductName,
    int     Quantity,
    decimal UnitPrice) : IRequest<Result<Guid>>;  // returns new item's PublicId

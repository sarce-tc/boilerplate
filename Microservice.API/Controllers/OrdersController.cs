using Asp.Versioning;
using MediatR;
using Microservice.API.Extensions;
using Microservice.Application.Common;
using Microservice.Application.DTOs.Orders;
using Microservice.Application.Features.Orders.Commands.AddOrderItem;
using Microservice.Application.Features.Orders.Commands.CancelOrder;
using Microservice.Application.Features.Orders.Commands.CompleteOrder;
using Microservice.Application.Features.Orders.Commands.CreateOrder;
using Microservice.Application.Features.Orders.Commands.RemoveOrderItem;
using Microservice.Application.Features.Orders.Commands.UpdateOrder;
using Microservice.Application.Features.Orders.Queries.GetOrderById;
using Microservice.Application.Features.Orders.Queries.GetOrders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Microservice.API.Controllers;

/// <summary>
/// Orders API — demonstrates Dapper + Unit-of-Work transaction pattern.
///
/// <b>Key patterns shown:</b>
/// <list type="bullet">
///   <item>POST creates Order + OrderItems in a single ACID transaction (Dapper UoW).</item>
///   <item>GET fetches Order + Items via a single JOIN query (no N+1, no EF).</item>
///   <item>DELETE applies a domain rule (<c>Order.Cancel()</c>) then persists
///         the status change inside a Dapper transaction.</item>
/// </list>
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[AllowAnonymous]  // Remove and add [Authorize] once JWT is configured end-to-end
[Tags("Orders")]
// Controller binding patterns:
//   A: route only       → new XCommand(publicId)                  — CancelOrder, CompleteOrder
//   B: route + body     → command with { PublicId = publicId }     — UpdateOrder, AddOrderItem
//   C: 201 Created      → result.ToActionResult(Status201Created)  — CreateOrder, AddOrderItem
public sealed class OrdersController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// GET /api/v1/orders
    /// Returns a paginated list of order summaries (without item details).
    /// </summary>
    /// <remarks>
    /// Uses a single <c>QueryMultipleAsync</c> call — one round-trip returns
    /// both the page rows and the total count.
    ///
    ///     GET /api/v1/orders?page=1&amp;pageSize=20
    ///
    /// </remarks>
    /// <response code="200">Paginated list of orders.</response>
    /// <response code="400">Invalid page or pageSize.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<OrderSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetOrders(
        [FromQuery] int page     = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetOrdersQuery(page, pageSize), cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// POST /api/v1/orders
    /// Creates an Order together with its line items in one atomic transaction.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/v1/orders
    ///     {
    ///         "customerName": "Acme Corp",
    ///         "items": [
    ///             { "productName": "Widget A", "quantity": 3, "unitPrice": 9.99 },
    ///             { "productName": "Gadget B", "quantity": 1, "unitPrice": 49.99 }
    ///         ]
    ///     }
    ///
    /// The handler opens a Npgsql transaction, inserts the Order header,
    /// inserts each OrderItem with the auto-generated OrderId, then commits.
    /// Any failure triggers a full rollback — no partial data.
    /// </remarks>
    /// <response code="201">Order created. Body: new Order's PublicId (Guid).</response>
    /// <response code="400">Validation failed.</response>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateOrder(
        [FromBody] CreateOrderCommand command,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
        return result.ToActionResult(StatusCodes.Status201Created);
    }

    /// <summary>
    /// GET /api/v1/orders/{publicId}
    /// Returns a full Order with all its line items.
    /// </summary>
    /// <remarks>
    /// Executed via a single Dapper JOIN query:
    ///
    ///     SELECT o.*, i.*
    ///     FROM orders o
    ///     LEFT JOIN order_items i ON i.order_id = o.id
    ///     WHERE o.public_id = @PublicId
    ///
    /// No EF context, no lazy loading, no N+1.
    /// </remarks>
    /// <response code="200">Order found with items.</response>
    /// <response code="404">Order not found.</response>
    [HttpGet("{publicId:guid}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrder(
        Guid publicId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetOrderByIdQuery(publicId), cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// PUT /api/v1/orders/{publicId}
    /// Updates the customer name of a Pending order.
    /// </summary>
    /// <response code="200">Order updated.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="404">Order not found.</response>
    /// <response code="409">Order is not in Pending status.</response>
    [HttpPut("{publicId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateOrder(
        Guid publicId,
        [FromBody] UpdateOrderCommand command,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command with { PublicId = publicId }, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// POST /api/v1/orders/{publicId}/complete
    /// Marks an order as Completed.
    /// </summary>
    /// <response code="200">Order completed.</response>
    /// <response code="404">Order not found.</response>
    /// <response code="409">Order is already Cancelled and cannot be completed.</response>
    [HttpPost("{publicId:guid}/complete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CompleteOrder(
        Guid publicId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CompleteOrderCommand(publicId), cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// POST /api/v1/orders/{publicId}/items
    /// Adds a line item to a Pending order.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/v1/orders/{publicId}/items
    ///     { "productName": "Widget C", "quantity": 2, "unitPrice": 14.99 }
    ///
    /// Inserts the item and updates TotalAmount in a single ACID transaction.
    /// </remarks>
    /// <response code="201">Item added. Body: new item's PublicId (Guid).</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="404">Order not found.</response>
    /// <response code="409">Order is not in Pending status.</response>
    [HttpPost("{publicId:guid}/items")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddOrderItem(
        Guid publicId,
        [FromBody] AddOrderItemCommand command,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command with { OrderPublicId = publicId }, cancellationToken);
        return result.ToActionResult(StatusCodes.Status201Created);
    }

    /// <summary>
    /// DELETE /api/v1/orders/{publicId}/items/{itemPublicId}
    /// Removes a line item from a Pending order.
    /// </summary>
    /// <response code="200">Item removed.</response>
    /// <response code="404">Order not found.</response>
    /// <response code="409">Order is not in Pending status, or item does not belong to the order.</response>
    [HttpDelete("{publicId:guid}/items/{itemPublicId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RemoveOrderItem(
        Guid publicId,
        Guid itemPublicId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new RemoveOrderItemCommand(publicId, itemPublicId), cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// DELETE /api/v1/orders/{publicId}
    /// Cancels an order that has not yet been Completed.
    /// </summary>
    /// <remarks>
    /// Applies the domain rule <c>order.Cancel()</c> — returns 409 Conflict if
    /// the order is already in a terminal state — then persists the status change
    /// inside a Dapper transaction.
    /// </remarks>
    /// <response code="200">Order cancelled.</response>
    /// <response code="404">Order not found.</response>
    /// <response code="409">Order is already Completed and cannot be cancelled.</response>
    [HttpDelete("{publicId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CancelOrder(
        Guid publicId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CancelOrderCommand(publicId), cancellationToken);
        return result.ToActionResult();
    }
}

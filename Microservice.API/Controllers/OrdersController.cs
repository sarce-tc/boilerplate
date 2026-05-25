using Asp.Versioning;
using MediatR;
using Microservice.API.Extensions;
using Microservice.Application.DTOs.Orders;
using Microservice.Application.Features.Orders.Commands.CancelOrder;
using Microservice.Application.Features.Orders.Commands.CreateOrder;
using Microservice.Application.Features.Orders.Queries.GetOrderById;
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
public sealed class OrdersController(IMediator mediator) : ControllerBase
{
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

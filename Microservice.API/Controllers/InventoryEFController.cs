using Asp.Versioning;
using MediatR;
using Microservice.API.Extensions;
using Microservice.Application.DTOs.EF;
using Microservice.Application.Features.InventoryEF.Commands.RegisterInventoryMovement;
using Microservice.Application.Features.InventoryEF.Queries.GetInventoryMovementsByProduct;
using Microservice.Application.Features.InventoryEF.Queries.GetStockByProduct;
using Microservice.Application.Features.InventoryEF.Queries.GetStockItemsPaginated;
using Microservice.Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace Microservice.API.Controllers;
// Controlador EF Core de Stock e Inventario — control de existencias y ledger de movimientos.
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class InventoryEFController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// POST /api/inventory/movements
    /// Registra un movimiento que ajusta el saldo del producto (compra, venta, ajuste, merma…).
    /// Returns: 201 Created con el PublicId del asiento · 400 validación · 409 stock insuficiente
    /// </summary>
    [HttpPost("movements")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegisterMovement(
        [FromBody] RegisterInventoryMovementCommand request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(request, cancellationToken);
        return result.ToActionResult(StatusCodes.Status201Created);
    }

    /// <summary>
    /// GET /api/inventory/stock/{productPublicId}
    /// Saldo actual de un producto.
    /// Returns: 200 OK · 404 si no hay registro de stock
    /// </summary>
    [HttpGet("stock/{productPublicId:guid}")]
    [ProducesResponseType(typeof(StockItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStockByProduct(
        Guid productPublicId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetStockByProductQuery(productPublicId), cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// GET /api/inventory/stock?page=1&amp;size=10
    /// Listado paginado de saldos de stock.
    /// Returns: 200 OK con PagedResult
    /// </summary>
    [HttpGet("stock")]
    [ProducesResponseType(typeof(PagedResult<StockItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStockPaginated(
        [FromQuery] int page = 1,
        [FromQuery] int size = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetStockItemsPaginatedQuery(page, size), cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// GET /api/inventory/movements/{productPublicId}
    /// Ledger de movimientos de un producto (más recientes primero).
    /// Returns: 200 OK con la lista de movimientos
    /// </summary>
    [HttpGet("movements/{productPublicId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<InventoryMovementDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMovementsByProduct(
        Guid productPublicId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetInventoryMovementsByProductQuery(productPublicId), cancellationToken);
        return result.ToActionResult();
    }
}

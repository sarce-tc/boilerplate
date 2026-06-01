using Asp.Versioning;
using MediatR;
using Microservice.API.Extensions;
using Microservice.Application.Contracts.Infrastructure;
using Microservice.Application.DTOs.EF;
using Microservice.Application.Features.SalesEF.Commands.CancelSale;
using Microservice.Application.Features.SalesEF.Commands.ConfirmSale;
using Microservice.Application.Features.SalesEF.Commands.CreateSale;
using Microservice.Application.Features.SalesEF.Queries.GetSaleById;
using Microservice.Application.Features.SalesEF.Queries.GetSalesPaginated;
using Microservice.Application.Features.SalesEF.Queries.GetSaleTicket;
using Microservice.Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace Microservice.API.Controllers;
// Controlador EF Core de Ventas (POS). El alta crea una venta PENDIENTE; la confirmación
// orquesta el descuento de stock y el cobro en caja.
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class SalesEFController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// POST /api/sales
    /// Crea una venta pendiente (precios/IVA tomados del catálogo).
    /// Returns: 201 Created con el PublicId · 400 validación · 404 producto inexistente
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateSale(
        [FromBody] CreateSaleCommand request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(request, cancellationToken);
        return result.ToActionResult(StatusCodes.Status201Created);
    }

    /// <summary>
    /// POST /api/sales/{publicId}/confirm
    /// Confirma la venta: descuenta stock y registra el cobro en caja.
    /// Returns: 200 OK con la venta confirmada · 404 · 409 (caja cerrada / stock insuficiente)
    /// </summary>
    [HttpPost("{publicId:guid}/confirm")]
    [ProducesResponseType(typeof(SaleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ConfirmSale(
        Guid publicId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ConfirmSaleCommand(publicId), cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// POST /api/sales/{publicId}/cancel
    /// Anula una venta pendiente.
    /// Returns: 200 OK · 404 · 409 si ya está confirmada/anulada
    /// </summary>
    [HttpPost("{publicId:guid}/cancel")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CancelSale(
        Guid publicId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CancelSaleCommand(publicId), cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// GET /api/sales/{publicId}
    /// Detalle de la venta con líneas y totales.
    /// Returns: 200 OK · 404
    /// </summary>
    [HttpGet("{publicId:guid}")]
    [ProducesResponseType(typeof(SaleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSaleById(
        Guid publicId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetSaleByIdQuery(publicId), cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// GET /api/sales/{publicId}/ticket
    /// Renderiza el ticket imprimible de la venta (HTML); incluye datos del comprobante si fue facturada.
    /// Returns: 200 OK con { contentType, content } · 404
    /// </summary>
    [HttpGet("{publicId:guid}/ticket")]
    [ProducesResponseType(typeof(TicketDocument), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSaleTicket(
        Guid publicId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetSaleTicketQuery(publicId), cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// GET /api/sales?page=1&amp;size=10
    /// Listado paginado de ventas.
    /// Returns: 200 OK con PagedResult
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<SalesPaginatedDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaginated(
        [FromQuery] int page = 1,
        [FromQuery] int size = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetSalesPaginatedQuery(page, size), cancellationToken);
        return result.ToActionResult();
    }
}

using Asp.Versioning;
using MediatR;
using Microservice.API.Extensions;
using Microservice.Application.DTOs.EF;
using Microservice.Application.Features.InvoicesEF.Commands.IssueInvoice;
using Microservice.Application.Features.InvoicesEF.Queries.GetInvoiceById;
using Microservice.Application.Features.InvoicesEF.Queries.GetInvoiceBySale;
using Microservice.Application.Features.InvoicesEF.Queries.GetInvoicesPaginated;
using Microservice.Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace Microservice.API.Controllers;
// Controlador EF Core de Facturación Electrónica (AFIP/ARCA). La emisión solicita el CAE al
// gateway (stub) y asocia el comprobante a la venta.
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class InvoicesEFController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// POST /api/invoices
    /// Emite el comprobante electrónico de una venta confirmada (solicita CAE).
    /// Returns: 201 Created con el comprobante · 404 venta inexistente · 409 (venta no confirmada / ya facturada)
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(InvoiceDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> IssueInvoice(
        [FromBody] IssueInvoiceCommand request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(request, cancellationToken);
        return result.ToActionResult(StatusCodes.Status201Created);
    }

    /// <summary>
    /// GET /api/invoices/{publicId}
    /// Detalle del comprobante.
    /// Returns: 200 OK · 404
    /// </summary>
    [HttpGet("{publicId:guid}")]
    [ProducesResponseType(typeof(InvoiceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetInvoiceById(
        Guid publicId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetInvoiceByIdQuery(publicId), cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// GET /api/invoices/by-sale/{salePublicId}
    /// Comprobante asociado a una venta.
    /// Returns: 200 OK · 404
    /// </summary>
    [HttpGet("by-sale/{salePublicId:guid}")]
    [ProducesResponseType(typeof(InvoiceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetInvoiceBySale(
        Guid salePublicId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetInvoiceBySaleQuery(salePublicId), cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// GET /api/invoices?page=1&amp;size=10
    /// Listado paginado de comprobantes.
    /// Returns: 200 OK con PagedResult
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<InvoicesPaginatedDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaginated(
        [FromQuery] int page = 1,
        [FromQuery] int size = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetInvoicesPaginatedQuery(page, size), cancellationToken);
        return result.ToActionResult();
    }
}

using Asp.Versioning;
using MediatR;
using Microservice.API.Extensions;
using Microservice.Application.DTOs.EF;
using Microservice.Application.Features.CustomersEF.Commands.CreateCustomer;
using Microservice.Application.Features.CustomersEF.Commands.DeleteCustomer;
using Microservice.Application.Features.CustomersEF.Commands.UpdateCustomer;
using Microservice.Application.Features.CustomersEF.Queries.GetCustomerByDocument;
using Microservice.Application.Features.CustomersEF.Queries.GetCustomerById;
using Microservice.Application.Features.CustomersEF.Queries.GetCustomersPaginated;
using Microservice.Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace Microservice.API.Controllers;
// Controlador EF Core del aggregate Customer — gestión de clientes del POS.
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class CustomersEFController(IMediator mediator) : ControllerBase
{
    /// <summary>POST /api/customers — crea un cliente. 201 Created · 400 validación.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCustomer(
        [FromBody] CreateCustomerCommand request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(request, cancellationToken);
        return result.ToActionResult(StatusCodes.Status201Created);
    }

    /// <summary>GET /api/customers/{publicId} — detalle. 200 · 404.</summary>
    [HttpGet("{publicId:guid}")]
    [ProducesResponseType(typeof(GetCustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCustomerById(
        Guid publicId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetCustomerByIdQuery(publicId), cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>GET /api/customers/by-document/{docNumber} — búsqueda por documento. 200 · 404.</summary>
    [HttpGet("by-document/{docNumber}")]
    [ProducesResponseType(typeof(GetCustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCustomerByDocument(
        string docNumber,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetCustomerByDocumentQuery(docNumber), cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>GET /api/customers?page=1&amp;size=10 — listado paginado. 200 OK.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<GetCustomersPaginatedDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaginated(
        [FromQuery] int page = 1,
        [FromQuery] int size = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetCustomersPaginatedQuery(page, size), cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>PUT /api/customers/{publicId} — actualiza datos/fiscal/contacto. 200 · 404 · 409.</summary>
    [HttpPut("{publicId:guid}")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateCustomer(
        Guid publicId,
        [FromBody] UpdateCustomerRequestDto? request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateCustomerCommand(
            publicId,
            request?.Name,
            request?.DocType,
            request?.DocNumber,
            request?.TaxCondition,
            request?.Email,
            request?.Phone,
            request?.Address);
        var result = await mediator.Send(command, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>DELETE /api/customers/{publicId} — elimina. 200 · 404.</summary>
    [HttpDelete("{publicId:guid}")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCustomer(
        Guid publicId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteCustomerCommand(publicId), cancellationToken);
        return result.ToActionResult();
    }
}

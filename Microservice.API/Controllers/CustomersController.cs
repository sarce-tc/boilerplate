using Asp.Versioning;
using MediatR;
using Microservice.API.Extensions;
using Microservice.Application.Common;
using Microservice.Application.DTOs.Customers;
using Microservice.Application.Features.Customers.Commands.CreateCustomer;
using Microservice.Application.Features.Customers.Commands.DeleteCustomer;
using Microservice.Application.Features.Customers.Commands.UpdateCustomer;
using Microservice.Application.Features.Customers.Commands.UpdateCustomerPhone;
using Microservice.Application.Features.Customers.Queries.GetCustomerByEmail;
using Microservice.Application.Features.Customers.Queries.GetCustomerById;
using Microservice.Application.Features.Customers.Queries.GetCustomers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Microservice.API.Controllers;

/// <summary>
/// Customers API — simple aggregate without child collections (Dapper UoW pattern).
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[AllowAnonymous]
[Tags("Customers")]
public sealed class CustomersController(IMediator mediator) : ControllerBase
{
    /// <summary>GET /api/v1/customers — paginated list of customers.</summary>
    /// <response code="200">Paginated list.</response>
    /// <response code="400">Invalid page or pageSize.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<CustomerSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetCustomers(
        [FromQuery] int page     = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetCustomersQuery(page, pageSize), ct);
        return result.ToActionResult();
    }

    /// <summary>GET /api/v1/customers/by-email?email=x — single customer by email.</summary>
    /// <response code="200">Customer found.</response>
    /// <response code="404">Customer not found.</response>
    [HttpGet("by-email")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCustomerByEmail([FromQuery] string email, CancellationToken ct)
    {
        var result = await mediator.Send(new GetCustomerByEmailQuery(email), ct);
        return result.ToActionResult();
    }

    /// <summary>GET /api/v1/customers/{publicId} — single customer by public ID.</summary>
    /// <response code="200">Customer found.</response>
    /// <response code="404">Customer not found.</response>
    [HttpGet("{publicId:guid}")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCustomer(Guid publicId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetCustomerByIdQuery(publicId), ct);
        return result.ToActionResult();
    }

    /// <summary>POST /api/v1/customers — creates a new customer.</summary>
    /// <response code="201">Customer created. Body: new PublicId (Guid).</response>
    /// <response code="400">Validation failed.</response>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCustomer(
        [FromBody] CreateCustomerCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return result.ToActionResult(StatusCodes.Status201Created);
    }

    /// <summary>PUT /api/v1/customers/{publicId} — updates name, email and phone.</summary>
    /// <response code="200">Customer updated.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="404">Customer not found.</response>
    [HttpPut("{publicId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCustomer(
        Guid publicId,
        [FromBody] UpdateCustomerCommand command,
        CancellationToken ct)
    {
        var result = await mediator.Send(command with { PublicId = publicId }, ct);
        return result.ToActionResult();
    }

    /// <summary>PATCH /api/v1/customers/{publicId}/phone — updates only the phone number.</summary>
    /// <response code="200">Phone updated.</response>
    /// <response code="404">Customer not found.</response>
    [HttpPatch("{publicId:guid}/phone")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCustomerPhone(
        Guid publicId,
        [FromBody] UpdateCustomerPhoneCommand command,
        CancellationToken ct)
    {
        var result = await mediator.Send(command with { PublicId = publicId }, ct);
        return result.ToActionResult();
    }

    /// <summary>DELETE /api/v1/customers/{publicId} — deletes a customer.</summary>
    /// <response code="200">Customer deleted.</response>
    /// <response code="404">Customer not found.</response>
    [HttpDelete("{publicId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCustomer(Guid publicId, CancellationToken ct)
    {
        var result = await mediator.Send(new DeleteCustomerCommand(publicId), ct);
        return result.ToActionResult();
    }
}

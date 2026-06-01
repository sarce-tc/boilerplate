using Asp.Versioning;
using MediatR;
using Microservice.API.Extensions;
using Microservice.Application.DTOs.EF;
using Microservice.Application.Features.CashEF.Commands.CloseCashSession;
using Microservice.Application.Features.CashEF.Commands.OpenCashSession;
using Microservice.Application.Features.CashEF.Commands.RegisterCashMovement;
using Microservice.Application.Features.CashEF.Queries.GetCashSessionById;
using Microservice.Application.Features.CashEF.Queries.GetCashSessionsPaginated;
using Microservice.Application.Features.CashEF.Queries.GetOpenCashSessions;
using Microservice.Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace Microservice.API.Controllers;
// Controlador EF Core de Gestión de Caja — apertura, movimientos y cierre con arqueo.
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class CashEFController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// POST /api/cash/sessions
    /// Abre un turno de caja.
    /// Returns: 201 Created con el PublicId · 400 validación (incl. caja ya abierta)
    /// </summary>
    [HttpPost("sessions")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> OpenSession(
        [FromBody] OpenCashSessionCommand request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(request, cancellationToken);
        return result.ToActionResult(StatusCodes.Status201Created);
    }

    /// <summary>
    /// POST /api/cash/sessions/{publicId}/movements
    /// Registra un movimiento de efectivo en el turno.
    /// Returns: 201 Created con el PublicId del movimiento · 404 · 409 si la caja está cerrada
    /// </summary>
    [HttpPost("sessions/{publicId:guid}/movements")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegisterMovement(
        Guid publicId,
        [FromBody] RegisterCashMovementRequestDto request,
        CancellationToken cancellationToken)
    {
        var command = new RegisterCashMovementCommand(
            publicId, request.MovementType, request.Amount, request.Description);
        var result = await mediator.Send(command, cancellationToken);
        return result.ToActionResult(StatusCodes.Status201Created);
    }

    /// <summary>
    /// POST /api/cash/sessions/{publicId}/close
    /// Cierra el turno con arqueo; devuelve esperado vs. declarado y la diferencia.
    /// Returns: 200 OK con el resumen · 404 · 409 si ya está cerrada
    /// </summary>
    [HttpPost("sessions/{publicId:guid}/close")]
    [ProducesResponseType(typeof(CashSessionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CloseSession(
        Guid publicId,
        [FromBody] CloseCashSessionRequestDto request,
        CancellationToken cancellationToken)
    {
        var command = new CloseCashSessionCommand(publicId, request.DeclaredBalance, request.ClosedBy);
        var result = await mediator.Send(command, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// GET /api/cash/sessions/{publicId}
    /// Detalle del turno con sus movimientos.
    /// Returns: 200 OK · 404
    /// </summary>
    [HttpGet("sessions/{publicId:guid}")]
    [ProducesResponseType(typeof(CashSessionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSessionById(
        Guid publicId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetCashSessionByIdQuery(publicId), cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// GET /api/cash/sessions/open
    /// Turnos de caja actualmente abiertos.
    /// Returns: 200 OK con la lista
    /// </summary>
    [HttpGet("sessions/open")]
    [ProducesResponseType(typeof(IReadOnlyList<CashSessionsPaginatedDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOpenSessions(
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetOpenCashSessionsQuery(), cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// GET /api/cash/sessions?page=1&amp;size=10
    /// Listado paginado de turnos.
    /// Returns: 200 OK con PagedResult
    /// </summary>
    [HttpGet("sessions")]
    [ProducesResponseType(typeof(PagedResult<CashSessionsPaginatedDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSessionsPaginated(
        [FromQuery] int page = 1,
        [FromQuery] int size = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetCashSessionsPaginatedQuery(page, size), cancellationToken);
        return result.ToActionResult();
    }
}

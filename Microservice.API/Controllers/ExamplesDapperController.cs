using Asp.Versioning;
using MediatR;
using Microservice.API.Extensions;
using Microservice.Application.DTOs;
using Microservice.Application.Features.ExamplesDapper.Commands.CreateExampleDapper;
using Microservice.Application.Features.ExamplesDapper.Commands.DeleteExampleDapper;
using Microservice.Application.Features.ExamplesDapper.Commands.UpdateExampleDapper;
using Microservice.Application.Features.ExamplesDapper.Queries.CountExamplesDapper;
using Microservice.Application.Features.ExamplesDapper.Queries.ExistsExampleByNameDapper;
using Microservice.Application.Features.ExamplesDapper.Queries.GetAllExamplesDapper;
using Microservice.Application.Features.ExamplesDapper.Queries.GetExampleByPublicIdDapper;
using Microservice.Application.Features.ExamplesDapper.Queries.GetExamplesPaginatedDapper;
using Microservice.Application.Features.ExamplesDapper.Queries.SearchExamplesByNameDapper;
using Microservice.Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace Microservice.API.Controllers;
// Controlador Dapper del aggregate Example — expone endpoints HTTP que delegan a MediatR.
// ── Parámetros ────────────────────────────────────────────────────────────
//   · mediator — IMediator (MediatR): despacha queries y commands al handler registrado
//     en el pipeline; desacopla el controller de la capa Application y habilita
//     el flujo de behaviors (validación, logging, caching).
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class ExamplesDapperController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// GET /api/v1/examplesdapper?page=1&amp;size=10
    /// Get paginated Examples via Dapper
    ///
    /// Query Parameters:
    /// - page: Page number (default: 1)
    /// - size: Page size (default: 10)
    ///
    /// Returns: 200 OK with PagedResult
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<GetExamplesPaginatedDapperDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaginated(
        [FromQuery] int page = 1,
        [FromQuery] int size = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetExamplesPaginatedDapperQuery(page, size);
        var result = await mediator.Send(query, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// GET /api/v1/examplesdapper/all
    /// Get all Examples via Dapper (unbounded — use only for bounded/reference datasets)
    ///
    /// Returns: 200 OK with all Examples
    /// </summary>
    [HttpGet("all")]
    [ProducesResponseType(typeof(IEnumerable<GetAllExamplesDapperDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllExamples(CancellationToken cancellationToken = default)
    {
        var query = new GetAllExamplesDapperQuery();
        var result = await mediator.Send(query, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// GET /api/v1/examplesdapper/count
    /// Count all Examples via Dapper
    ///
    /// Use Case: Statistics, metrics, pagination calculations
    ///
    /// Returns: 200 OK with total count
    /// </summary>
    [HttpGet("count")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<IActionResult> CountExamples(CancellationToken cancellationToken = default)
    {
        var query = new CountExamplesDapperQuery();
        var result = await mediator.Send(query, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// GET /api/v1/examplesdapper/search?name=
    /// Search Examples by name via Dapper (case-insensitive ILike)
    ///
    /// Query Parameters:
    /// - name: Search term to match against the Example name
    ///
    /// Returns: 200 OK with matching Examples collection
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(IEnumerable<SearchExamplesByNameDapperDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchExamplesByName(
        [FromQuery] string name,
        CancellationToken cancellationToken = default)
    {
        var query = new SearchExamplesByNameDapperQuery(name);
        var result = await mediator.Send(query, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// GET /api/v1/examplesdapper/exists-by-name?name=
    /// Check if an Example with the given name exists via Dapper
    ///
    /// Query Parameters:
    /// - name: Exact name to check for existence
    ///
    /// Returns: 200 OK with boolean (true/false)
    /// Performance: Optimized existence check (no data load)
    /// </summary>
    [HttpGet("exists-by-name")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExistsExampleByName(
        [FromQuery] string name,
        CancellationToken cancellationToken = default)
    {
        var query = new ExistsExampleByNameDapperQuery(name);
        var result = await mediator.Send(query, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// GET /api/v1/examplesdapper/{publicId}
    /// Get Example by PublicId via Dapper
    ///
    /// Returns: 200 OK with Example data
    /// Error: 404 Not Found if not exists
    /// </summary>
    [HttpGet("{publicId:guid}")]
    [ProducesResponseType(typeof(GetExampleByPublicIdDapperDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetExampleByPublicId(
        Guid publicId,
        CancellationToken cancellationToken = default)
    {
        var query = new GetExampleByPublicIdDapperQuery(publicId);
        var result = await mediator.Send(query, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// POST /api/v1/examplesdapper
    /// Create a new Example via Dapper
    ///
    /// Returns: 201 Created with the new resource PublicId
    /// Error: 400 Bad Request if validation fails
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateExample(
        [FromBody] CreateExampleDapperCommand request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(request, cancellationToken);
        return result.ToActionResult(StatusCodes.Status201Created);
    }

    /// <summary>
    /// PUT /api/v1/examplesdapper/{publicId}
    /// Update an existing Example via Dapper
    ///
    /// Use Case: Full entity replacement (PUT semantics)
    /// Body: { name?: string, description?: string }
    ///
    /// Returns: 200 OK with updated PublicId
    /// Error: 404 Not Found if not exists · 409 Conflict if domain invariant violated
    /// </summary>
    [HttpPut("{publicId:guid}")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateExample(
        Guid publicId,
        [FromBody] UpdateExampleRequestDto? request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateExampleDapperCommand(publicId, request?.Name, request?.Description);
        var result = await mediator.Send(command, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// DELETE /api/v1/examplesdapper/{publicId}
    /// Delete Example by PublicId via Dapper
    ///
    /// Returns: 200 OK with deleted PublicId
    /// Error: 404 Not Found if not exists
    /// </summary>
    [HttpDelete("{publicId:guid}")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteExample(
        Guid publicId,
        CancellationToken cancellationToken)
    {
        var command = new DeleteExampleDapperCommand(publicId);
        var result = await mediator.Send(command, cancellationToken);
        return result.ToActionResult();
    }
}

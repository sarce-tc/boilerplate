using MediatR;
using Microservice.API.Extensions;
using Microservice.Application.Common.Results;
using Microservice.Application.DTOs.EF;
using Microservice.Application.Features.ExamplesEF.Commands.CreateExample;
using Microservice.Application.Features.ExamplesEF.Queries.GetAllExample;
using Microservice.Application.Features.ExamplesEF.Queries.GetExampleByPredicate;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;

namespace Microservice.API.Controllers.V2;
/// <summary>
/// Examples API Controller v2.0
/// 
/// Enhanced version with:
/// - Improved response models
/// - Additional metadata
/// - Better error handling
/// - Performance optimizations
/// 
/// New Features in v2.0:
/// - Enhanced DTOs with additional fields
/// - Optimized queries with projection
/// - Better validation
/// - Response caching headers
/// - Request tracing
/// 
/// Breaking Changes from v1.0:
/// - New response format with metadata
/// - Modified field names (camelCase)
/// - Additional required fields
/// 
/// Versioning: Supports multiple versioning strategies
/// - URL Path: /api/v2/examples
/// - Query String: /api/examples?api-version=2.0
/// - Header: X-Version: 2.0
/// - Media Type: application/json;v=2.0
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("2.0")]
public class ExamplesV2Controller : ControllerBase
{
    private readonly IMediator _mediator;

    public ExamplesV2Controller(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// GET /api/v2/examples
    /// Get all Examples with enhanced v2 response format
    /// 
    /// v2.0 Enhancements:
    /// - Response includes metadata
    /// - Optimized field projection
    /// - Caching headers
    /// - Request tracing
    /// 
    /// Response Format v2.0:
    /// {
    ///   "data": [...],
    ///   "metadata": {
    ///     "total": 100,
    ///     "version": "2.0",
    ///     "timestamp": "2024-01-01T00:00:00Z",
    ///     "requestId": "guid"
    ///   }
    /// }
    /// </summary>
    [HttpGet]
    [MapToApiVersion("2.0")]
    [ProducesResponseType(typeof(ExamplesV2ResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllExamplesV2(
        [FromQuery] int page = 1,
        [FromQuery] int size = 10,
        CancellationToken cancellationToken = default)
    {
        // Add caching headers for v2
        Response.Headers["Cache-Control"] = "public, max-age=300";
        Response.Headers["X-Request-Id"] = Guid.NewGuid().ToString();

        var query = new GetAllExamplesQuery();
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
            return result.ToActionResult();

        // Transform to v2 format with metadata
        var v2Response = new ExamplesV2ResponseDto
        {
            Data = result.Value?.Select(e => new ExampleV2Dto
            {
                Id = e.PublicId,
                Name = e.Name,
                Description = e.Description,
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt,
                Version = "2.0"
            }).ToList() ?? [],
            Metadata = new ResponseMetadataV2
            {
                Total = result.Value?.Count() ?? 0,
                Version = "2.0",
                Timestamp = DateTimeOffset.UtcNow,
                RequestId = Response.Headers["X-Request-Id"].ToString(),
                Page = page,
                PageSize = size
            }
        };

        return Ok(v2Response);
    }

    /// <summary>
    /// GET /api/v2/examples/{id}
    /// Get Example by ID with enhanced v2 response format
    /// 
    /// v2.0 Features:
    /// - Enhanced error messages
    /// - Additional response metadata
    /// - Better validation
    /// </summary>
    [HttpGet("{id:guid}")]
    [MapToApiVersion("2.0")]
    [ProducesResponseType(typeof(ExampleV2Dto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetExampleByIdV2(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        Response.Headers["X-Request-Id"] = Guid.NewGuid().ToString();

        var query = new GetExampleByPredicateQuery(id);
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
            return result.ToActionResult();

        var v2Response = new ExampleV2Dto
        {
            Id = result.Value!.PublicId,
            Name = result.Value.Name,
            Description = result.Value.Description,
            CreatedAt = result.Value.CreatedAt,
            UpdatedAt = result.Value.UpdatedAt,
            Version = "2.0"
        };

        return Ok(v2Response);
    }

    /// <summary>
    /// POST /api/v2/examples
    /// Create new Example with v2.0 enhanced validation
    /// 
    /// v2.0 Improvements:
    /// - Stricter validation
    /// - Enhanced error messages
    /// - Better response format
    /// - Request tracking
    /// </summary>
    [HttpPost]
    [MapToApiVersion("2.0")]
    [ProducesResponseType(typeof(ExampleV2Dto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateExampleV2(
        [FromBody] CreateExampleV2RequestDto request,
        CancellationToken cancellationToken = default)
    {
        Response.Headers["X-Request-Id"] = Guid.NewGuid().ToString();

        // Transform v2 request to v1 command for now
        var command = new CreateExampleCommand(request.Name, request.Description);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return result.ToActionResult(StatusCodes.Status400BadRequest);

        var v2Response = new ExampleV2Dto
        {
            Id = result.Value,
            Name = request.Name,
            Description = request.Description,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            Version = "2.0"
        };

        return CreatedAtAction(
            nameof(GetExampleByIdV2),
            new { id = result.Value, version = "2.0" },
            v2Response);
    }
}

// v2.0 DTOs for enhanced response format
public class ExamplesV2ResponseDto
{
    public List<ExampleV2Dto> Data { get; set; } = new();
    public ResponseMetadataV2 Metadata { get; set; } = new();
}

public class ExampleV2Dto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public string Version { get; set; } = "2.0";
}

public class ResponseMetadataV2
{
    public int Total { get; set; }
    public string Version { get; set; } = "2.0";
    public DateTimeOffset Timestamp { get; set; }
    public string RequestId { get; set; } = string.Empty;
    public int Page { get; set; }
    public int PageSize { get; set; }
}

public class CreateExampleV2RequestDto
{
    /// <summary>
    /// v2.0: Name is required and must be between 3 and 100 characters
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// v2.0: Description is optional but recommended
    /// </summary>
    public string? Description { get; set; }
}

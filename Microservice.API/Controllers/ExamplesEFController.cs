using Asp.Versioning;
using MediatR;
using Microservice.API.Extensions;
using Microservice.Application.DTOs;
using Microservice.Application.Features.ExamplesEF.Commands.ActivateExample;
using Microservice.Application.Features.ExamplesEF.Commands.CreateExample;
using Microservice.Application.Features.ExamplesEF.Commands.DeleteExample;
using Microservice.Application.Features.ExamplesEF.Commands.DeleteManyExamples;
using Microservice.Application.Features.ExamplesEF.Commands.ExecuteInTransaction;
using Microservice.Application.Features.ExamplesEF.Commands.ExecuteSql;
using Microservice.Application.Features.ExamplesEF.Commands.ExecuteStoredProcedure;
using Microservice.Application.Features.ExamplesEF.Commands.UpdateExample;
using Microservice.Application.Features.ExamplesEF.Commands.UpdateExampleFields;
using Microservice.Application.Features.ExamplesEF.Commands.UpdateManyExamples;
using Microservice.Application.Features.ExamplesEF.Queries.CountExamples;
using Microservice.Application.Features.ExamplesEF.Queries.ExecuteSqlWithResult;
using Microservice.Application.Features.ExamplesEF.Queries.ExistsExample;
using Microservice.Application.Features.ExamplesEF.Queries.GetAllExample;
using Microservice.Application.Features.ExamplesEF.Queries.GetExampleByPredicate;
using Microservice.Application.Features.ExamplesEF.Queries.GetExampleItemByPublicId;
using Microservice.Application.Features.ExamplesEF.Queries.GetExampleItems;
using Microservice.Application.Features.ExamplesEF.Queries.GetExamplesFromSql;
using Microservice.Application.Features.ExamplesEF.Queries.GetExampleSummary;
using Microservice.Application.Features.ExamplesEF.Queries.GetExamplesPaginated;
using Microservice.Application.Features.ExamplesEF.Queries.GetExamplesWithProjection;
using Microservice.Application.Features.ExamplesEF.Queries.GetExampleWithItems;
using Microservice.Application.Features.ExamplesEF.Queries.GetExampleWithProjection;
using Microservice.Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace Microservice.API.Controllers;
// Reference controller demonstrating the full EF feature set:
// CRUD, batch ops, pagination, field projection, raw SQL, stored procedures, transactions.
// All actions delegate to MediatR and return result.ToActionResult().
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class ExamplesEFController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    /// <summary>
    /// POST /api/examples
    /// Create a new Example
    /// 
    /// Returns: 201 Created with the new resource PublicId
    /// Error: 400 Bad Request if validation fails
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateExample(
        [FromBody] CreateExampleCommand request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(request, cancellationToken);
        return result.ToActionResult(StatusCodes.Status201Created);
    }

    /// <summary>
    /// GET /api/examples/{publicId}
    /// Get Example by PublicId
    /// 
    /// Returns: 200 OK with Example data
    /// Error: 404 Not Found if not exists
    /// </summary>
    [HttpGet("{publicId:guid}")]
    [ProducesResponseType(typeof(GetExampleByPredicateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetExampleById(
        Guid publicId,
        CancellationToken cancellationToken)
    {
        var query = new GetExampleByPredicateQuery(publicId);
        var result = await _mediator.Send(query, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// GET /api/examples?page=1&amp;size=10
    /// Get paginated Examples
    /// 
    /// Query Parameters:
    /// - page: Page number (default: 1)
    /// - size: Page size (default: 10)
    /// 
    /// Returns: 200 OK with PagedResult
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<GetExamplesPaginatedDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaginated(
        [FromQuery] int page = 1,
        [FromQuery] int size = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetExamplesPaginatedQuery(page, size);
        var result = await _mediator.Send(query, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// GET /api/examples/all
    /// Get all Examples
    /// 
    /// Note: Returns all records without pagination — use GetPaginated for large datasets
    /// 
    /// Returns: 200 OK with all Examples
    /// </summary>
    [HttpGet("all")]
    [ProducesResponseType(typeof(IEnumerable<GetAllExamplesDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllExamples(
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllExamplesQuery();
        var result = await _mediator.Send(query, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// GET /api/examples/count
    /// Count all Examples
    /// 
    /// Use Case: Statistics, metrics, pagination calculations
    /// 
    /// Returns: 200 OK with total count
    /// </summary>
    [HttpGet("count")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<IActionResult> CountExamples(
        CancellationToken cancellationToken = default)
    {
        var query = new CountExamplesQuery();
        var result = await _mediator.Send(query, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// GET /api/examples/{publicId}/exists
    /// Check if Example exists
    /// 
    /// Returns: 200 OK with boolean (true/false)
    /// Performance: Optimized existence check (no data load)
    /// </summary>
    [HttpGet("{publicId:guid}/exists")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExistsExample(
        Guid publicId,
        CancellationToken cancellationToken)
    {
        var query = new ExistsExampleQuery(publicId);
        var result = await _mediator.Send(query, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// GET /api/examples/projection
    /// Get Examples with field projection
    /// 
    /// Use Case: Lightweight responses with only needed fields
    /// Performance: Reduced bandwidth (select specific columns only)
    /// 
    /// Returns: 200 OK with projected data
    /// </summary>
    [HttpGet("projection")]
    [ProducesResponseType(typeof(IEnumerable<GetExamplesWithProjectionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExamplesWithProjection(
        CancellationToken cancellationToken = default)
    {
        var query = new GetExamplesWithProjectionQuery();
        var result = await _mediator.Send(query, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// GET /api/examples/{publicId}/with-items
    /// Get Example with its child items collection
    ///
    /// Use Case: Detail view when items are needed alongside the aggregate root.
    ///
    /// Returns: 200 OK with Example + Items
    /// Error: 404 Not Found if not exists
    /// </summary>
    [HttpGet("{publicId:guid}/with-items")]
    [ProducesResponseType(typeof(GetExampleWithItemsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetExampleWithItems(
        Guid publicId,
        CancellationToken cancellationToken)
    {
        var query = new GetExampleWithItemsQuery(publicId);
        var result = await _mediator.Send(query, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// GET /api/examples/{publicId}/items
    /// Get all items belonging to an Example
    ///
    /// Returns: 200 OK with items collection
    /// Error: 404 Not Found if the parent Example does not exist
    /// </summary>
    [HttpGet("{publicId:guid}/items")]
    [ProducesResponseType(typeof(IEnumerable<GetExampleItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetExampleItems(
        Guid publicId,
        CancellationToken cancellationToken)
    {
        var query = new GetExampleItemsQuery(publicId);
        var result = await _mediator.Send(query, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// GET /api/examples/{publicId}/items/{itemPublicId}
    /// Get a single item by its PublicId within an Example
    ///
    /// Returns: 200 OK with item data
    /// Error: 404 Not Found if the Example or the item does not exist
    /// </summary>
    [HttpGet("{publicId:guid}/items/{itemPublicId:guid}")]
    [ProducesResponseType(typeof(GetExampleItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetExampleItemByPublicId(
        Guid publicId,
        Guid itemPublicId,
        CancellationToken cancellationToken)
    {
        var query = new GetExampleItemByPublicIdQuery(publicId, itemPublicId);
        var result = await _mediator.Send(query, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// GET /api/examples/{publicId}/projection
    /// Get single Example with field projection
    /// 
    /// Use Case: Detail view with specific fields
    /// 
    /// Returns: 200 OK with projected data
    /// Error: 404 Not Found
    /// </summary>
    [HttpGet("{publicId:guid}/projection")]
    [ProducesResponseType(typeof(GetExampleWithProjectionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetExampleWithProjection(
        Guid publicId,
        CancellationToken cancellationToken)
    {
        var query = new GetExampleWithProjectionQuery(publicId);
        var result = await _mediator.Send(query, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// GET /api/examples/from-sql?sql=...
    /// Execute raw SQL SELECT query
    /// 
    /// Query Parameters:
    /// - sql: FormattableString SQL query (parameterized)
    /// 
    /// Note: SQL is handler-supplied (not from query string), FormattableString-parameterized
    /// Use Case: Complex analytical queries
    /// 
    /// Returns: 200 OK with query results
    /// </summary>
    [HttpGet("from-sql")]
    [ProducesResponseType(typeof(IEnumerable<GetExamplesFromSqlDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFromSql(
        [FromQuery] string? sql = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetExamplesFromSqlQuery(sql ?? string.Empty);
        var result = await _mediator.Send(query, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// PUT /api/examples/{publicId}
    /// Update entire Example
    /// 
    /// Use Case: Full entity replacement (PUT semantics)
    /// Body: { name?: string, description?: string }
    /// 
    /// Returns: 200 OK with updated PublicId
    /// Error: 404 Not Found if not exists
    /// </summary>
    [HttpPut("{publicId:guid}")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateExample(
        Guid publicId,
        [FromBody] UpdateExampleRequestDto? request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateExampleCommand(publicId, request?.Name, request?.Description);
        var result = await _mediator.Send(command, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// PUT /api/examples/{publicId}/fields
    /// Update specific fields only
    /// 
    /// Use Case: PATCH-style partial updates
    /// Body: { name?: string, description?: string }
    /// Performance: Only modified columns in SQL
    /// 
    /// Returns: 200 OK with updated PublicId
    /// Error: 404 Not Found if not exists
    /// </summary>
    [HttpPut("{publicId:guid}/fields")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateExampleFields(
        Guid publicId,
        [FromBody] UpdateExampleFieldsRequestDto? request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateExampleFieldsCommand(publicId, request?.Name, request?.Description);
        var result = await _mediator.Send(command, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// PUT /api/examples/batch
    /// Update multiple Examples in bulk
    /// 
    /// Body: JSON array of PublicIds
    /// Example: ["guid1", "guid2", "guid3"]
    /// 
    /// Returns: 200 OK with count of updated records
    /// </summary>
    [HttpPut("batch")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateManyExamples(
        [FromBody] Guid[] publicIds,
        CancellationToken cancellationToken)
    {
        var command = new UpdateManyExamplesCommand(publicIds);
        var result = await _mediator.Send(command, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// DELETE /api/examples/{publicId}
    /// Delete Example by PublicId
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
        var command = new DeleteExampleCommand(publicId);
        var result = await _mediator.Send(command, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// DELETE /api/examples/batch
    /// Delete multiple Examples in bulk
    /// 
    /// Body: JSON array of PublicIds
    /// Example: ["guid1", "guid2", "guid3"]
    /// 
    /// Returns: 200 OK with count of deleted records
    /// </summary>
    [HttpDelete("batch")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteManyExamples(
        [FromBody] Guid[] publicIds,
        CancellationToken cancellationToken)
    {
        var command = new DeleteManyExamplesCommand(publicIds);
        var result = await _mediator.Send(command, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// GET /api/examples/{publicId}/summary
    /// Get Example aggregate summary with computed item statistics
    ///
    /// Use Case: Dashboard cards, list-view rows where item counts matter but full item detail is not needed.
    ///
    /// Returns: 200 OK with PublicId, Name, Status, ItemCount, PendingItemCount, CompletedItemCount
    /// Error: 404 Not Found if not exists
    /// </summary>
    [HttpGet("{publicId:guid}/summary")]
    [ProducesResponseType(typeof(GetExampleSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetExampleSummary(
        Guid publicId,
        CancellationToken cancellationToken)
    {
        var query = new GetExampleSummaryQuery(publicId);
        var result = await _mediator.Send(query, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// PUT /api/examples/{publicId}/activate
    /// Reactivate an Inactive Example
    ///
    /// Use Case: Restore a previously deactivated aggregate.
    /// Domain rule: if the Example is already Active, the domain throws DomainException → HTTP 409.
    ///
    /// Returns: 200 OK with PublicId
    /// Error: 404 Not Found · 409 Conflict if already active
    /// </summary>
    [HttpPut("{publicId:guid}/activate")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ActivateExample(
        Guid publicId,
        CancellationToken cancellationToken)
    {
        var command = new ActivateExampleCommand(publicId);
        var result = await _mediator.Send(command, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// POST /api/examples/execute-sql
    /// Execute raw SQL command (INSERT, UPDATE, DELETE)
    /// 
    /// Body: { sql: "INSERT INTO ..." }
    /// 
    /// Note: Always use FormattableString in the command for safe parameterization
    /// Returns: 200 OK with affected row count
    /// </summary>
    [HttpPost("execute-sql")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExecuteSql(
        [FromBody] ExecuteSqlCommand request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(request, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// POST /api/examples/execute-stored-procedure
    /// Execute database stored procedure
    /// 
    /// Body: { sql: "EXEC sp_StoredProcedureName ..." }
    /// 
    /// Use Case: Complex business logic in database
    /// Returns: 200 OK with result
    /// </summary>
    [HttpPost("execute-stored-procedure")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExecuteStoredProcedure(
        [FromBody] ExecuteStoredProcedureCommand request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(request, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// POST /api/examples/execute-sql-with-result
    /// Execute raw SQL SELECT and get mapped results
    /// 
    /// Body: { sql: "SELECT * FROM ..." }
    /// 
    /// Returns: 200 OK with query results
    /// </summary>
    [HttpPost("execute-sql-with-result")]
    [ProducesResponseType(typeof(IReadOnlyList<ExecuteSqlWithResultDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExecuteSqlWithResult(
        [FromBody] ExecuteSqlWithResultQuery request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(request, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// POST /api/examples/execute-in-transaction
    /// Execute multiple operations atomically
    /// 
    /// Use Case: Multi-step workflows with ACID guarantees
    /// Returns: 200 OK with result
    /// Error: 400 Bad Request if any step fails (full rollback)
    /// </summary>
    [HttpPost("execute-in-transaction")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExecuteInTransaction(
        [FromBody] ExecuteInTransactionCommand request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(request, cancellationToken);
        return result.ToActionResult();
    }
}

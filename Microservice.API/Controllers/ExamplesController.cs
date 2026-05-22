using MediatR;
using Microservice.API.Extensions;
using Microservice.Application.Common.Results;
using Microservice.Application.DTOs;
using Microservice.Application.Features.Examples.Commands.CreateExample;
using Microservice.Application.Features.Examples.Commands.DeleteExample;
using Microservice.Application.Features.Examples.Commands.DeleteManyExamples;
using Microservice.Application.Features.Examples.Commands.ExecuteInTransaction;
using Microservice.Application.Features.Examples.Commands.ExecuteSql;
using Microservice.Application.Features.Examples.Commands.ExecuteStoredProcedure;
using Microservice.Application.Features.Examples.Commands.UpdateExample;
using Microservice.Application.Features.Examples.Commands.UpdateExampleFields;
using Microservice.Application.Features.Examples.Commands.UpdateManyExamples;
using Microservice.Application.Features.Examples.Queries.CountExamples;
using Microservice.Application.Features.Examples.Queries.ExecuteSqlWithResult;
using Microservice.Application.Features.Examples.Queries.ExistsExample;
using Microservice.Application.Features.Examples.Queries.GetAllExample;
using Microservice.Application.Features.Examples.Queries.GetExampleByPredicate;
using Microservice.Application.Features.Examples.Queries.GetExamplesFromSql;
using Microservice.Application.Features.Examples.Queries.GetExamplesPaginated;
using Microservice.Application.Features.Examples.Queries.GetExamplesWithProjection;
using Microservice.Application.Features.Examples.Queries.GetExampleWithProjection;
using Microservice.Application.Models;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;

namespace Microservice.API.Controllers
{
    /// <summary>
    /// Examples API Controller v1.0
    /// 
    /// Use Case: REST API endpoints for CRUD and query operations
    /// 
    /// Pattern: CQRS with MediatR
    /// - Commands: Create, Update, Delete
    /// - Queries: Get, Count, Exists
    /// - Advanced: ExecuteSql, ExecuteStoredProcedure, ExecuteInTransaction
    /// 
    /// Versioning: Supports multiple versioning strategies
    /// - URL Path: /api/v1/examples
    /// - Query String: /api/examples?api-version=1.0
    /// - Header: X-Version: 1.0
    /// 
    /// Error Handling: Uses Result Pattern with ToActionResult() extension
    /// - Success: Returns data with appropriate HTTP status
    /// - Failure: Returns RFC 7807 ProblemDetails
    /// 
    /// Features:
    /// - Automatic HTTP status mapping from error codes
    /// - Support for single and batch operations
    /// - Raw SQL and stored procedure execution
    /// - Pagination support
    /// - Field projection for optimized responses
    /// - API version reporting in headers
    /// </summary>
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class ExamplesController(IMediator mediator) : ControllerBase
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
        /// GET /api/examples?page=1&size=10
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
        /// ⚠️ Warning: Returns all records (no pagination)
        /// Use GetPaginated for large datasets
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
        /// ⚠️ Security: Only use pre-defined or AI-generated queries
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
        /// POST /api/examples/execute-sql
        /// Execute raw SQL command (INSERT, UPDATE, DELETE)
        /// 
        /// Body: { sql: "INSERT INTO ..." }
        /// 
        /// ⚠️ Security: Only use FormattableString for parameterization
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
}
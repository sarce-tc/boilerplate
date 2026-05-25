using Asp.Versioning;
using Microservice.Application.Contracts.Jobs;
using Microservice.Application.Models.Jobs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Microservice.API.Controllers;

/// <summary>
/// Universal job status endpoint.
///
/// Clients (including AI agents) submit long-running work via domain controllers
/// (e.g. POST /orders → 202 Accepted + Location: /jobs/{jobId}) and poll here
/// until the job reaches a terminal state (Completed or Failed).
///
/// Polling pattern:
/// <code>
/// do {
///     await Task.Delay(500);
///     var status = await GET /api/v1/jobs/{jobId};
/// } while (status.status is "Queued" or "Running");
/// </code>
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[AllowAnonymous]  // Status reads are public; secure if needed
[Tags("Jobs")]
public sealed class JobsController(IJobStatusStore statusStore) : ControllerBase
{
    /// <summary>
    /// GET /api/v1/jobs/{jobId}
    /// Returns the current status of a background job.
    /// </summary>
    /// <remarks>
    /// Poll this endpoint after receiving a 202 Accepted response from any
    /// domain operation that enqueues background work.
    ///
    /// Terminal states: <b>Completed</b> (check <c>result</c>) or
    /// <b>Failed</b> (check <c>error</c>).
    /// </remarks>
    /// <response code="200">Job found — check <c>status</c> field.</response>
    /// <response code="404">No job with this ID exists (or it has expired).</response>
    [HttpGet("{jobId:guid}")]
    [ProducesResponseType(typeof(JobRecord), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStatus(
        Guid jobId,
        CancellationToken cancellationToken)
    {
        var record = await statusStore.GetAsync(jobId, cancellationToken);

        return record is null
            ? NotFound(new { error = $"Job '{jobId}' not found or expired." })
            : Ok(record);
    }
}

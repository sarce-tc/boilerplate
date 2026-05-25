namespace Microservice.Application.Contracts.Jobs;

/// <summary>
/// Submits work items for asynchronous background processing.
///
/// Typical flow (202 Accepted pattern):
/// <code>
/// // 1. Controller enqueues work and returns 202 Accepted
/// var jobId = await _jobQueue.EnqueueAsync(new ProcessOrderWorkItem(orderId), ct);
/// return Accepted(new { jobId, statusUrl = $"/api/v1/jobs/{jobId}" });
///
/// // 2. Worker picks it up, processes, writes status via IJobStatusStore
/// // 3. Client polls GET /api/v1/jobs/{jobId} until Completed or Failed
/// </code>
///
/// Concrete implementations:
/// - <b>In-process</b>  — <c>System.Threading.Channels.Channel&lt;T&gt;</c> + <c>BackgroundService</c>
/// - <b>Durable</b>     — Hangfire, Quartz.NET, MassTransit Sagas
/// - <b>Cloud</b>       — Azure Service Bus, AWS SQS, Google Pub/Sub
/// </summary>
public interface IJobQueue
{
    /// <summary>
    /// Enqueues <paramref name="workItem"/> and returns the assigned job ID.
    /// </summary>
    /// <typeparam name="TWorkItem">
    /// A serializable POCO that carries the data the worker needs.
    /// Typically a record: <c>record ProcessOrderWorkItem(Guid OrderId);</c>
    /// </typeparam>
    /// <param name="workItem">The work item to process.</param>
    /// <param name="ct">Cancellation token (used to abort the enqueue, not the work itself).</param>
    /// <returns>A stable <see cref="Guid"/> that clients can use to poll for status.</returns>
    Task<Guid> EnqueueAsync<TWorkItem>(TWorkItem workItem, CancellationToken ct = default)
        where TWorkItem : class;
}

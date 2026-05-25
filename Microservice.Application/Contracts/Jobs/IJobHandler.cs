namespace Microservice.Application.Contracts.Jobs;

/// <summary>
/// Processes a specific type of background work item.
///
/// Register a concrete handler in DI:
/// <code>
/// services.AddScoped&lt;IJobHandler&lt;MyWorkItem&gt;, MyJobHandler&gt;();
/// </code>
///
/// The in-memory job worker (<c>JobWorker</c>) resolves the handler from a
/// scoped DI scope, so scoped services (repositories, DbContext, etc.) work
/// correctly inside the handler.
/// </summary>
/// <typeparam name="TWorkItem">
/// A POCO that carries the data the handler needs.
/// Example: <c>record SendEmailWorkItem(string To, string Subject, string Body);</c>
/// </typeparam>
public interface IJobHandler<TWorkItem>
    where TWorkItem : class
{
    /// <summary>
    /// Executes the work item.
    /// </summary>
    /// <param name="workItem">The data needed for this unit of work.</param>
    /// <param name="ct">Cancellation token; honour it for graceful shutdown.</param>
    /// <returns>
    /// An optional JSON-serialized result string stored in
    /// <see cref="Models.Jobs.JobRecord.Result"/> on success, or <c>null</c>.
    /// </returns>
    Task<string?> HandleAsync(TWorkItem workItem, CancellationToken ct);
}

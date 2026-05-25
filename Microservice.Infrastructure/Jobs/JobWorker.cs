using Microservice.Application.Contracts.Jobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace Microservice.Infrastructure.Jobs;

/// <summary>
/// Long-running background worker that drains <see cref="ChannelWorkItem"/>s
/// from the in-process channel and executes them one at a time.
///
/// A fresh DI scope is created for each item so scoped services
/// (repositories, DbContext, MediatR handlers, etc.) work correctly.
///
/// To process items in parallel, instantiate multiple workers or replace
/// the sequential <c>await foreach</c> with a <c>Parallel.ForEachAsync</c>.
/// </summary>
internal sealed class JobWorker(
    Channel<ChannelWorkItem> channel,
    IJobStatusStore statusStore,
    IServiceScopeFactory scopeFactory,
    ILogger<JobWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("JobWorker started");

        // ReadAllAsync completes when the channel is marked completed
        // (application shutdown) or stoppingToken is cancelled.
        await foreach (var item in channel.Reader.ReadAllAsync(stoppingToken))
        {
            await ProcessAsync(item, stoppingToken);
        }

        logger.LogInformation("JobWorker stopped");
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private async Task ProcessAsync(ChannelWorkItem item, CancellationToken ct)
    {
        logger.LogInformation("Job {JobId} starting", item.JobId);

        // Transition to Running
        var record = await statusStore.GetAsync(item.JobId, ct);
        if (record is not null)
            await statusStore.SetAsync(item.JobId, record.AsRunning(), ct);

        try
        {
            // New DI scope per job — keeps scoped services isolated
            await using var scope = scopeFactory.CreateAsyncScope();
            var result = await item.Execute(scope.ServiceProvider, ct);

            // Transition to Completed
            record = await statusStore.GetAsync(item.JobId, ct);
            await statusStore.SetAsync(item.JobId, record!.AsCompleted(result), ct);

            logger.LogInformation("Job {JobId} completed", item.JobId);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            // Application is shutting down; mark as failed so pollers know
            record = await statusStore.GetAsync(item.JobId, CancellationToken.None);
            await statusStore.SetAsync(item.JobId,
                record!.AsFailed("Job cancelled due to application shutdown."),
                CancellationToken.None);

            logger.LogWarning("Job {JobId} cancelled (shutdown)", item.JobId);
        }
        catch (Exception ex)
        {
            record = await statusStore.GetAsync(item.JobId, CancellationToken.None);
            await statusStore.SetAsync(item.JobId, record!.AsFailed(ex.Message), CancellationToken.None);

            logger.LogError(ex, "Job {JobId} failed", item.JobId);
        }
    }
}

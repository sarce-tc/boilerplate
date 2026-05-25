using Microservice.Application.Contracts.Jobs;
using Microservice.Application.Models.Jobs;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Channels;

namespace Microservice.Infrastructure.Jobs;

/// <summary>
/// In-process job queue backed by <see cref="System.Threading.Channels"/>.
///
/// How it works:
/// <list type="number">
///   <item>A new <see cref="Guid"/> job ID is generated.</item>
///   <item>The initial status (<see cref="JobStatus.Queued"/>) is stored.</item>
///   <item>A <see cref="ChannelWorkItem"/> is written to the unbounded channel.</item>
///   <item><see cref="JobWorker"/> reads it, creates a DI scope and resolves
///         <see cref="IJobHandler{TWorkItem}"/> to execute the work.</item>
/// </list>
///
/// For distributed scenarios, replace this with a RabbitMQ / Azure Service Bus
/// implementation that publishes the serialized work item to a message broker.
/// </summary>
internal sealed class InMemoryJobQueue(
    IJobStatusStore statusStore,
    Channel<ChannelWorkItem> channel) : IJobQueue
{
    public async Task<Guid> EnqueueAsync<TWorkItem>(
        TWorkItem workItem,
        CancellationToken ct = default)
        where TWorkItem : class
    {
        var jobId = Guid.NewGuid();

        // Persist initial status before the item hits the channel,
        // so callers can poll immediately after receiving the job ID.
        await statusStore.SetAsync(jobId, JobRecord.Create(jobId), ct);

        // Capture the work item in a closure; the channel carries only
        // ChannelWorkItem so generic type info stays at the call site.
        var channelItem = new ChannelWorkItem(
            JobId: jobId,
            Execute: (serviceProvider, cancellationToken) =>
            {
                var handler = serviceProvider.GetRequiredService<IJobHandler<TWorkItem>>();
                return handler.HandleAsync(workItem, cancellationToken);
            });

        await channel.Writer.WriteAsync(channelItem, ct);

        return jobId;
    }
}

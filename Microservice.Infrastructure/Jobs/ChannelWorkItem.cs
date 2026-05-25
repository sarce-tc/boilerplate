namespace Microservice.Infrastructure.Jobs;

/// <summary>
/// Internal envelope that carries a job through the in-process Channel.
/// The <see cref="Execute"/> delegate closes over the deserialized work item
/// and the resolved <see cref="IJobHandler{T}"/>, so the channel doesn't need
/// to know about generic type parameters at the transport layer.
/// </summary>
internal sealed record ChannelWorkItem(
    Guid JobId,
    Func<IServiceProvider, CancellationToken, Task<string?>> Execute
);

namespace Microservice.Application.Models.Jobs;

/// <summary>
/// Lifecycle state of a background job.
/// Transitions: Queued → Running → Completed | Failed
/// </summary>
public enum JobStatus
{
    /// <summary>Accepted; waiting in the queue to be picked up.</summary>
    Queued,

    /// <summary>A worker has dequeued the item and is processing it.</summary>
    Running,

    /// <summary>Processing finished successfully. Result is available.</summary>
    Completed,

    /// <summary>Processing finished with an error. Error message is available.</summary>
    Failed
}

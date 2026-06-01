using System.Text.Json;
using Microservice.Client.Infrastructure.Offline;
using Microservice.Client.Infrastructure.Offline.Sync;
using Microservice.Client.Shared.Results;

namespace Microservice.Client.Infrastructure.Gateways;

/// <summary>
/// Base for feature gateways that mutate server state with offline support. Encapsulates the
/// canonical write pipeline so every feature replicates it identically (the archetype rule):
///   online → apply and return the server id;
///   network race / offline → enqueue with a stable idempotency key and report Queued.
/// Reads stay in the concrete gateway (they own their cache keys/shapes).
/// </summary>
public abstract class OfflineGateway(IConnectivity connectivity, ISyncQueue syncQueue)
{
    protected static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    protected bool IsOnline => connectivity.IsOnline;

    protected SyncOperation NewOperation(string method, string url, object? body, string entityType) => new()
    {
        Method = method,
        Url = url,
        JsonBody = body is null ? null : JsonSerializer.Serialize(body, JsonOptions),
        EntityType = entityType
    };

    protected async Task<UiResult<CommandAck>> SubmitAsync(SyncOperation op, Func<Task<UiResult<Guid>>> online)
    {
        if (IsOnline)
        {
            var result = await online();
            if (result.IsSuccess)
                return UiResult<CommandAck>.Success(new CommandAck(Queued: false, ResourceId: result.Value));
            if (result.Error!.Kind is not ErrorKind.Network)
                return UiResult<CommandAck>.Failure(result.Error);
            // fall through: network race → queue for retry
        }

        await syncQueue.EnqueueAsync(op);
        return UiResult<CommandAck>.Success(new CommandAck(Queued: true));
    }
}

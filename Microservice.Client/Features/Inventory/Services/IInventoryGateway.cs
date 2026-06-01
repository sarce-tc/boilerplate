using Microservice.Client.Features.Inventory.Models;
using Microservice.Client.Shared.Contracts;
using Microservice.Client.Shared.Results;

namespace Microservice.Client.Features.Inventory.Services;

/// <summary>
/// Inventory gateway. Stock balances and the ledger are read-only projections; the only write is
/// registering a movement (append-only ledger entry that adjusts the balance server-side), which
/// may be queued offline. Stock is server-authoritative — never mutated directly from the client.
/// </summary>
public interface IInventoryGateway
{
    Task<UiResult<PagedResult<StockItemVm>>> GetStockPagedAsync(PageRequest request, CancellationToken ct = default);
    Task<UiResult<StockItemVm>> GetStockByProductAsync(Guid productPublicId, CancellationToken ct = default);
    Task<UiResult<IReadOnlyList<InventoryMovementVm>>> GetMovementsByProductAsync(Guid productPublicId, CancellationToken ct = default);

    Task<UiResult<CommandAck>> RegisterMovementAsync(RegisterInventoryMovementRequest request, CancellationToken ct = default);
}

using Microservice.Client.Features.Inventory.Models;
using Microservice.Client.Infrastructure.Gateways;
using Microservice.Client.Infrastructure.Http;
using Microservice.Client.Infrastructure.Offline;
using Microservice.Client.Infrastructure.Offline.IndexedDb;
using Microservice.Client.Infrastructure.Offline.Sync;
using Microservice.Client.Shared.Contracts;
using Microservice.Client.Shared.Results;

namespace Microservice.Client.Features.Inventory.Services;

/// <summary>Inventory gateway — replicates the archetype, adapted to read-only stock + append-only ledger.</summary>
public sealed class InventoryGateway(
    ApiClient api,
    IIndexedDb cache,
    IConnectivity connectivity,
    ISyncQueue syncQueue,
    ApiOptions options) : OfflineGateway(connectivity, syncQueue), IInventoryGateway
{
    private const string EntityType = "inventory-movement";
    private string Resource => options.ResourcePath("inventoryef"); // api/v1/inventoryef ([controller] = InventoryEF)

    public async Task<UiResult<PagedResult<StockItemVm>>> GetStockPagedAsync(PageRequest request, CancellationToken ct = default)
    {
        var url = $"{Resource}/stock?{request.ToQueryString()}";
        var cacheKey = $"inventory:stock:page:{request.Page}:{request.Size}";

        if (IsOnline)
        {
            var result = await api.GetAsync<PagedResult<StockItemDto>>(url, ct);
            if (result.IsSuccess)
            {
                await cache.PutAsync(Stores.ReadCache, cacheKey, result.Value!);
                return UiResult<PagedResult<StockItemVm>>.Success(MapPage(result.Value!));
            }
            if (result.Error!.Kind is not ErrorKind.Network)
                return UiResult<PagedResult<StockItemVm>>.Failure(result.Error);
        }

        var cached = await cache.GetAsync<PagedResult<StockItemDto>>(Stores.ReadCache, cacheKey);
        return cached is not null
            ? UiResult<PagedResult<StockItemVm>>.Success(MapPage(cached))
            : UiResult<PagedResult<StockItemVm>>.Failure(UiError.Network());
    }

    public async Task<UiResult<StockItemVm>> GetStockByProductAsync(Guid productPublicId, CancellationToken ct = default)
    {
        var url = $"{Resource}/stock/{productPublicId}";
        var cacheKey = $"inventory:stock:{productPublicId}";

        if (IsOnline)
        {
            var result = await api.GetAsync<StockItemDto>(url, ct);
            if (result.IsSuccess)
            {
                await cache.PutAsync(Stores.ReadCache, cacheKey, result.Value!);
                return UiResult<StockItemVm>.Success(InventoryMapper.ToStockItem(result.Value!));
            }
            if (result.Error!.Kind is not ErrorKind.Network)
                return UiResult<StockItemVm>.Failure(result.Error);
        }

        var cached = await cache.GetAsync<StockItemDto>(Stores.ReadCache, cacheKey);
        return cached is not null
            ? UiResult<StockItemVm>.Success(InventoryMapper.ToStockItem(cached))
            : UiResult<StockItemVm>.Failure(UiError.Network());
    }

    public async Task<UiResult<IReadOnlyList<InventoryMovementVm>>> GetMovementsByProductAsync(Guid productPublicId, CancellationToken ct = default)
    {
        var url = $"{Resource}/movements/{productPublicId}";
        var cacheKey = $"inventory:movements:{productPublicId}";

        if (IsOnline)
        {
            var result = await api.GetAsync<IReadOnlyList<InventoryMovementDto>>(url, ct);
            if (result.IsSuccess)
            {
                await cache.PutAsync(Stores.ReadCache, cacheKey, result.Value!);
                return UiResult<IReadOnlyList<InventoryMovementVm>>.Success(Map(result.Value!));
            }
            if (result.Error!.Kind is not ErrorKind.Network)
                return UiResult<IReadOnlyList<InventoryMovementVm>>.Failure(result.Error);
        }

        var cached = await cache.GetAsync<IReadOnlyList<InventoryMovementDto>>(Stores.ReadCache, cacheKey);
        return cached is not null
            ? UiResult<IReadOnlyList<InventoryMovementVm>>.Success(Map(cached))
            : UiResult<IReadOnlyList<InventoryMovementVm>>.Failure(UiError.Network());
    }

    public Task<UiResult<CommandAck>> RegisterMovementAsync(RegisterInventoryMovementRequest request, CancellationToken ct = default)
    {
        var url = $"{Resource}/movements";
        var op = NewOperation("POST", url, request, EntityType);
        return SubmitAsync(op, () => api.PostAsync<Guid>(url, request, op.IdempotencyKey, ct));
    }

    private static PagedResult<StockItemVm> MapPage(PagedResult<StockItemDto> dto) => new()
    {
        Results = dto.Results.Select(InventoryMapper.ToStockItem).ToList(),
        RowsCount = dto.RowsCount,
        PageCount = dto.PageCount,
        PageSize = dto.PageSize,
        CurrentPage = dto.CurrentPage
    };

    private static IReadOnlyList<InventoryMovementVm> Map(IReadOnlyList<InventoryMovementDto> dtos) =>
        dtos.Select(InventoryMapper.ToMovement).ToList();
}

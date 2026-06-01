using Microservice.Client.Features.Products.Models;
using Microservice.Client.Infrastructure.Gateways;
using Microservice.Client.Infrastructure.Http;
using Microservice.Client.Infrastructure.Offline;
using Microservice.Client.Infrastructure.Offline.IndexedDb;
using Microservice.Client.Infrastructure.Offline.Sync;
using Microservice.Client.Shared.Contracts;
using Microservice.Client.Shared.Results;

namespace Microservice.Client.Features.Products.Services;

/// <summary>
/// Canonical gateway implementation. Other features replicate this exact shape:
///   reads  → online-fetch + cache, offline-fallback to cache;
///   writes → SubmitAsync (inherited): online when possible, else enqueue with a stable key.
/// </summary>
public sealed class ProductsGateway(
    ApiClient api,
    IIndexedDb cache,
    IConnectivity connectivity,
    ISyncQueue syncQueue,
    ApiOptions options) : OfflineGateway(connectivity, syncQueue), IProductsGateway
{
    private const string EntityType = "product";
    private string Resource => options.ResourcePath("productsef"); // api/v1/products

    // ── Reads ─────────────────────────────────────────────────────────────────

    public async Task<UiResult<PagedResult<ProductListItemVm>>> GetPagedAsync(PageRequest request, CancellationToken ct = default)
    {
        var url = $"{Resource}?{request.ToQueryString()}";
        var cacheKey = $"products:page:{request.Page}:{request.Size}";

        if (IsOnline)
        {
            var result = await api.GetAsync<PagedResult<GetProductsPaginatedDto>>(url, ct);
            if (result.IsSuccess)
            {
                await cache.PutAsync(Stores.ReadCache, cacheKey, result.Value!);
                return UiResult<PagedResult<ProductListItemVm>>.Success(MapPage(result.Value!));
            }
            if (result.Error!.Kind is not ErrorKind.Network)
                return UiResult<PagedResult<ProductListItemVm>>.Failure(result.Error);
            // network race → fall through to cache
        }

        var cached = await cache.GetAsync<PagedResult<GetProductsPaginatedDto>>(Stores.ReadCache, cacheKey);
        return cached is not null
            ? UiResult<PagedResult<ProductListItemVm>>.Success(MapPage(cached))
            : UiResult<PagedResult<ProductListItemVm>>.Failure(UiError.Network());
    }

    public async Task<UiResult<ProductFormModel>> GetByIdAsync(Guid publicId, CancellationToken ct = default)
    {
        var url = $"{Resource}/{publicId}";
        var cacheKey = $"products:item:{publicId}";

        if (IsOnline)
        {
            var result = await api.GetAsync<GetProductDto>(url, ct);
            if (result.IsSuccess)
            {
                await cache.PutAsync(Stores.ReadCache, cacheKey, result.Value!);
                return UiResult<ProductFormModel>.Success(ProductMapper.ToFormModel(result.Value!));
            }
            if (result.Error!.Kind is not ErrorKind.Network)
                return UiResult<ProductFormModel>.Failure(result.Error);
        }

        var cached = await cache.GetAsync<GetProductDto>(Stores.ReadCache, cacheKey);
        return cached is not null
            ? UiResult<ProductFormModel>.Success(ProductMapper.ToFormModel(cached))
            : UiResult<ProductFormModel>.Failure(UiError.Network());
    }

    public async Task<UiResult<ProductListItemVm>> GetByBarcodeAsync(string code, CancellationToken ct = default)
    {
        var url = $"{Resource}/by-barcode/{Uri.EscapeDataString(code)}";

        // Scans are time-critical; try the network first, then any cached detail for that code.
        if (IsOnline)
        {
            var result = await api.GetAsync<GetProductDto>(url, ct);
            if (result.IsSuccess)
            {
                await cache.PutAsync(Stores.ReadCache, $"products:barcode:{code}", result.Value!);
                return UiResult<ProductListItemVm>.Success(ToListItem(result.Value!));
            }
            if (result.Error!.Kind is not ErrorKind.Network)
                return UiResult<ProductListItemVm>.Failure(result.Error);
        }

        var cached = await cache.GetAsync<GetProductDto>(Stores.ReadCache, $"products:barcode:{code}");
        return cached is not null
            ? UiResult<ProductListItemVm>.Success(ToListItem(cached))
            : UiResult<ProductListItemVm>.Failure(UiError.Network());
    }

    // ── Writes ────────────────────────────────────────────────────────────────

    public Task<UiResult<CommandAck>> CreateAsync(ProductFormModel model, CancellationToken ct = default)
    {
        var body = ProductMapper.ToCreateRequest(model);
        var op = NewOperation("POST", Resource, body, EntityType);
        return SubmitAsync(op, () => api.PostAsync<Guid>(Resource, body, op.IdempotencyKey, ct));
    }

    public Task<UiResult<CommandAck>> UpdateAsync(ProductFormModel model, IReadOnlyList<Guid>? removeBarcodeIds = null, CancellationToken ct = default)
    {
        var url = $"{Resource}/{model.PublicId}";
        var body = ProductMapper.ToUpdateRequest(model, removeBarcodeIds);
        var op = NewOperation("PUT", url, body, EntityType);
        return SubmitAsync(op, () => api.PutAsync<Guid>(url, body, op.IdempotencyKey, ct));
    }

    public Task<UiResult<CommandAck>> DeleteAsync(Guid publicId, CancellationToken ct = default)
    {
        var url = $"{Resource}/{publicId}";
        var op = NewOperation("DELETE", url, body: null, EntityType);
        return SubmitAsync(op, () => api.DeleteAsync<Guid>(url, op.IdempotencyKey, ct));
    }

    // ── Local mapping helpers ───────────────────────────────────────────────────

    private static PagedResult<ProductListItemVm> MapPage(PagedResult<GetProductsPaginatedDto> dto) => new()
    {
        Results = dto.Results.Select(ProductMapper.ToListItem).ToList(),
        RowsCount = dto.RowsCount,
        PageCount = dto.PageCount,
        PageSize = dto.PageSize,
        CurrentPage = dto.CurrentPage
    };

    private static ProductListItemVm ToListItem(GetProductDto dto) =>
        new(dto.PublicId, dto.Sku, dto.Name, dto.Price, dto.TaxRate, dto.CategoryName, dto.IsActive);
}

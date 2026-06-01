using Microservice.Client.Features.Sales.Models;
using Microservice.Client.Infrastructure.Gateways;
using Microservice.Client.Infrastructure.Http;
using Microservice.Client.Infrastructure.Offline;
using Microservice.Client.Infrastructure.Offline.IndexedDb;
using Microservice.Client.Infrastructure.Offline.Sync;
using Microservice.Client.Shared.Contracts;
using Microservice.Client.Shared.Results;

namespace Microservice.Client.Features.Sales.Services;

/// <summary>Sales gateway — replicates the Products archetype, adapted to the sale lifecycle.</summary>
public sealed class SalesGateway(
    ApiClient api,
    IIndexedDb cache,
    IConnectivity connectivity,
    ISyncQueue syncQueue,
    ApiOptions options) : OfflineGateway(connectivity, syncQueue), ISalesGateway
{
    private const string EntityType = "sale";
    private string Resource => options.ResourcePath("salesef"); // api/v1/salesef ([controller] = SalesEF)

    // ── Writes ────────────────────────────────────────────────────────────────

    public Task<UiResult<CommandAck>> CreateAsync(CreateSaleRequest request, CancellationToken ct = default)
    {
        // Append-only: safe to queue offline. The op id is the idempotency key (exactly-once replay).
        var op = NewOperation("POST", Resource, request, EntityType);
        return SubmitAsync(op, () => api.PostAsync<Guid>(Resource, request, op.IdempotencyKey, ct));
    }

    public async Task<UiResult<SaleResultVm>> ConfirmAsync(Guid salePublicId, CancellationToken ct = default)
    {
        // Server transaction (stock + cash) — online only. Stable key so a retry can't double-confirm.
        var result = await api.PostEmptyAsync<SaleDto>($"{Resource}/{salePublicId}/confirm", $"confirm:{salePublicId}", ct);
        return result.Map(SaleMapper.ToResult);
    }

    public async Task<UiResult> CancelAsync(Guid salePublicId, CancellationToken ct = default)
    {
        var result = await api.PostEmptyAsync<Guid>($"{Resource}/{salePublicId}/cancel", $"cancel:{salePublicId}", ct);
        return result.IsSuccess ? UiResult.Success() : UiResult.Failure(result.Error!);
    }

    // ── Reads ─────────────────────────────────────────────────────────────────

    public async Task<UiResult<TicketVm>> GetTicketAsync(Guid salePublicId, CancellationToken ct = default)
    {
        var url = $"{Resource}/{salePublicId}/ticket";
        var cacheKey = $"sales:ticket:{salePublicId}";

        if (IsOnline)
        {
            var result = await api.GetAsync<TicketDocument>(url, ct);
            if (result.IsSuccess)
            {
                await cache.PutAsync(Stores.ReadCache, cacheKey, result.Value!);
                return UiResult<TicketVm>.Success(SaleMapper.ToTicket(result.Value!));
            }
            if (result.Error!.Kind is not ErrorKind.Network)
                return UiResult<TicketVm>.Failure(result.Error);
        }

        var cached = await cache.GetAsync<TicketDocument>(Stores.ReadCache, cacheKey);
        return cached is not null
            ? UiResult<TicketVm>.Success(SaleMapper.ToTicket(cached))
            : UiResult<TicketVm>.Failure(UiError.Network());
    }

    public async Task<UiResult<SaleResultVm>> GetByIdAsync(Guid salePublicId, CancellationToken ct = default)
    {
        var result = await api.GetAsync<SaleDto>($"{Resource}/{salePublicId}", ct);
        return result.Map(SaleMapper.ToResult);
    }

    public async Task<UiResult<PagedResult<SaleListItemVm>>> GetPagedAsync(PageRequest request, CancellationToken ct = default)
    {
        var url = $"{Resource}?{request.ToQueryString()}";
        var cacheKey = $"sales:page:{request.Page}:{request.Size}";

        if (IsOnline)
        {
            var result = await api.GetAsync<PagedResult<SalesPaginatedDto>>(url, ct);
            if (result.IsSuccess)
            {
                await cache.PutAsync(Stores.ReadCache, cacheKey, result.Value!);
                return UiResult<PagedResult<SaleListItemVm>>.Success(MapPage(result.Value!));
            }
            if (result.Error!.Kind is not ErrorKind.Network)
                return UiResult<PagedResult<SaleListItemVm>>.Failure(result.Error);
        }

        var cached = await cache.GetAsync<PagedResult<SalesPaginatedDto>>(Stores.ReadCache, cacheKey);
        return cached is not null
            ? UiResult<PagedResult<SaleListItemVm>>.Success(MapPage(cached))
            : UiResult<PagedResult<SaleListItemVm>>.Failure(UiError.Network());
    }

    private static PagedResult<SaleListItemVm> MapPage(PagedResult<SalesPaginatedDto> dto) => new()
    {
        Results = dto.Results.Select(SaleMapper.ToListItem).ToList(),
        RowsCount = dto.RowsCount,
        PageCount = dto.PageCount,
        PageSize = dto.PageSize,
        CurrentPage = dto.CurrentPage
    };
}

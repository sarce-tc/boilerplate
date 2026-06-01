using Microservice.Client.Features.CashRegister.Models;
using Microservice.Client.Infrastructure.Gateways;
using Microservice.Client.Infrastructure.Http;
using Microservice.Client.Infrastructure.Offline;
using Microservice.Client.Infrastructure.Offline.IndexedDb;
using Microservice.Client.Infrastructure.Offline.Sync;
using Microservice.Client.Shared.Contracts;
using Microservice.Client.Shared.Results;

namespace Microservice.Client.Features.CashRegister.Services;

/// <summary>Cash gateway. Replicates the Products/Sales archetype: typed results, offline-aware writes.</summary>
public sealed class CashGateway(
    ApiClient api,
    IIndexedDb cache,
    IConnectivity connectivity,
    ISyncQueue syncQueue,
    ApiOptions options) : OfflineGateway(connectivity, syncQueue), ICashGateway
{
    private const string EntityType = "cash-session";
    private string Sessions => options.ResourcePath("cashef/sessions"); // api/v1/cash/sessions

    // ── Reads ─────────────────────────────────────────────────────────────────

    public async Task<UiResult<IReadOnlyList<CashSessionSummaryVm>>> GetOpenSessionsAsync(CancellationToken ct = default)
    {
        // Open-session state gates server-side selling → online only (no offline fallback).
        var result = await api.GetAsync<IReadOnlyList<CashSessionsPaginatedDto>>($"{Sessions}/open", ct);
        return result.Map(list => (IReadOnlyList<CashSessionSummaryVm>)list.Select(CashMapper.ToSummary).ToList());
    }

    public async Task<UiResult<CashSessionDetailVm>> GetByIdAsync(Guid publicId, CancellationToken ct = default)
    {
        var url = $"{Sessions}/{publicId}";
        var cacheKey = $"cash:session:{publicId}";

        if (IsOnline)
        {
            var result = await api.GetAsync<CashSessionDto>(url, ct);
            if (result.IsSuccess)
            {
                await cache.PutAsync(Stores.ReadCache, cacheKey, result.Value!);
                return UiResult<CashSessionDetailVm>.Success(CashMapper.ToDetail(result.Value!));
            }
            if (result.Error!.Kind is not ErrorKind.Network)
                return UiResult<CashSessionDetailVm>.Failure(result.Error);
        }

        var cached = await cache.GetAsync<CashSessionDto>(Stores.ReadCache, cacheKey);
        return cached is not null
            ? UiResult<CashSessionDetailVm>.Success(CashMapper.ToDetail(cached))
            : UiResult<CashSessionDetailVm>.Failure(UiError.Network());
    }

    public async Task<UiResult<PagedResult<CashSessionSummaryVm>>> GetPagedAsync(PageRequest request, CancellationToken ct = default)
    {
        var url = $"{Sessions}?{request.ToQueryString()}";
        var cacheKey = $"cash:page:{request.Page}:{request.Size}";

        if (IsOnline)
        {
            var result = await api.GetAsync<PagedResult<CashSessionsPaginatedDto>>(url, ct);
            if (result.IsSuccess)
            {
                await cache.PutAsync(Stores.ReadCache, cacheKey, result.Value!);
                return UiResult<PagedResult<CashSessionSummaryVm>>.Success(MapPage(result.Value!));
            }
            if (result.Error!.Kind is not ErrorKind.Network)
                return UiResult<PagedResult<CashSessionSummaryVm>>.Failure(result.Error);
        }

        var cached = await cache.GetAsync<PagedResult<CashSessionsPaginatedDto>>(Stores.ReadCache, cacheKey);
        return cached is not null
            ? UiResult<PagedResult<CashSessionSummaryVm>>.Success(MapPage(cached))
            : UiResult<PagedResult<CashSessionSummaryVm>>.Failure(UiError.Network());
    }

    // ── Writes ────────────────────────────────────────────────────────────────

    public Task<UiResult<CommandAck>> OpenSessionAsync(OpenCashSessionRequest request, CancellationToken ct = default)
    {
        var op = NewOperation("POST", Sessions, request, EntityType);
        return SubmitAsync(op, () => api.PostAsync<Guid>(Sessions, request, op.IdempotencyKey, ct));
    }

    public Task<UiResult<CommandAck>> RegisterMovementAsync(Guid sessionPublicId, RegisterCashMovementRequest request, CancellationToken ct = default)
    {
        // Append-only ledger entry → safe to queue offline (idempotent replay).
        var url = $"{Sessions}/{sessionPublicId}/movements";
        var op = NewOperation("POST", url, request, EntityType);
        return SubmitAsync(op, () => api.PostAsync<Guid>(url, request, op.IdempotencyKey, ct));
    }

    public async Task<UiResult<CashCloseResultVm>> CloseSessionAsync(Guid sessionPublicId, CloseCashSessionRequest request, CancellationToken ct = default)
    {
        // Arqueo is a server transaction whose result we need immediately → online only,
        // stable key so a retry can't double-close.
        var result = await api.PostAsync<CashSessionDto>(
            $"{Sessions}/{sessionPublicId}/close", request, idempotencyKey: $"close:{sessionPublicId}", ct);
        return result.Map(CashMapper.ToCloseResult);
    }

    private static PagedResult<CashSessionSummaryVm> MapPage(PagedResult<CashSessionsPaginatedDto> dto) => new()
    {
        Results = dto.Results.Select(CashMapper.ToSummary).ToList(),
        RowsCount = dto.RowsCount,
        PageCount = dto.PageCount,
        PageSize = dto.PageSize,
        CurrentPage = dto.CurrentPage
    };
}

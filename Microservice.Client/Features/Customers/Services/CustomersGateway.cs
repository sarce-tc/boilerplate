using Microservice.Client.Features.Customers.Models;
using Microservice.Client.Infrastructure.Gateways;
using Microservice.Client.Infrastructure.Http;
using Microservice.Client.Infrastructure.Offline;
using Microservice.Client.Infrastructure.Offline.IndexedDb;
using Microservice.Client.Infrastructure.Offline.Sync;
using Microservice.Client.Shared.Contracts;
using Microservice.Client.Shared.Results;

namespace Microservice.Client.Features.Customers.Services;

/// <summary>Customers gateway — 1:1 with the Products archetype.</summary>
public sealed class CustomersGateway(
    ApiClient api,
    IIndexedDb cache,
    IConnectivity connectivity,
    ISyncQueue syncQueue,
    ApiOptions options) : OfflineGateway(connectivity, syncQueue), ICustomersGateway
{
    private const string EntityType = "customer";
    private string Resource => options.ResourcePath("customersef"); // api/v1/customersef ([controller] = CustomersEF)

    // ── Reads ─────────────────────────────────────────────────────────────────

    public async Task<UiResult<PagedResult<CustomerListItemVm>>> GetPagedAsync(PageRequest request, CancellationToken ct = default)
    {
        var url = $"{Resource}?{request.ToQueryString()}";
        var cacheKey = $"customers:page:{request.Page}:{request.Size}";

        if (IsOnline)
        {
            var result = await api.GetAsync<PagedResult<GetCustomersPaginatedDto>>(url, ct);
            if (result.IsSuccess)
            {
                await cache.PutAsync(Stores.ReadCache, cacheKey, result.Value!);
                return UiResult<PagedResult<CustomerListItemVm>>.Success(MapPage(result.Value!));
            }
            if (result.Error!.Kind is not ErrorKind.Network)
                return UiResult<PagedResult<CustomerListItemVm>>.Failure(result.Error);
        }

        var cached = await cache.GetAsync<PagedResult<GetCustomersPaginatedDto>>(Stores.ReadCache, cacheKey);
        return cached is not null
            ? UiResult<PagedResult<CustomerListItemVm>>.Success(MapPage(cached))
            : UiResult<PagedResult<CustomerListItemVm>>.Failure(UiError.Network());
    }

    public async Task<UiResult<CustomerFormModel>> GetByIdAsync(Guid publicId, CancellationToken ct = default)
    {
        var url = $"{Resource}/{publicId}";
        var cacheKey = $"customers:item:{publicId}";

        if (IsOnline)
        {
            var result = await api.GetAsync<GetCustomerDto>(url, ct);
            if (result.IsSuccess)
            {
                await cache.PutAsync(Stores.ReadCache, cacheKey, result.Value!);
                return UiResult<CustomerFormModel>.Success(CustomerMapper.ToFormModel(result.Value!));
            }
            if (result.Error!.Kind is not ErrorKind.Network)
                return UiResult<CustomerFormModel>.Failure(result.Error);
        }

        var cached = await cache.GetAsync<GetCustomerDto>(Stores.ReadCache, cacheKey);
        return cached is not null
            ? UiResult<CustomerFormModel>.Success(CustomerMapper.ToFormModel(cached))
            : UiResult<CustomerFormModel>.Failure(UiError.Network());
    }

    public async Task<UiResult<CustomerListItemVm>> GetByDocumentAsync(string docNumber, CancellationToken ct = default)
    {
        var url = $"{Resource}/by-document/{Uri.EscapeDataString(docNumber)}";

        if (IsOnline)
        {
            var result = await api.GetAsync<GetCustomerDto>(url, ct);
            if (result.IsSuccess)
            {
                await cache.PutAsync(Stores.ReadCache, $"customers:doc:{docNumber}", result.Value!);
                return UiResult<CustomerListItemVm>.Success(CustomerMapper.ToListItem(result.Value!));
            }
            if (result.Error!.Kind is not ErrorKind.Network)
                return UiResult<CustomerListItemVm>.Failure(result.Error);
        }

        var cached = await cache.GetAsync<GetCustomerDto>(Stores.ReadCache, $"customers:doc:{docNumber}");
        return cached is not null
            ? UiResult<CustomerListItemVm>.Success(CustomerMapper.ToListItem(cached))
            : UiResult<CustomerListItemVm>.Failure(UiError.Network());
    }

    // ── Writes ────────────────────────────────────────────────────────────────

    public Task<UiResult<CommandAck>> CreateAsync(CustomerFormModel model, CancellationToken ct = default)
    {
        var body = CustomerMapper.ToCreateRequest(model);
        var op = NewOperation("POST", Resource, body, EntityType);
        return SubmitAsync(op, () => api.PostAsync<Guid>(Resource, body, op.IdempotencyKey, ct));
    }

    public Task<UiResult<CommandAck>> UpdateAsync(CustomerFormModel model, CancellationToken ct = default)
    {
        var url = $"{Resource}/{model.PublicId}";
        var body = CustomerMapper.ToUpdateRequest(model);
        var op = NewOperation("PUT", url, body, EntityType);
        return SubmitAsync(op, () => api.PutAsync<Guid>(url, body, op.IdempotencyKey, ct));
    }

    public Task<UiResult<CommandAck>> DeleteAsync(Guid publicId, CancellationToken ct = default)
    {
        var url = $"{Resource}/{publicId}";
        var op = NewOperation("DELETE", url, body: null, EntityType);
        return SubmitAsync(op, () => api.DeleteAsync<Guid>(url, op.IdempotencyKey, ct));
    }

    private static PagedResult<CustomerListItemVm> MapPage(PagedResult<GetCustomersPaginatedDto> dto) => new()
    {
        Results = dto.Results.Select(CustomerMapper.ToListItem).ToList(),
        RowsCount = dto.RowsCount,
        PageCount = dto.PageCount,
        PageSize = dto.PageSize,
        CurrentPage = dto.CurrentPage
    };
}

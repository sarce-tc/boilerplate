using Microservice.Client.Features.Products.Models;
using Microservice.Client.Features.Products.Services;
using Microservice.Client.Shared.Contracts;
using Microservice.Client.Shared.Results;
using Microservice.Client.Shared.State;

namespace Microservice.Client.Features.Products.State;

/// <summary>
/// Observable state for the catalog list screen. Smart pages subscribe to <c>Changed</c> and
/// call <c>StateHasChanged</c>; all reads happen through the gateway. Pure orchestration —
/// no HTTP, no JS — so it is unit-testable with a mocked gateway.
/// </summary>
public sealed class ProductsState(IProductsGateway gateway) : ObservableState
{
    private IReadOnlyList<ProductListItemVm> _items = [];
    private PageRequest _page = new(1, 20);
    private int _rowsCount;
    private int _pageCount;
    private bool _isLoading;
    private UiError? _error;

    public IReadOnlyList<ProductListItemVm> Items => _items;
    public PageRequest Page => _page;
    public int RowsCount => _rowsCount;
    public int PageCount => _pageCount;
    public bool IsLoading => _isLoading;
    public UiError? Error => _error;
    public bool HasError => _error is not null;

    public async Task LoadAsync(CancellationToken ct = default)
    {
        Set(ref _isLoading, true);
        _error = null;

        var result = await gateway.GetPagedAsync(_page, ct);
        result.Match(
            page =>
            {
                _items = page.Results;
                _rowsCount = page.RowsCount;
                _pageCount = page.PageCount;
                return true;
            },
            err => { _error = err; return false; });

        Set(ref _isLoading, false); // notifies subscribers
    }

    public Task GoToPageAsync(int page, CancellationToken ct = default)
    {
        if (page < 1 || (_pageCount > 0 && page > _pageCount))
            return Task.CompletedTask;
        _page = _page with { Page = page };
        return LoadAsync(ct);
    }

    /// <summary>Optimistically drop a row after a confirmed delete; reload to stay authoritative.</summary>
    public Task RemoveAndReloadAsync(Guid publicId, CancellationToken ct = default)
    {
        _items = _items.Where(i => i.PublicId != publicId).ToList();
        NotifyChanged();
        return LoadAsync(ct);
    }
}

using Microservice.Client.Features.Customers.Models;
using Microservice.Client.Features.Customers.Services;
using Microservice.Client.Shared.Contracts;
using Microservice.Client.Shared.Results;
using Microservice.Client.Shared.State;

namespace Microservice.Client.Features.Customers.State;

/// <summary>Observable state for the customers list. Mirrors ProductsState (the archetype).</summary>
public sealed class CustomersState(ICustomersGateway gateway) : ObservableState
{
    private IReadOnlyList<CustomerListItemVm> _items = [];
    private PageRequest _page = new(1, 20);
    private int _rowsCount;
    private int _pageCount;
    private bool _isLoading;
    private UiError? _error;

    public IReadOnlyList<CustomerListItemVm> Items => _items;
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
            page => { _items = page.Results; _rowsCount = page.RowsCount; _pageCount = page.PageCount; return true; },
            err => { _error = err; return false; });

        Set(ref _isLoading, false);
    }

    public Task GoToPageAsync(int page, CancellationToken ct = default)
    {
        if (page < 1 || (_pageCount > 0 && page > _pageCount))
            return Task.CompletedTask;
        _page = _page with { Page = page };
        return LoadAsync(ct);
    }

    public Task RemoveAndReloadAsync(Guid publicId, CancellationToken ct = default)
    {
        _items = _items.Where(i => i.PublicId != publicId).ToList();
        NotifyChanged();
        return LoadAsync(ct);
    }
}

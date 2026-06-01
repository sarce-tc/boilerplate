using Microservice.Client.Features.Inventory.Models;
using Microservice.Client.Features.Inventory.Services;
using Microservice.Client.Features.Products.Services;
using Microservice.Client.Shared.Results;
using Microservice.Client.Shared.State;
using Microservice.Client.Shared.Contracts;

namespace Microservice.Client.Features.Inventory.State;

/// <summary>
/// Observable state for the stock overview. Loads the paginated balances and enriches each row
/// with the product name/SKU (resolved through the Products gateway, which caches them — so the
/// list shows real product names instead of raw ids, and stays usable offline).
/// </summary>
public sealed class InventoryState(IInventoryGateway inventory, IProductsGateway products) : ObservableState
{
    private IReadOnlyList<StockItemVm> _items = [];
    private PageRequest _page = new(1, 20);
    private int _pageCount;
    private bool _isLoading;
    private UiError? _error;

    public IReadOnlyList<StockItemVm> Items => _items;
    public PageRequest Page => _page;
    public int PageCount => _pageCount;
    public bool IsLoading => _isLoading;
    public UiError? Error => _error;
    public bool HasError => _error is not null;

    public async Task LoadAsync(CancellationToken ct = default)
    {
        Set(ref _isLoading, true);
        _error = null;

        var result = await inventory.GetStockPagedAsync(_page, ct);
        if (result.IsFailure)
        {
            _error = result.Error;
            Set(ref _isLoading, false);
            return;
        }

        var page = result.Value!;
        _items = await EnrichAsync(page.Results, ct);
        _pageCount = page.PageCount;
        Set(ref _isLoading, false);
    }

    public Task GoToPageAsync(int page, CancellationToken ct = default)
    {
        if (page < 1 || (_pageCount > 0 && page > _pageCount))
            return Task.CompletedTask;
        _page = _page with { Page = page };
        return LoadAsync(ct);
    }

    // Resolve product names for the visible page in parallel (bounded to page size, products cached).
    private async Task<IReadOnlyList<StockItemVm>> EnrichAsync(IReadOnlyList<StockItemVm> rows, CancellationToken ct)
    {
        var tasks = rows.Select(async row =>
        {
            var product = await products.GetByIdAsync(row.ProductPublicId, ct);
            return product.IsSuccess
                ? row with { ProductName = product.Value!.Name, Sku = product.Value.Sku }
                : row;
        });
        return await Task.WhenAll(tasks);
    }
}

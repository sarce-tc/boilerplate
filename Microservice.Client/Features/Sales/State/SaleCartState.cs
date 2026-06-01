using Microservice.Client.Features.Products.Models;
using Microservice.Client.Features.Sales.Models;
using Microservice.Client.Shared.State;

namespace Microservice.Client.Features.Sales.State;

/// <summary>
/// The in-progress sale cart — the most critical, auditable observable state in the POS.
/// Pure in-memory aggregate with NO HTTP/JS: every mutation goes through a method and raises
/// <c>Changed</c>, so it is fully unit-testable. Totals are indicative; the server recomputes
/// them authoritatively when the sale is created.
/// </summary>
public sealed class SaleCartState : ObservableState
{
    private readonly List<CartLineVm> _lines = [];

    public IReadOnlyList<CartLineVm> Lines => _lines;
    public Guid? CustomerPublicId { get; private set; }

    public bool IsEmpty => _lines.Count == 0;
    public int LineCount => _lines.Count;
    public decimal ItemCount => _lines.Sum(l => l.Quantity);
    public decimal Subtotal => _lines.Sum(l => l.LineNet);
    public decimal TaxAmount => _lines.Sum(l => l.LineTax);
    public decimal Total => _lines.Sum(l => l.LineTotal);

    /// <summary>Add a scanned/selected product. Merges into the existing line by product id.</summary>
    public void AddProduct(ProductListItemVm product, decimal quantity = 1)
    {
        if (quantity <= 0)
            return;

        var existing = _lines.FirstOrDefault(l => l.ProductPublicId == product.PublicId);
        if (existing is not null)
            existing.Quantity += quantity;
        else
            _lines.Add(new CartLineVm(product.PublicId, product.Sku, product.Name, product.Price, product.TaxRate)
            {
                Quantity = quantity
            });

        NotifyChanged();
    }

    public void SetQuantity(Guid productPublicId, decimal quantity)
    {
        var line = _lines.FirstOrDefault(l => l.ProductPublicId == productPublicId);
        if (line is null)
            return;

        if (quantity <= 0)
            _lines.Remove(line);
        else
            line.Quantity = quantity;

        NotifyChanged();
    }

    public void Remove(Guid productPublicId)
    {
        if (_lines.RemoveAll(l => l.ProductPublicId == productPublicId) > 0)
            NotifyChanged();
    }

    public void SetCustomer(Guid? customerPublicId)
    {
        CustomerPublicId = customerPublicId;
        NotifyChanged();
    }

    public void Clear()
    {
        _lines.Clear();
        CustomerPublicId = null;
        NotifyChanged();
    }

    /// <summary>Project the cart into the POST /sales body for the given open cash session.</summary>
    public CreateSaleRequest ToCreateRequest(Guid cashSessionPublicId) => new(
        cashSessionPublicId,
        _lines.Select(l => new CreateSaleItemRequest(l.ProductPublicId, l.Quantity)).ToList(),
        CustomerPublicId);
}

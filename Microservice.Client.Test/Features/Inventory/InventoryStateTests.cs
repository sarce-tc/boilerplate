using FluentAssertions;
using Microservice.Client.Features.Inventory.Models;
using Microservice.Client.Features.Inventory.Services;
using Microservice.Client.Features.Inventory.State;
using Microservice.Client.Features.Products.Models;
using Microservice.Client.Features.Products.Services;
using Microservice.Client.Shared.Contracts;
using Microservice.Client.Shared.Results;
using Moq;
using Xunit;

namespace Microservice.Client.Test.Features.Inventory;

public class InventoryStateTests
{
    private readonly Mock<IInventoryGateway> _inventory = new();
    private readonly Mock<IProductsGateway> _products = new();

    private static PagedResult<StockItemVm> Page(params StockItemVm[] items) => new()
    {
        Results = items,
        RowsCount = items.Length,
        PageCount = 1,
        PageSize = 20,
        CurrentPage = 1
    };

    [Fact]
    public async Task LoadAsync_enriches_rows_with_product_name_and_sku()
    {
        var productId = Guid.NewGuid();
        _inventory.Setup(g => g.GetStockPagedAsync(It.IsAny<PageRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(UiResult<PagedResult<StockItemVm>>.Success(Page(new StockItemVm(productId, 12m))));
        _products.Setup(p => p.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(UiResult<ProductFormModel>.Success(new ProductFormModel { Name = "Café", Sku = "CAF-1" }));
        var state = new InventoryState(_inventory.Object, _products.Object);

        await state.LoadAsync();

        state.Items.Should().ContainSingle();
        state.Items[0].ProductName.Should().Be("Café");
        state.Items[0].Sku.Should().Be("CAF-1");
        state.Items[0].QuantityOnHand.Should().Be(12m);
    }

    [Fact]
    public async Task LoadAsync_keeps_row_when_product_lookup_fails()
    {
        var productId = Guid.NewGuid();
        _inventory.Setup(g => g.GetStockPagedAsync(It.IsAny<PageRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(UiResult<PagedResult<StockItemVm>>.Success(Page(new StockItemVm(productId, 4m))));
        _products.Setup(p => p.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(UiError.Network());
        var state = new InventoryState(_inventory.Object, _products.Object);

        await state.LoadAsync();

        state.Items.Should().ContainSingle();
        state.Items[0].ProductName.Should().BeNull();
        state.HasError.Should().BeFalse();
    }

    [Fact]
    public async Task LoadAsync_captures_stock_error()
    {
        _inventory.Setup(g => g.GetStockPagedAsync(It.IsAny<PageRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(UiError.Network());
        var state = new InventoryState(_inventory.Object, _products.Object);

        await state.LoadAsync();

        state.HasError.Should().BeTrue();
    }
}

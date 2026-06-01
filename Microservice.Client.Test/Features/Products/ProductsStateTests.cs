using FluentAssertions;
using Microservice.Client.Features.Products.Models;
using Microservice.Client.Features.Products.Services;
using Microservice.Client.Features.Products.State;
using Microservice.Client.Shared.Contracts;
using Microservice.Client.Shared.Results;
using Moq;
using Xunit;

namespace Microservice.Client.Test.Features.Products;

public class ProductsStateTests
{
    private readonly Mock<IProductsGateway> _gateway = new();

    private static PagedResult<ProductListItemVm> Page(params ProductListItemVm[] items) => new()
    {
        Results = items,
        RowsCount = items.Length,
        PageCount = 1,
        PageSize = 20,
        CurrentPage = 1
    };

    private static ProductListItemVm Item(string sku) =>
        new(Guid.NewGuid(), sku, sku, 10m, 21m, null, true);

    [Fact]
    public async Task LoadAsync_populates_items_and_clears_loading()
    {
        _gateway.Setup(g => g.GetPagedAsync(It.IsAny<PageRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(UiResult<PagedResult<ProductListItemVm>>.Success(Page(Item("A"), Item("B"))));
        var state = new ProductsState(_gateway.Object);

        await state.LoadAsync();

        state.Items.Should().HaveCount(2);
        state.IsLoading.Should().BeFalse();
        state.HasError.Should().BeFalse();
    }

    [Fact]
    public async Task LoadAsync_captures_error_and_raises_Changed()
    {
        _gateway.Setup(g => g.GetPagedAsync(It.IsAny<PageRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(UiError.Network());
        var state = new ProductsState(_gateway.Object);
        var notified = false;
        state.Changed += () => notified = true;

        await state.LoadAsync();

        state.HasError.Should().BeTrue();
        state.Error!.Kind.Should().Be(ErrorKind.Network);
        notified.Should().BeTrue();
    }

    [Fact]
    public async Task GoToPageAsync_ignores_out_of_range_pages()
    {
        _gateway.Setup(g => g.GetPagedAsync(It.IsAny<PageRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(UiResult<PagedResult<ProductListItemVm>>.Success(Page(Item("A"))));
        var state = new ProductsState(_gateway.Object);
        await state.LoadAsync(); // PageCount = 1

        await state.GoToPageAsync(5);

        state.Page.Page.Should().Be(1);
    }
}

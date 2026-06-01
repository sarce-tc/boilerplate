using Bunit;
using FluentAssertions;
using Microservice.Client.Features.Products.Components;
using Microservice.Client.Features.Products.Models;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using Xunit;

namespace Microservice.Client.Test.Features.Products;

public class ProductTableTests : TestContext
{
    public ProductTableTests()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose; // MudBlazor interop returns defaults
    }

    private static ProductListItemVm Item(string sku, string name) =>
        new(Guid.NewGuid(), sku, name, 100m, 21m, "Bebidas", true);

    [Fact]
    public void Renders_a_row_per_item()
    {
        var items = new[] { Item("S1", "Café"), Item("S2", "Té") };

        var cut = RenderComponent<ProductTable>(p => p
            .Add(x => x.Items, items)
            .Add(x => x.PageCount, 1));

        cut.Markup.Should().Contain("Café");
        cut.Markup.Should().Contain("Té");
    }

    [Fact]
    public void Edit_button_raises_OnEdit_with_the_row()
    {
        ProductListItemVm? edited = null;
        var item = Item("S1", "Café");

        var cut = RenderComponent<ProductTable>(p => p
            .Add(x => x.Items, new[] { item })
            .Add(x => x.OnEdit, vm => edited = vm));

        cut.Find("button[aria-label=Editar]").Click();

        edited.Should().Be(item);
    }
}

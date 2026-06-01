using FluentAssertions;
using Microservice.Client.Features.Products.Models;
using Microservice.Client.Features.Sales.State;
using Xunit;

namespace Microservice.Client.Test.Features.Sales;

public class SaleCartStateTests
{
    private static ProductListItemVm Product(string sku = "S1", decimal price = 100m, decimal tax = 21m) =>
        new(Guid.NewGuid(), sku, sku, price, tax, null, true);

    [Fact]
    public void AddProduct_creates_line_and_computes_totals()
    {
        var cart = new SaleCartState();
        var p = Product(price: 100m, tax: 21m);

        cart.AddProduct(p, 2);

        cart.LineCount.Should().Be(1);
        cart.Subtotal.Should().Be(200m);
        cart.TaxAmount.Should().Be(42m);
        cart.Total.Should().Be(242m);
    }

    [Fact]
    public void AddProduct_merges_quantity_for_same_product()
    {
        var cart = new SaleCartState();
        var p = Product();

        cart.AddProduct(p);
        cart.AddProduct(p, 3);

        cart.LineCount.Should().Be(1);
        cart.Lines[0].Quantity.Should().Be(4);
    }

    [Fact]
    public void SetQuantity_to_zero_removes_the_line()
    {
        var cart = new SaleCartState();
        var p = Product();
        cart.AddProduct(p);

        cart.SetQuantity(p.PublicId, 0);

        cart.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void Mutations_raise_Changed()
    {
        var cart = new SaleCartState();
        var count = 0;
        cart.Changed += () => count++;

        cart.AddProduct(Product());
        cart.Clear();

        count.Should().Be(2);
    }

    [Fact]
    public void ToCreateRequest_projects_lines_and_customer()
    {
        var cart = new SaleCartState();
        var p = Product();
        cart.AddProduct(p, 5);
        var customer = Guid.NewGuid();
        cart.SetCustomer(customer);
        var session = Guid.NewGuid();

        var request = cart.ToCreateRequest(session);

        request.CashSessionPublicId.Should().Be(session);
        request.CustomerPublicId.Should().Be(customer);
        request.Items.Should().ContainSingle();
        request.Items[0].ProductPublicId.Should().Be(p.PublicId);
        request.Items[0].Quantity.Should().Be(5);
    }
}

using FluentAssertions;
using Microservice.Domain.Entities;
using Microservice.Domain.Exceptions;

namespace Microservice.Test.DomainTests.Entities;

public class ProductTests
{
    private static Product NewProduct() =>
        new("SKU-1", "Coca Cola 500ml", "Gaseosa", price: 100m, cost: 60m, taxRate: 21m, categoryName: "Bebidas");

    // ── Factory ────────────────────────────────────────────────────────────────

    [Fact]
    public void Ctor_WithValidData_ShouldCreateActiveProductWithPublicId()
    {
        var product = NewProduct();

        product.IsActive.Should().BeTrue();
        product.Sku.Should().Be("SKU-1");
        product.Price.Should().Be(100m);
        product.TaxRate.Should().Be(21m);
        product.PublicId.Should().NotBe(Guid.Empty);
        product.Barcodes.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Ctor_WithBlankSku_ShouldThrowArgumentException(string sku)
    {
        var act = () => new Product(sku, "Name", null, 1m, 1m, 21m, null);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Ctor_WithBlankName_ShouldThrowArgumentException()
    {
        var act = () => new Product("SKU", "  ", null, 1m, 1m, 21m, null);

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(-100)]
    public void Ctor_WithNegativePrice_ShouldThrowDomainException(decimal price)
    {
        var act = () => new Product("SKU", "Name", null, price, 1m, 21m, null);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Ctor_WithNegativeCost_ShouldThrowDomainException()
    {
        var act = () => new Product("SKU", "Name", null, 1m, -1m, 21m, null);

        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(100.01)]
    [InlineData(150)]
    public void Ctor_WithTaxRateOutOfRange_ShouldThrowDomainException(decimal taxRate)
    {
        var act = () => new Product("SKU", "Name", null, 1m, 1m, taxRate, null);

        act.Should().Throw<DomainException>();
    }

    // ── Pricing / details ───────────────────────────────────────────────────────

    [Fact]
    public void UpdatePricing_WithValidValues_ShouldUpdate()
    {
        var product = NewProduct();

        product.UpdatePricing(200m, 120m, 10.5m);

        product.Price.Should().Be(200m);
        product.Cost.Should().Be(120m);
        product.TaxRate.Should().Be(10.5m);
    }

    [Fact]
    public void UpdatePricing_OnInactiveProduct_ShouldThrowDomainException()
    {
        var product = NewProduct();
        product.Deactivate();

        var act = () => product.UpdatePricing(1m, 1m, 21m);

        act.Should().Throw<DomainException>();
    }

    // ── Barcodes ─────────────────────────────────────────────────────────────────

    [Fact]
    public void AddBarcode_ShouldAppendBarcode()
    {
        var product = NewProduct();

        var barcode = product.AddBarcode("7790895000997", "EAN13");

        product.Barcodes.Should().ContainSingle();
        barcode.Code.Should().Be("7790895000997");
        barcode.Symbology.Should().Be("EAN13");
    }

    [Fact]
    public void AddBarcode_WithDuplicateCode_ShouldThrowDomainException()
    {
        var product = NewProduct();
        product.AddBarcode("123", null);

        var act = () => product.AddBarcode("123", null);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void AddBarcode_WithDuplicateCodeCaseInsensitive_ShouldThrowDomainException()
    {
        var product = NewProduct();
        product.AddBarcode("abc", null);

        var act = () => product.AddBarcode("ABC", null);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void RemoveBarcode_WithUnknownId_ShouldThrowDomainException()
    {
        var product = NewProduct();

        var act = () => product.RemoveBarcode(Guid.NewGuid());

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void RemoveBarcode_WithExistingId_ShouldRemove()
    {
        var product = NewProduct();
        var barcode = product.AddBarcode("123", null);

        product.RemoveBarcode(barcode.PublicId);

        product.Barcodes.Should().BeEmpty();
    }

    // ── Activation ───────────────────────────────────────────────────────────────

    [Fact]
    public void Deactivate_WhenAlreadyInactive_ShouldThrowDomainException()
    {
        var product = NewProduct();
        product.Deactivate();

        var act = () => product.Deactivate();

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Activate_WhenAlreadyActive_ShouldThrowDomainException()
    {
        var product = NewProduct();

        var act = () => product.Activate();

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Activate_AfterDeactivate_ShouldReactivate()
    {
        var product = NewProduct();
        product.Deactivate();

        product.Activate();

        product.IsActive.Should().BeTrue();
    }
}

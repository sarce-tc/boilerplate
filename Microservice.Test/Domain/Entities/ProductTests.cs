using FluentAssertions;
using Microservice.Domain.Entities;

namespace Microservice.Test.Domain.Entities;

public class ProductTests
{
    // ── Factory method: happy path ────────────────────────────────────────────

    [Fact]
    public void Create_WithValidNameAndPrice_ShouldReturnProduct()
    {
        var product = Product.Create("Widget", 9.99m);

        product.Should().NotBeNull();
        product.Name.Should().Be("Widget");
        product.Price.Should().Be(9.99m);
    }

    [Fact]
    public void Create_ShouldAssignNonEmptyPublicId()
    {
        var product = Product.Create("Widget", 1m);
        product.PublicId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Create_CalledTwice_ShouldProduceDistinctPublicIds()
    {
        var p1 = Product.Create("Widget", 10m);
        var p2 = Product.Create("Gadget", 20m);

        p1.PublicId.Should().NotBe(p2.PublicId);
    }

    // ── Name validation ───────────────────────────────────────────────────────

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyOrWhitespaceName_ShouldThrow(string name)
    {
        var act = () => Product.Create(name, 10m);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithNullName_ShouldThrow()
    {
        var act = () => Product.Create(null!, 10m);
        act.Should().Throw<ArgumentException>();
    }

    // ── Price validation ──────────────────────────────────────────────────────

    [Theory]
    [InlineData(0)]
    [InlineData(-0.01)]
    [InlineData(-100)]
    public void Create_WithNonPositivePrice_ShouldThrow(decimal price)
    {
        var act = () => Product.Create("Widget", price);
        act.Should().Throw<ArgumentException>()
            .WithMessage("Price must be positive*");
    }

    [Fact]
    public void Create_WithMinimalPositivePrice_ShouldSucceed()
    {
        var product = Product.Create("Cheap", 0.01m);
        product.Price.Should().Be(0.01m);
    }

    // ── Sealed class: cannot be subclassed (compile-time guarantee) ───────────

    [Fact]
    public void Product_ShouldBeSealedType()
    {
        typeof(Product).IsSealed.Should().BeTrue();
    }
}

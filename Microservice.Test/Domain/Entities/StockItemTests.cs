using FluentAssertions;
using Microservice.Domain.Entities;
using Microservice.Domain.Exceptions;

namespace Microservice.Test.DomainTests.Entities;

public class StockItemTests
{
    [Fact]
    public void Ctor_WithDefaults_ShouldStartAtZero()
    {
        var stock = new StockItem(Guid.NewGuid());

        stock.QuantityOnHand.Should().Be(0m);
        stock.PublicId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Ctor_WithEmptyProductId_ShouldThrowArgumentException()
    {
        var act = () => new StockItem(Guid.Empty);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Ctor_WithNegativeInitialQuantity_ShouldThrowDomainException()
    {
        var act = () => new StockItem(Guid.NewGuid(), -1m);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Increase_ShouldAddToBalance()
    {
        var stock = new StockItem(Guid.NewGuid(), 10m);

        stock.Increase(5.5m);

        stock.QuantityOnHand.Should().Be(15.5m);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public void Increase_WithNonPositive_ShouldThrowDomainException(decimal qty)
    {
        var stock = new StockItem(Guid.NewGuid(), 10m);

        var act = () => stock.Increase(qty);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Decrease_WithSufficientStock_ShouldSubtract()
    {
        var stock = new StockItem(Guid.NewGuid(), 10m);

        stock.Decrease(4m);

        stock.QuantityOnHand.Should().Be(6m);
    }

    [Fact]
    public void Decrease_BeyondAvailable_ShouldThrowDomainException()
    {
        var stock = new StockItem(Guid.NewGuid(), 3m);

        var act = () => stock.Decrease(4m);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Decrease_ExactlyToZero_ShouldBeAllowed()
    {
        var stock = new StockItem(Guid.NewGuid(), 3m);

        stock.Decrease(3m);

        stock.QuantityOnHand.Should().Be(0m);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-2)]
    public void Decrease_WithNonPositive_ShouldThrowDomainException(decimal qty)
    {
        var stock = new StockItem(Guid.NewGuid(), 10m);

        var act = () => stock.Decrease(qty);

        act.Should().Throw<DomainException>();
    }
}

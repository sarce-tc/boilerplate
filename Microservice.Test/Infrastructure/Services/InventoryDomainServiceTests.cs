using FluentAssertions;
using Microservice.Application.Contracts.Interfaces;
using Microservice.Domain.Entities;
using Microservice.Domain.Exceptions;
using Microservice.Infrastructure.Services;

namespace Microservice.Test.Infrastructure.Services;

public class InventoryDomainServiceTests
{
    private readonly InventoryDomainService _service = new();

    [Fact]
    public void RegisterMovement_WithIncreaseType_ShouldRaiseStockAndReturnMovement()
    {
        var stock = new StockItem(Guid.NewGuid(), 10m);

        var movement = _service.RegisterMovement(stock, InventoryMovementType.Purchase, 5m, "compra", "ref-1");

        stock.QuantityOnHand.Should().Be(15m);
        movement.MovementType.Should().Be(InventoryMovementType.Purchase);
        movement.Quantity.Should().Be(5m);
        movement.BalanceAfter.Should().Be(15m);
        movement.ProductPublicId.Should().Be(stock.ProductPublicId);
        movement.Reference.Should().Be("ref-1");
    }

    [Fact]
    public void RegisterMovement_WithSaleType_ShouldLowerStock()
    {
        var stock = new StockItem(Guid.NewGuid(), 10m);

        var movement = _service.RegisterMovement(stock, InventoryMovementType.Sale, 4m, null, null);

        stock.QuantityOnHand.Should().Be(6m);
        movement.BalanceAfter.Should().Be(6m);
    }

    [Fact]
    public void RegisterMovement_SaleBeyondStock_ShouldThrowDomainException()
    {
        var stock = new StockItem(Guid.NewGuid(), 2m);

        var act = () => _service.RegisterMovement(stock, InventoryMovementType.Sale, 5m, null, null);

        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(InventoryMovementType.Purchase, true)]
    [InlineData(InventoryMovementType.Return, true)]
    [InlineData(InventoryMovementType.AdjustmentIn, true)]
    [InlineData(InventoryMovementType.InitialLoad, true)]
    [InlineData(InventoryMovementType.Sale, false)]
    [InlineData(InventoryMovementType.AdjustmentOut, false)]
    [InlineData(InventoryMovementType.Loss, false)]
    public void IsIncrease_ShouldClassifyMovementTypes(InventoryMovementType type, bool expected)
    {
        IInventoryDomainService.IsIncrease(type).Should().Be(expected);
    }
}

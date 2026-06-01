using FluentAssertions;
using Microservice.Client.Features.Inventory.Models;
using Xunit;

namespace Microservice.Client.Test.Features.Inventory;

public class InventoryModelsTests
{
    [Theory]
    [InlineData(InventoryMovementType.Purchase, true)]
    [InlineData(InventoryMovementType.Return, true)]
    [InlineData(InventoryMovementType.AdjustmentIn, true)]
    [InlineData(InventoryMovementType.InitialLoad, true)]
    [InlineData(InventoryMovementType.Sale, false)]
    [InlineData(InventoryMovementType.AdjustmentOut, false)]
    [InlineData(InventoryMovementType.Loss, false)]
    public void IsInbound_classifies_direction(InventoryMovementType type, bool inbound) =>
        InventoryLabels.IsInbound(type).Should().Be(inbound);

    [Fact]
    public void SignedQuantity_is_negative_for_outbound_movements()
    {
        var outbound = new InventoryMovementVm(Guid.NewGuid(), InventoryMovementType.Sale, 5m, 10m, null, null, DateTimeOffset.UtcNow);
        var inbound = new InventoryMovementVm(Guid.NewGuid(), InventoryMovementType.Purchase, 5m, 20m, null, null, DateTimeOffset.UtcNow);

        outbound.SignedQuantity.Should().Be(-5m);
        inbound.SignedQuantity.Should().Be(5m);
    }

    [Fact]
    public void StockItem_flags_out_of_stock()
    {
        new StockItemVm(Guid.NewGuid(), 0m).IsOutOfStock.Should().BeTrue();
        new StockItemVm(Guid.NewGuid(), 3m).IsOutOfStock.Should().BeFalse();
    }

    [Fact]
    public void ToRequest_builds_command_and_nulls_empty_optionals()
    {
        var pid = Guid.NewGuid();
        var model = new MovementFormModel
        {
            ProductPublicId = pid,
            MovementType = InventoryMovementType.AdjustmentIn,
            Quantity = 7m,
            Reason = "  conteo  ",
            Reference = ""
        };

        var request = model.ToRequest();

        request.ProductPublicId.Should().Be(pid);
        request.Quantity.Should().Be(7m);
        request.Reason.Should().Be("conteo");
        request.Reference.Should().BeNull();
    }
}

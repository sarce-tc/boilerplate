using FluentAssertions;
using Microservice.Domain.Entities;
using Microservice.Domain.Exceptions;
using Microservice.Infrastructure.Services;

namespace Microservice.Test.Infrastructure.Services;

public class SaleDomainServiceTests
{
    private readonly SaleDomainService _service = new(new InventoryDomainService());

    private static (Sale sale, Guid productA, Guid productB) BuildSale()
    {
        var productA = Guid.NewGuid();
        var productB = Guid.NewGuid();
        var sale = new Sale(null, Guid.NewGuid());
        sale.AddItem(productA, "A", 2m, 100m, 21m); // total 242
        sale.AddItem(productB, "B", 1m, 50m, 0m);   // total 50  → sale total 292
        return (sale, productA, productB);
    }

    [Fact]
    public void Confirm_ShouldDecrementStock_RegisterCash_AndConfirmSale()
    {
        var (sale, productA, productB) = BuildSale();
        var cash = new CashSession("Caja 1", 0m, "c1");
        var stockByProduct = new Dictionary<Guid, StockItem>
        {
            [productA] = new(productA, 10m),
            [productB] = new(productB, 10m),
        };

        var movements = _service.Confirm(sale, cash, stockByProduct);

        // Stock descontado por cada ítem
        stockByProduct[productA].QuantityOnHand.Should().Be(8m);
        stockByProduct[productB].QuantityOnHand.Should().Be(9m);

        // Un asiento de inventario por ítem
        movements.Should().HaveCount(2);
        movements.Should().OnlyContain(m => m.MovementType == InventoryMovementType.Sale);

        // Cobro en caja por el total de la venta
        cash.Movements.Should().ContainSingle();
        cash.Movements[0].MovementType.Should().Be(CashMovementType.Sale);
        cash.Movements[0].Amount.Should().Be(sale.Total);
        cash.Movements[0].Amount.Should().Be(292m);

        // Venta confirmada
        sale.Status.Should().Be(SaleStatus.Confirmed);
    }

    [Fact]
    public void Confirm_WithMissingStockRecord_ShouldThrowDomainException()
    {
        var (sale, productA, _) = BuildSale();
        var cash = new CashSession("Caja 1", 0m, null);
        // Falta el StockItem de productB
        var stockByProduct = new Dictionary<Guid, StockItem>
        {
            [productA] = new(productA, 10m),
        };

        var act = () => _service.Confirm(sale, cash, stockByProduct);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Confirm_WithInsufficientStock_ShouldThrowDomainException()
    {
        var (sale, productA, productB) = BuildSale();
        var cash = new CashSession("Caja 1", 0m, null);
        var stockByProduct = new Dictionary<Guid, StockItem>
        {
            [productA] = new(productA, 1m), // necesita 2
            [productB] = new(productB, 10m),
        };

        var act = () => _service.Confirm(sale, cash, stockByProduct);

        act.Should().Throw<DomainException>();
    }
}

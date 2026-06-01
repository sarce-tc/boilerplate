using FluentAssertions;
using Microservice.Domain.Entities;
using Microservice.Domain.Exceptions;

namespace Microservice.Test.DomainTests.Entities;

public class SaleTests
{
    private static Sale NewSale() => new(customerPublicId: null, cashSessionPublicId: Guid.NewGuid());

    [Fact]
    public void Ctor_WithEmptyCashSession_ShouldThrowArgumentException()
    {
        var act = () => new Sale(null, Guid.Empty);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Ctor_ShouldStartPendingWithZeroTotals()
    {
        var sale = NewSale();

        sale.Status.Should().Be(SaleStatus.Pending);
        sale.Total.Should().Be(0m);
        sale.Items.Should().BeEmpty();
    }

    [Fact]
    public void AddItem_ShouldComputeLineAndAggregateTotals()
    {
        var sale = NewSale();

        // 2 x 100 net, IVA 21% → net 200, tax 42, total 242
        sale.AddItem(Guid.NewGuid(), "Producto", quantity: 2m, unitPrice: 100m, taxRate: 21m);

        sale.Items.Should().ContainSingle();
        var line = sale.Items[0];
        line.LineNet.Should().Be(200m);
        line.LineTax.Should().Be(42m);
        line.LineTotal.Should().Be(242m);

        sale.Subtotal.Should().Be(200m);
        sale.TaxAmount.Should().Be(42m);
        sale.Total.Should().Be(242m);
    }

    [Fact]
    public void AddItem_MultipleLines_ShouldSumTotals()
    {
        var sale = NewSale();
        sale.AddItem(Guid.NewGuid(), "A", 1m, 100m, 21m); // total 121
        sale.AddItem(Guid.NewGuid(), "B", 2m, 50m, 10.5m); // net 100, tax 10.5, total 110.5

        sale.Subtotal.Should().Be(200m);
        sale.TaxAmount.Should().Be(31.5m);
        sale.Total.Should().Be(231.5m);
    }

    [Fact]
    public void AddItem_WithNonPositiveQuantity_ShouldThrowDomainException()
    {
        var sale = NewSale();

        var act = () => sale.AddItem(Guid.NewGuid(), "X", 0m, 100m, 21m);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void RemoveItem_ShouldRecalculateTotals()
    {
        var sale = NewSale();
        var item = sale.AddItem(Guid.NewGuid(), "A", 1m, 100m, 21m);
        sale.AddItem(Guid.NewGuid(), "B", 1m, 100m, 21m);

        sale.RemoveItem(item.PublicId);

        sale.Items.Should().ContainSingle();
        sale.Total.Should().Be(121m);
    }

    [Fact]
    public void Confirm_WithItems_ShouldTransitionToConfirmed()
    {
        var sale = NewSale();
        sale.AddItem(Guid.NewGuid(), "A", 1m, 100m, 21m);

        sale.Confirm();

        sale.Status.Should().Be(SaleStatus.Confirmed);
        sale.ConfirmedAt.Should().NotBeNull();
    }

    [Fact]
    public void Confirm_WithoutItems_ShouldThrowDomainException()
    {
        var sale = NewSale();

        var act = () => sale.Confirm();

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void AddItem_OnConfirmedSale_ShouldThrowDomainException()
    {
        var sale = NewSale();
        sale.AddItem(Guid.NewGuid(), "A", 1m, 100m, 21m);
        sale.Confirm();

        var act = () => sale.AddItem(Guid.NewGuid(), "B", 1m, 100m, 21m);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Cancel_OnPendingSale_ShouldTransitionToCancelled()
    {
        var sale = NewSale();

        sale.Cancel();

        sale.Status.Should().Be(SaleStatus.Cancelled);
    }

    [Fact]
    public void Cancel_OnConfirmedSale_ShouldThrowDomainException()
    {
        var sale = NewSale();
        sale.AddItem(Guid.NewGuid(), "A", 1m, 100m, 21m);
        sale.Confirm();

        var act = () => sale.Cancel();

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void AttachInvoice_OnConfirmedSale_ShouldSetInvoiceId()
    {
        var sale = NewSale();
        sale.AddItem(Guid.NewGuid(), "A", 1m, 100m, 21m);
        sale.Confirm();
        var invoiceId = Guid.NewGuid();

        sale.AttachInvoice(invoiceId);

        sale.InvoicePublicId.Should().Be(invoiceId);
    }

    [Fact]
    public void AttachInvoice_OnPendingSale_ShouldThrowDomainException()
    {
        var sale = NewSale();
        sale.AddItem(Guid.NewGuid(), "A", 1m, 100m, 21m);

        var act = () => sale.AttachInvoice(Guid.NewGuid());

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void AttachInvoice_Twice_ShouldThrowDomainException()
    {
        var sale = NewSale();
        sale.AddItem(Guid.NewGuid(), "A", 1m, 100m, 21m);
        sale.Confirm();
        sale.AttachInvoice(Guid.NewGuid());

        var act = () => sale.AttachInvoice(Guid.NewGuid());

        act.Should().Throw<DomainException>();
    }
}

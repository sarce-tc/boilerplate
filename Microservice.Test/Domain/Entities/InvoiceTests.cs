using FluentAssertions;
using Microservice.Domain.Entities;
using Microservice.Domain.Exceptions;

namespace Microservice.Test.DomainTests.Entities;

public class InvoiceTests
{
    private static Invoice NewInvoice() =>
        new(Guid.NewGuid(), Guid.NewGuid(), InvoiceType.FacturaB, pointOfSale: 1, net: 200m, tax: 42m, total: 242m);

    [Fact]
    public void Ctor_ShouldStartPending()
    {
        var invoice = NewInvoice();

        invoice.Status.Should().Be(InvoiceStatus.Pending);
        invoice.InvoiceNumber.Should().BeNull();
        invoice.Cae.Should().BeNull();
    }

    [Fact]
    public void Ctor_WithEmptySale_ShouldThrowArgumentException()
    {
        var act = () => new Invoice(Guid.Empty, null, InvoiceType.FacturaB, 1, 0m, 0m, 0m);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Ctor_WithNonPositivePointOfSale_ShouldThrowDomainException()
    {
        var act = () => new Invoice(Guid.NewGuid(), null, InvoiceType.FacturaB, 0, 0m, 0m, 0m);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Authorize_ShouldSetCaeNumberAndStatus()
    {
        var invoice = NewInvoice();
        var expiration = DateTimeOffset.UtcNow.AddDays(10);

        invoice.Authorize("70123456789012", expiration, invoiceNumber: 42);

        invoice.Status.Should().Be(InvoiceStatus.Authorized);
        invoice.Cae.Should().Be("70123456789012");
        invoice.InvoiceNumber.Should().Be(42);
        invoice.CaeExpiration.Should().Be(expiration);
        invoice.AuthorizedAt.Should().NotBeNull();
    }

    [Fact]
    public void Authorize_WithBlankCae_ShouldThrowArgumentException()
    {
        var invoice = NewInvoice();

        var act = () => invoice.Authorize("  ", DateTimeOffset.UtcNow, 1);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Reject_ShouldSetStatusAndReason()
    {
        var invoice = NewInvoice();

        invoice.Reject("CUIT inválido");

        invoice.Status.Should().Be(InvoiceStatus.Rejected);
        invoice.RejectionReason.Should().Be("CUIT inválido");
    }

    [Fact]
    public void Authorize_AfterAuthorized_ShouldThrowDomainException()
    {
        var invoice = NewInvoice();
        invoice.Authorize("123", DateTimeOffset.UtcNow, 1);

        var act = () => invoice.Authorize("456", DateTimeOffset.UtcNow, 2);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Reject_AfterAuthorized_ShouldThrowDomainException()
    {
        var invoice = NewInvoice();
        invoice.Authorize("123", DateTimeOffset.UtcNow, 1);

        var act = () => invoice.Reject("late");

        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(TaxCondition.ResponsableInscripto, InvoiceType.FacturaA)]
    [InlineData(TaxCondition.Monotributista, InvoiceType.FacturaB)]
    [InlineData(TaxCondition.ConsumidorFinal, InvoiceType.FacturaB)]
    [InlineData(TaxCondition.Exento, InvoiceType.FacturaB)]
    [InlineData(TaxCondition.NoResponsable, InvoiceType.FacturaB)]
    public void ResolveType_ShouldMapCustomerCondition(TaxCondition condition, InvoiceType expected)
    {
        Invoice.ResolveType(condition).Should().Be(expected);
    }

    [Fact]
    public void ResolveType_WithNoCustomer_ShouldDefaultToFacturaB()
    {
        Invoice.ResolveType(null).Should().Be(InvoiceType.FacturaB);
    }
}

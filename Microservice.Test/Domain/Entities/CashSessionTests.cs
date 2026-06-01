using FluentAssertions;
using Microservice.Domain.Entities;
using Microservice.Domain.Exceptions;

namespace Microservice.Test.DomainTests.Entities;

public class CashSessionTests
{
    private static CashSession OpenSession(decimal opening = 1000m) =>
        new("Caja 1", opening, "cajero1");

    [Fact]
    public void Ctor_ShouldOpenSession()
    {
        var session = OpenSession();

        session.Status.Should().Be(CashSessionStatus.Open);
        session.OpeningBalance.Should().Be(1000m);
        session.Movements.Should().BeEmpty();
    }

    [Fact]
    public void Ctor_WithNegativeOpeningBalance_ShouldThrowDomainException()
    {
        var act = () => new CashSession("Caja", -1m, null);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Ctor_WithBlankRegisterName_ShouldThrowArgumentException()
    {
        var act = () => new CashSession("  ", 0m, null);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void RegisterMovement_ShouldAppendMovement()
    {
        var session = OpenSession();

        session.RegisterMovement(CashMovementType.Sale, 250m, "venta 1");

        session.Movements.Should().ContainSingle();
    }

    [Fact]
    public void RegisterMovement_WithNonPositiveAmount_ShouldThrowDomainException()
    {
        var session = OpenSession();

        var act = () => session.RegisterMovement(CashMovementType.Sale, 0m, null);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void CurrentBalance_ShouldReflectSignedMovements()
    {
        var session = OpenSession(1000m);
        session.RegisterMovement(CashMovementType.Sale, 500m, null);       // +500
        session.RegisterMovement(CashMovementType.Deposit, 200m, null);    // +200
        session.RegisterMovement(CashMovementType.Withdrawal, 300m, null); // -300
        session.RegisterMovement(CashMovementType.Refund, 100m, null);     // -100

        // 1000 + 500 + 200 - 300 - 100 = 1300
        session.CurrentBalance().Should().Be(1300m);
    }

    [Fact]
    public void Close_ShouldComputeExpectedAndDifference()
    {
        var session = OpenSession(1000m);
        session.RegisterMovement(CashMovementType.Sale, 500m, null); // expected = 1500

        session.Close(declaredBalance: 1480m, closedBy: "supervisor");

        session.Status.Should().Be(CashSessionStatus.Closed);
        session.ClosingBalanceExpected.Should().Be(1500m);
        session.ClosingBalanceDeclared.Should().Be(1480m);
        session.Difference.Should().Be(-20m); // faltante
        session.ClosedBy.Should().Be("supervisor");
        session.ClosedAt.Should().NotBeNull();
    }

    [Fact]
    public void Close_WithMatchingCount_ShouldHaveZeroDifference()
    {
        var session = OpenSession(1000m);
        session.RegisterMovement(CashMovementType.Sale, 500m, null);

        session.Close(1500m, null);

        session.Difference.Should().Be(0m);
    }

    [Fact]
    public void Close_Twice_ShouldThrowDomainException()
    {
        var session = OpenSession();
        session.Close(1000m, null);

        var act = () => session.Close(1000m, null);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Close_WithNegativeDeclaredBalance_ShouldThrowDomainException()
    {
        var session = OpenSession();

        var act = () => session.Close(-1m, null);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void RegisterMovement_OnClosedSession_ShouldThrowDomainException()
    {
        var session = OpenSession();
        session.Close(1000m, null);

        var act = () => session.RegisterMovement(CashMovementType.Sale, 10m, null);

        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(CashMovementType.Sale, true)]
    [InlineData(CashMovementType.Deposit, true)]
    [InlineData(CashMovementType.AdjustmentIn, true)]
    [InlineData(CashMovementType.Refund, false)]
    [InlineData(CashMovementType.Withdrawal, false)]
    [InlineData(CashMovementType.AdjustmentOut, false)]
    public void CashMovement_IsCredit_ShouldMatchSign(CashMovementType type, bool expected)
    {
        CashMovement.IsCredit(type).Should().Be(expected);
    }
}

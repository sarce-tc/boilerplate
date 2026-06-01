using FluentAssertions;
using Microservice.Client.Features.CashRegister.Models;
using Xunit;

namespace Microservice.Client.Test.Features.CashRegister;

public class CashMapperTests
{
    [Fact]
    public void ToDetail_computes_current_balance_from_signed_movements()
    {
        var dto = new CashSessionDto(
            Guid.NewGuid(), "Caja 1", CashSessionStatus.Open, OpeningBalance: 1000m,
            OpenedBy: "ana", OpenedAt: DateTimeOffset.UtcNow,
            ClosedBy: null, ClosedAt: null, ClosingBalanceDeclared: null, ClosingBalanceExpected: null, Difference: null,
            Movements:
            [
                new CashMovementDto(Guid.NewGuid(), CashMovementType.Sale, 500m, 500m, "venta", DateTimeOffset.UtcNow),
                new CashMovementDto(Guid.NewGuid(), CashMovementType.Withdrawal, 200m, -200m, "retiro", DateTimeOffset.UtcNow)
            ]);

        var detail = CashMapper.ToDetail(dto);

        detail.IsOpen.Should().BeTrue();
        detail.Movements.Should().HaveCount(2);
        detail.CurrentBalance.Should().Be(1300m); // 1000 + 500 - 200
    }

    [Fact]
    public void ToCloseResult_flags_shortage_when_declared_below_expected()
    {
        var dto = new CashSessionDto(
            Guid.NewGuid(), "Caja 1", CashSessionStatus.Closed, 1000m, "ana", DateTimeOffset.UtcNow,
            ClosedBy: "ana", ClosedAt: DateTimeOffset.UtcNow,
            ClosingBalanceDeclared: 1250m, ClosingBalanceExpected: 1300m, Difference: -50m,
            Movements: []);

        var result = CashMapper.ToCloseResult(dto);

        result.Expected.Should().Be(1300m);
        result.Declared.Should().Be(1250m);
        result.Difference.Should().Be(-50m);
        result.IsShortage.Should().BeTrue();
        result.IsBalanced.Should().BeFalse();
    }
}

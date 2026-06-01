using FluentAssertions;
using Microservice.Client.Features.CashRegister.Models;
using Microservice.Client.Features.CashRegister.Services;
using Microservice.Client.Features.CashRegister.State;
using Microservice.Client.Shared.Results;
using Moq;
using Xunit;

namespace Microservice.Client.Test.Features.CashRegister;

public class CashSessionStateTests
{
    private readonly Mock<ICashGateway> _gateway = new();

    private static CashSessionSummaryVm OpenSession(string name) =>
        new(Guid.NewGuid(), name, 0m, DateTimeOffset.UtcNow, true);

    [Fact]
    public async Task LoadActiveAsync_adopts_first_open_session()
    {
        var first = OpenSession("Caja 1");
        _gateway.Setup(g => g.GetOpenSessionsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(UiResult<IReadOnlyList<CashSessionSummaryVm>>.Success([first, OpenSession("Caja 2")]));
        var state = new CashSessionState(_gateway.Object);

        await state.LoadActiveAsync();

        state.HasActiveSession.Should().BeTrue();
        state.Active!.RegisterName.Should().Be("Caja 1");
    }

    [Fact]
    public async Task LoadActiveAsync_with_no_open_sessions_leaves_no_active()
    {
        _gateway.Setup(g => g.GetOpenSessionsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(UiResult<IReadOnlyList<CashSessionSummaryVm>>.Success([]));
        var state = new CashSessionState(_gateway.Object);

        await state.LoadActiveAsync();

        state.HasActiveSession.Should().BeFalse();
    }

    [Fact]
    public async Task LoadActiveAsync_captures_error()
    {
        _gateway.Setup(g => g.GetOpenSessionsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(UiError.Network());
        var state = new CashSessionState(_gateway.Object);

        await state.LoadActiveAsync();

        state.Error!.Kind.Should().Be(ErrorKind.Network);
        state.HasActiveSession.Should().BeFalse();
    }
}

using FluentAssertions;
using Microservice.Client.Features.Customers.Models;
using Microservice.Client.Features.Customers.Services;
using Microservice.Client.Features.Customers.State;
using Microservice.Client.Shared.Contracts;
using Microservice.Client.Shared.Results;
using Moq;
using Xunit;

namespace Microservice.Client.Test.Features.Customers;

public class CustomersStateTests
{
    private readonly Mock<ICustomersGateway> _gateway = new();

    private static PagedResult<CustomerListItemVm> Page(params CustomerListItemVm[] items) => new()
    {
        Results = items,
        RowsCount = items.Length,
        PageCount = 1,
        PageSize = 20,
        CurrentPage = 1
    };

    private static CustomerListItemVm Item(string name) =>
        new(Guid.NewGuid(), name, DocumentType.Dni, "1", TaxCondition.ConsumidorFinal, true);

    [Fact]
    public async Task LoadAsync_populates_items()
    {
        _gateway.Setup(g => g.GetPagedAsync(It.IsAny<PageRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(UiResult<PagedResult<CustomerListItemVm>>.Success(Page(Item("A"), Item("B"))));
        var state = new CustomersState(_gateway.Object);

        await state.LoadAsync();

        state.Items.Should().HaveCount(2);
        state.HasError.Should().BeFalse();
    }

    [Fact]
    public async Task LoadAsync_captures_error()
    {
        _gateway.Setup(g => g.GetPagedAsync(It.IsAny<PageRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(UiError.Network());
        var state = new CustomersState(_gateway.Object);

        await state.LoadAsync();

        state.HasError.Should().BeTrue();
        state.Error!.Kind.Should().Be(ErrorKind.Network);
    }
}

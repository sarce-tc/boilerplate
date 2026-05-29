using FluentAssertions;
using Moq;
using Microservice.Application.Contracts.Persistence.Dapper;
using Microservice.Application.Features.ExamplesDapper.Commands.CreateExampleDapper;
using Microservice.Domain.Entities;
using Microservice.Domain.Exceptions;

using DapperContracts = Microservice.Application.Contracts.Persistence.Dapper;

namespace Microservice.Test.Application.Features.ExamplesDapper.Commands.CreateExampleDapper;

// Cubre el camino DAPPER de crear con hijos (ExampleItem). El handler aplica los
// items con example.AddItem(...) ANTES del TX; el write repo los persiste en AddAsync.
public class CreateExampleDapperWithItemsTests
{
    private readonly Mock<DapperContracts.IUnitOfWork> _mockUnitOfWork = new();
    private readonly Mock<IExampleWriteRepository>     _mockExamplesWrite = new();
    private readonly CreateExampleDapperCommandHandler _handler;

    public CreateExampleDapperWithItemsTests()
    {
        _mockUnitOfWork.Setup(u => u.ExamplesWrite).Returns(_mockExamplesWrite.Object);
        _mockUnitOfWork.Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(u => u.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(u => u.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockExamplesWrite
            .Setup(r => r.AddAsync(It.IsAny<Example>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Example e, CancellationToken _) => e);

        _handler = new CreateExampleDapperCommandHandler(_mockUnitOfWork.Object);
    }

    [Fact]
    public async Task Handle_WithItems_ShouldPassExampleWithItemsToAddAsync()
    {
        var command = new CreateExampleDapperCommand("Demo", null, new List<CreateExampleItemDapperRequest>
        {
            new("Dapper item A", 2),
            new("Dapper item B", 5),
        });

        Example? captured = null;
        _mockExamplesWrite
            .Setup(r => r.AddAsync(It.IsAny<Example>(), It.IsAny<CancellationToken>()))
            .Callback<Example, CancellationToken>((e, _) => captured = e)
            .ReturnsAsync((Example e, CancellationToken _) => e);

        await _handler.Handle(command, CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.Items.Should().HaveCount(2);
        captured.Items[0].Label.Should().Be("Dapper item A");
        captured.Items[0].Quantity.Should().Be(2);
        captured.Items[1].Label.Should().Be("Dapper item B");
    }

    [Fact]
    public async Task Handle_WithNullItems_ShouldAddExampleWithNoItems()
    {
        var command = new CreateExampleDapperCommand("Demo", null, Items: null);

        Example? captured = null;
        _mockExamplesWrite
            .Setup(r => r.AddAsync(It.IsAny<Example>(), It.IsAny<CancellationToken>()))
            .Callback<Example, CancellationToken>((e, _) => captured = e)
            .ReturnsAsync((Example e, CancellationToken _) => e);

        await _handler.Handle(command, CancellationToken.None);

        captured!.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithDuplicateItemLabels_ShouldThrowDomainException()
    {
        var command = new CreateExampleDapperCommand("Demo", null, new List<CreateExampleItemDapperRequest>
        {
            new("widget", 1),
            new("WIDGET", 2),
        });

        await Assert.ThrowsAsync<DomainException>(() => _handler.Handle(command, CancellationToken.None));
    }
}

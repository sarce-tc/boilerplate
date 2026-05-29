using FluentAssertions;
using Moq;
using Microservice.Application.Contracts.Persistence.Dapper;
using Microservice.Application.Features.ExamplesDapper.Commands.UpdateExampleDapper;
using Microservice.Domain.Entities;

using DapperContracts = Microservice.Application.Contracts.Persistence.Dapper;

namespace Microservice.Test.Application.Features.ExamplesDapper.Commands.UpdateExampleDapper;

// Cubre el camino DAPPER de editar con hijos (replace-all). El handler elige:
//   request.Items == null  → UpdateAsync           (no toca los items)
//   request.Items != null  → UpdateWithItemsAsync  (reemplaza el conjunto)
public class UpdateExampleDapperWithItemsTests
{
    private readonly Mock<IExampleReadRepository>      _mockRead = new();
    private readonly Mock<DapperContracts.IUnitOfWork> _mockUnitOfWork = new();
    private readonly Mock<IExampleWriteRepository>     _mockWrite = new();
    private readonly UpdateExampleDapperCommandHandler _handler;

    public UpdateExampleDapperWithItemsTests()
    {
        _mockUnitOfWork.Setup(u => u.ExamplesWrite).Returns(_mockWrite.Object);
        _mockUnitOfWork.Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(u => u.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(u => u.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockWrite.Setup(r => r.UpdateAsync(It.IsAny<Example>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync((Example e, CancellationToken _) => e);
        _mockWrite.Setup(r => r.UpdateWithItemsAsync(It.IsAny<Example>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync((Example e, CancellationToken _) => e);

        _handler = new UpdateExampleDapperCommandHandler(_mockRead.Object, _mockUnitOfWork.Object);
    }

    private void SetupExample(Guid publicId) =>
        _mockRead.Setup(r => r.GetByPublicIdAsync(publicId, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new Example("Original", null) { Id = 1, PublicId = publicId });

    [Fact]
    public async Task Handle_WithItems_ShouldCallUpdateWithItems_NotUpdate()
    {
        var publicId = Guid.NewGuid();
        SetupExample(publicId);
        var command = new UpdateExampleDapperCommand(publicId, "N", null,
            new List<UpdateExampleItemDapperRequest> { new("Item reemplazado", 9) });

        Example? captured = null;
        _mockWrite.Setup(r => r.UpdateWithItemsAsync(It.IsAny<Example>(), It.IsAny<CancellationToken>()))
                  .Callback<Example, CancellationToken>((e, _) => captured = e)
                  .ReturnsAsync((Example e, CancellationToken _) => e);

        await _handler.Handle(command, CancellationToken.None);

        _mockWrite.Verify(r => r.UpdateWithItemsAsync(It.IsAny<Example>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockWrite.Verify(r => r.UpdateAsync(It.IsAny<Example>(), It.IsAny<CancellationToken>()), Times.Never);
        captured!.Items.Should().ContainSingle(i => i.Label == "Item reemplazado" && i.Quantity == 9);
    }

    [Fact]
    public async Task Handle_WithNullItems_ShouldCallUpdate_NotUpdateWithItems()
    {
        var publicId = Guid.NewGuid();
        SetupExample(publicId);
        var command = new UpdateExampleDapperCommand(publicId, "N", null, Items: null);

        await _handler.Handle(command, CancellationToken.None);

        _mockWrite.Verify(r => r.UpdateAsync(It.IsAny<Example>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockWrite.Verify(r => r.UpdateWithItemsAsync(It.IsAny<Example>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithEmptyItems_ShouldCallUpdateWithItems_AndPassNoItems()
    {
        var publicId = Guid.NewGuid();
        SetupExample(publicId);
        var command = new UpdateExampleDapperCommand(publicId, null, null,
            Items: new List<UpdateExampleItemDapperRequest>());

        Example? captured = null;
        _mockWrite.Setup(r => r.UpdateWithItemsAsync(It.IsAny<Example>(), It.IsAny<CancellationToken>()))
                  .Callback<Example, CancellationToken>((e, _) => captured = e)
                  .ReturnsAsync((Example e, CancellationToken _) => e);

        await _handler.Handle(command, CancellationToken.None);

        _mockWrite.Verify(r => r.UpdateWithItemsAsync(It.IsAny<Example>(), It.IsAny<CancellationToken>()), Times.Once);
        captured!.Items.Should().BeEmpty();
    }
}

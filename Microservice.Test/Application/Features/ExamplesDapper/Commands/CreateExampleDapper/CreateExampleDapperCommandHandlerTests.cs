using FluentAssertions;
using Moq;
using Microservice.Application.Contracts.Persistence.Dapper;
using Microservice.Application.Features.ExamplesDapper.Commands.CreateExampleDapper;
using Microservice.Domain.Entities;

using DapperContracts = Microservice.Application.Contracts.Persistence.Dapper;

namespace Microservice.Test.Application.Features.ExamplesDapper.Commands.CreateExampleDapper;

public class CreateExampleDapperCommandHandlerTests
{
    private readonly Mock<DapperContracts.IUnitOfWork>         _mockUnitOfWork;
    private readonly Mock<IExampleWriteRepository>             _mockExamplesWrite;
    private readonly CreateExampleDapperCommandHandler         _handler;

    public CreateExampleDapperCommandHandlerTests()
    {
        _mockUnitOfWork    = new Mock<DapperContracts.IUnitOfWork>();
        _mockExamplesWrite = new Mock<IExampleWriteRepository>();

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
    public async Task Handle_WithValidCommand_ShouldBeginTransactionAndCommit()
    {
        // Arrange
        var command = new CreateExampleDapperCommand("NewExample", "Description");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockUnitOfWork.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCallAddAsync()
    {
        // Arrange
        var command = new CreateExampleDapperCommand("NewExample", null);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockExamplesWrite.Verify(r => r.AddAsync(It.IsAny<Example>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnSuccessWithPublicId()
    {
        // Arrange
        var command = new CreateExampleDapperCommand("NewExample", "Desc");
        Guid capturedPublicId = Guid.Empty;

        _mockExamplesWrite
            .Setup(r => r.AddAsync(It.IsAny<Example>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Example e, CancellationToken _) =>
            {
                capturedPublicId = e.PublicId;
                return e;
            });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(capturedPublicId);
        result.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task Handle_WhenAddAsyncThrows_ShouldRollbackAndRethrow()
    {
        // Arrange
        var command = new CreateExampleDapperCommand("Name", null);

        _mockExamplesWrite
            .Setup(r => r.AddAsync(It.IsAny<Example>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("DB error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        _mockUnitOfWork.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenCommitThrows_ShouldRollbackAndRethrow()
    {
        // Arrange
        var command = new CreateExampleDapperCommand("Name", null);

        _mockUnitOfWork
            .Setup(u => u.CommitAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Commit error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        _mockUnitOfWork.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCallOperationsInOrder()
    {
        // Arrange
        var command   = new CreateExampleDapperCommand("Ordered", null);
        List<string>  callOrder = [];

        _mockUnitOfWork
            .Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add("BeginTx"))
            .Returns(Task.CompletedTask);

        _mockExamplesWrite
            .Setup(r => r.AddAsync(It.IsAny<Example>(), It.IsAny<CancellationToken>()))
            .Callback<Example, CancellationToken>((_, _) => callOrder.Add("Add"))
            .ReturnsAsync((Example e, CancellationToken _) => e);

        _mockUnitOfWork
            .Setup(u => u.CommitAsync(It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add("Commit"))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        callOrder.Should().Equal("BeginTx", "Add", "Commit");
    }
}

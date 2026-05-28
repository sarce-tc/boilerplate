using FluentAssertions;
using Moq;
using Microservice.Application.Contracts.Persistence.Dapper;
using Microservice.Application.Features.ExamplesDapper.Commands.DeleteExampleDapper;
using Microservice.Domain.Entities;

using DapperContracts = Microservice.Application.Contracts.Persistence.Dapper;

namespace Microservice.Test.Application.Features.ExamplesDapper.Commands.DeleteExampleDapper;

public class DeleteExampleDapperCommandHandlerTests
{
    private readonly Mock<IExampleReadRepository>              _mockReadRepository;
    private readonly Mock<DapperContracts.IUnitOfWork>         _mockUnitOfWork;
    private readonly Mock<IExampleWriteRepository>             _mockExamplesWrite;
    private readonly DeleteExampleDapperCommandHandler         _handler;

    public DeleteExampleDapperCommandHandlerTests()
    {
        _mockReadRepository = new Mock<IExampleReadRepository>();
        _mockUnitOfWork     = new Mock<DapperContracts.IUnitOfWork>();
        _mockExamplesWrite  = new Mock<IExampleWriteRepository>();

        _mockUnitOfWork.Setup(u => u.ExamplesWrite).Returns(_mockExamplesWrite.Object);
        _mockUnitOfWork.Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(u => u.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(u => u.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        _mockExamplesWrite
            .Setup(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _handler = new DeleteExampleDapperCommandHandler(_mockReadRepository.Object, _mockUnitOfWork.Object);
    }

    [Fact]
    public async Task Handle_WithExistingPublicId_ShouldDeleteAndReturnSuccess()
    {
        // Arrange
        var publicId = Guid.NewGuid();
        var command  = new DeleteExampleDapperCommand(publicId);
        var example  = new Example("ToDelete", null) { Id = 7, PublicId = publicId };

        _mockReadRepository
            .Setup(r => r.GetByPublicIdAsync(publicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(example);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(publicId);
        _mockExamplesWrite.Verify(r => r.DeleteAsync(7, It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentPublicId_ShouldReturnNotFoundFailure()
    {
        // Arrange
        var publicId = Guid.NewGuid();
        var command  = new DeleteExampleDapperCommand(publicId);

        _mockReadRepository
            .Setup(r => r.GetByPublicIdAsync(publicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Example?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Code == "NotFound");
        _mockUnitOfWork.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockExamplesWrite.Verify(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldCallDeleteAsync_WithEntityInternalId()
    {
        // Arrange
        var publicId  = Guid.NewGuid();
        const int internalId = 42;
        var command   = new DeleteExampleDapperCommand(publicId);
        var example   = new Example("Entity", null) { Id = internalId, PublicId = publicId };

        _mockReadRepository
            .Setup(r => r.GetByPublicIdAsync(publicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(example);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockExamplesWrite.Verify(r => r.DeleteAsync(internalId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenDeleteAsyncThrows_ShouldRollbackAndRethrow()
    {
        // Arrange
        var publicId = Guid.NewGuid();
        var command  = new DeleteExampleDapperCommand(publicId);
        var example  = new Example("Entity", null) { Id = 1, PublicId = publicId };

        _mockReadRepository
            .Setup(r => r.GetByPublicIdAsync(publicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(example);

        _mockExamplesWrite
            .Setup(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("DB error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        _mockUnitOfWork.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldBeginTransactionBeforeDelete()
    {
        // Arrange
        var publicId = Guid.NewGuid();
        var command  = new DeleteExampleDapperCommand(publicId);
        var example  = new Example("Entity", null) { Id = 1, PublicId = publicId };
        List<string> callOrder = [];

        _mockReadRepository
            .Setup(r => r.GetByPublicIdAsync(publicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(example);

        _mockUnitOfWork
            .Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add("BeginTx"))
            .Returns(Task.CompletedTask);

        _mockExamplesWrite
            .Setup(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Callback<int, CancellationToken>((_, _) => callOrder.Add("Delete"))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(u => u.CommitAsync(It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add("Commit"))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        callOrder.Should().Equal("BeginTx", "Delete", "Commit");
    }
}

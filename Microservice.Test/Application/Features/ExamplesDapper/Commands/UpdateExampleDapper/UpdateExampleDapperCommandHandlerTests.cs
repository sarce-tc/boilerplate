using FluentAssertions;
using Moq;
using Microservice.Application.Contracts.Persistence.Dapper;
using Microservice.Application.Features.ExamplesDapper.Commands.UpdateExampleDapper;
using Microservice.Domain.Entities;
using Microservice.Domain.Exceptions;

using DapperContracts = Microservice.Application.Contracts.Persistence.Dapper;

namespace Microservice.Test.Application.Features.ExamplesDapper.Commands.UpdateExampleDapper;

public class UpdateExampleDapperCommandHandlerTests
{
    private readonly Mock<IExampleReadRepository>              _mockReadRepository;
    private readonly Mock<DapperContracts.IUnitOfWork>         _mockUnitOfWork;
    private readonly Mock<IExampleWriteRepository>             _mockExamplesWrite;
    private readonly UpdateExampleDapperCommandHandler         _handler;

    public UpdateExampleDapperCommandHandlerTests()
    {
        _mockReadRepository = new Mock<IExampleReadRepository>();
        _mockUnitOfWork     = new Mock<DapperContracts.IUnitOfWork>();
        _mockExamplesWrite  = new Mock<IExampleWriteRepository>();

        _mockUnitOfWork.Setup(u => u.ExamplesWrite).Returns(_mockExamplesWrite.Object);
        _mockUnitOfWork.Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(u => u.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(u => u.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        _mockExamplesWrite
            .Setup(r => r.UpdateAsync(It.IsAny<Example>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Example e, CancellationToken _) => e);

        _handler = new UpdateExampleDapperCommandHandler(_mockReadRepository.Object, _mockUnitOfWork.Object);
    }

    [Fact]
    public async Task Handle_WithExistingPublicId_ShouldUpdateAndReturnSuccess()
    {
        // Arrange
        var publicId = Guid.NewGuid();
        var command  = new UpdateExampleDapperCommand(publicId, "UpdatedName", null);
        var example  = new Example("Original", null) { Id = 1, PublicId = publicId };

        _mockReadRepository
            .Setup(r => r.GetByPublicIdAsync(publicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(example);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(publicId);
        _mockExamplesWrite.Verify(r => r.UpdateAsync(example, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentPublicId_ShouldReturnNotFoundFailure()
    {
        // Arrange
        var publicId = Guid.NewGuid();
        var command  = new UpdateExampleDapperCommand(publicId, "Name", null);

        _mockReadRepository
            .Setup(r => r.GetByPublicIdAsync(publicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Example?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Code == "NotFound");
        _mockUnitOfWork.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNullName_ShouldNotCallUpdateName()
    {
        // Arrange
        var publicId = Guid.NewGuid();
        var command  = new UpdateExampleDapperCommand(publicId, null, "New description");
        var example  = new Example("OriginalName", null) { Id = 1, PublicId = publicId };

        _mockReadRepository
            .Setup(r => r.GetByPublicIdAsync(publicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(example);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert — name should remain unchanged
        example.Name.Should().Be("OriginalName");
        _mockExamplesWrite.Verify(r => r.UpdateAsync(example, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithName_ShouldCallUpdateNameBeforeTransaction()
    {
        // Arrange
        var publicId  = Guid.NewGuid();
        var command   = new UpdateExampleDapperCommand(publicId, "NewName", null);
        var example   = new Example("OldName", null) { Id = 1, PublicId = publicId };
        List<string>  callOrder = [];

        _mockReadRepository
            .Setup(r => r.GetByPublicIdAsync(publicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(example);

        _mockUnitOfWork
            .Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add($"BeginTx:{example.Name}"))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert — name was already updated when BeginTx was called
        callOrder.Should().ContainSingle(s => s == "BeginTx:NewName");
        example.Name.Should().Be("NewName");
    }

    [Fact]
    public async Task Handle_WhenUpdateAsyncThrows_ShouldRollbackAndRethrow()
    {
        // Arrange
        var publicId = Guid.NewGuid();
        var command  = new UpdateExampleDapperCommand(publicId, "Name", null);
        var example  = new Example("Original", null) { Id = 1, PublicId = publicId };

        _mockReadRepository
            .Setup(r => r.GetByPublicIdAsync(publicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(example);

        _mockExamplesWrite
            .Setup(r => r.UpdateAsync(It.IsAny<Example>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("DB error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        _mockUnitOfWork.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithInactiveExample_WhenNameProvided_ShouldPropagateDomainException()
    {
        // Arrange
        var publicId = Guid.NewGuid();
        var command  = new UpdateExampleDapperCommand(publicId, "AnyName", null);
        var example  = new Example("Original", null) { Id = 1, PublicId = publicId };
        example.Deactivate();

        _mockReadRepository
            .Setup(r => r.GetByPublicIdAsync(publicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(example);

        // Act & Assert — DomainException thrown before BeginTransactionAsync
        await Assert.ThrowsAsync<DomainException>(
            () => _handler.Handle(command, CancellationToken.None));

        _mockUnitOfWork.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}

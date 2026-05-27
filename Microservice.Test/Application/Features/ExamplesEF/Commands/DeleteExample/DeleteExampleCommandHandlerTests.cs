using System.Linq.Expressions;
using FluentAssertions;
using Moq;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Application.Features.ExamplesEF.Commands.DeleteExample;
using Microservice.Domain.Entities;

namespace Microservice.Test.Application.Features.ExamplesEF.Commands.DeleteExample;

public class DeleteExampleCommandHandlerTests
{
    private readonly Mock<IReadRepository<Example>>  _mockReadRepository;
    private readonly Mock<IExampleWriteRepository>   _mockExamplesWrite;
    private readonly Mock<IUnitOfWork>               _mockUnitOfWork;
    private readonly DeleteExampleCommandHandler     _handler;

    public DeleteExampleCommandHandlerTests()
    {
        _mockReadRepository = new Mock<IReadRepository<Example>>();
        _mockExamplesWrite  = new Mock<IExampleWriteRepository>();
        _mockUnitOfWork     = new Mock<IUnitOfWork>();

        _mockUnitOfWork.Setup(u => u.ExamplesWrite).Returns(_mockExamplesWrite.Object);

        _handler = new DeleteExampleCommandHandler(_mockReadRepository.Object, _mockUnitOfWork.Object);
    }

    [Fact]
    public async Task Handle_WithExistingPublicId_ShouldDeleteExampleAndReturnSuccess()
    {
        var publicId = Guid.NewGuid();
        var command  = new DeleteExampleCommand(publicId);
        var example  = new Example("Test", "Description") { Id = 1, PublicId = publicId };

        _mockReadRepository
            .Setup(r => r.GetEntityAsync(It.IsAny<Expression<Func<Example, bool>>>(), null, null, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(example);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(publicId);
        _mockExamplesWrite.Verify(r => r.Delete(example), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentId_ShouldReturnFailureResult()
    {
        var command = new DeleteExampleCommand(Guid.NewGuid());
        _mockReadRepository
            .Setup(r => r.GetEntityAsync(It.IsAny<Expression<Func<Example, bool>>>(), null, null, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Example?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Code.Should().Be("NotFound");
        _mockExamplesWrite.Verify(r => r.Delete(It.IsAny<Example>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNonExistentId_ShouldNotCallSaveChanges()
    {
        _mockReadRepository
            .Setup(r => r.GetEntityAsync(It.IsAny<Expression<Func<Example, bool>>>(), null, null, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Example?)null);

        await _handler.Handle(new DeleteExampleCommand(Guid.NewGuid()), CancellationToken.None);

        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldCallDeleteWithCorrectExample()
    {
        var publicId = Guid.NewGuid();
        var example  = new Example("Test", "Description") { PublicId = publicId };
        _mockReadRepository
            .Setup(r => r.GetEntityAsync(It.IsAny<Expression<Func<Example, bool>>>(), null, null, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(example);

        await _handler.Handle(new DeleteExampleCommand(publicId), CancellationToken.None);

        _mockExamplesWrite.Verify(r => r.Delete(example), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldSaveChangesAfterDelete()
    {
        var publicId = Guid.NewGuid();
        var example  = new Example("Test", "Description") { PublicId = publicId };
        _mockReadRepository
            .Setup(r => r.GetEntityAsync(It.IsAny<Expression<Func<Example, bool>>>(), null, null, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(example);

        await _handler.Handle(new DeleteExampleCommand(publicId), CancellationToken.None);

        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldRespectCancellationToken()
    {
        var publicId          = Guid.NewGuid();
        var example           = new Example("Test", "Description") { PublicId = publicId };
        var cancellationToken = new CancellationToken(canceled: false);
        _mockReadRepository
            .Setup(r => r.GetEntityAsync(It.IsAny<Expression<Func<Example, bool>>>(), null, null, It.IsAny<bool>(), cancellationToken))
            .ReturnsAsync(example);

        await _handler.Handle(new DeleteExampleCommand(publicId), cancellationToken);

        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldPropagateException()
    {
        _mockReadRepository
            .Setup(r => r.GetEntityAsync(It.IsAny<Expression<Func<Example, bool>>>(), null, null, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(new DeleteExampleCommand(Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenSaveChangesThrows_ShouldPropagateException()
    {
        var example = new Example("Test", "Description") { Id = 1 };
        _mockReadRepository
            .Setup(r => r.GetEntityAsync(It.IsAny<Expression<Func<Example, bool>>>(), null, null, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(example);
        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Concurrency conflict"));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(new DeleteExampleCommand(Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldCallRepositoryOperationsInCorrectOrder()
    {
        var example   = new Example("Test", "Description") { Id = 1 };
        var callOrder = new List<string>();

        _mockReadRepository
            .Setup(r => r.GetEntityAsync(It.IsAny<Expression<Func<Example, bool>>>(), null, null, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .Callback(async () => { callOrder.Add("Find"); await Task.CompletedTask; })
            .ReturnsAsync(example);
        _mockExamplesWrite
            .Setup(r => r.Delete(example))
            .Callback(() => callOrder.Add("Delete"));
        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(async () => { callOrder.Add("Save"); await Task.CompletedTask; })
            .ReturnsAsync(1);

        await _handler.Handle(new DeleteExampleCommand(Guid.NewGuid()), CancellationToken.None);

        callOrder.Should().Equal("Find", "Delete", "Save");
    }

    [Fact]
    public async Task Handle_WithNonExistentPublicId_ShouldReturnNotFoundError()
    {
        var publicId = Guid.NewGuid();
        _mockReadRepository
            .Setup(r => r.GetEntityAsync(It.IsAny<Expression<Func<Example, bool>>>(), null, null, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Example?)null);

        var result = await _handler.Handle(new DeleteExampleCommand(publicId), CancellationToken.None);

        result.Errors[0].Message.Should().Contain(publicId.ToString());
        result.Errors[0].Message.Should().Contain("not found");
    }
}

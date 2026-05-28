using FluentAssertions;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Application.Features.ExamplesEF.Commands.ActivateExample;
using Microservice.Application.Services;
using Microservice.Domain.Entities;
using Microservice.Domain.Exceptions;
using Moq;

namespace Microservice.Test.Application.Features.ExamplesEF.Commands.ActivateExample;

public class ActivateExampleCommandHandlerTests
{
    private readonly Mock<IExampleService>        _mockExampleService;
    private readonly Mock<IExampleWriteRepository> _mockExamplesWrite;
    private readonly Mock<IUnitOfWork>             _mockUnitOfWork;
    private readonly ActivateExampleCommandHandler _handler;

    public ActivateExampleCommandHandlerTests()
    {
        _mockExampleService = new Mock<IExampleService>();
        _mockExamplesWrite  = new Mock<IExampleWriteRepository>();
        _mockUnitOfWork     = new Mock<IUnitOfWork>();

        _mockUnitOfWork.Setup(u => u.ExamplesWrite).Returns(_mockExamplesWrite.Object);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _handler = new ActivateExampleCommandHandler(_mockExampleService.Object, _mockUnitOfWork.Object);
    }

    private static Example BuildInactiveExample()
    {
        var example = new Example("Test", "Description");
        example.Deactivate();
        return example;
    }

    [Fact]
    public async Task Handle_WithExistingInactiveExample_ShouldReturnSuccess()
    {
        // Arrange
        var example = BuildInactiveExample();
        var command = new ActivateExampleCommand(example.PublicId);

        _mockExampleService
            .Setup(s => s.FindTrackedAsync(example.PublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(example);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(example.PublicId);
    }

    [Fact]
    public async Task Handle_WithExistingInactiveExample_ShouldSetStatusToActive()
    {
        // Arrange
        var example = BuildInactiveExample();
        _mockExampleService
            .Setup(s => s.FindTrackedAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(example);

        // Act
        await _handler.Handle(new ActivateExampleCommand(example.PublicId), CancellationToken.None);

        // Assert
        example.Status.Should().Be(ExampleStatus.Active);
    }

    [Fact]
    public async Task Handle_WithNonExistentExample_ShouldReturnNotFound()
    {
        // Arrange
        var publicId = Guid.NewGuid();
        _mockExampleService
            .Setup(s => s.FindTrackedAsync(publicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Example?)null);

        // Act
        var result = await _handler.Handle(new ActivateExampleCommand(publicId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors[0].Code.Should().Be("NotFound");
        result.Errors[0].Message.Should().Contain(publicId.ToString());
    }

    [Fact]
    public async Task Handle_WithNonExistentExample_ShouldNotCallUpdateOrSave()
    {
        // Arrange
        _mockExampleService
            .Setup(s => s.FindTrackedAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Example?)null);

        // Act
        await _handler.Handle(new ActivateExampleCommand(Guid.NewGuid()), CancellationToken.None);

        // Assert
        _mockExamplesWrite.Verify(r => r.Update(It.IsAny<Example>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithExistingExample_ShouldCallUpdateAndSaveChanges()
    {
        // Arrange
        var example = BuildInactiveExample();
        _mockExampleService
            .Setup(s => s.FindTrackedAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(example);

        // Act
        await _handler.Handle(new ActivateExampleCommand(example.PublicId), CancellationToken.None);

        // Assert
        _mockExamplesWrite.Verify(r => r.Update(example), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithAlreadyActiveExample_ShouldPropagateDomainException()
    {
        // Arrange — Example is Active by default (constructor sets Status = Active)
        var example = new Example("Test", null);
        _mockExampleService
            .Setup(s => s.FindTrackedAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(example);

        // Act & Assert — DomainException propagates; GlobalExceptionHandler maps it to HTTP 409
        await Assert.ThrowsAsync<DomainException>(
            () => _handler.Handle(new ActivateExampleCommand(example.PublicId), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithAlreadyActiveExample_ShouldNotCallSave()
    {
        // Arrange
        var example = new Example("Test", null); // Active by default
        _mockExampleService
            .Setup(s => s.FindTrackedAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(example);

        // Act
        try { await _handler.Handle(new ActivateExampleCommand(example.PublicId), CancellationToken.None); }
        catch (DomainException) { /* expected */ }

        // Assert
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldCallFindTrackedAsync_WithCorrectPublicId()
    {
        // Arrange
        var publicId = Guid.NewGuid();
        _mockExampleService
            .Setup(s => s.FindTrackedAsync(publicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Example?)null);

        // Act
        await _handler.Handle(new ActivateExampleCommand(publicId), CancellationToken.None);

        // Assert
        _mockExampleService.Verify(s => s.FindTrackedAsync(publicId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCallRepositoryOperationsInCorrectOrder()
    {
        // Arrange
        var example = BuildInactiveExample();
        List<string> callOrder = [];

        _mockExampleService
            .Setup(s => s.FindTrackedAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Callback(async () => { callOrder.Add("Find"); await Task.CompletedTask; })
            .ReturnsAsync(example);
        _mockExamplesWrite
            .Setup(r => r.Update(example))
            .Callback(() => callOrder.Add("Update"));
        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(async () => { callOrder.Add("Save"); await Task.CompletedTask; })
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(new ActivateExampleCommand(example.PublicId), CancellationToken.None);

        // Assert
        callOrder.Should().Equal("Find", "Update", "Save");
    }

    [Fact]
    public async Task Handle_WhenSaveChangesThrows_ShouldPropagateException()
    {
        // Arrange
        var example = BuildInactiveExample();
        _mockExampleService
            .Setup(s => s.FindTrackedAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(example);
        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Concurrency conflict"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(new ActivateExampleCommand(example.PublicId), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldRespectCancellationToken()
    {
        // Arrange
        var example           = BuildInactiveExample();
        var cancellationToken = new CancellationToken(canceled: false);

        _mockExampleService
            .Setup(s => s.FindTrackedAsync(example.PublicId, cancellationToken))
            .ReturnsAsync(example);

        // Act
        await _handler.Handle(new ActivateExampleCommand(example.PublicId), cancellationToken);

        // Assert
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(cancellationToken), Times.Once);
    }
}

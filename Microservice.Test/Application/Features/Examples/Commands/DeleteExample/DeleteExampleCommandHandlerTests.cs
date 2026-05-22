using System.Linq.Expressions;
using FluentAssertions;
using Moq;
using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Application.Features.Examples.Commands.DeleteExample;
using Microservice.Domain.Entities;

namespace Microservice.Test.Application.Features.Examples.Commands.DeleteExample
{
    /// <summary>
    /// Unit tests for DeleteExampleCommandHandler
    /// Tests delete command handling, entity validation, and repository interactions
    /// </summary>
    public class DeleteExampleCommandHandlerTests
    {
        private readonly Mock<IReadRepository<Example>> _mockReadRepository;
        private readonly Mock<IWriteRepository<Example>> _mockWriteRepository;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly DeleteExampleCommandHandler _handler;

        public DeleteExampleCommandHandlerTests()
        {
            _mockReadRepository = new Mock<IReadRepository<Example>>();
            _mockWriteRepository = new Mock<IWriteRepository<Example>>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _handler = new DeleteExampleCommandHandler(
                _mockReadRepository.Object,
                _mockWriteRepository.Object,
                _mockUnitOfWork.Object);
        }

        [Fact]
        public async Task Handle_WithExistingPublicId_ShouldDeleteExampleAndReturnSuccess()
        {
            // Arrange
            var publicId = Guid.NewGuid();
            var command = new DeleteExampleCommand(publicId);
            var example = new Example("Test", "Description");
            example.Id = 1;
            example.PublicId = publicId;

            _mockReadRepository
                .Setup(r => r.GetEntityAsync(It.IsAny<Expression<Func<Example, bool>>>(), null, null, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(example);

            _mockWriteRepository
                .Setup(r => r.Delete(example))
                .Verifiable();

            _mockUnitOfWork
                .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(publicId);
            _mockWriteRepository.Verify(r => r.Delete(example), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithNonExistentId_ShouldReturnFailureResult()
        {
            // Arrange
            var command = new DeleteExampleCommand(Guid.NewGuid());

            _mockReadRepository
                .Setup(r => r.GetEntityAsync(It.IsAny<Expression<Func<Example, bool>>>(), null, null, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Example?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].Code.Should().Be("NotFound");
            _mockWriteRepository.Verify(r => r.Delete(It.IsAny<Example>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WithNonExistentId_ShouldNotCallSaveChanges()
        {
            // Arrange
            var command = new DeleteExampleCommand(Guid.NewGuid());

            _mockReadRepository
                .Setup(r => r.GetEntityAsync(It.IsAny<Expression<Func<Example, bool>>>(), null, null, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Example?)null);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WithDifferentPublicIds_ShouldReturnCorrectPublicId()
        {
            // Arrange
            var publicId = Guid.NewGuid();
            var command = new DeleteExampleCommand(publicId);
            var example = new Example("Test", "Description");
            example.PublicId = publicId;

            _mockReadRepository
                .Setup(r => r.GetEntityAsync(It.IsAny<Expression<Func<Example, bool>>>(), null, null, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(example);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(publicId);
        }

        [Fact]
        public async Task Handle_ShouldCallGetEntityAsyncWithPredicate()
        {
            // Arrange
            var publicId = Guid.NewGuid();
            var command = new DeleteExampleCommand(publicId);
            var example = new Example("Test", "Description");
            example.PublicId = publicId;

            _mockReadRepository
                .Setup(r => r.GetEntityAsync(It.IsAny<Expression<Func<Example, bool>>>(), null, null, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(example);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _mockReadRepository.Verify(r => r.GetEntityAsync(It.IsAny<Expression<Func<Example, bool>>>(), null, null, It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldCallDeleteWithCorrectExample()
        {
            // Arrange
            var publicId = Guid.NewGuid();
            var command = new DeleteExampleCommand(publicId);
            var example = new Example("Test", "Description");
            example.PublicId = publicId;

            _mockReadRepository
                .Setup(r => r.GetEntityAsync(It.IsAny<Expression<Func<Example, bool>>>(), null, null, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(example);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _mockWriteRepository.Verify(r => r.Delete(example), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldSaveChangesAfterDelete()
        {
            // Arrange
            var publicId = Guid.NewGuid();
            var command = new DeleteExampleCommand(publicId);
            var example = new Example("Test", "Description");
            example.PublicId = publicId;

            _mockReadRepository
                .Setup(r => r.GetEntityAsync(It.IsAny<Expression<Func<Example, bool>>>(), null, null, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(example);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _mockUnitOfWork.Verify(
                u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldRespectCancellationToken()
        {
            // Arrange
            var publicId = Guid.NewGuid();
            var command = new DeleteExampleCommand(publicId);
            var example = new Example("Test", "Description");
            example.PublicId = publicId;
            var cancellationToken = new CancellationToken(canceled: false);

            _mockReadRepository
                .Setup(r => r.GetEntityAsync(It.IsAny<Expression<Func<Example, bool>>>(), null, null, true, cancellationToken))
                .ReturnsAsync(example);

            // Act
            await _handler.Handle(command, cancellationToken);

            // Assert
            _mockUnitOfWork.Verify(
                u => u.SaveChangesAsync(cancellationToken),
                Times.Once);
        }

        [Fact]
        public async Task Handle_WhenRepositoryThrows_ShouldPropagateException()
        {
            // Arrange
            var command = new DeleteExampleCommand(Guid.NewGuid());

            _mockReadRepository
                .Setup(r => r.GetEntityAsync(It.IsAny<Expression<Func<Example, bool>>>(), null, null, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_WhenSaveChangesThrows_ShouldPropagateException()
        {
            // Arrange
            var command = new DeleteExampleCommand(Guid.NewGuid());
            var example = new Example("Test", "Description") { Id = 1 };

            _mockReadRepository
                .Setup(r => r.GetEntityAsync(It.IsAny<Expression<Func<Example, bool>>>(), null, null, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(example);

            _mockUnitOfWork
                .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Concurrency conflict"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ShouldCallRepositoryOperationsInCorrectOrder()
        {
            // Arrange
            var command = new DeleteExampleCommand(Guid.NewGuid());
            var example = new Example("Test", "Description") { Id = 1 };
            var callOrder = new List<string>();

            _mockReadRepository
                .Setup(r => r.GetEntityAsync(It.IsAny<Expression<Func<Example, bool>>>(), null, null, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .Callback(async () =>
                {
                    callOrder.Add("Find");
                    await Task.CompletedTask;
                })
                .ReturnsAsync(example);

            _mockWriteRepository
                .Setup(r => r.Delete(example))
                .Callback(() => callOrder.Add("Delete"));

            _mockUnitOfWork
                .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Callback(async () =>
                {
                    callOrder.Add("Save");
                    await Task.CompletedTask;
                })
                .ReturnsAsync(1);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            callOrder.Should().Equal("Find", "Delete", "Save");
        }

        [Fact]
        public async Task Handle_WithNonExistentPublicId_ShouldReturnErrorMessage()
        {
            // Arrange
            var publicId = Guid.NewGuid();
            var command = new DeleteExampleCommand(publicId);

            _mockReadRepository
                .Setup(r => r.GetEntityAsync(It.IsAny<Expression<Func<Example, bool>>>(), null, null, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Example?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Errors[0].Message.Should().Contain(publicId.ToString());
            result.Errors[0].Message.Should().Contain("no encontrado");
        }
    }
}

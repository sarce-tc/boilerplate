using FluentAssertions;
using Moq;
using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Application.Features.Examples.Commands.DeleteManyExamples;
using Microservice.Domain.Entities;
using System.Linq.Expressions;

namespace Microservice.Test.Application.Features.Examples.Commands.DeleteManyExamples
{
    /// <summary>
    /// Unit tests for DeleteManyExamplesCommandHandler
    /// Tests bulk deletion operations
    /// </summary>
    public class DeleteManyExamplesCommandHandlerTests
    {
        private readonly Mock<IWriteRepository<Example>> _mockWriteRepository;
        private readonly DeleteManyExamplesCommandHandler _handler;

        public DeleteManyExamplesCommandHandlerTests()
        {
            _mockWriteRepository = new Mock<IWriteRepository<Example>>();
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork
                .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);
            
            _handler = new DeleteManyExamplesCommandHandler(
                _mockWriteRepository.Object,
                mockUnitOfWork.Object);
        }

        [Fact]
        public async Task Handle_WithValidPublicIds_ShouldDeleteMultipleAndReturnCount()
        {
            // Arrange
            var publicIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
            var command = new DeleteManyExamplesCommand(publicIds);

            _mockWriteRepository
                .Setup(r => r.DeleteManyAsync(It.IsAny<Expression<Func<Example, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(3);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(3);
        }

        [Fact]
        public async Task Handle_WithEmptyArray_ShouldReturnZero()
        {
            // Arrange
            var publicIds = Array.Empty<Guid>();
            var command = new DeleteManyExamplesCommand(publicIds);

            _mockWriteRepository
                .Setup(r => r.DeleteManyAsync(It.IsAny<Expression<Func<Example, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(0);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(0);
        }

        [Fact]
        public async Task Handle_ShouldCallDeleteManyAsync()
        {
            // Arrange
            var publicIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
            var command = new DeleteManyExamplesCommand(publicIds);

            _mockWriteRepository
                .Setup(r => r.DeleteManyAsync(It.IsAny<Expression<Func<Example, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(3);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _mockWriteRepository.Verify(
                r => r.DeleteManyAsync(It.IsAny<Expression<Func<Example, bool>>>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_WithSinglePublicId_ShouldDeleteOne()
        {
            // Arrange
            var publicIds = new[] { Guid.NewGuid() };
            var command = new DeleteManyExamplesCommand(publicIds);

            _mockWriteRepository
                .Setup(r => r.DeleteManyAsync(It.IsAny<Expression<Func<Example, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Value.Should().Be(1);
        }

        [Fact]
        public async Task Handle_WithManyPublicIds_ShouldDeleteAll()
        {
            // Arrange
            var publicIds = Enumerable.Range(1, 100).Select(_ => Guid.NewGuid()).ToArray();
            var command = new DeleteManyExamplesCommand(publicIds);

            _mockWriteRepository
                .Setup(r => r.DeleteManyAsync(It.IsAny<Expression<Func<Example, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(100);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(100);
        }

        [Fact]
        public async Task Handle_ShouldRespectCancellationToken()
        {
            // Arrange
            var publicIds = new[] { Guid.NewGuid(), Guid.NewGuid() };
            var command = new DeleteManyExamplesCommand(publicIds);
            var cancellationToken = new CancellationToken(canceled: false);

            _mockWriteRepository
                .Setup(r => r.DeleteManyAsync(It.IsAny<Expression<Func<Example, bool>>>(), cancellationToken))
                .ReturnsAsync(2);

            // Act
            await _handler.Handle(command, cancellationToken);

            // Assert
            _mockWriteRepository.Verify(
                r => r.DeleteManyAsync(It.IsAny<Expression<Func<Example, bool>>>(), cancellationToken),
                Times.Once);
        }

        [Fact]
        public async Task Handle_WhenRepositoryThrows_ShouldPropagateException()
        {
            // Arrange
            var publicIds = new[] { Guid.NewGuid(), Guid.NewGuid() };
            var command = new DeleteManyExamplesCommand(publicIds);

            _mockWriteRepository
                .Setup(r => r.DeleteManyAsync(It.IsAny<Expression<Func<Example, bool>>>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _handler.Handle(command, CancellationToken.None));
        }
    }
}

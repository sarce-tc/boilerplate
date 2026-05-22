using FluentAssertions;
using Moq;
using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Application.Features.Examples.Queries.ExistsExample;
using Microservice.Domain.Entities;

namespace Microservice.Test.Application.Features.Examples.Queries.ExistsExample
{
    /// <summary>
    /// Unit tests for ExistsExampleQueryHandler
    /// Tests entity existence checking
    /// </summary>
    public class ExistsExampleQueryHandlerTests
    {
        private readonly Mock<IReadRepository<Example>> _mockReadRepository;
        private readonly ExistsExampleQueryHandler _handler;

        public ExistsExampleQueryHandlerTests()
        {
            _mockReadRepository = new Mock<IReadRepository<Example>>();
            _handler = new ExistsExampleQueryHandler(_mockReadRepository.Object);
        }

        [Fact]
        public async Task Handle_WithExistingId_ShouldReturnTrue()
        {
            // Arrange
            var query = new ExistsExampleQuery(Guid.NewGuid());

            _mockReadRepository
                .Setup(r => r.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Example, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_WithNonExistentId_ShouldReturnFalse()
        {
            // Arrange
            var query = new ExistsExampleQuery(Guid.NewGuid());

            _mockReadRepository
                .Setup(r => r.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Example, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_ShouldCallExistsAsync()
        {
            // Arrange
            var query = new ExistsExampleQuery(Guid.NewGuid());

            _mockReadRepository
                .Setup(r => r.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Example, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            await _handler.Handle(query, CancellationToken.None);

            // Assert
            _mockReadRepository.Verify(
                r => r.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Example, bool>>>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Handle_WithDifferentPublicIds_ShouldReturnCorrectValue(bool shouldExist)
        {
            // Arrange
            var query = new ExistsExampleQuery(Guid.NewGuid());

            _mockReadRepository
                .Setup(r => r.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Example, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(shouldExist);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(shouldExist);
        }

        [Fact]
        public async Task Handle_ShouldRespectCancellationToken()
        {
            // Arrange
            var query = new ExistsExampleQuery(Guid.NewGuid());
            var cancellationToken = new CancellationToken(canceled: false);

            _mockReadRepository
                .Setup(r => r.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Example, bool>>>(), cancellationToken))
                .ReturnsAsync(true);

            // Act
            await _handler.Handle(query, cancellationToken);

            // Assert
            _mockReadRepository.Verify(
                r => r.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Example, bool>>>(), cancellationToken),
                Times.Once);
        }

        [Fact]
        public async Task Handle_WhenRepositoryThrows_ShouldPropagateException()
        {
            // Arrange
            var query = new ExistsExampleQuery(Guid.NewGuid());

            _mockReadRepository
                .Setup(r => r.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Example, bool>>>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _handler.Handle(query, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccessResult()
        {
            // Arrange
            var query = new ExistsExampleQuery(Guid.NewGuid());

            _mockReadRepository
                .Setup(r => r.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Example, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_WhenEntityExists_ShouldNotReturnErrors()
        {
            // Arrange
            var query = new ExistsExampleQuery(Guid.NewGuid());

            _mockReadRepository
                .Setup(r => r.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Example, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Errors.Should().BeEmpty();
            result.Value.Should().BeTrue();
        }
    }
}

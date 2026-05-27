using FluentAssertions;
using Moq;
using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Application.Features.ExamplesEF.Queries.CountExamples;
using Microservice.Domain.Entities;

namespace Microservice.Test.Application.Features.Examples.Queries.CountExamples
{
    /// <summary>
    /// Unit tests for CountExamplesQueryHandler
    /// Tests counting of total entities in database
    /// </summary>
    public class CountExamplesQueryHandlerTests
    {
        private readonly Mock<IReadRepository<Example>> _mockReadRepository;
        private readonly CountExamplesQueryHandler _handler;

        public CountExamplesQueryHandlerTests()
        {
            _mockReadRepository = new Mock<IReadRepository<Example>>();
            _handler = new CountExamplesQueryHandler(_mockReadRepository.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnTotalCount()
        {
            // Arrange
            var query = new CountExamplesQuery();
            var expectedCount = 42;

            _mockReadRepository
                .Setup(r => r.CountAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedCount);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(expectedCount);
        }

        [Fact]
        public async Task Handle_WithEmptyDatabase_ShouldReturnZero()
        {
            // Arrange
            var query = new CountExamplesQuery();

            _mockReadRepository
                .Setup(r => r.CountAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(0);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(0);
        }

        [Fact]
        public async Task Handle_ShouldCallCountAsync()
        {
            // Arrange
            var query = new CountExamplesQuery();

            _mockReadRepository
                .Setup(r => r.CountAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(0);

            // Act
            await _handler.Handle(query, CancellationToken.None);

            // Assert
            _mockReadRepository.Verify(
                r => r.CountAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(100)]
        [InlineData(1000000)]
        public async Task Handle_WithDifferentCounts_ShouldReturnCorrectValue(int count)
        {
            // Arrange
            var query = new CountExamplesQuery();

            _mockReadRepository
                .Setup(r => r.CountAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(count);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(count);
        }

        [Fact]
        public async Task Handle_ShouldRespectCancellationToken()
        {
            // Arrange
            var query = new CountExamplesQuery();
            var cancellationToken = new CancellationToken(canceled: false);

            _mockReadRepository
                .Setup(r => r.CountAsync(cancellationToken))
                .ReturnsAsync(10);

            // Act
            await _handler.Handle(query, cancellationToken);

            // Assert
            _mockReadRepository.Verify(
                r => r.CountAsync(cancellationToken),
                Times.Once);
        }

        [Fact]
        public async Task Handle_WhenRepositoryThrows_ShouldPropagateException()
        {
            // Arrange
            var query = new CountExamplesQuery();

            _mockReadRepository
                .Setup(r => r.CountAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _handler.Handle(query, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccessResult()
        {
            // Arrange
            var query = new CountExamplesQuery();

            _mockReadRepository
                .Setup(r => r.CountAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(50);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_WithLargeCount_ShouldHandleCorrectly()
        {
            // Arrange
            var query = new CountExamplesQuery();
            var largeCount = int.MaxValue;

            _mockReadRepository
                .Setup(r => r.CountAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(largeCount);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(largeCount);
        }
    }
}

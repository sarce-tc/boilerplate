using FluentAssertions;
using Moq;
using Microservice.Application.Contracts.Persistence.Dapper;
using Microservice.Application.Features.ExamplesDapper.Queries.CountExamplesDapper;

namespace Microservice.Test.Application.Features.ExamplesDapper.Queries.CountExamplesDapper;

public class CountExamplesDapperQueryHandlerTests
{
    private readonly Mock<IExampleReadRepository>        _mockReadRepository;
    private readonly CountExamplesDapperQueryHandler     _handler;

    public CountExamplesDapperQueryHandlerTests()
    {
        _mockReadRepository = new Mock<IExampleReadRepository>();
        _handler            = new CountExamplesDapperQueryHandler(_mockReadRepository.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccessWithCount()
    {
        // Arrange
        var query = new CountExamplesDapperQuery();

        _mockReadRepository
            .Setup(r => r.CountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(42);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public async Task Handle_WithZeroRecords_ShouldReturnZero()
    {
        // Arrange
        var query = new CountExamplesDapperQuery();

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
    public async Task Handle_ShouldCallCountAsync_Once()
    {
        // Arrange
        var query = new CountExamplesDapperQuery();

        _mockReadRepository
            .Setup(r => r.CountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _mockReadRepository.Verify(r => r.CountAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldPropagateException()
    {
        // Arrange
        var query = new CountExamplesDapperQuery();

        _mockReadRepository
            .Setup(r => r.CountAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("DB error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(query, CancellationToken.None));
    }
}

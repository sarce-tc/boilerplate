using FluentAssertions;
using Moq;
using Microservice.Application.Contracts.Persistence.Dapper;
using Microservice.Application.Features.ExamplesDapper.Queries.ExistsExampleByNameDapper;

namespace Microservice.Test.Application.Features.ExamplesDapper.Queries.ExistsExampleByNameDapper;

public class ExistsExampleByNameDapperQueryHandlerTests
{
    private readonly Mock<IExampleReadRepository>               _mockReadRepository;
    private readonly ExistsExampleByNameDapperQueryHandler      _handler;

    public ExistsExampleByNameDapperQueryHandlerTests()
    {
        _mockReadRepository = new Mock<IExampleReadRepository>();
        _handler            = new ExistsExampleByNameDapperQueryHandler(_mockReadRepository.Object);
    }

    [Fact]
    public async Task Handle_WhenExampleExists_ShouldReturnTrue()
    {
        // Arrange
        var query = new ExistsExampleByNameDapperQuery("Existing");

        _mockReadRepository
            .Setup(r => r.ExistsByNameAsync("Existing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenExampleDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        var query = new ExistsExampleByNameDapperQuery("Missing");

        _mockReadRepository
            .Setup(r => r.ExistsByNameAsync("Missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldCallExistsByNameAsync_WithCorrectName()
    {
        // Arrange
        var name  = "TargetName";
        var query = new ExistsExampleByNameDapperQuery(name);

        _mockReadRepository
            .Setup(r => r.ExistsByNameAsync(name, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _mockReadRepository.Verify(r => r.ExistsByNameAsync(name, It.IsAny<CancellationToken>()), Times.Once);
    }
}

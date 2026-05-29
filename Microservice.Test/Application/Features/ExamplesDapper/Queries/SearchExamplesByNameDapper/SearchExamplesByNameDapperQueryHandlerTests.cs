using AutoMapper;
using FluentAssertions;
using Moq;
using Microservice.Application.Contracts.Persistence.Dapper;
using Microservice.Application.DTOs.Dapper;
using Microservice.Application.Features.ExamplesDapper.Queries.SearchExamplesByNameDapper;
using Microservice.Domain.Entities;

namespace Microservice.Test.Application.Features.ExamplesDapper.Queries.SearchExamplesByNameDapper;

public class SearchExamplesByNameDapperQueryHandlerTests
{
    private readonly Mock<IExampleReadRepository>                  _mockReadRepository;
    private readonly Mock<IMapper>                                 _mockMapper;
    private readonly SearchExamplesByNameDapperQueryHandler        _handler;

    public SearchExamplesByNameDapperQueryHandlerTests()
    {
        _mockReadRepository = new Mock<IExampleReadRepository>();
        _mockMapper         = new Mock<IMapper>();
        _handler            = new SearchExamplesByNameDapperQueryHandler(_mockReadRepository.Object, _mockMapper.Object);
    }

    [Fact]
    public async Task Handle_WithMatchingName_ShouldReturnSuccessWithDtos()
    {
        // Arrange
        var query    = new SearchExamplesByNameDapperQuery("Widget");
        var examples = new List<Example>
        {
            new("Widget A", null) { Id = 1 },
            new("Widget B", null) { Id = 2 },
        };
        var dtos = new List<SearchExamplesByNameDto>
        {
            new(Guid.NewGuid(), "Widget A", null, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow),
            new(Guid.NewGuid(), "Widget B", null, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow),
        };

        _mockReadRepository
            .Setup(r => r.SearchByNameAsync("Widget", It.IsAny<CancellationToken>()))
            .ReturnsAsync(examples);
        _mockMapper
            .Setup(m => m.Map<IEnumerable<SearchExamplesByNameDto>>(examples))
            .Returns(dtos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(dtos);
    }

    [Fact]
    public async Task Handle_WithNoMatches_ShouldReturnSuccessWithEmptyCollection()
    {
        // Arrange
        var query = new SearchExamplesByNameDapperQuery("NonExistent");

        _mockReadRepository
            .Setup(r => r.SearchByNameAsync("NonExistent", It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        _mockMapper
            .Setup(m => m.Map<IEnumerable<SearchExamplesByNameDto>>(It.IsAny<IEnumerable<Example>>()))
            .Returns([]);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldCallSearchByNameAsync_WithCorrectName()
    {
        // Arrange
        var searchTerm = "Gadget";
        var query      = new SearchExamplesByNameDapperQuery(searchTerm);

        _mockReadRepository
            .Setup(r => r.SearchByNameAsync(searchTerm, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        _mockMapper
            .Setup(m => m.Map<IEnumerable<SearchExamplesByNameDto>>(It.IsAny<IEnumerable<Example>>()))
            .Returns([]);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _mockReadRepository.Verify(r => r.SearchByNameAsync(searchTerm, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldMapResultsToDtos()
    {
        // Arrange
        var query    = new SearchExamplesByNameDapperQuery("Part");
        var examples = new List<Example> { new("Part X", null) { Id = 1 } };

        _mockReadRepository
            .Setup(r => r.SearchByNameAsync("Part", It.IsAny<CancellationToken>()))
            .ReturnsAsync(examples);
        _mockMapper
            .Setup(m => m.Map<IEnumerable<SearchExamplesByNameDto>>(It.IsAny<IEnumerable<Example>>()))
            .Returns([]);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _mockMapper.Verify(
            m => m.Map<IEnumerable<SearchExamplesByNameDto>>(It.IsAny<IEnumerable<Example>>()),
            Times.Once);
    }
}

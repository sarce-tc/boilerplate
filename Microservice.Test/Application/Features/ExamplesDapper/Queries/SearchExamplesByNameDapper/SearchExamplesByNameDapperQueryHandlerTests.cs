using FluentAssertions;
using Moq;
using Microservice.Application.Contracts.Persistence.Dapper;
using Microservice.Application.DTOs.Dapper;
using Microservice.Application.Features.ExamplesDapper.Queries.SearchExamplesByNameDapper;

namespace Microservice.Test.Application.Features.ExamplesDapper.Queries.SearchExamplesByNameDapper;

public class SearchExamplesByNameDapperQueryHandlerTests
{
    private readonly Mock<IExampleReadRepository>         _mockReadRepository = new();
    private readonly SearchExamplesByNameDapperQueryHandler _handler;

    public SearchExamplesByNameDapperQueryHandlerTests()
    {
        _handler = new SearchExamplesByNameDapperQueryHandler(_mockReadRepository.Object);
    }

    [Fact]
    public async Task Handle_WithMatchingName_ShouldReturnDtosWithItems()
    {
        var query = new SearchExamplesByNameDapperQuery("Widget");
        var dtos  = new List<SearchExamplesByNameDto>
        {
            new(Guid.NewGuid(), "Widget A", null, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow,
                [new(Guid.NewGuid(), "A", 2, Domain.Entities.ExampleItemStatus.Pending, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow)]),
            new(Guid.NewGuid(), "Widget B", null, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, []),
        };

        _mockReadRepository
            .Setup(r => r.SearchByNameWithItemsAsync("Widget", It.IsAny<CancellationToken>()))
            .ReturnsAsync(dtos);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(dtos);
        result.Value!.First().Items.Should().ContainSingle(i => i.Label == "A");
    }

    [Fact]
    public async Task Handle_WithNoMatches_ShouldReturnEmptyCollection()
    {
        var query = new SearchExamplesByNameDapperQuery("NonExistent");
        _mockReadRepository
            .Setup(r => r.SearchByNameWithItemsAsync("NonExistent", It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldCallSearchByNameWithItemsAsync_WithCorrectName()
    {
        var query = new SearchExamplesByNameDapperQuery("Gadget");
        _mockReadRepository
            .Setup(r => r.SearchByNameWithItemsAsync("Gadget", It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        await _handler.Handle(query, CancellationToken.None);

        _mockReadRepository.Verify(r => r.SearchByNameWithItemsAsync("Gadget", It.IsAny<CancellationToken>()), Times.Once);
    }
}

using FluentAssertions;
using Moq;
using Microservice.Application.Contracts.Persistence.Dapper;
using Microservice.Application.DTOs.Dapper;
using Microservice.Application.Features.ExamplesDapper.Queries.GetAllExamplesDapper;

namespace Microservice.Test.Application.Features.ExamplesDapper.Queries.GetAllExamplesDapper;

public class GetAllExamplesDapperQueryHandlerTests
{
    private readonly Mock<IExampleReadRepository>     _mockReadRepository = new();
    private readonly GetAllExamplesDapperQueryHandler _handler;

    public GetAllExamplesDapperQueryHandlerTests()
    {
        _handler = new GetAllExamplesDapperQueryHandler(_mockReadRepository.Object);
    }

    [Fact]
    public async Task Handle_WithExistingRecords_ShouldReturnDtosWithItems()
    {
        var query = new GetAllExamplesDapperQuery();
        var dtos  = new List<GetAllExamplesDto>
        {
            new(Guid.NewGuid(), "Test1", "Desc1", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow,
                [new(Guid.NewGuid(), "A", 2, Domain.Entities.ExampleItemStatus.Pending, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow)]),
            new(Guid.NewGuid(), "Test2", "Desc2", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, []),
        };

        _mockReadRepository
            .Setup(r => r.GetAllWithItemsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(dtos);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(dtos);
        result.Value!.First().Items.Should().ContainSingle(i => i.Label == "A");
    }

    [Fact]
    public async Task Handle_WithEmptyTable_ShouldReturnEmptyCollection()
    {
        var query = new GetAllExamplesDapperQuery();
        _mockReadRepository
            .Setup(r => r.GetAllWithItemsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldCallGetAllWithItemsAsync_Once()
    {
        var query = new GetAllExamplesDapperQuery();
        _mockReadRepository
            .Setup(r => r.GetAllWithItemsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        await _handler.Handle(query, CancellationToken.None);

        _mockReadRepository.Verify(r => r.GetAllWithItemsAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldPropagateException()
    {
        var query = new GetAllExamplesDapperQuery();
        _mockReadRepository
            .Setup(r => r.GetAllWithItemsAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("DB error"));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(query, CancellationToken.None));
    }
}

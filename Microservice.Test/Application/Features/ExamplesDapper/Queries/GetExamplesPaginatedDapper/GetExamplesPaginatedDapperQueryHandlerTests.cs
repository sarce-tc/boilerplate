using FluentAssertions;
using Moq;
using Microservice.Application.Contracts.Persistence.Dapper;
using Microservice.Application.DTOs.Dapper;
using Microservice.Application.Features.ExamplesDapper.Queries.GetExamplesPaginatedDapper;
using Microservice.Application.Models;

namespace Microservice.Test.Application.Features.ExamplesDapper.Queries.GetExamplesPaginatedDapper;

public class GetExamplesPaginatedDapperQueryHandlerTests
{
    private readonly Mock<IExampleReadRepository>            _mockReadRepository = new();
    private readonly GetExamplesPaginatedDapperQueryHandler  _handler;

    public GetExamplesPaginatedDapperQueryHandlerTests()
    {
        _handler = new GetExamplesPaginatedDapperQueryHandler(_mockReadRepository.Object);
    }

    private static GetExamplesPaginatedDto Dto(string name, params GetExampleItemDto[] items) =>
        new(Guid.NewGuid(), name, null, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, items);

    [Fact]
    public async Task Handle_ShouldReturnPagedResultWithItems()
    {
        var query = new GetExamplesPaginatedDapperQuery(1, 10);
        var dtos  = new List<GetExamplesPaginatedDto>
        {
            Dto("Alpha", new GetExampleItemDto(Guid.NewGuid(), "A", 2, Domain.Entities.ExampleItemStatus.Pending, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow)),
            Dto("Beta"),
        };
        var paged = new PagedResult<GetExamplesPaginatedDto>(dtos, 2, 1, 10);

        _mockReadRepository
            .Setup(r => r.GetPaginatedWithItemsAsync(1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(paged);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Results.Should().BeSameAs(dtos);
        result.Value.Results.First().Items.Should().ContainSingle(i => i.Label == "A");
    }

    [Fact]
    public async Task Handle_ShouldPreservePageMetadata()
    {
        const int currentPage = 2, pageSize = 5, totalCount = 23;
        var query = new GetExamplesPaginatedDapperQuery(currentPage, pageSize);
        var paged = new PagedResult<GetExamplesPaginatedDto>([], totalCount, currentPage, pageSize);

        _mockReadRepository
            .Setup(r => r.GetPaginatedWithItemsAsync(currentPage, pageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync(paged);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.CurrentPage.Should().Be(currentPage);
        result.Value.PageSize.Should().Be(pageSize);
        result.Value.RowsCount.Should().Be(totalCount);
    }

    [Fact]
    public async Task Handle_ShouldCallGetPaginatedWithItemsAsync_WithCorrectParameters()
    {
        var query = new GetExamplesPaginatedDapperQuery(3, 20);
        _mockReadRepository
            .Setup(r => r.GetPaginatedWithItemsAsync(3, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<GetExamplesPaginatedDto>([], 0, 3, 20));

        await _handler.Handle(query, CancellationToken.None);

        _mockReadRepository.Verify(r => r.GetPaginatedWithItemsAsync(3, 20, It.IsAny<CancellationToken>()), Times.Once);
    }
}

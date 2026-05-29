using AutoMapper;
using FluentAssertions;
using Moq;
using Microservice.Application.Contracts.Persistence.Dapper;
using Microservice.Application.DTOs.Dapper;
using Microservice.Application.Features.ExamplesDapper.Queries.GetExamplesPaginatedDapper;
using Microservice.Application.Models;
using Microservice.Domain.Entities;

namespace Microservice.Test.Application.Features.ExamplesDapper.Queries.GetExamplesPaginatedDapper;

public class GetExamplesPaginatedDapperQueryHandlerTests
{
    private readonly Mock<IExampleReadRepository>                  _mockReadRepository;
    private readonly Mock<IMapper>                                 _mockMapper;
    private readonly GetExamplesPaginatedDapperQueryHandler        _handler;

    public GetExamplesPaginatedDapperQueryHandlerTests()
    {
        _mockReadRepository = new Mock<IExampleReadRepository>();
        _mockMapper         = new Mock<IMapper>();
        _handler            = new GetExamplesPaginatedDapperQueryHandler(_mockReadRepository.Object, _mockMapper.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnPagedResultWithMappedDtos()
    {
        // Arrange
        var query     = new GetExamplesPaginatedDapperQuery(1, 10);
        var examples  = new List<Example> { new("Alpha", null) { Id = 1 }, new("Beta", null) { Id = 2 } };
        var paged     = new PagedResult<Example>(examples, 2, 1, 10);
        var dtos      = new List<GetExamplesPaginatedDto>
        {
            new(Guid.NewGuid(), "Alpha", null, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow),
            new(Guid.NewGuid(), "Beta",  null, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow),
        };

        _mockReadRepository
            .Setup(r => r.GetListPaginatedAsync(1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(paged);
        _mockMapper
            .Setup(m => m.Map<IEnumerable<GetExamplesPaginatedDto>>(examples))
            .Returns(dtos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Results.Should().BeSameAs(dtos);
    }

    [Fact]
    public async Task Handle_ShouldPreservePageMetadata()
    {
        // Arrange
        const int currentPage = 2;
        const int pageSize    = 5;
        const int totalCount  = 23;

        var query    = new GetExamplesPaginatedDapperQuery(currentPage, pageSize);
        var examples = new List<Example> { new("X", null) { Id = 1 } };
        var paged    = new PagedResult<Example>(examples, totalCount, currentPage, pageSize);

        _mockReadRepository
            .Setup(r => r.GetListPaginatedAsync(currentPage, pageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync(paged);
        _mockMapper
            .Setup(m => m.Map<IEnumerable<GetExamplesPaginatedDto>>(It.IsAny<IEnumerable<Example>>()))
            .Returns([]);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.CurrentPage.Should().Be(currentPage);
        result.Value!.PageSize.Should().Be(pageSize);
        result.Value!.RowsCount.Should().Be(totalCount);
    }

    [Fact]
    public async Task Handle_ShouldCallGetListPaginatedAsync_WithCorrectParameters()
    {
        // Arrange
        var query = new GetExamplesPaginatedDapperQuery(3, 20);
        var paged = new PagedResult<Example>([], 0, 3, 20);

        _mockReadRepository
            .Setup(r => r.GetListPaginatedAsync(3, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(paged);
        _mockMapper
            .Setup(m => m.Map<IEnumerable<GetExamplesPaginatedDto>>(It.IsAny<IEnumerable<Example>>()))
            .Returns([]);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _mockReadRepository.Verify(r => r.GetListPaginatedAsync(3, 20, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldMapEntitiesToDtos()
    {
        // Arrange
        var query    = new GetExamplesPaginatedDapperQuery(1, 10);
        var examples = new List<Example> { new("Item", "Desc") { Id = 1 } };
        var paged    = new PagedResult<Example>(examples, 1, 1, 10);

        _mockReadRepository
            .Setup(r => r.GetListPaginatedAsync(1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(paged);
        _mockMapper
            .Setup(m => m.Map<IEnumerable<GetExamplesPaginatedDto>>(It.IsAny<IEnumerable<Example>>()))
            .Returns([]);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _mockMapper.Verify(
            m => m.Map<IEnumerable<GetExamplesPaginatedDto>>(It.IsAny<IEnumerable<Example>>()),
            Times.Once);
    }
}

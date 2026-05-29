using FluentAssertions;
using Moq;
using AutoMapper;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Application.DTOs.EF;
using Microservice.Application.Features.ExamplesEF.Queries.GetExamplesPaginated;
using Microservice.Application.Models;
using Microservice.Domain.Entities;
using System.Linq.Expressions;

namespace Microservice.Test.Application.Features.Examples.Queries.GetExamplesPaginated;
/// <summary>
/// Unit tests for GetExamplesPaginatedQueryHandler
/// Tests paginated data retrieval
/// </summary>
public class GetExamplesPaginatedQueryHandlerTests
{
    private readonly Mock<IReadRepository<Example>> _mockReadRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly GetExamplesPaginatedQueryHandler _handler;

    public GetExamplesPaginatedQueryHandlerTests()
    {
        _mockReadRepository = new Mock<IReadRepository<Example>>();
        _mockMapper = new Mock<IMapper>();
        _handler = new GetExamplesPaginatedQueryHandler(_mockReadRepository.Object, _mockMapper.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnPaginatedResults()
    {
        // Arrange
        var query = new GetExamplesPaginatedQuery(1, 10);
        var examples = new List<Example>
        {
            new Example("Test1", "Desc1") { Id = 1 },
            new Example("Test2", "Desc2") { Id = 2 }
        };
        var pagedResult = new PagedResult<Example>(examples, 25, 1, 10);
        List<GetExamplesPaginatedDto> dtos = [];

        _mockReadRepository
            .Setup(r => r.GetListPaginatedAsync(
                1,
                10,
                It.IsAny<Expression<Func<Example, bool>>>(),
                It.IsAny<Func<IQueryable<Example>, IOrderedQueryable<Example>>>(),
                It.IsAny<IEnumerable<Expression<Func<Example, object>>>>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        _mockMapper
            .Setup(m => m.Map<IEnumerable<GetExamplesPaginatedDto>>(examples))
            .Returns(dtos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WithFirstPage_ShouldReturnFirstPageData()
    {
        // Arrange
        var query = new GetExamplesPaginatedQuery(1, 5);
        var examples = Enumerable.Range(1, 5).Select(i => new Example($"Test{i}", "Desc") { Id = i }).ToList();
        var pagedResult = new PagedResult<Example>(examples, 50, 1, 5);
        List<GetExamplesPaginatedDto> dtos = [];

        _mockReadRepository
            .Setup(r => r.GetListPaginatedAsync(
                1,
                5,
                It.IsAny<Expression<Func<Example, bool>>>(),
                It.IsAny<Func<IQueryable<Example>, IOrderedQueryable<Example>>>(),
                It.IsAny<IEnumerable<Expression<Func<Example, object>>>>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        _mockMapper
            .Setup(m => m.Map<IEnumerable<GetExamplesPaginatedDto>>(examples))
            .Returns(dtos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.CurrentPage.Should().Be(1);
        result.Value!.PageSize.Should().Be(5);
    }

    [Fact]
    public async Task Handle_WithLastPage_ShouldReturnLastPageData()
    {
        // Arrange
        var query = new GetExamplesPaginatedQuery(3, 5);
        var examples = Enumerable.Range(11, 5).Select(i => new Example($"Test{i}", "Desc") { Id = i }).ToList();
        var pagedResult = new PagedResult<Example>(examples, 25, 3, 5);
        List<GetExamplesPaginatedDto> dtos = [];

        _mockReadRepository
            .Setup(r => r.GetListPaginatedAsync(
                3,
                5,
                It.IsAny<Expression<Func<Example, bool>>>(),
                It.IsAny<Func<IQueryable<Example>, IOrderedQueryable<Example>>>(),
                It.IsAny<IEnumerable<Expression<Func<Example, object>>>>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        _mockMapper
            .Setup(m => m.Map<IEnumerable<GetExamplesPaginatedDto>>(examples))
            .Returns(dtos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.CurrentPage.Should().Be(3);
    }

    [Fact]
    public async Task Handle_ShouldCallGetListPaginatedAsync()
    {
        // Arrange
        var query = new GetExamplesPaginatedQuery(2, 10);

        _mockReadRepository
            .Setup(r => r.GetListPaginatedAsync(
                2,
                10,
                It.IsAny<Expression<Func<Example, bool>>>(),
                It.IsAny<Func<IQueryable<Example>, IOrderedQueryable<Example>>>(),
                It.IsAny<IEnumerable<Expression<Func<Example, object>>>>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<Example>([], 0, 2, 10));

        _mockMapper
            .Setup(m => m.Map<IEnumerable<GetExamplesPaginatedDto>>(It.IsAny<IEnumerable<Example>>()))
            .Returns([]);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _mockReadRepository.Verify(
            r => r.GetListPaginatedAsync(
                2,
                10,
                It.IsAny<Expression<Func<Example, bool>>>(),
                It.IsAny<Func<IQueryable<Example>, IOrderedQueryable<Example>>>(),
                It.IsAny<IEnumerable<Expression<Func<Example, object>>>>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineData(1, 10)]
    [InlineData(2, 20)]
    [InlineData(5, 100)]
    public async Task Handle_WithDifferentPages_ShouldCallWithCorrectParameters(int page, int pageSize)
    {
        // Arrange
        var query = new GetExamplesPaginatedQuery(page, pageSize);

        _mockReadRepository
            .Setup(r => r.GetListPaginatedAsync(
                page,
                pageSize,
                It.IsAny<Expression<Func<Example, bool>>>(),
                It.IsAny<Func<IQueryable<Example>, IOrderedQueryable<Example>>>(),
                It.IsAny<IEnumerable<Expression<Func<Example, object>>>>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<Example>([], 0, page, pageSize));

        _mockMapper
            .Setup(m => m.Map<IEnumerable<GetExamplesPaginatedDto>>(It.IsAny<IEnumerable<Example>>()))
            .Returns([]);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _mockReadRepository.Verify(
            r => r.GetListPaginatedAsync(page, pageSize, It.IsAny<Expression<Func<Example, bool>>>(), It.IsAny<Func<IQueryable<Example>, IOrderedQueryable<Example>>>(), It.IsAny<IEnumerable<Expression<Func<Example, object>>>>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldRespectCancellationToken()
    {
        // Arrange
        var query = new GetExamplesPaginatedQuery(1, 10);
        var cancellationToken = new CancellationToken(canceled: false);

        _mockReadRepository
            .Setup(r => r.GetListPaginatedAsync(
                1,
                10,
                It.IsAny<Expression<Func<Example, bool>>>(),
                It.IsAny<Func<IQueryable<Example>, IOrderedQueryable<Example>>>(),
                It.IsAny<IEnumerable<Expression<Func<Example, object>>>>(),
                It.IsAny<bool>(),
                cancellationToken))
            .ReturnsAsync(new PagedResult<Example>([], 0, 1, 10));

        _mockMapper
            .Setup(m => m.Map<IEnumerable<GetExamplesPaginatedDto>>(It.IsAny<IEnumerable<Example>>()))
            .Returns([]);

        // Act
        await _handler.Handle(query, cancellationToken);

        // Assert
        _mockReadRepository.Verify(
            r => r.GetListPaginatedAsync(1, 10, It.IsAny<Expression<Func<Example, bool>>>(), It.IsAny<Func<IQueryable<Example>, IOrderedQueryable<Example>>>(), It.IsAny<IEnumerable<Expression<Func<Example, object>>>>(), It.IsAny<bool>(), cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldPropagateException()
    {
        // Arrange
        var query = new GetExamplesPaginatedQuery(1, 10);

        _mockReadRepository
            .Setup(r => r.GetListPaginatedAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<Expression<Func<Example, bool>>>(),
                It.IsAny<Func<IQueryable<Example>, IOrderedQueryable<Example>>>(),
                It.IsAny<IEnumerable<Expression<Func<Example, object>>>>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(query, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldMapEntitiesToDtos()
    {
        // Arrange
        var query = new GetExamplesPaginatedQuery(1, 10);
        var examples = new List<Example> { new Example("Test", "Description") { Id = 1 } };
        var pagedResult = new PagedResult<Example>(examples, 1, 1, 10);

        _mockReadRepository
            .Setup(r => r.GetListPaginatedAsync(
                1,
                10,
                It.IsAny<Expression<Func<Example, bool>>>(),
                It.IsAny<Func<IQueryable<Example>, IOrderedQueryable<Example>>>(),
                It.IsAny<IEnumerable<Expression<Func<Example, object>>>>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        _mockMapper
            .Setup(m => m.Map<IEnumerable<GetExamplesPaginatedDto>>(examples))
            .Returns([]);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _mockMapper.Verify(
            m => m.Map<IEnumerable<GetExamplesPaginatedDto>>(examples),
            Times.Once);
    }
}

using AutoMapper;
using FluentAssertions;
using Moq;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.Dapper;
using Microservice.Application.DTOs.Dapper;
using Microservice.Application.Features.ExamplesDapper.Queries.GetAllExamplesDapper;
using Microservice.Domain.Entities;

namespace Microservice.Test.Application.Features.ExamplesDapper.Queries.GetAllExamplesDapper;

public class GetAllExamplesDapperQueryHandlerTests
{
    private readonly Mock<IExampleReadRepository>         _mockReadRepository;
    private readonly Mock<IMapper>                        _mockMapper;
    private readonly GetAllExamplesDapperQueryHandler     _handler;

    public GetAllExamplesDapperQueryHandlerTests()
    {
        _mockReadRepository = new Mock<IExampleReadRepository>();
        _mockMapper         = new Mock<IMapper>();
        _handler            = new GetAllExamplesDapperQueryHandler(_mockReadRepository.Object, _mockMapper.Object);
    }

    [Fact]
    public async Task Handle_WithExistingRecords_ShouldReturnSuccessWithMappedDtos()
    {
        // Arrange
        var query   = new GetAllExamplesDapperQuery();
        var examples = new List<Example>
        {
            new("Test1", "Desc1") { Id = 1 },
            new("Test2", "Desc2") { Id = 2 },
        };
        var dtos = new List<GetAllExamplesDapperDto>
        {
            new(Guid.NewGuid(), "Test1", "Desc1", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow),
            new(Guid.NewGuid(), "Test2", "Desc2", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow),
        };

        _mockReadRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(examples);
        _mockMapper
            .Setup(m => m.Map<IEnumerable<GetAllExamplesDapperDto>>(examples))
            .Returns(dtos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(dtos);
    }

    [Fact]
    public async Task Handle_WithEmptyTable_ShouldReturnSuccessWithEmptyCollection()
    {
        // Arrange
        var query = new GetAllExamplesDapperQuery();
        List<Example>                   emptyEntities = [];
        List<GetAllExamplesDapperDto>   emptyDtos     = [];

        _mockReadRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyEntities);
        _mockMapper
            .Setup(m => m.Map<IEnumerable<GetAllExamplesDapperDto>>(emptyEntities))
            .Returns(emptyDtos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldCallGetAllAsync_Once()
    {
        // Arrange
        var query = new GetAllExamplesDapperQuery();

        _mockReadRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        _mockMapper
            .Setup(m => m.Map<IEnumerable<GetAllExamplesDapperDto>>(It.IsAny<IEnumerable<Example>>()))
            .Returns([]);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _mockReadRepository.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldMapEntitiesToDtos()
    {
        // Arrange
        var query    = new GetAllExamplesDapperQuery();
        var examples = new List<Example> { new("Name", "Desc") { Id = 1 } };

        _mockReadRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(examples);
        _mockMapper
            .Setup(m => m.Map<IEnumerable<GetAllExamplesDapperDto>>(It.IsAny<IEnumerable<Example>>()))
            .Returns([]);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _mockMapper.Verify(
            m => m.Map<IEnumerable<GetAllExamplesDapperDto>>(It.IsAny<IEnumerable<Example>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldPropagateException()
    {
        // Arrange
        var query = new GetAllExamplesDapperQuery();

        _mockReadRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("DB error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(query, CancellationToken.None));
    }
}

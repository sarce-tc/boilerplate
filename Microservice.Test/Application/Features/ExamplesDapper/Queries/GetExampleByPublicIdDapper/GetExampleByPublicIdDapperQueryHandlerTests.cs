using AutoMapper;
using FluentAssertions;
using Moq;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.Dapper;
using Microservice.Application.DTOs.Dapper;
using Microservice.Application.Features.ExamplesDapper.Queries.GetExampleByPublicIdDapper;
using Microservice.Domain.Entities;

namespace Microservice.Test.Application.Features.ExamplesDapper.Queries.GetExampleByPublicIdDapper;

public class GetExampleByPublicIdDapperQueryHandlerTests
{
    private readonly Mock<IExampleReadRepository>                  _mockReadRepository;
    private readonly Mock<IMapper>                                 _mockMapper;
    private readonly GetExampleByPublicIdDapperQueryHandler        _handler;

    public GetExampleByPublicIdDapperQueryHandlerTests()
    {
        _mockReadRepository = new Mock<IExampleReadRepository>();
        _mockMapper         = new Mock<IMapper>();
        _handler            = new GetExampleByPublicIdDapperQueryHandler(_mockReadRepository.Object, _mockMapper.Object);
    }

    [Fact]
    public async Task Handle_WithExistingPublicId_ShouldReturnSuccessWithDto()
    {
        // Arrange
        var publicId = Guid.NewGuid();
        var query    = new GetExampleByPublicIdDapperQuery(publicId);
        var example  = new Example("Sample", "Desc") { Id = 1, PublicId = publicId };
        var dto      = new GetExampleByPublicIdDto(publicId, "Sample", "Desc", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);

        _mockReadRepository
            .Setup(r => r.GetByPublicIdAsync(publicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(example);
        _mockMapper
            .Setup(m => m.Map<GetExampleByPublicIdDto>(example))
            .Returns(dto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(dto);
    }

    [Fact]
    public async Task Handle_WithNonExistentPublicId_ShouldReturnNotFoundFailure()
    {
        // Arrange
        var publicId = Guid.NewGuid();
        var query    = new GetExampleByPublicIdDapperQuery(publicId);

        _mockReadRepository
            .Setup(r => r.GetByPublicIdAsync(publicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Example?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Code == "NotFound");
    }

    [Fact]
    public async Task Handle_ShouldCallGetByPublicIdAsync_WithCorrectPublicId()
    {
        // Arrange
        var publicId = Guid.NewGuid();
        var query    = new GetExampleByPublicIdDapperQuery(publicId);
        var example  = new Example("Name", null) { Id = 1, PublicId = publicId };

        _mockReadRepository
            .Setup(r => r.GetByPublicIdAsync(publicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(example);
        _mockMapper
            .Setup(m => m.Map<GetExampleByPublicIdDto>(It.IsAny<Example>()))
            .Returns(new GetExampleByPublicIdDto(publicId, "Name", null, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow));

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _mockReadRepository.Verify(r => r.GetByPublicIdAsync(publicId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenFound_ShouldMapEntityToDto()
    {
        // Arrange
        var publicId = Guid.NewGuid();
        var query    = new GetExampleByPublicIdDapperQuery(publicId);
        var example  = new Example("Name", "Desc") { Id = 1, PublicId = publicId };

        _mockReadRepository
            .Setup(r => r.GetByPublicIdAsync(publicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(example);
        _mockMapper
            .Setup(m => m.Map<GetExampleByPublicIdDto>(example))
            .Returns(new GetExampleByPublicIdDto(publicId, "Name", "Desc", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow));

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _mockMapper.Verify(m => m.Map<GetExampleByPublicIdDto>(example), Times.Once);
    }
}

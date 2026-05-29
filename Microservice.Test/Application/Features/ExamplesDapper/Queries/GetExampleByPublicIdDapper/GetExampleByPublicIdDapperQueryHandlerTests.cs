using FluentAssertions;
using Moq;
using Microservice.Application.Contracts.Persistence.Dapper;
using Microservice.Application.DTOs.Dapper;
using Microservice.Application.Features.ExamplesDapper.Queries.GetExampleByPublicIdDapper;

namespace Microservice.Test.Application.Features.ExamplesDapper.Queries.GetExampleByPublicIdDapper;

public class GetExampleByPublicIdDapperQueryHandlerTests
{
    private readonly Mock<IExampleReadRepository>           _mockReadRepository = new();
    private readonly GetExampleByPublicIdDapperQueryHandler _handler;

    public GetExampleByPublicIdDapperQueryHandlerTests()
    {
        _handler = new GetExampleByPublicIdDapperQueryHandler(_mockReadRepository.Object);
    }

    [Fact]
    public async Task Handle_WithExistingPublicId_ShouldReturnDtoWithItems()
    {
        var publicId = Guid.NewGuid();
        var query    = new GetExampleByPublicIdDapperQuery(publicId);
        var dto      = new GetExampleByPublicIdDto(
            publicId, "Sample", "Desc", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow,
            [new(Guid.NewGuid(), "A", 2, Domain.Entities.ExampleItemStatus.Pending, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow)]);

        _mockReadRepository
            .Setup(r => r.GetByPublicIdWithItemsAsync(publicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(dto);
        result.Value!.Items.Should().ContainSingle(i => i.Label == "A");
    }

    [Fact]
    public async Task Handle_WithNonExistentPublicId_ShouldReturnNotFoundFailure()
    {
        var publicId = Guid.NewGuid();
        var query    = new GetExampleByPublicIdDapperQuery(publicId);

        _mockReadRepository
            .Setup(r => r.GetByPublicIdWithItemsAsync(publicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetExampleByPublicIdDto?)null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Code == "NotFound");
    }

    [Fact]
    public async Task Handle_ShouldCallGetByPublicIdWithItemsAsync()
    {
        var publicId = Guid.NewGuid();
        var query    = new GetExampleByPublicIdDapperQuery(publicId);

        _mockReadRepository
            .Setup(r => r.GetByPublicIdWithItemsAsync(publicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetExampleByPublicIdDto(publicId, "N", null, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, []));

        await _handler.Handle(query, CancellationToken.None);

        _mockReadRepository.Verify(r => r.GetByPublicIdWithItemsAsync(publicId, It.IsAny<CancellationToken>()), Times.Once);
    }
}

using FluentAssertions;
using Microservice.Application.Features.ExamplesEF.Queries.GetExampleSummary;
using Microservice.Application.Services;
using Microservice.Domain.Entities;
using Moq;

namespace Microservice.Test.Application.Features.ExamplesEF.Queries.GetExampleSummary;

public class GetExampleSummaryQueryHandlerTests
{
    private readonly Mock<IExampleService>       _mockExampleService;
    private readonly GetExampleSummaryQueryHandler _handler;

    public GetExampleSummaryQueryHandlerTests()
    {
        _mockExampleService = new Mock<IExampleService>();
        _handler = new GetExampleSummaryQueryHandler(_mockExampleService.Object);
    }

    private void SetupFind(Example? example) =>
        _mockExampleService
            .Setup(s => s.FindWithItemsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(example);

    private static Example BuildExampleWithMixedItems()
    {
        var example = new Example("Mixed", "Description");
        example.AddItem("Pending A", 1);
        example.AddItem("Pending B", 2);
        var completedItem = example.AddItem("Completed C", 3);
        example.CompleteItem(completedItem.PublicId);
        return example;
    }

    [Fact]
    public async Task Handle_WithExistingExample_ShouldReturnSuccess()
    {
        // Arrange
        var publicId = Guid.NewGuid();
        var example  = new Example("Test", "Desc");
        var query    = new GetExampleSummaryQuery(publicId);
        SetupFind(example);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WithExistingExample_ShouldMapScalarFieldsCorrectly()
    {
        // Arrange
        var example = new Example("My Example", "My Description");
        SetupFind(example);

        // Act
        var result = await _handler.Handle(new GetExampleSummaryQuery(Guid.NewGuid()), CancellationToken.None);

        // Assert
        result.Value!.Name.Should().Be("My Example");
        result.Value.Description.Should().Be("My Description");
        result.Value.Status.Should().Be(ExampleStatus.Active);
        result.Value.PublicId.Should().Be(example.PublicId);
    }

    [Fact]
    public async Task Handle_WithMixedItemStatuses_ShouldComputeCountsCorrectly()
    {
        // Arrange — 2 pending + 1 completed = 3 total
        var example = BuildExampleWithMixedItems();
        SetupFind(example);

        // Act
        var result = await _handler.Handle(new GetExampleSummaryQuery(example.PublicId), CancellationToken.None);

        // Assert
        result.Value!.ItemCount.Should().Be(3);
        result.Value.PendingItemCount.Should().Be(2);
        result.Value.CompletedItemCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WithNoItems_ShouldReturnZeroCounts()
    {
        // Arrange
        var example = new Example("Empty", null);
        SetupFind(example);

        // Act
        var result = await _handler.Handle(new GetExampleSummaryQuery(example.PublicId), CancellationToken.None);

        // Assert
        result.Value!.ItemCount.Should().Be(0);
        result.Value.PendingItemCount.Should().Be(0);
        result.Value.CompletedItemCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WithAllPendingItems_ShouldReturnZeroCompletedCount()
    {
        // Arrange
        var example = new Example("Pending only", null);
        example.AddItem("A", 1);
        example.AddItem("B", 2);
        SetupFind(example);

        // Act
        var result = await _handler.Handle(new GetExampleSummaryQuery(example.PublicId), CancellationToken.None);

        // Assert
        result.Value!.PendingItemCount.Should().Be(2);
        result.Value.CompletedItemCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WithNonExistentExample_ShouldReturnNotFound()
    {
        // Arrange
        var publicId = Guid.NewGuid();
        SetupFind(null);

        // Act
        var result = await _handler.Handle(new GetExampleSummaryQuery(publicId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors[0].Code.Should().Be("NotFound");
        result.Errors[0].Message.Should().Contain(publicId.ToString());
    }

    [Fact]
    public async Task Handle_ShouldCallFindWithItemsAsync_WithCorrectPublicId()
    {
        // Arrange
        var publicId = Guid.NewGuid();
        _mockExampleService
            .Setup(s => s.FindWithItemsAsync(publicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Example?)null);

        // Act
        await _handler.Handle(new GetExampleSummaryQuery(publicId), CancellationToken.None);

        // Assert
        _mockExampleService.Verify(s => s.FindWithItemsAsync(publicId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentExample_ShouldNotBuildDto()
    {
        // Arrange — service returns null; handler must exit before constructing the DTO
        SetupFind(null);

        // Act
        var result = await _handler.Handle(new GetExampleSummaryQuery(Guid.NewGuid()), CancellationToken.None);

        // Assert
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenServiceThrows_ShouldPropagateException()
    {
        // Arrange
        _mockExampleService
            .Setup(s => s.FindWithItemsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(new GetExampleSummaryQuery(Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldRespectCancellationToken()
    {
        // Arrange
        var cancellationToken = new CancellationToken(canceled: false);
        var publicId          = Guid.NewGuid();
        _mockExampleService
            .Setup(s => s.FindWithItemsAsync(publicId, cancellationToken))
            .ReturnsAsync((Example?)null);

        // Act
        await _handler.Handle(new GetExampleSummaryQuery(publicId), cancellationToken);

        // Assert
        _mockExampleService.Verify(s => s.FindWithItemsAsync(publicId, cancellationToken), Times.Once);
    }
}

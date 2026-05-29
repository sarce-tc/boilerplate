using AutoMapper;
using FluentAssertions;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Application.DTOs.EF;
using Microservice.Application.Features.ExamplesEF.Queries.GetExampleItemByPublicId;
using Microservice.Domain.Entities;
using Moq;
using System.Linq.Expressions;

namespace Microservice.Test.Application.Features.ExamplesEF.Queries.GetExampleItemByPublicId;
public class GetExampleItemByPublicIdQueryHandlerTests
{
    private readonly Mock<IReadRepository<Example>> _mockReadRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly GetExampleItemByPublicIdQueryHandler _handler;

    public GetExampleItemByPublicIdQueryHandlerTests()
    {
        _mockReadRepository = new Mock<IReadRepository<Example>>();
        _mockMapper = new Mock<IMapper>();
        _handler = new GetExampleItemByPublicIdQueryHandler(_mockReadRepository.Object, _mockMapper.Object);
    }

    private void SetupRead(Example? example) =>
        _mockReadRepository
            .Setup(r => r.GetEntityAsync(
                It.IsAny<Expression<Func<Example, bool>>>(),
                null,
                It.IsAny<IEnumerable<Expression<Func<Example, object>>>>(),
                true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(example);

    private static Example BuildExampleWithItem(out ExampleItem addedItem)
    {
        var example = new Example("Test", "Description");
        example.AddItem("Item A", 5);
        addedItem = example.Items.First();
        return example;
    }

    [Fact]
    public async Task Handle_WithValidExampleAndItem_ShouldReturnItemDto()
    {
        // Arrange
        var examplePublicId = Guid.NewGuid();
        var example = BuildExampleWithItem(out var item);
        var query = new GetExampleItemByPublicIdQuery(examplePublicId, item.PublicId);
        var dto = new GetExampleItemDto(item.PublicId, item.Label, item.Quantity,
            item.Status, item.CreatedAt, item.UpdatedAt);

        SetupRead(example);
        _mockMapper.Setup(m => m.Map<GetExampleItemDto>(item)).Returns(dto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(dto);
    }

    [Fact]
    public async Task Handle_WithNonExistentExample_ShouldReturnNotFound()
    {
        // Arrange
        var query = new GetExampleItemByPublicIdQuery(Guid.NewGuid(), Guid.NewGuid());
        SetupRead(null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors[0].Code.Should().Be("NotFound");
        result.Errors[0].Message.Should().Contain("Example");
    }

    [Fact]
    public async Task Handle_WithExampleButMissingItem_ShouldReturnNotFound()
    {
        // Arrange
        var example = new Example("Test", null);
        var query = new GetExampleItemByPublicIdQuery(Guid.NewGuid(), Guid.NewGuid());
        SetupRead(example);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors[0].Code.Should().Be("NotFound");
        result.Errors[0].Message.Should().Contain("Item");
    }

    [Fact]
    public async Task Handle_WithMissingItem_ShouldNotMapEntity()
    {
        // Arrange
        var example = new Example("Test", null);
        var query = new GetExampleItemByPublicIdQuery(Guid.NewGuid(), Guid.NewGuid());
        SetupRead(example);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _mockMapper.Verify(m => m.Map<GetExampleItemDto>(It.IsAny<ExampleItem>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldCallGetEntityAsync_WithIncludeProperties()
    {
        // Arrange
        var example = BuildExampleWithItem(out var item);
        var query = new GetExampleItemByPublicIdQuery(Guid.NewGuid(), item.PublicId);

        SetupRead(example);
        _mockMapper
            .Setup(m => m.Map<GetExampleItemDto>(item))
            .Returns(new GetExampleItemDto(item.PublicId, item.Label, item.Quantity,
                item.Status, item.CreatedAt, item.UpdatedAt));

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _mockReadRepository.Verify(
            r => r.GetEntityAsync(
                It.IsAny<Expression<Func<Example, bool>>>(),
                null,
                It.IsAny<IEnumerable<Expression<Func<Example, object>>>>(),
                true,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithMultipleItems_ShouldReturnCorrectOne()
    {
        // Arrange
        var example = new Example("Multi", null);
        example.AddItem("First", 1);
        example.AddItem("Second", 2);
        var targetItem = example.Items.Last();
        var query = new GetExampleItemByPublicIdQuery(Guid.NewGuid(), targetItem.PublicId);
        var dto = new GetExampleItemDto(targetItem.PublicId, targetItem.Label, targetItem.Quantity,
            targetItem.Status, targetItem.CreatedAt, targetItem.UpdatedAt);

        SetupRead(example);
        _mockMapper.Setup(m => m.Map<GetExampleItemDto>(targetItem)).Returns(dto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.PublicId.Should().Be(targetItem.PublicId);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldPropagateException()
    {
        // Arrange
        var query = new GetExampleItemByPublicIdQuery(Guid.NewGuid(), Guid.NewGuid());

        _mockReadRepository
            .Setup(r => r.GetEntityAsync(
                It.IsAny<Expression<Func<Example, bool>>>(),
                null,
                It.IsAny<IEnumerable<Expression<Func<Example, object>>>>(),
                true,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(query, CancellationToken.None));
    }
}

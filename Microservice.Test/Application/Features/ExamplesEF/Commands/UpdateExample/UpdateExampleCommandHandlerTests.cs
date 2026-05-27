using System.Linq.Expressions;
using FluentAssertions;
using Moq;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Application.Features.ExamplesEF.Commands.UpdateExample;
using Microservice.Domain.Entities;
using Microservice.Domain.Exceptions;

namespace Microservice.Test.Application.Features.ExamplesEF.Commands.UpdateExample;

public class UpdateExampleCommandHandlerTests
{
    private readonly Mock<IReadRepository<Example>>  _mockReadRepository;
    private readonly Mock<IExampleWriteRepository>   _mockExamplesWrite;
    private readonly Mock<IUnitOfWork>               _mockUnitOfWork;
    private readonly UpdateExampleCommandHandler     _handler;

    public UpdateExampleCommandHandlerTests()
    {
        _mockReadRepository = new Mock<IReadRepository<Example>>();
        _mockExamplesWrite  = new Mock<IExampleWriteRepository>();
        _mockUnitOfWork     = new Mock<IUnitOfWork>();

        _mockUnitOfWork.Setup(u => u.ExamplesWrite).Returns(_mockExamplesWrite.Object);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _handler = new UpdateExampleCommandHandler(_mockReadRepository.Object, _mockUnitOfWork.Object);
    }

    // Helper: sets up the read repo to return the given example for any GetEntityAsync call.
    private void SetupRead(Example example) =>
        _mockReadRepository
            .Setup(r => r.GetEntityAsync(
                It.IsAny<Expression<Func<Example, bool>>>(),
                It.IsAny<Expression<Func<Example, Example>>>(),
                It.IsAny<IEnumerable<Expression<Func<Example, object>>>>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(example);

    private void SetupReadNull() =>
        _mockReadRepository
            .Setup(r => r.GetEntityAsync(
                It.IsAny<Expression<Func<Example, bool>>>(),
                It.IsAny<Expression<Func<Example, Example>>>(),
                It.IsAny<IEnumerable<Expression<Func<Example, object>>>>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Example?)null);

    // ── Scalar field updates ──────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WithExistingPublicId_ShouldUpdateAndReturnSuccess()
    {
        var publicId = Guid.NewGuid();
        var example  = new Example("Test", "Description") { Id = 1, PublicId = publicId };
        SetupRead(example);

        var result = await _handler.Handle(
            new UpdateExampleCommand(publicId, "Updated Name", "Updated Description"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(publicId);
        _mockExamplesWrite.Verify(r => r.Update(example), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentPublicId_ShouldReturnFailure()
    {
        SetupReadNull();

        var result = await _handler.Handle(
            new UpdateExampleCommand(Guid.NewGuid(), null, null),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors[0].Code.Should().Be("NotFound");
        _mockExamplesWrite.Verify(r => r.Update(It.IsAny<Example>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldCallGetEntityAsyncWithIncludeItems()
    {
        var publicId = Guid.NewGuid();
        var example  = new Example("Test", "Description") { PublicId = publicId };
        SetupRead(example);

        await _handler.Handle(new UpdateExampleCommand(publicId, null, null), CancellationToken.None);

        _mockReadRepository.Verify(
            r => r.GetEntityAsync(
                It.IsAny<Expression<Func<Example, bool>>>(),
                It.IsAny<Expression<Func<Example, Example>>>(),
                It.Is<IEnumerable<Expression<Func<Example, object>>>>(p => p != null),
                false,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCallUpdateWithCorrectExample()
    {
        var publicId = Guid.NewGuid();
        var example  = new Example("Test", "Description") { PublicId = publicId };
        SetupRead(example);

        await _handler.Handle(new UpdateExampleCommand(publicId, "New Name", null), CancellationToken.None);

        _mockExamplesWrite.Verify(r => r.Update(example), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldSaveChangesAfterUpdate()
    {
        var publicId = Guid.NewGuid();
        var example  = new Example("Test", "Description") { PublicId = publicId };
        SetupRead(example);

        await _handler.Handle(new UpdateExampleCommand(publicId, null, "New Description"), CancellationToken.None);

        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldRespectCancellationToken()
    {
        var publicId          = Guid.NewGuid();
        var example           = new Example("Test", "Description") { PublicId = publicId };
        var cancellationToken = new CancellationToken(canceled: false);
        SetupRead(example);

        await _handler.Handle(new UpdateExampleCommand(publicId, null, null), cancellationToken);

        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldPropagateException()
    {
        _mockReadRepository
            .Setup(r => r.GetEntityAsync(
                It.IsAny<Expression<Func<Example, bool>>>(),
                It.IsAny<Expression<Func<Example, Example>>>(),
                It.IsAny<IEnumerable<Expression<Func<Example, object>>>>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(new UpdateExampleCommand(Guid.NewGuid(), null, null), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenSaveChangesThrows_ShouldPropagateException()
    {
        var publicId = Guid.NewGuid();
        var example  = new Example("Test", "Description") { PublicId = publicId };
        SetupRead(example);
        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Concurrency error"));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(new UpdateExampleCommand(publicId, null, null), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithNonExistentPublicId_ShouldNotUpdateEntity()
    {
        SetupReadNull();

        await _handler.Handle(new UpdateExampleCommand(Guid.NewGuid(), null, null), CancellationToken.None);

        _mockExamplesWrite.Verify(r => r.Update(It.IsAny<Example>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFoundError_WhenNotFound()
    {
        var publicId = Guid.NewGuid();
        SetupReadNull();

        var result = await _handler.Handle(new UpdateExampleCommand(publicId, null, null), CancellationToken.None);

        result.Errors[0].Message.Should().Contain(publicId.ToString());
        result.Errors[0].Message.Should().Contain("not found");
    }

    // ── AddItems ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WithAddItems_ShouldAddItemsToExampleViaAddItem()
    {
        var publicId = Guid.NewGuid();
        var example  = new Example("Test", null) { PublicId = publicId };
        SetupRead(example);

        var addItems = new List<UpdateExampleItemRequest> { new("Widget", 3), new("Gadget", 7) };
        await _handler.Handle(new UpdateExampleCommand(publicId, null, null, AddItems: addItems), CancellationToken.None);

        example.Items.Should().HaveCount(2);
        example.Items[0].Label.Should().Be("Widget");
        example.Items[0].Quantity.Should().Be(3);
        example.Items[1].Label.Should().Be("Gadget");
        example.Items[1].Quantity.Should().Be(7);
    }

    [Fact]
    public async Task Handle_WithNullAddItems_ShouldNotAddAnyItems()
    {
        var publicId = Guid.NewGuid();
        var example  = new Example("Test", null) { PublicId = publicId };
        SetupRead(example);

        await _handler.Handle(new UpdateExampleCommand(publicId, null, null, AddItems: null), CancellationToken.None);

        example.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithDuplicateAddItemLabel_ShouldThrowDomainException()
    {
        var publicId = Guid.NewGuid();
        var example  = new Example("Test", null) { PublicId = publicId };
        example.AddItem("Widget", 1);
        SetupRead(example);

        var addItems = new List<UpdateExampleItemRequest> { new("Widget", 5) };

        await Assert.ThrowsAsync<DomainException>(
            () => _handler.Handle(new UpdateExampleCommand(publicId, null, null, AddItems: addItems), CancellationToken.None));
    }

    // ── RemoveItemIds ─────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WithRemoveItemIds_ShouldRemoveItemsFromExample()
    {
        var publicId = Guid.NewGuid();
        var example  = new Example("Test", null) { PublicId = publicId };
        example.AddItem("Alpha", 1);
        example.AddItem("Beta",  2);
        var itemToRemove = example.Items[0].PublicId;
        SetupRead(example);

        await _handler.Handle(
            new UpdateExampleCommand(publicId, null, null, RemoveItemIds: [itemToRemove]),
            CancellationToken.None);

        example.Items.Should().HaveCount(1);
        example.Items[0].Label.Should().Be("Beta");
    }

    [Fact]
    public async Task Handle_WithNonExistentRemoveItemId_ShouldThrowDomainException()
    {
        var publicId = Guid.NewGuid();
        var example  = new Example("Test", null) { PublicId = publicId };
        SetupRead(example);

        await Assert.ThrowsAsync<DomainException>(
            () => _handler.Handle(
                new UpdateExampleCommand(publicId, null, null, RemoveItemIds: [Guid.NewGuid()]),
                CancellationToken.None));
    }

    // ── CompleteItemIds ───────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WithCompleteItemIds_ShouldMarkItemAsCompleted()
    {
        var publicId = Guid.NewGuid();
        var example  = new Example("Test", null) { PublicId = publicId };
        example.AddItem("Task A", 1);
        var itemId = example.Items[0].PublicId;
        SetupRead(example);

        await _handler.Handle(
            new UpdateExampleCommand(publicId, null, null, CompleteItemIds: [itemId]),
            CancellationToken.None);

        example.Items[0].Status.Should().Be(ExampleItemStatus.Completed);
    }

    [Fact]
    public async Task Handle_WithAlreadyCompletedItemId_ShouldThrowDomainException()
    {
        var publicId = Guid.NewGuid();
        var example  = new Example("Test", null) { PublicId = publicId };
        example.AddItem("Task A", 1);
        var itemId = example.Items[0].PublicId;
        example.CompleteItem(itemId);
        SetupRead(example);

        await Assert.ThrowsAsync<DomainException>(
            () => _handler.Handle(
                new UpdateExampleCommand(publicId, null, null, CompleteItemIds: [itemId]),
                CancellationToken.None));
    }

    // ── Combined ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WithScalarAndItemChanges_ShouldApplyAll()
    {
        var publicId = Guid.NewGuid();
        var example  = new Example("Old Name", null) { PublicId = publicId };
        example.AddItem("Existing", 1);
        var existingId = example.Items[0].PublicId;
        SetupRead(example);

        var addItems = new List<UpdateExampleItemRequest> { new("New Item", 10) };
        await _handler.Handle(
            new UpdateExampleCommand(
                publicId,
                Name: "New Name",
                Description: null,
                AddItems: addItems,
                CompleteItemIds: [existingId]),
            CancellationToken.None);

        example.Name.Should().Be("New Name");
        example.Items.Should().HaveCount(2);
        example.Items.First(i => i.Label == "Existing").Status.Should().Be(ExampleItemStatus.Completed);
        example.Items.First(i => i.Label == "New Item").Status.Should().Be(ExampleItemStatus.Pending);
    }
}

using AutoMapper;
using FluentAssertions;
using Moq;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Application.Features.ExamplesEF.Commands.CreateExample;
using Microservice.Domain.Entities;
using Microservice.Domain.Exceptions;

namespace Microservice.Test.Application.Features.ExamplesEF.Commands.CreateExample;

public class CreateExampleCommandHandlerTests
{
    private readonly Mock<IExampleWriteRepository> _mockExamplesWrite;
    private readonly Mock<IUnitOfWork>             _mockUnitOfWork;
    private readonly Mock<IMapper>                 _mockMapper;
    private readonly CreateExampleCommandHandler   _handler;

    public CreateExampleCommandHandlerTests()
    {
        _mockExamplesWrite = new Mock<IExampleWriteRepository>();
        _mockUnitOfWork    = new Mock<IUnitOfWork>();
        _mockMapper        = new Mock<IMapper>();

        _mockUnitOfWork.Setup(u => u.ExamplesWrite).Returns(_mockExamplesWrite.Object);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _handler = new CreateExampleCommandHandler(_mockUnitOfWork.Object, _mockMapper.Object);
    }

    // ── Base behaviour ────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WithValidCommand_ShouldAddExampleAndReturnPublicId()
    {
        var command       = new CreateExampleCommand("Test", "Description");
        var mappedExample = new Example("Test", "Description") { Id = 1 };

        _mockMapper.Setup(m => m.Map<Example>(command)).Returns(mappedExample);
        _mockExamplesWrite.Setup(r => r.AddAsync(mappedExample, It.IsAny<CancellationToken>())).ReturnsAsync(mappedExample);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(mappedExample.PublicId);
        _mockMapper.Verify(m => m.Map<Example>(command), Times.Once);
        _mockExamplesWrite.Verify(r => r.AddAsync(mappedExample, It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldMapCommandToExample()
    {
        var command       = new CreateExampleCommand("Test", "Description");
        var mappedExample = new Example("Test", "Description");
        _mockMapper.Setup(m => m.Map<Example>(command)).Returns(mappedExample);

        await _handler.Handle(command, CancellationToken.None);

        _mockMapper.Verify(m => m.Map<Example>(command), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCallAddAsyncWithMappedExample()
    {
        var command       = new CreateExampleCommand("Test", "Description");
        var mappedExample = new Example("Test", "Description");
        _mockMapper.Setup(m => m.Map<Example>(command)).Returns(mappedExample);

        await _handler.Handle(command, CancellationToken.None);

        _mockExamplesWrite.Verify(r => r.AddAsync(mappedExample, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldSaveChangesAfterAdding()
    {
        var command       = new CreateExampleCommand("Test", "Description");
        var mappedExample = new Example("Test", "Description");
        _mockMapper.Setup(m => m.Map<Example>(command)).Returns(mappedExample);

        await _handler.Handle(command, CancellationToken.None);

        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccessResultWithExamplePublicId()
    {
        var command       = new CreateExampleCommand("Test", "Description");
        var mappedExample = new Example("Test", "Description");
        _mockMapper.Setup(m => m.Map<Example>(command)).Returns(mappedExample);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(mappedExample.PublicId);
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldRespectCancellationToken()
    {
        var command           = new CreateExampleCommand("Test", "Description");
        var mappedExample     = new Example("Test", "Description");
        var cancellationToken = new CancellationToken(canceled: false);
        _mockMapper.Setup(m => m.Map<Example>(command)).Returns(mappedExample);

        await _handler.Handle(command, cancellationToken);

        _mockExamplesWrite.Verify(r => r.AddAsync(mappedExample, cancellationToken), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenAddAsyncThrows_ShouldPropagateException()
    {
        var command       = new CreateExampleCommand("Test", "Description");
        var mappedExample = new Example("Test", "Description");
        _mockMapper.Setup(m => m.Map<Example>(command)).Returns(mappedExample);
        _mockExamplesWrite
            .Setup(r => r.AddAsync(mappedExample, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenSaveChangesThrows_ShouldPropagateException()
    {
        var command       = new CreateExampleCommand("Test", "Description");
        var mappedExample = new Example("Test", "Description");
        _mockMapper.Setup(m => m.Map<Example>(command)).Returns(mappedExample);
        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Concurrency conflict"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldCallRepositoryOperationsInCorrectOrder()
    {
        var command       = new CreateExampleCommand("Test", "Description");
        var mappedExample = new Example("Test", "Description");
        List<string> callOrder = [];

        _mockMapper
            .Setup(m => m.Map<Example>(command))
            .Callback(() => callOrder.Add("Map"))
            .Returns(mappedExample);
        _mockExamplesWrite
            .Setup(r => r.AddAsync(mappedExample, It.IsAny<CancellationToken>()))
            .Callback(async () => { callOrder.Add("Add"); await Task.CompletedTask; })
            .ReturnsAsync(mappedExample);
        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(async () => { callOrder.Add("Save"); await Task.CompletedTask; })
            .ReturnsAsync(1);

        await _handler.Handle(command, CancellationToken.None);

        callOrder.Should().Equal("Map", "Add", "Save");
    }

    // ── Items ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WithNullItems_ShouldAddExampleWithNoItems()
    {
        var command       = new CreateExampleCommand("Test", null, Items: null);
        var mappedExample = new Example("Test", null);
        _mockMapper.Setup(m => m.Map<Example>(command)).Returns(mappedExample);

        await _handler.Handle(command, CancellationToken.None);

        mappedExample.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithEmptyItemsList_ShouldAddExampleWithNoItems()
    {
        var command       = new CreateExampleCommand("Test", null, Items: []);
        var mappedExample = new Example("Test", null);
        _mockMapper.Setup(m => m.Map<Example>(command)).Returns(mappedExample);

        await _handler.Handle(command, CancellationToken.None);

        mappedExample.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithItems_ShouldAddItemsToExampleViaAddItem()
    {
        var items = new List<CreateExampleItemRequest>
        {
            new("Widget", 3),
            new("Gadget", 7),
        };
        var command       = new CreateExampleCommand("Test", null, items);
        var mappedExample = new Example("Test", null);
        _mockMapper.Setup(m => m.Map<Example>(command)).Returns(mappedExample);

        await _handler.Handle(command, CancellationToken.None);

        mappedExample.Items.Should().HaveCount(2);
        mappedExample.Items[0].Label.Should().Be("Widget");
        mappedExample.Items[0].Quantity.Should().Be(3);
        mappedExample.Items[1].Label.Should().Be("Gadget");
        mappedExample.Items[1].Quantity.Should().Be(7);
    }

    [Fact]
    public async Task Handle_WithItems_ShouldPassExampleWithItemsToAddAsync()
    {
        var items = new List<CreateExampleItemRequest> { new("Alpha", 1) };
        var command       = new CreateExampleCommand("Test", null, items);
        var mappedExample = new Example("Test", null);
        _mockMapper.Setup(m => m.Map<Example>(command)).Returns(mappedExample);

        Example? capturedExample = null;
        _mockExamplesWrite
            .Setup(r => r.AddAsync(It.IsAny<Example>(), It.IsAny<CancellationToken>()))
            .Callback<Example, CancellationToken>((e, _) => capturedExample = e)
            .ReturnsAsync(mappedExample);

        await _handler.Handle(command, CancellationToken.None);

        capturedExample.Should().NotBeNull();
        capturedExample!.Items.Should().HaveCount(1);
        capturedExample.Items[0].Label.Should().Be("Alpha");
    }

    [Fact]
    public async Task Handle_WithItems_ShouldReturnSuccessPublicId()
    {
        var items = new List<CreateExampleItemRequest> { new("Part A", 5) };
        var command       = new CreateExampleCommand("Test", null, items);
        var mappedExample = new Example("Test", null);
        _mockMapper.Setup(m => m.Map<Example>(command)).Returns(mappedExample);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(mappedExample.PublicId);
    }

    [Fact]
    public async Task Handle_WithDuplicateItemLabels_ShouldThrowDomainException()
    {
        var items = new List<CreateExampleItemRequest>
        {
            new("Widget", 1),
            new("Widget", 2),
        };
        var command       = new CreateExampleCommand("Test", null, items);
        var mappedExample = new Example("Test", null);
        _mockMapper.Setup(m => m.Map<Example>(command)).Returns(mappedExample);

        await Assert.ThrowsAsync<DomainException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithDuplicateItemLabelsCaseInsensitive_ShouldThrowDomainException()
    {
        var items = new List<CreateExampleItemRequest>
        {
            new("widget", 1),
            new("WIDGET", 2),
        };
        var command       = new CreateExampleCommand("Test", null, items);
        var mappedExample = new Example("Test", null);
        _mockMapper.Setup(m => m.Map<Example>(command)).Returns(mappedExample);

        await Assert.ThrowsAsync<DomainException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithSingleItem_ShouldHaveCorrectStatus()
    {
        var items = new List<CreateExampleItemRequest> { new("Part", 2) };
        var command       = new CreateExampleCommand("Test", null, items);
        var mappedExample = new Example("Test", null);
        _mockMapper.Setup(m => m.Map<Example>(command)).Returns(mappedExample);

        await _handler.Handle(command, CancellationToken.None);

        mappedExample.Items[0].Status.Should().Be(ExampleItemStatus.Pending);
    }
}

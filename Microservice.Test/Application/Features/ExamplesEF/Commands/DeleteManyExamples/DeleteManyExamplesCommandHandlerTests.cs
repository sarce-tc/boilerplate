using System.Linq.Expressions;
using FluentAssertions;
using Moq;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Application.Features.ExamplesEF.Commands.DeleteManyExamples;
using Microservice.Domain.Entities;

namespace Microservice.Test.Application.Features.ExamplesEF.Commands.DeleteManyExamples;

public class DeleteManyExamplesCommandHandlerTests
{
    private readonly Mock<IWriteRepository<Example>>       _mockWriteRepository;
    private readonly Mock<IUnitOfWork>                     _mockUnitOfWork;
    private readonly DeleteManyExamplesCommandHandler      _handler;

    public DeleteManyExamplesCommandHandlerTests()
    {
        _mockWriteRepository = new Mock<IWriteRepository<Example>>();
        _mockUnitOfWork      = new Mock<IUnitOfWork>();

        _mockUnitOfWork.Setup(u => u.WriteRepository).Returns(_mockWriteRepository.Object);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _handler = new DeleteManyExamplesCommandHandler(_mockUnitOfWork.Object);
    }

    [Fact]
    public async Task Handle_WithValidPublicIds_ShouldDeleteMultipleAndReturnCount()
    {
        var publicIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        _mockWriteRepository
            .Setup(r => r.DeleteManyAsync(It.IsAny<Expression<Func<Example, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);

        var result = await _handler.Handle(new DeleteManyExamplesCommand(publicIds), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(3);
    }

    [Fact]
    public async Task Handle_WithEmptyArray_ShouldReturnZero()
    {
        _mockWriteRepository
            .Setup(r => r.DeleteManyAsync(It.IsAny<Expression<Func<Example, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var result = await _handler.Handle(new DeleteManyExamplesCommand([]), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldCallDeleteManyAsync()
    {
        var publicIds = new[] { Guid.NewGuid(), Guid.NewGuid() };
        _mockWriteRepository
            .Setup(r => r.DeleteManyAsync(It.IsAny<Expression<Func<Example, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        await _handler.Handle(new DeleteManyExamplesCommand(publicIds), CancellationToken.None);

        _mockWriteRepository.Verify(
            r => r.DeleteManyAsync(It.IsAny<Expression<Func<Example, bool>>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithSinglePublicId_ShouldDeleteOne()
    {
        _mockWriteRepository
            .Setup(r => r.DeleteManyAsync(It.IsAny<Expression<Func<Example, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _handler.Handle(new DeleteManyExamplesCommand([Guid.NewGuid()]), CancellationToken.None);

        result.Value.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WithManyPublicIds_ShouldDeleteAll()
    {
        var publicIds = Enumerable.Range(1, 100).Select(_ => Guid.NewGuid()).ToArray();
        _mockWriteRepository
            .Setup(r => r.DeleteManyAsync(It.IsAny<Expression<Func<Example, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(100);

        var result = await _handler.Handle(new DeleteManyExamplesCommand(publicIds), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(100);
    }

    [Fact]
    public async Task Handle_ShouldRespectCancellationToken()
    {
        var cancellationToken = new CancellationToken(canceled: false);
        _mockWriteRepository
            .Setup(r => r.DeleteManyAsync(It.IsAny<Expression<Func<Example, bool>>>(), cancellationToken))
            .ReturnsAsync(2);

        await _handler.Handle(new DeleteManyExamplesCommand([Guid.NewGuid(), Guid.NewGuid()]), cancellationToken);

        _mockWriteRepository.Verify(
            r => r.DeleteManyAsync(It.IsAny<Expression<Func<Example, bool>>>(), cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldPropagateException()
    {
        _mockWriteRepository
            .Setup(r => r.DeleteManyAsync(It.IsAny<Expression<Func<Example, bool>>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(new DeleteManyExamplesCommand([Guid.NewGuid()]), CancellationToken.None));
    }
}

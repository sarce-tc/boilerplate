using AutoMapper;
using FluentAssertions;
using Moq;
using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Application.Features.Examples.Commands.CreateExample;
using Microservice.Domain.Entities;

namespace Microservice.Test.Application.Features.Examples.Commands.CreateExample
{
    /// <summary>
    /// Unit tests for CreateExampleCommandHandler
    /// Tests command handling, repository interactions, and error scenarios
    /// </summary>
    public class CreateExampleCommandHandlerTests
    {
        private readonly Mock<IWriteRepository<Example>> _mockWriteRepository;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMapper> _mockMapper;
        private readonly CreateExampleCommandHandler _handler;

        public CreateExampleCommandHandlerTests()
        {
            _mockWriteRepository = new Mock<IWriteRepository<Example>>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _handler = new CreateExampleCommandHandler(_mockWriteRepository.Object, _mockUnitOfWork.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task Handle_WithValidCommand_ShouldAddExampleAndReturnPublicId()
        {
            // Arrange
            var command = new CreateExampleCommand("Test", "Description");
            var mappedExample = new Example("Test", "Description");
            mappedExample.Id = 1;

            _mockMapper
                .Setup(m => m.Map<Example>(command))
                .Returns(mappedExample);

            _mockWriteRepository
                .Setup(r => r.AddAsync(mappedExample, It.IsAny<CancellationToken>()))
                .ReturnsAsync(mappedExample);

            _mockUnitOfWork
                .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(mappedExample.PublicId);
            _mockMapper.Verify(m => m.Map<Example>(command), Times.Once);
            _mockWriteRepository.Verify(r => r.AddAsync(mappedExample, It.IsAny<CancellationToken>()), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldMapCommandToExample()
        {
            // Arrange
            var command = new CreateExampleCommand("Test", "Description");
            var mappedExample = new Example("Test", "Description");

            _mockMapper
                .Setup(m => m.Map<Example>(command))
                .Returns(mappedExample);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _mockMapper.Verify(m => m.Map<Example>(command), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldCallAddAsyncWithMappedExample()
        {
            // Arrange
            var command = new CreateExampleCommand("Test", "Description");
            var mappedExample = new Example("Test", "Description");

            _mockMapper
                .Setup(m => m.Map<Example>(command))
                .Returns(mappedExample);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _mockWriteRepository.Verify(
                r => r.AddAsync(mappedExample, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldSaveChangesAfterAdding()
        {
            // Arrange
            var command = new CreateExampleCommand("Test", "Description");
            var mappedExample = new Example("Test", "Description");

            _mockMapper
                .Setup(m => m.Map<Example>(command))
                .Returns(mappedExample);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _mockUnitOfWork.Verify(
                u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccessResultWithExamplePublicId()
        {
            // Arrange
            var command = new CreateExampleCommand("Test", "Description");
            var mappedExample = new Example("Test", "Description");

            _mockMapper
                .Setup(m => m.Map<Example>(command))
                .Returns(mappedExample);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(mappedExample.PublicId);
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_WithDifferentExamples_ShouldReturnCorrespondingPublicId()
        {
            // Arrange
            var command = new CreateExampleCommand("Test", "Description");
            var mappedExample = new Example("Test", "Description");

            _mockMapper
                .Setup(m => m.Map<Example>(command))
                .Returns(mappedExample);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Value.Should().Be(mappedExample.PublicId);
        }

        [Fact]
        public async Task Handle_ShouldRespectCancellationToken()
        {
            // Arrange
            var command = new CreateExampleCommand("Test", "Description");
            var mappedExample = new Example("Test", "Description");
            var cancellationToken = new CancellationToken(canceled: false);

            _mockMapper
                .Setup(m => m.Map<Example>(command))
                .Returns(mappedExample);

            // Act
            await _handler.Handle(command, cancellationToken);

            // Assert
            _mockWriteRepository.Verify(
                r => r.AddAsync(mappedExample, cancellationToken),
                Times.Once);
            _mockUnitOfWork.Verify(
                u => u.SaveChangesAsync(cancellationToken),
                Times.Once);
        }

        [Fact]
        public async Task Handle_WhenAddAsyncThrows_ShouldPropagateException()
        {
            // Arrange
            var command = new CreateExampleCommand("Test", "Description");
            var mappedExample = new Example("Test", "Description");

            _mockMapper
                .Setup(m => m.Map<Example>(command))
                .Returns(mappedExample);

            _mockWriteRepository
                .Setup(r => r.AddAsync(mappedExample, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_WhenSaveChangesThrows_ShouldPropagateException()
        {
            // Arrange
            var command = new CreateExampleCommand("Test", "Description");
            var mappedExample = new Example("Test", "Description");

            _mockMapper
                .Setup(m => m.Map<Example>(command))
                .Returns(mappedExample);

            _mockUnitOfWork
                .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Concurrency conflict"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ShouldCallRepositoryOperationsInCorrectOrder()
        {
            // Arrange
            var command = new CreateExampleCommand("Test", "Description");
            var mappedExample = new Example("Test", "Description");
            var callOrder = new List<string>();

            _mockMapper
                .Setup(m => m.Map<Example>(command))
                .Callback(() => callOrder.Add("Map"))
                .Returns(mappedExample);

            _mockWriteRepository
                .Setup(r => r.AddAsync(mappedExample, It.IsAny<CancellationToken>()))
                .Callback(async () =>
                {
                    callOrder.Add("Add");
                    await Task.CompletedTask;
                })
                .ReturnsAsync(mappedExample);

            _mockUnitOfWork
                .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Callback(async () =>
                {
                    callOrder.Add("Save");
                    await Task.CompletedTask;
                })
                .ReturnsAsync(1);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            callOrder.Should().Equal("Map", "Add", "Save");
        }
    }
}

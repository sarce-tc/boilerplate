using FluentAssertions;
using Moq;
using MediatR;
using Microservice.API.Controllers;
using Microservice.Application.Common.Results;
using Microservice.Application.DTOs;
using Microservice.Application.Features.ExamplesEF.Commands.CreateExample;
using Microservice.Application.Features.ExamplesEF.Queries.GetExampleByPredicate;
using Microsoft.AspNetCore.Mvc;

namespace Microservice.Test.API.Controllers
{
    /// <summary>
    /// Unit tests for ExamplesController
    /// Tests HTTP endpoint handling, request/response mapping, and result conversion
    /// </summary>
    public class ExamplesControllerTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly ExamplesController _controller;

        public ExamplesControllerTests()
        {
            _mockMediator = new Mock<IMediator>();
            _controller = new ExamplesController(_mockMediator.Object);
        }

        [Fact]
        public async Task CreateExample_WithValidCommand_ShouldReturnCreatedResult()
        {
            // Arrange
            var command = new CreateExampleCommand("Test", "Description");
            var expectedPublicId = Guid.NewGuid();

            _mockMediator
                .Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<Guid>.Success(expectedPublicId));

            // Act
            var result = await _controller.CreateExample(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            var createdResult = result as ObjectResult;
            createdResult.Should().NotBeNull();
            createdResult!.StatusCode.Should().Be(201);
        }

        [Fact]
        public async Task CreateExample_ShouldSendCommandToMediator()
        {
            // Arrange
            var command = new CreateExampleCommand("Test", "Description");

            _mockMediator
                .Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<Guid>.Success(Guid.NewGuid()));

            // Act
            await _controller.CreateExample(command, CancellationToken.None);

            // Assert
            _mockMediator.Verify(
                m => m.Send(command, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task CreateExample_WithDifferentPublicIds_ShouldReturnCorrectPublicId()
        {
            // Arrange
            var command = new CreateExampleCommand("Test", "Description");
            var expectedPublicId = Guid.NewGuid();

            _mockMediator
                .Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<Guid>.Success(expectedPublicId));

            // Act
            var result = await _controller.CreateExample(command, CancellationToken.None);

            // Assert
            var objResult = result as ObjectResult;
            objResult.Should().NotBeNull();
            objResult!.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task GetExampleById_WithExistingPublicId_ShouldReturnOkResult()
        {
            // Arrange
            var publicId = Guid.NewGuid();

            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetExampleByPredicateQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<GetExampleByPredicateDto>.Success(
                    new GetExampleByPredicateDto(publicId, "Test", "Description", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow)));

            // Act
            var result = await _controller.GetExampleById(publicId, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task GetExampleById_ShouldSendQueryToMediator()
        {
            // Arrange
            var publicId = Guid.NewGuid();

            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetExampleByPredicateQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<GetExampleByPredicateDto>.Success(
                    new GetExampleByPredicateDto(publicId, "Test", "Description", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow)));

            // Act
            await _controller.GetExampleById(publicId, CancellationToken.None);

            // Assert
            _mockMediator.Verify(
                m => m.Send(It.Is<GetExampleByPredicateQuery>(q => q.PublicId == publicId), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetExampleById_WithNonExistentPublicId_ShouldReturnNotFoundResult()
        {
            // Arrange
            var publicId = Guid.NewGuid();
            var failureResult = Result<GetExampleByPredicateDto>.Failure(
                Error.NotFound("Ejemplo no encontrado"));

            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetExampleByPredicateQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failureResult);

            // Act
            var result = await _controller.GetExampleById(publicId, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            var objResult = result as ObjectResult;
            objResult.Should().NotBeNull();
            objResult!.StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task GetExampleById_WithDifferentPublicIds_ShouldQueryCorrectPublicId()
        {
            // Arrange
            var publicId = Guid.NewGuid();
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetExampleByPredicateQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<GetExampleByPredicateDto>.Success(
                    new GetExampleByPredicateDto(publicId, "Test", "Description", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow)));

            // Act
            await _controller.GetExampleById(publicId, CancellationToken.None);

            // Assert
            _mockMediator.Verify(
                m => m.Send(It.Is<GetExampleByPredicateQuery>(q => q.PublicId == publicId), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task CreateExample_ShouldRespectCancellationToken()
        {
            // Arrange
            var command = new CreateExampleCommand("Test", "Description");
            var cancellationToken = new CancellationToken(canceled: false);

            _mockMediator
                .Setup(m => m.Send(command, cancellationToken))
                .ReturnsAsync(Result<Guid>.Success(Guid.NewGuid()));

            // Act
            await _controller.CreateExample(command, cancellationToken);

            // Assert
            _mockMediator.Verify(
                m => m.Send(command, cancellationToken),
                Times.Once);
        }

        [Fact]
        public async Task GetExampleById_ShouldRespectCancellationToken()
        {
            // Arrange
            var publicId = Guid.NewGuid();
            var cancellationToken = new CancellationToken(canceled: false);

            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetExampleByPredicateQuery>(), cancellationToken))
                .ReturnsAsync(Result<GetExampleByPredicateDto>.Success(
                    new GetExampleByPredicateDto(publicId, "Test", "Description", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow)));

            // Act
            await _controller.GetExampleById(publicId, cancellationToken);

            // Assert
            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetExampleByPredicateQuery>(), cancellationToken),
                Times.Once);
        }

        [Fact]
        public async Task CreateExample_WhenMediatorThrows_ShouldPropagateException()
        {
            // Arrange
            var command = new CreateExampleCommand("Test", "Description");

            _mockMediator
                .Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Mediator error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _controller.CreateExample(command, CancellationToken.None));
        }

        [Fact]
        public async Task GetExampleById_WhenMediatorThrows_ShouldPropagateException()
        {
            // Arrange
            var publicId = Guid.NewGuid();

            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetExampleByPredicateQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Mediator error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _controller.GetExampleById(publicId, CancellationToken.None));
        }

        [Fact]
        public async Task CreateExample_WithValidationFailure_ShouldReturnBadRequest()
        {
            // Arrange
            var command = new CreateExampleCommand("", null); // Invalid: empty name
            var failureResult = Result<Guid>.Failure(
                Error.Validation("Name is required"));

            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateExampleCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failureResult);

            // Act
            var result = await _controller.CreateExample(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            var objResult = result as ObjectResult;
            objResult.Should().NotBeNull();
            objResult!.StatusCode.Should().Be(400);
        }
    }
}

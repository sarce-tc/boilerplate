using FluentAssertions;
using Moq;
using AutoMapper;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Application.DTOs;
using Microservice.Application.Features.Examples.Queries.GetExampleByPredicate;
using Microservice.Domain.Entities;
using System.Linq.Expressions;

namespace Microservice.Test.Application.Features.Examples.Queries.GetExampleByPredicate
{
    /// <summary>
    /// Unit tests for GetExampleByPredicateQueryHandler
    /// Tests retrieval by custom predicates
    /// </summary>
    public class GetExampleByPredicateQueryHandlerTests
    {
        private readonly Mock<IReadRepository<Example>> _mockReadRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly GetExampleByPredicateQueryHandler _handler;

        public GetExampleByPredicateQueryHandlerTests()
        {
            _mockReadRepository = new Mock<IReadRepository<Example>>();
            _mockMapper = new Mock<IMapper>();
            _handler = new GetExampleByPredicateQueryHandler(_mockReadRepository.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task Handle_WithExistingId_ShouldReturnEntity()
        {
            // Arrange
            var query = new GetExampleByPredicateQuery(Guid.NewGuid());
            var example = new Example("Test", "Description");
            var dto = new GetExampleByPredicateDto(example.PublicId, "Test", "Description", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);

            _mockReadRepository
                .Setup(r => r.GetEntityAsync(
                    It.IsAny<Expression<Func<Example, bool>>>(),
                    null,
                    null,
                    true,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(example);

            _mockMapper
                .Setup(m => m.Map<GetExampleByPredicateDto>(example))
                .Returns(dto);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(dto);
        }

        [Fact]
        public async Task Handle_WithNonExistentId_ShouldReturnNotFound()
        {
            // Arrange
            var query = new GetExampleByPredicateQuery(Guid.NewGuid());

            _mockReadRepository
                .Setup(r => r.GetEntityAsync(
                    It.IsAny<Expression<Func<Example, bool>>>(),
                    null,
                    null,
                    true,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Example)null!);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].Code.Should().Be("NotFound");
        }

        [Fact]
        public async Task Handle_ShouldCallGetEntityAsync()
        {
            // Arrange
            var query = new GetExampleByPredicateQuery(Guid.NewGuid());
            var example = new Example("Test", "Description");

            _mockReadRepository
                .Setup(r => r.GetEntityAsync(
                    It.IsAny<Expression<Func<Example, bool>>>(),
                    null,
                    null,
                    true,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(example);

            _mockMapper
                .Setup(m => m.Map<GetExampleByPredicateDto>(example))
                .Returns(new GetExampleByPredicateDto(example.PublicId, "Test", "Description", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow));

            // Act
            await _handler.Handle(query, CancellationToken.None);

            // Assert
            _mockReadRepository.Verify(
                r => r.GetEntityAsync(
                    It.IsAny<Expression<Func<Example, bool>>>(),
                    null,
                    null,
                    true,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_WithDifferentPublicIds_ShouldReturnCorrectDto()
        {
            // Arrange
            var publicId = Guid.NewGuid();
            var query = new GetExampleByPredicateQuery(publicId);
            var example = new Example("Test", "Description");
            example.PublicId = publicId;
            var dto = new GetExampleByPredicateDto(publicId, "Test", "Description", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);

            _mockReadRepository
                .Setup(r => r.GetEntityAsync(
                    It.IsAny<Expression<Func<Example, bool>>>(),
                    null,
                    null,
                    true,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(example);

            _mockMapper
                .Setup(m => m.Map<GetExampleByPredicateDto>(example))
                .Returns(dto);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value!.PublicId.Should().Be(publicId);
        }

        [Fact]
        public async Task Handle_ShouldRespectCancellationToken()
        {
            // Arrange
            var query = new GetExampleByPredicateQuery(Guid.NewGuid());
            var example = new Example("Test", "Description");
            var cancellationToken = new CancellationToken(canceled: false);

            _mockReadRepository
                .Setup(r => r.GetEntityAsync(
                    It.IsAny<Expression<Func<Example, bool>>>(),
                    null,
                    null,
                    true,
                    cancellationToken))
                .ReturnsAsync(example);

            _mockMapper
                .Setup(m => m.Map<GetExampleByPredicateDto>(example))
                .Returns(new GetExampleByPredicateDto(example.PublicId, "Test", "Description", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow));

            // Act
            await _handler.Handle(query, cancellationToken);

            // Assert
            _mockReadRepository.Verify(
                r => r.GetEntityAsync(
                    It.IsAny<Expression<Func<Example, bool>>>(),
                    null,
                    null,
                    true,
                    cancellationToken),
                Times.Once);
        }

        [Fact]
        public async Task Handle_WhenRepositoryThrows_ShouldPropagateException()
        {
            // Arrange
            var query = new GetExampleByPredicateQuery(Guid.NewGuid());

            _mockReadRepository
                .Setup(r => r.GetEntityAsync(
                    It.IsAny<Expression<Func<Example, bool>>>(),
                    null,
                    null,
                    true,
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _handler.Handle(query, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_WithNonExistentId_ShouldNotMapEntity()
        {
            // Arrange
            var query = new GetExampleByPredicateQuery(Guid.NewGuid());

            _mockReadRepository
                .Setup(r => r.GetEntityAsync(
                    It.IsAny<Expression<Func<Example, bool>>>(),
                    null,
                    null,
                    true,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Example)null!);

            // Act
            await _handler.Handle(query, CancellationToken.None);

            // Assert
            _mockMapper.Verify(
                m => m.Map<GetExampleByPredicateDto>(It.IsAny<Example>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnErrorMessage()
        {
            // Arrange
            var query = new GetExampleByPredicateQuery(Guid.NewGuid());

            _mockReadRepository
                .Setup(r => r.GetEntityAsync(
                    It.IsAny<Expression<Func<Example, bool>>>(),
                    null,
                    null,
                    true,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Example)null!);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Errors[0].Message.Should().Contain("no encontrado");
        }
    }
}

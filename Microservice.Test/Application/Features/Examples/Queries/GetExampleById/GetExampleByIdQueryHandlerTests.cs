using AutoMapper;
using FluentAssertions;
using Moq;
using MediatR;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Application.DTOs;
using Microservice.Application.Features.Examples.Queries.GetExampleById;
using Microservice.Domain.Entities;

namespace Microservice.Test.Application.Features.Examples.Queries.GetExampleById
{
    /// <summary>
    /// Unit tests for GetExampleByIdQueryHandler
    /// Tests query handling, data retrieval, and error scenarios
    /// </summary>
    public class GetExampleByIdQueryHandlerTests
    {
        private readonly Mock<IReadRepository<Example>> _mockReadRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly GetExampleByIdQueryHandler _handler;

        public GetExampleByIdQueryHandlerTests()
        {
            _mockReadRepository = new Mock<IReadRepository<Example>>();
            _mockMapper = new Mock<IMapper>();
            _handler = new GetExampleByIdQueryHandler(_mockReadRepository.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task Handle_WithExistingId_ShouldReturnSuccessResultWithDto()
        {
            // Arrange
            var query = new GetExampleByIdQuery(1);
            var example = new Example("Test", "Description") { Id = 1 };
            var dto = new GetExampleByIdDto(Guid.NewGuid(), "Test", "Description", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);

            _mockReadRepository
                .Setup(r => r.FindAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(example);

            _mockMapper
                .Setup(m => m.Map<GetExampleByIdDto>(example))
                .Returns(dto);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(dto);
            _mockReadRepository.Verify(r => r.FindAsync(1, It.IsAny<CancellationToken>()), Times.Once);
            _mockMapper.Verify(m => m.Map<GetExampleByIdDto>(example), Times.Once);
        }

        [Fact]
        public async Task Handle_WithNonExistentId_ShouldReturnFailureResult()
        {
            // Arrange
            var query = new GetExampleByIdQuery(999);

            _mockReadRepository
                .Setup(r => r.FindAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Example)null!);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Value.Should().BeNull();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].Code.Should().Be("NotFound");
        }

        [Fact]
        public async Task Handle_ShouldCallFindAsyncWithCorrectId()
        {
            // Arrange
            var query = new GetExampleByIdQuery(5);
            var example = new Example("Test", "Description") { Id = 5 };

            _mockReadRepository
                .Setup(r => r.FindAsync(5, It.IsAny<CancellationToken>()))
                .ReturnsAsync(example);

            _mockMapper
                .Setup(m => m.Map<GetExampleByIdDto>(example))
                .Returns(new GetExampleByIdDto(Guid.NewGuid(), "Test", "Description", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow));

            // Act
            await _handler.Handle(query, CancellationToken.None);

            // Assert
            _mockReadRepository.Verify(r => r.FindAsync(5, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldMapExampleToDto()
        {
            // Arrange
            var query = new GetExampleByIdQuery(1);
            var example = new Example("Test", "Description") { Id = 1 };
            var dto = new GetExampleByIdDto(Guid.NewGuid(), "Test", "Description", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);

            _mockReadRepository
                .Setup(r => r.FindAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(example);

            _mockMapper
                .Setup(m => m.Map<GetExampleByIdDto>(example))
                .Returns(dto);

            // Act
            await _handler.Handle(query, CancellationToken.None);

            // Assert
            _mockMapper.Verify(m => m.Map<GetExampleByIdDto>(example), Times.Once);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(100)]
        [InlineData(int.MaxValue)]
        public async Task Handle_WithDifferentIds_ShouldReturnCorrectDto(int id)
        {
            // Arrange
            var query = new GetExampleByIdQuery(id);
            var example = new Example("Test", "Description") { Id = id };
            var dto = new GetExampleByIdDto(Guid.NewGuid(), "Test", "Description", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);

            _mockReadRepository
                .Setup(r => r.FindAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(example);

            _mockMapper
                .Setup(m => m.Map<GetExampleByIdDto>(example))
                .Returns(dto);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value!.PublicId.Should().Be(dto.PublicId);
        }

        [Fact]
        public async Task Handle_ShouldRespectCancellationToken()
        {
            // Arrange
            var query = new GetExampleByIdQuery(1);
            var example = new Example("Test", "Description") { Id = 1 };
            var cancellationToken = new CancellationToken(canceled: false);

            _mockReadRepository
                .Setup(r => r.FindAsync(1, cancellationToken))
                .ReturnsAsync(example);

            var dto = new GetExampleByIdDto(Guid.NewGuid(), "Test", "Description", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);

            _mockMapper
                .Setup(m => m.Map<GetExampleByIdDto>(example))
                .Returns(dto);

            // Act
            await _handler.Handle(query, cancellationToken);

            // Assert
            _mockReadRepository.Verify(r => r.FindAsync(1, cancellationToken), Times.Once);
        }

        [Fact]
        public async Task Handle_WhenRepositoryThrows_ShouldPropagateException()
        {
            // Arrange
            var query = new GetExampleByIdQuery(1);

            _mockReadRepository
                .Setup(r => r.FindAsync(1, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _handler.Handle(query, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_WithNonExistentId_ShouldNotMapEntity()
        {
            // Arrange
            var query = new GetExampleByIdQuery(999);

            _mockReadRepository
                .Setup(r => r.FindAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Example)null!);

            // Act
            await _handler.Handle(query, CancellationToken.None);

            // Assert
            _mockMapper.Verify(m => m.Map<GetExampleByIdDto>(It.IsAny<Example>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnErrorMessage_WhenEntityNotFound()
        {
            // Arrange
            var query = new GetExampleByIdQuery(999);

            _mockReadRepository
                .Setup(r => r.FindAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Example)null!);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Errors[0].Message.Should().Contain("Ejemplo no encontrado");
        }

        [Fact]
        public async Task Handle_WithValidData_ShouldReturnDtoWithCorrectProperties()
        {
            // Arrange
            var query = new GetExampleByIdQuery(1);
            var createdAt = DateTimeOffset.UtcNow.AddDays(-1);
            var updatedAt = DateTimeOffset.UtcNow;
            var example = new Example("Test", "Description") { Id = 1, CreatedAt = createdAt, UpdatedAt = updatedAt };
            var dto = new GetExampleByIdDto(Guid.NewGuid(), "Test", "Description", createdAt, updatedAt);

            _mockReadRepository
                .Setup(r => r.FindAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(example);

            _mockMapper
                .Setup(m => m.Map<GetExampleByIdDto>(example))
                .Returns(dto);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value!.CreatedAt.Should().Be(createdAt);
            result.Value!.UpdatedAt.Should().Be(updatedAt);
        }
    }
}

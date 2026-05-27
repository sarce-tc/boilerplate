using AutoMapper;
using FluentAssertions;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Application.DTOs;
using Microservice.Application.Features.ExamplesEF.Queries.GetExampleItems;
using Microservice.Domain.Entities;
using Moq;
using System.Linq.Expressions;

namespace Microservice.Test.Application.Features.ExamplesEF.Queries.GetExampleItems
{
    public class GetExampleItemsQueryHandlerTests
    {
        private readonly Mock<IReadRepository<Example>> _mockReadRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly GetExampleItemsQueryHandler _handler;

        public GetExampleItemsQueryHandlerTests()
        {
            _mockReadRepository = new Mock<IReadRepository<Example>>();
            _mockMapper = new Mock<IMapper>();
            _handler = new GetExampleItemsQueryHandler(_mockReadRepository.Object, _mockMapper.Object);
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

        private static Example BuildExampleWithItems()
        {
            var example = new Example("Test", "Description");
            example.AddItem("Item A", 3);
            example.AddItem("Item B", 1);
            return example;
        }

        [Fact]
        public async Task Handle_WithExistingExample_ShouldReturnItemsCollection()
        {
            // Arrange
            var publicId = Guid.NewGuid();
            var query = new GetExampleItemsQuery(publicId);
            var example = BuildExampleWithItems();
            var dtos = example.Items.Select(i =>
                new GetExampleItemDto(i.PublicId, i.Label, i.Quantity, i.Status, i.CreatedAt, i.UpdatedAt));

            SetupRead(example);
            _mockMapper.Setup(m => m.Map<IEnumerable<GetExampleItemDto>>(example.Items)).Returns(dtos);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().HaveCount(2);
        }

        [Fact]
        public async Task Handle_WithExampleHavingNoItems_ShouldReturnEmptyCollection()
        {
            // Arrange
            var publicId = Guid.NewGuid();
            var query = new GetExampleItemsQuery(publicId);
            var example = new Example("Empty", null);

            SetupRead(example);
            _mockMapper.Setup(m => m.Map<IEnumerable<GetExampleItemDto>>(example.Items)).Returns([]);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_WithNonExistentExample_ShouldReturnNotFound()
        {
            // Arrange
            var query = new GetExampleItemsQuery(Guid.NewGuid());
            SetupRead(null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors[0].Code.Should().Be("NotFound");
        }

        [Fact]
        public async Task Handle_WithNonExistentExample_ShouldNotMapItems()
        {
            // Arrange
            var query = new GetExampleItemsQuery(Guid.NewGuid());
            SetupRead(null);

            // Act
            await _handler.Handle(query, CancellationToken.None);

            // Assert
            _mockMapper.Verify(
                m => m.Map<IEnumerable<GetExampleItemDto>>(It.IsAny<object>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldCallGetEntityAsync_WithIncludeProperties()
        {
            // Arrange
            var publicId = Guid.NewGuid();
            var query = new GetExampleItemsQuery(publicId);
            var example = BuildExampleWithItems();

            SetupRead(example);
            _mockMapper.Setup(m => m.Map<IEnumerable<GetExampleItemDto>>(example.Items)).Returns([]);

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
        public async Task Handle_WhenRepositoryThrows_ShouldPropagateException()
        {
            // Arrange
            var query = new GetExampleItemsQuery(Guid.NewGuid());

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
}

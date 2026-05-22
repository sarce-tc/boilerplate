using FluentAssertions;
using Moq;
using AutoMapper;
using Microservice.Application.Common.Results;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Application.DTOs;
using Microservice.Application.Features.Examples.Queries.GetAllExample;
using Microservice.Domain.Entities;
using System.Linq.Expressions;

namespace Microservice.Test.Application.Features.Examples.Queries.GetAllExample
{
    /// <summary>
    /// Unit tests for GetAllExamplesQueryHandler
    /// Tests retrieval of all examples without filtering
    /// </summary>
    public class GetAllExamplesQueryHandlerTests
    {
        private readonly Mock<IReadRepository<Example>> _mockReadRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly GetAllExamplesQueryHandler _handler;

        public GetAllExamplesQueryHandlerTests()
        {
            _mockReadRepository = new Mock<IReadRepository<Example>>();
            _mockMapper = new Mock<IMapper>();
            _handler = new GetAllExamplesQueryHandler(_mockReadRepository.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnAllExamples()
        {
            // Arrange
            var query = new GetAllExamplesQuery();
            var examples = new List<Example>
            {
                new Example("Test1", "Desc1") { Id = 1 },
                new Example("Test2", "Desc2") { Id = 2 },
                new Example("Test3", "Desc3") { Id = 3 }
            };
            var dtos = new List<GetAllExamplesDto>();

            _mockReadRepository
                .Setup(r => r.GetListAsync(
                    It.IsAny<Expression<Func<Example, bool>>>(),
                    It.IsAny<Expression<Func<Example, Example>>>(),
                    It.IsAny<Func<IQueryable<Example>, IOrderedQueryable<Example>>>(),
                    It.IsAny<IEnumerable<Expression<Func<Example, object>>>>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(examples);

            _mockMapper
                .Setup(m => m.Map<IEnumerable<GetAllExamplesDto>>(examples))
                .Returns(dtos);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_WithEmptyDatabase_ShouldReturnEmptyList()
        {
            // Arrange
            var query = new GetAllExamplesQuery();
            var emptyList = new List<Example>();
            var emptyDtos = new List<GetAllExamplesDto>();

            _mockReadRepository
                .Setup(r => r.GetListAsync(
                    null,
                    null,
                    null,
                    null,
                    true,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(emptyList);

            _mockMapper
                .Setup(m => m.Map<IEnumerable<GetAllExamplesDto>>(emptyList))
                .Returns(emptyDtos);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_ShouldCallGetListAsync()
        {
            // Arrange
            var query = new GetAllExamplesQuery();

            _mockReadRepository
                .Setup(r => r.GetListAsync(
                    null,
                    null,
                    null,
                    null,
                    true,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Example>());

            _mockMapper
                .Setup(m => m.Map<IEnumerable<GetAllExamplesDto>>(It.IsAny<IEnumerable<Example>>()))
                .Returns(new List<GetAllExamplesDto>());

            // Act
            await _handler.Handle(query, CancellationToken.None);

            // Assert
            _mockReadRepository.Verify(
                r => r.GetListAsync(
                    It.IsAny<Expression<Func<Example, bool>>>(),
                    It.IsAny<Expression<Func<Example, Example>>>(),
                    It.IsAny<Func<IQueryable<Example>, IOrderedQueryable<Example>>>(),
                    It.IsAny<IEnumerable<Expression<Func<Example, object>>>>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldMapEntitiesToDtos()
        {
            // Arrange
            var query = new GetAllExamplesQuery();
            var examples = new List<Example> { new Example("Test", "Description") { Id = 1 } };

            _mockReadRepository
                .Setup(r => r.GetListAsync(
                    null,
                    null,
                    null,
                    null,
                    true,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(examples);

            _mockMapper
                .Setup(m => m.Map<IEnumerable<GetAllExamplesDto>>(examples))
                .Returns(new List<GetAllExamplesDto>());

            // Act
            await _handler.Handle(query, CancellationToken.None);

            // Assert
            _mockMapper.Verify(
                m => m.Map<IEnumerable<GetAllExamplesDto>>(examples),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldRespectCancellationToken()
        {
            // Arrange
            var query = new GetAllExamplesQuery();
            var cancellationToken = new CancellationToken(canceled: false);

            _mockReadRepository
                .Setup(r => r.GetListAsync(
                    null,
                    null,
                    null,
                    null,
                    true,
                    cancellationToken))
                .ReturnsAsync(new List<Example>());

            _mockMapper
                .Setup(m => m.Map<IEnumerable<GetAllExamplesDto>>(It.IsAny<IEnumerable<Example>>()))
                .Returns(new List<GetAllExamplesDto>());

            // Act
            await _handler.Handle(query, cancellationToken);

            // Assert
            _mockReadRepository.Verify(
                r => r.GetListAsync(
                    null,
                    null,
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
            var query = new GetAllExamplesQuery();

            _mockReadRepository
                .Setup(r => r.GetListAsync(
                    null,
                    null,
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
        public async Task Handle_WithLargeDataset_ShouldReturnSuccessfully()
        {
            // Arrange
            var query = new GetAllExamplesQuery();
            var examples = Enumerable.Range(1, 1000)
                .Select(i => new Example($"Test{i}", "Description") { Id = i })
                .ToList();
            var dtos = new List<GetAllExamplesDto>();

            _mockReadRepository
                .Setup(r => r.GetListAsync(
                    null,
                    null,
                    null,
                    null,
                    true,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(examples);

            _mockMapper
                .Setup(m => m.Map<IEnumerable<GetAllExamplesDto>>(examples))
                .Returns(dtos);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
        }
    }
}

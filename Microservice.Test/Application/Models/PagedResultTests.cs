using FluentAssertions;
using Microservice.Application.Models;

namespace Microservice.Test.Application.Models
{
    /// <summary>
    /// Unit tests for the PagedResult<T> model
    /// Tests pagination logic and properties
    /// </summary>
    public class PagedResultTests
    {
        [Fact]
        public void Constructor_ShouldInitializeProperties()
        {
            // Arrange
            var results = new List<string> { "a", "b", "c", "d", "e" };
            var rowsCount = 25;
            var currentPage = 1;
            var pageSize = 5;

            // Act
            var pagedResult = new PagedResult<string>(results, rowsCount, currentPage, pageSize);

            // Assert
            pagedResult.Results.Should().BeEquivalentTo(results);
            pagedResult.RowsCount.Should().Be(rowsCount);
            pagedResult.CurrentPage.Should().Be(currentPage);
            pagedResult.PageSize.Should().Be(pageSize);
        }

        [Fact]
        public void Constructor_ShouldCalculatePageCountCorrectly()
        {
            // Arrange
            var results = new List<string> { "a", "b", "c" };
            var rowsCount = 25;
            var pageSize = 10;

            // Act
            var pagedResult = new PagedResult<string>(results, rowsCount, 1, pageSize);

            // Assert
            // 25 / 10 = 2.5, ceiling = 3
            pagedResult.PageCount.Should().Be(3);
        }

        [Theory]
        [InlineData(10, 5, 2)]  // 10 items, page size 5 = 2 pages
        [InlineData(15, 5, 3)]  // 15 items, page size 5 = 3 pages
        [InlineData(11, 5, 3)]  // 11 items, page size 5 = 3 pages (ceiling)
        [InlineData(5, 5, 1)]   // 5 items, page size 5 = 1 page
        [InlineData(0, 5, 0)]   // 0 items = 0 pages
        public void Constructor_ShouldCalculatePageCountCorrectly_WithVariousScenarios(
            int rowsCount, int pageSize, int expectedPageCount)
        {
            // Act
            var pagedResult = new PagedResult<string>(
                Enumerable.Empty<string>(), 
                rowsCount, 
                1, 
                pageSize);

            // Assert
            pagedResult.PageCount.Should().Be(expectedPageCount);
        }

        [Fact]
        public void Constructor_WithEmptyResults_ShouldInitializeEmptyResults()
        {
            // Act
            var pagedResult = new PagedResult<string>(
                Enumerable.Empty<string>(), 
                0, 
                1, 
                10);

            // Assert
            pagedResult.Results.Should().BeEmpty();
            pagedResult.RowsCount.Should().Be(0);
            pagedResult.PageCount.Should().Be(0);
        }

        [Fact]
        public void Constructor_WithMultiplePages_ShouldSetCorrectCurrentPage()
        {
            // Arrange
            var results = new List<string> { "11", "12", "13", "14", "15" };

            // Act
            var pagedResult = new PagedResult<string>(results, 25, 3, 5);

            // Assert
            pagedResult.CurrentPage.Should().Be(3);
            pagedResult.PageSize.Should().Be(5);
            pagedResult.PageCount.Should().Be(5); // 25/5 = 5 pages
        }

        [Fact]
        public void Constructor_WithComplexObjects_ShouldInitializeCorrectly()
        {
            // Arrange
            var results = new List<TestDto>
            {
                new TestDto { Id = 1, Name = "Test 1" },
                new TestDto { Id = 2, Name = "Test 2" }
            };

            // Act
            var pagedResult = new PagedResult<TestDto>(results, 100, 1, 10);

            // Assert
            pagedResult.Results.Should().HaveCount(2);
            pagedResult.Results.First().Id.Should().Be(1);
            pagedResult.PageCount.Should().Be(10); // 100/10 = 10
        }

        [Fact]
        public void Results_ShouldDefaultToEmptyEnumerable_WhenNull()
        {
            // Act
            var pagedResult = new PagedResult<string>(
                null!, 
                0, 
                1, 
                10);

            // Assert
            // The property initializer sets it to Enumerable.Empty() if null is passed
            pagedResult.Results.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_ShouldHandageLargePageNumbers()
        {
            // Act
            var pagedResult = new PagedResult<string>(
                Enumerable.Empty<string>(), 
                1000000, 
                9999, 
                100);

            // Assert
            pagedResult.CurrentPage.Should().Be(9999);
            pagedResult.PageCount.Should().Be(10000); // 1000000/100 = 10000
        }

        [Fact]
        public void Constructor_WithPageSizeOne_ShouldCalculatePageCountAsRowsCount()
        {
            // Act
            var pagedResult = new PagedResult<string>(
                Enumerable.Empty<string>(), 
                50, 
                1, 
                1);

            // Assert
            pagedResult.PageCount.Should().Be(50); // 50/1 = 50
            pagedResult.PageSize.Should().Be(1);
        }

        private class TestDto
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
        }
    }
}

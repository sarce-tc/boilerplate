using FluentAssertions;
using Microservice.Domain.Entities;

namespace Microservice.Test.Domain.Entities
{
    /// <summary>
    /// Unit tests for the Example domain entity
    /// Tests business logic and validation rules of the Example class
    /// </summary>
    public class ExampleTests
    {
        [Fact]
        public void Constructor_WithValidNameAndDescription_ShouldCreateInstance()
        {
            // Arrange
            var name = "Test Name";
            var description = "Test Description";

            // Act
            var example = new Example(name, description);

            // Assert
            example.Should().NotBeNull();
            example.Name.Should().Be(name);
            example.Description.Should().Be(description);
            example.CreatedAt.Should().NotBeAfter(DateTimeOffset.UtcNow.AddSeconds(1));
            example.UpdatedAt.Should().NotBeAfter(DateTimeOffset.UtcNow.AddSeconds(1));
        }

        [Fact]
        public void Constructor_WithValidNameAndNullDescription_ShouldCreateInstance()
        {
            // Arrange
            var name = "Test Name";

            // Act
            var example = new Example(name, null);

            // Assert
            example.Should().NotBeNull();
            example.Name.Should().Be(name);
            example.Description.Should().BeNull();
            example.CreatedAt.Should().NotBeAfter(DateTimeOffset.UtcNow.AddSeconds(1));
            example.UpdatedAt.Should().NotBeAfter(DateTimeOffset.UtcNow.AddSeconds(1));
        }

        [Fact]
        public void Constructor_WithValidName_ShouldTrimWhitespace()
        {
            // Arrange
            var name = "  Test Name  ";
            var description = "  Test Description  ";

            // Act
            var example = new Example(name, description);

            // Assert
            example.Name.Should().Be("Test Name");
            example.Description.Should().Be("Test Description");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_WithInvalidName_ShouldThrowArgumentException(string invalidName)
        {
            // Act & Assert
            var action = () => new Example(invalidName, null);
            action.Should().Throw<ArgumentException>()
                .WithMessage("Name is required.*");
        }

        [Fact]
        public void Constructor_WithNullName_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var action = () => new Example(null!, null);
            action.Should().Throw<ArgumentException>()
                .WithMessage("Name is required.*");
        }

        [Fact]
        public void Constructor_WithEmptyName_ShouldThrowArgumentExceptionWithCorrectParameterName()
        {
            // Act & Assert
            var action = () => new Example("", null);
            action.Should().Throw<ArgumentException>()
                .WithParameterName("name");
        }

        [Fact]
        public void CreatedAtAndUpdatedAt_ShouldBeEqual_WhenJustCreated()
        {
            // Arrange & Act
            var example = new Example("Test Name", "Test Description");

            // Assert - allow small time difference as they are set in sequence
            (example.UpdatedAt - example.CreatedAt).Should().BeLessThan(TimeSpan.FromSeconds(1));
        }

        [Theory]
        [InlineData("Name 1")]
        [InlineData("Another Name")]
        [InlineData("Single")]
        public void Constructor_WithDifferentValidNames_ShouldHaveCorrectName(string name)
        {
            // Act
            var example = new Example(name, null);

            // Assert
            example.Name.Should().Be(name);
        }
    }
}

using FluentAssertions;
using FluentValidation.Results;
using Microservice.Application.Exceptions;

namespace Microservice.Test.Application.Exceptions
{
    /// <summary>
    /// Unit tests for custom application exceptions
    /// Tests exception creation and validation
    /// </summary>
    public class ApplicationExceptionsTests
    {
        [Fact]
        public void NotFoundException_ShouldContainFormattedMessage()
        {
            // Arrange
            var name = "Example";
            var key = 1;

            // Act
            var exception = new NotFoundException(name, key);

            // Assert
            exception.Message.Should().Contain(name);
            exception.Message.Should().Contain(key.ToString());
            exception.Message.Should().Contain("no fue encontrada");
        }

        [Fact]
        public void NotFoundException_ShouldBeDerivableFromException()
        {
            // Arrange & Act
            var exception = new NotFoundException("Test", 1);

            // Assert
            exception.Should().BeOfType<NotFoundException>();
            exception.Should().BeAssignableTo<Exception>();
        }

        [Fact]
        public void ValidationException_ShouldInitializeWithEmptyErrors()
        {
            // Act
            var exception = new ValidationException();

            // Assert
            exception.Errors.Should().NotBeNull();
            exception.Errors.Should().BeEmpty();
            exception.Failures.Should().NotBeNull();
            exception.Failures.Should().BeEmpty();
        }

        [Fact]
        public void ValidationException_WithValidationFailures_ShouldMapErrorsCorrectly()
        {
            // Arrange
            var failures = new List<ValidationFailure>
            {
                new ValidationFailure("Email", "Email is required"),
                new ValidationFailure("Email", "Email must be valid"),
                new ValidationFailure("Name", "Name is required")
            };

            // Act
            var exception = new ValidationException(failures);

            // Assert
            exception.Errors.Should().HaveCount(2);
            exception.Errors.Should().ContainKey("Email");
            exception.Errors.Should().ContainKey("Name");
            exception.Errors["Email"].Should().HaveCount(2);
            exception.Errors["Name"].Should().HaveCount(1);
        }

        [Fact]
        public void ValidationException_ShouldStoreRawFailures()
        {
            // Arrange
            var failures = new List<ValidationFailure>
            {
                new ValidationFailure("Field1", "Error 1"),
                new ValidationFailure("Field2", "Error 2")
            };

            // Act
            var exception = new ValidationException(failures);

            // Assert
            exception.Failures.Should().HaveCount(2);
            exception.Failures.Select(f => f.PropertyName).Should().Contain("Field1");
            exception.Failures.Select(f => f.PropertyName).Should().Contain("Field2");
        }

        [Fact]
        public void NotFoundException_WithStringKey_ShouldFormatMessage()
        {
            // Arrange
            var name = "User";
            var key = "admin@example.com";

            // Act
            var exception = new NotFoundException(name, key);

            // Assert
            exception.Message.Should().Contain(name);
            exception.Message.Should().Contain(key);
        }

        [Fact]
        public void ValidationException_ShouldHaveDefaultMessage()
        {
            // Act
            var exception = new ValidationException();

            // Assert
            exception.Message.Should().Contain("validacion");
        }

        [Fact]
        public void ValidationException_WithDuplicatePropertyFailures_ShouldGroupCorrectly()
        {
            // Arrange
            var failures = new List<ValidationFailure>
            {
                new ValidationFailure("Age", "Age must be greater than 0"),
                new ValidationFailure("Age", "Age must be less than 150"),
                new ValidationFailure("Age", "Age is required")
            };

            // Act
            var exception = new ValidationException(failures);

            // Assert
            exception.Errors["Age"].Should().HaveCount(3);
            exception.Errors["Age"].Should().Contain("Age must be greater than 0");
            exception.Errors["Age"].Should().Contain("Age must be less than 150");
            exception.Errors["Age"].Should().Contain("Age is required");
        }

        [Fact]
        public void NotFoundException_ShouldIncludeEntityName()
        {
            // Act
            var exception = new NotFoundException("Product", 42);

            // Assert
            exception.Message.Should().Contain("Product");
        }
    }
}

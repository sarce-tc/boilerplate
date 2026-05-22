using FluentAssertions;
using Microservice.Application.Common.Results;

namespace Microservice.Test.Application.Common.Results
{
    /// <summary>
    /// Unit tests for the Result pattern implementation
    /// Tests success and failure scenarios for both Result and Result<T>
    /// </summary>
    public class ResultTests
    {
        [Fact]
        public void Success_ShouldCreateSuccessfulResult()
        {
            // Act
            var result = Result.Success();

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Failure_WithSingleError_ShouldCreateFailureResult()
        {
            // Arrange
            var error = Error.Validation("Invalid input");

            // Act
            var result = Result.Failure(error);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].Code.Should().Be("Validation");
            result.Errors[0].Message.Should().Be("Invalid input");
        }

        [Fact]
        public void Failure_WithMultipleErrors_ShouldCreateFailureResultWithAllErrors()
        {
            // Arrange
            var errors = new List<Error>
            {
                Error.Validation("Error 1"),
                Error.Validation("Error 2"),
                Error.NotFound("Not found")
            };

            // Act
            var result = Result.Failure(errors);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().HaveCount(3);
            result.Errors.Should().ContainSingle(e => e.Code == "NotFound");
        }

        [Fact]
        public void FailureFromValidation_ShouldCreateFailureResultWithValidationErrors()
        {
            // Arrange
            var messages = new List<string> { "Field is required", "Field must be positive" };

            // Act
            var result = Result.FailureFromValidation(messages);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().HaveCount(2);
            result.Errors.Should().AllSatisfy(e => e.Code.Should().Be("Validation"));
            result.Errors.Select(e => e.Message).Should().Contain("Field is required");
            result.Errors.Select(e => e.Message).Should().Contain("Field must be positive");
        }

        [Fact]
        public void Failure_WithEmptyErrorList_ShouldCreateFailureResult()
        {
            // Arrange
            var errors = new List<Error>();

            // Act
            var result = Result.Failure(errors);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().BeEmpty();
        }
    }

    /// <summary>
    /// Unit tests for the generic Result<T> pattern
    /// Tests success and failure scenarios with typed values
    /// </summary>
    public class ResultGenericTests
    {
        [Fact]
        public void Success_WithValue_ShouldCreateSuccessfulResultWithData()
        {
            // Arrange
            var expectedValue = 42;

            // Act
            var result = Result<int>.Success(expectedValue);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(expectedValue);
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Success_WithNullValue_ShouldCreateSuccessfulResultWithNull()
        {
            // Act
            var result = Result<string>.Success(null!);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeNull();
        }

        [Fact]
        public void Failure_WithError_ShouldCreateFailureResultWithoutValue()
        {
            // Arrange
            var error = Error.NotFound("Item not found");

            // Act
            var result = Result<int>.Failure(error);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Value.Should().Be(default(int));
            result.Errors.Should().HaveCount(1);
            result.Errors[0].Code.Should().Be("NotFound");
        }

        [Fact]
        public void Success_WithDifferentTypes_ShouldWorkCorrectly()
        {
            // Act & Assert
            var stringResult = Result<string>.Success("test");
            stringResult.Value.Should().Be("test");

            var listResult = Result<List<int>>.Success(new List<int> { 1, 2, 3 });
            listResult.Value.Should().HaveCount(3);

            var objectResult = Result<object>.Success(new { Id = 1 });
            objectResult.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public void Success_WithComplexObject_ShouldStoreValueCorrectly()
        {
            // Arrange
            var dto = new { Id = 123, Name = "Example" };

            // Act
            var result = Result<object>.Success(dto);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(dto);
        }
    }

    /// <summary>
    /// Unit tests for the Error class
    /// Tests error creation and properties
    /// </summary>
    public class ErrorTests
    {
        [Fact]
        public void Validation_ShouldCreateValidationError()
        {
            // Act
            var error = Error.Validation("Invalid email");

            // Assert
            error.Code.Should().Be("Validation");
            error.Message.Should().Be("Invalid email");
        }

        [Fact]
        public void NotFound_ShouldCreateNotFoundError()
        {
            // Act
            var error = Error.NotFound("User not found");

            // Assert
            error.Code.Should().Be("NotFound");
            error.Message.Should().Be("User not found");
        }

        [Fact]
        public void Constructor_ShouldSetCodeAndMessage()
        {
            // Arrange
            var code = "CustomError";
            var message = "Custom error message";

            // Act
            var error = new Error(code, message);

            // Assert
            error.Code.Should().Be(code);
            error.Message.Should().Be(message);
        }

        [Fact]
        public void ErrorWithEmptyMessage_ShouldBeAllowed()
        {
            // Act
            var error = new Error("Code", "");

            // Assert
            error.Code.Should().Be("Code");
            error.Message.Should().BeEmpty();
        }
    }
}

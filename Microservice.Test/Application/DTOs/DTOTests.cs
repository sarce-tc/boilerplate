using FluentAssertions;
using Microservice.Application.DTOs;

namespace Microservice.Test.Application.DTOs;
/// <summary>
/// Unit tests for Data Transfer Objects (DTOs)
/// Tests DTO property initialization and data integrity
/// </summary>
public class GetExampleByIdDtoTests
{
    [Fact]
    public void DTO_ShouldInitializeProperties()
    {
        // Arrange
        var createdAt = DateTimeOffset.UtcNow;
        var updatedAt = DateTimeOffset.UtcNow;

        // Act
        var dto = new GetExampleByIdDto(Guid.NewGuid(), "Test", "Description", createdAt, updatedAt);

        // Assert
        dto.PublicId.Should().NotBe(Guid.Empty);
        dto.Name.Should().Be("Test");
        dto.Description.Should().Be("Description");
        dto.CreatedAt.Should().Be(createdAt);
        dto.UpdatedAt.Should().Be(updatedAt);
    }

    [Fact]
    public void DTO_ShouldInitializeNameAndDescription()
    {
        // Act
        var dto = new GetExampleByIdDto(Guid.NewGuid(), "Test", "Description", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);

        // Assert
        dto.Name.Should().Be("Test");
        dto.Description.Should().Be("Description");
    }

    [Fact]
    public void DTO_ShouldHandleDifferentIds()
    {
        // Act
        var dto = new GetExampleByIdDto(Guid.NewGuid(), "Test", "Description", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);

        // Assert
        dto.PublicId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void DTO_ShouldPreserveTimestamps()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;

        // Act
        var dto = new GetExampleByIdDto(Guid.NewGuid(), "Test", "Description", now, now.AddHours(1));

        // Assert
        dto.CreatedAt.Should().Be(now);
        dto.UpdatedAt.Should().Be(now.AddHours(1));
    }

    [Fact]
    public void DTO_ShouldAllowCreatedBeforeUpdatedAt()
    {
        // Arrange
        var createdAt = DateTimeOffset.UtcNow.AddDays(-1);
        var updatedAt = DateTimeOffset.UtcNow;

        // Act
        var dto = new GetExampleByIdDto(Guid.NewGuid(), "Test", "Description", createdAt, updatedAt);

        // Assert
        dto.CreatedAt.Should().BeBefore(dto.UpdatedAt);
    }
}

public class GetExampleByPredicateDtoTests
{
    [Fact]
    public void DTO_ShouldInitializeProperties()
    {
        // Arrange
        var createdAt = DateTimeOffset.UtcNow;
        var updatedAt = DateTimeOffset.UtcNow;

        // Act
        var dto = new GetExampleByPredicateDto(Guid.NewGuid(), "Test", "Description", createdAt, updatedAt);

        // Assert
        dto.PublicId.Should().NotBe(Guid.Empty);
        dto.Name.Should().Be("Test");
        dto.Description.Should().Be("Description");
        dto.CreatedAt.Should().Be(createdAt);
        dto.UpdatedAt.Should().Be(updatedAt);
    }

    [Fact]
    public void DTO_ShouldInitializeNameAndDescription()
    {
        // Act
        var dto = new GetExampleByPredicateDto(Guid.NewGuid(), "Test", "Description", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);

        // Assert
        dto.Name.Should().Be("Test");
        dto.Description.Should().Be("Description");
    }

    [Fact]
    public void DTO_WithDifferentIds_ShouldStoreCorrectValue()
    {
        // Act
        var dto = new GetExampleByPredicateDto(Guid.NewGuid(), "Test", "Description", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);

        // Assert
        dto.PublicId.Should().NotBe(Guid.Empty);
    }
}

public class DTOGeneralTests
{
    [Fact]
    public void MultipleObjects_ShouldMaintainSeparateState()
    {
        // Arrange
        var dto1 = new GetExampleByIdDto(Guid.NewGuid(), "Test", "Description", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);
        var dto2 = new GetExampleByIdDto(Guid.NewGuid(), "Test", "Description", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);

        // Act & Assert
        dto1.PublicId.Should().NotBe(dto2.PublicId);
        dto1.Should().NotBe(dto2);
    }

    [Fact]
    public void DTO_ShouldBeReusable()
    {
        // Arrange
        var createdAt = DateTimeOffset.UtcNow;

        // Act
        var dto = new GetExampleByIdDto(Guid.NewGuid(), "Test", "Description", createdAt, createdAt);

        // Since records are immutable, we create a new instance instead of modifying
        var updatedDto = new GetExampleByIdDto(Guid.NewGuid(), "Test", "Description", createdAt, createdAt);

        // Assert
        updatedDto.PublicId.Should().NotBe(dto.PublicId);
    }
}

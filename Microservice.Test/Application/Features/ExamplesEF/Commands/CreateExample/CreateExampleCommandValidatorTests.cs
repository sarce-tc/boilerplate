using FluentAssertions;
using Microservice.Application.Contracts.Persistence.EF;
using Microservice.Application.Features.ExamplesEF.Commands.CreateExample;
using Microservice.Domain.Entities;
using Moq;
using System.Linq.Expressions;

namespace Microservice.Test.Application.Features.Examples.Commands.CreateExample;

public class CreateExampleCommandValidatorTests
{
    private readonly Mock<IReadRepository<Example>> _mockRepo;
    private readonly CreateExampleCommandValidator  _validator;

    public CreateExampleCommandValidatorTests()
    {
        _mockRepo  = new Mock<IReadRepository<Example>>();
        _validator = new CreateExampleCommandValidator(_mockRepo.Object);

        // Default: name does not exist in DB
        _mockRepo
            .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Example, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
    }

    // ── Happy path ───────────────────────────────────────────────────────────

    [Fact]
    public async Task Validate_WithValidCommand_ShouldPass()
    {
        var result = await _validator.ValidateAsync(new CreateExampleCommand("Valid Name", "Description"));

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WithNullDescription_ShouldPass()
    {
        var result = await _validator.ValidateAsync(new CreateExampleCommand("Valid Name", null));

        result.IsValid.Should().BeTrue();
    }

    // ── Name: required ───────────────────────────────────────────────────────

    [Fact]
    public async Task Validate_WithNullName_ShouldFail_WithRequiredMessage()
    {
        var result = await _validator.ValidateAsync(new CreateExampleCommand(null!, null));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name" && e.ErrorMessage == "Name is required");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Validate_WithEmptyName_ShouldFail(string name)
    {
        var result = await _validator.ValidateAsync(new CreateExampleCommand(name, null));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    // ── Name: max length 200 ─────────────────────────────────────────────────

    [Fact]
    public async Task Validate_With201CharName_ShouldFail()
    {
        var result = await _validator.ValidateAsync(
            new CreateExampleCommand(new string('a', 201), null));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Name must not exceed 200 characters");
    }

    [Fact]
    public async Task Validate_WithExactly200CharName_ShouldPass()
    {
        var result = await _validator.ValidateAsync(
            new CreateExampleCommand(new string('a', 200), null));

        result.IsValid.Should().BeTrue();
    }

    // ── Name: duplicate check (async DB call) ─────────────────────────────────

    [Fact]
    public async Task Validate_WithDuplicateName_ShouldFail()
    {
        _mockRepo
            .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Example, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _validator.ValidateAsync(new CreateExampleCommand("Existing", null));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "An example with this name already exists");
    }

    // ── Description: max length 1000 ─────────────────────────────────────────

    [Fact]
    public async Task Validate_With1001CharDescription_ShouldFail()
    {
        var result = await _validator.ValidateAsync(
            new CreateExampleCommand("Name", new string('x', 1001)));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Description" &&
            e.ErrorMessage == "Description must not exceed 1000 characters");
    }

    [Fact]
    public async Task Validate_WithExactly1000CharDescription_ShouldPass()
    {
        var result = await _validator.ValidateAsync(
            new CreateExampleCommand("Name", new string('x', 1000)));

        result.IsValid.Should().BeTrue();
    }
}

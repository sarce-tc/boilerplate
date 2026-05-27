using FluentAssertions;
using Microservice.Application.Features.ExamplesEF.Commands.UpdateExample;

namespace Microservice.Test.Application.Features.Examples.Commands.UpdateExample;

public class UpdateExampleCommandValidatorTests
{
    private readonly UpdateExampleCommandValidator _validator = new();

    // ── Valid PublicId ────────────────────────────────────────────────────────

    [Fact]
    public async Task Validate_WithValidPublicId_ShouldPass()
    {
        var result = await _validator.ValidateAsync(
            new UpdateExampleCommand(Guid.NewGuid(), "Name", "Description"));

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    // ── Name and Description are optional ─────────────────────────────────────

    [Fact]
    public async Task Validate_WithNullNameAndDescription_ShouldPass()
    {
        var result = await _validator.ValidateAsync(
            new UpdateExampleCommand(Guid.NewGuid(), null, null));

        result.IsValid.Should().BeTrue();
    }

    // ── Empty PublicId fails ──────────────────────────────────────────────────

    [Fact]
    public async Task Validate_WithEmptyPublicId_ShouldFail_WithRequiredMessage()
    {
        var result = await _validator.ValidateAsync(
            new UpdateExampleCommand(Guid.Empty, "Name", "Description"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "PublicId" &&
            e.ErrorMessage == "PublicId is required");
    }

    [Fact]
    public async Task Validate_WithEmptyPublicId_ShouldHaveCorrectErrorCode()
    {
        var result = await _validator.ValidateAsync(
            new UpdateExampleCommand(Guid.Empty, null, null));

        result.Errors.Should().Contain(e => e.ErrorCode == "PublicIdInvalid");
    }

    // ── Exactly one error for empty PublicId ──────────────────────────────────

    [Fact]
    public async Task Validate_WithEmptyPublicId_ShouldHaveExactlyOneError()
    {
        var result = await _validator.ValidateAsync(
            new UpdateExampleCommand(Guid.Empty, null, null));

        result.Errors.Should().HaveCount(1);
    }
}

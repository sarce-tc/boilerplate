using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microservice.Application.Behaviours;
using Microservice.Application.Common.Results;
using Moq;
using AppValidationException = Microservice.Application.Exceptions.ValidationException;

namespace Microservice.Test.Application.Behaviours;

// Types must be at namespace level so Moq can proxy IValidator<T> for them
// (Moq + strong-named FluentValidation cannot create proxies for private nested types)
public record VBResultTCommand(string Name) : IRequest<Result<int>>;
public record VBResultCommand(string Name)  : IRequest<Result>;
public record VBPlainCommand(string Name)   : IRequest<string>;

public class ValidationBehaviourTests
{
    // ── Helpers ───────────────────────────────────────────────────────────────
    private static Mock<IValidator<T>> PassingValidator<T>()
    {
        var mock = new Mock<IValidator<T>>();
        mock.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<T>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        return mock;
    }

    private static Mock<IValidator<T>> FailingValidator<T>(params string[] messages)
    {
        var failures = messages.Select(m => new ValidationFailure("Field", m)).ToList();
        var mock = new Mock<IValidator<T>>();
        mock.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<T>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(failures));
        return mock;
    }

    // ── No validators: always passes through ─────────────────────────────────
    [Fact]
    public async Task Handle_WithNoValidators_ShouldPassThroughToNext()
    {
        var behavior = new ValidationBehaviour<VBResultTCommand, Result<int>>([]);
        var expected = Result<int>.Success(42);

        var result = await behavior.Handle(
            new VBResultTCommand("x"),
            ct => Task.FromResult(expected),
            CancellationToken.None);

        result.Should().Be(expected);
    }

    // ── All validators pass: next is called ───────────────────────────────────
    [Fact]
    public async Task Handle_WithPassingValidators_ShouldCallNext()
    {
        var behavior = new ValidationBehaviour<VBResultTCommand, Result<int>>(
            [PassingValidator<VBResultTCommand>().Object]);

        var nextCalled = false;
        await behavior.Handle(
            new VBResultTCommand("valid"),
            ct => { nextCalled = true; return Task.FromResult(Result<int>.Success(1)); },
            CancellationToken.None);

        nextCalled.Should().BeTrue();
    }

    // ── Result<T> response: failure returns Result.Failure, never throws ──────
    [Fact]
    public async Task Handle_WithFailingValidators_AndResultTResponse_ShouldReturnResultFailure()
    {
        var behavior = new ValidationBehaviour<VBResultTCommand, Result<int>>(
            [FailingValidator<VBResultTCommand>("Name is required").Object]);

        var nextCalled = false;
        var result = await behavior.Handle(
            new VBResultTCommand(""),
            ct => { nextCalled = true; return Task.FromResult(Result<int>.Success(1)); },
            CancellationToken.None);

        nextCalled.Should().BeFalse();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Code == "Validation");
    }

    [Fact]
    public async Task Handle_WithMultipleFailures_AndResultTResponse_ShouldContainAllErrors()
    {
        var behavior = new ValidationBehaviour<VBResultTCommand, Result<int>>(
            [FailingValidator<VBResultTCommand>("Error 1", "Error 2").Object]);

        var result = await behavior.Handle(
            new VBResultTCommand(""),
            ct => Task.FromResult(Result<int>.Success(1)),
            CancellationToken.None);

        result.Errors.Should().HaveCount(2);
    }

    // ── Result response: same behaviour ──────────────────────────────────────
    [Fact]
    public async Task Handle_WithFailingValidators_AndResultResponse_ShouldReturnResultFailure()
    {
        var behavior = new ValidationBehaviour<VBResultCommand, Result>(
            [FailingValidator<VBResultCommand>("Required").Object]);

        var nextCalled = false;
        var result = await behavior.Handle(
            new VBResultCommand(""),
            ct => { nextCalled = true; return Task.FromResult(Result.Success()); },
            CancellationToken.None);

        nextCalled.Should().BeFalse();
        result.IsSuccess.Should().BeFalse();
    }

    // ── Non-Result response: should throw ValidationException ─────────────────
    [Fact]
    public async Task Handle_WithFailingValidators_AndNonResultResponse_ShouldThrowValidationException()
    {
        var behavior = new ValidationBehaviour<VBPlainCommand, string>(
            [FailingValidator<VBPlainCommand>("Required").Object]);

        var act = () => behavior.Handle(
            new VBPlainCommand(""),
            ct => Task.FromResult("ok"),
            CancellationToken.None);

        await act.Should().ThrowAsync<AppValidationException>();
    }

    // ── Multiple validators aggregate errors ──────────────────────────────────
    [Fact]
    public async Task Handle_WithMultipleValidators_ShouldAggregateAllErrors()
    {
        var v1 = FailingValidator<VBResultTCommand>("Error from validator 1");
        var v2 = FailingValidator<VBResultTCommand>("Error from validator 2");

        var behavior = new ValidationBehaviour<VBResultTCommand, Result<int>>(
            [v1.Object, v2.Object]);

        var result = await behavior.Handle(
            new VBResultTCommand(""),
            ct => Task.FromResult(Result<int>.Success(1)),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
    }
}

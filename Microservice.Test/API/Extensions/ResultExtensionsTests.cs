using FluentAssertions;
using Microservice.API.Extensions;
using Microservice.Application.Common.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Microservice.Test.API.Extensions;

public class ResultExtensionsTests
{
    // ── Result (non-generic) ──────────────────────────────────────────────────

    [Fact]
    public void ToActionResult_SuccessResult_Returns204NoContent()
    {
        var actionResult = Result.Success().ToActionResult();

        actionResult.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public void ToActionResult_FailureResult_Returns400()
    {
        var actionResult = Result.Failure(Error.Validation("Bad")).ToActionResult();

        var obj = actionResult as ObjectResult;
        obj!.StatusCode.Should().Be(400);
    }

    // ── Result<T> ─────────────────────────────────────────────────────────────

    [Fact]
    public void ToActionResult_SuccessWithValue_Returns200Ok()
    {
        var actionResult = Result<string>.Success("hello").ToActionResult();

        var ok = actionResult as OkObjectResult;
        ok.Should().NotBeNull();
        ok!.Value.Should().Be("hello");
    }

    [Fact]
    public void ToActionResult_SuccessWithNullValue_Returns204NoContent()
    {
        var actionResult = Result<string>.Success(null!).ToActionResult();

        actionResult.Should().BeOfType<NoContentResult>();
    }

    // ── Error code → HTTP status mapping ─────────────────────────────────────

    [Theory]
    [InlineData("Validation",   400)]
    [InlineData("NotFound",     404)]
    [InlineData("Conflict",     409)]
    [InlineData("Unauthorized", 401)]
    [InlineData("Forbidden",    403)]
    [InlineData("Generic",      400)]
    public void ToActionResult_KnownErrorCodes_MapsToCorrectStatus(string code, int expectedStatus)
    {
        var error  = new Error(code, "message");
        var result = Result<string>.Failure(error);

        var obj = result.ToActionResult() as ObjectResult;
        obj!.StatusCode.Should().Be(expectedStatus);
    }

    // ── Custom success status code ────────────────────────────────────────────

    [Fact]
    public void ToActionResult_WithSuccessAndCustomStatus_Returns201()
    {
        var result = Result<Guid>.Success(Guid.NewGuid());

        var obj = result.ToActionResult(StatusCodes.Status201Created) as ObjectResult;
        obj!.StatusCode.Should().Be(201);
    }

    [Fact]
    public void ToActionResult_WithFailureAndCustomStatus_IgnoresCustomStatusAndMapsErrorCode()
    {
        var result = Result<Guid>.Failure(Error.NotFound("Not found"));

        var obj = result.ToActionResult(StatusCodes.Status201Created) as ObjectResult;
        obj!.StatusCode.Should().Be(404); // custom success code is ignored on failure
    }

    // ── Multiple errors: extras surfaced in extensions ────────────────────────

    [Fact]
    public void ToActionResult_WithMultipleErrors_ShouldIncludeExtensionsErrors()
    {
        var result = Result<string>.Failure(new List<Error>
        {
            Error.Validation("Field A required"),
            Error.Validation("Field B too long")
        });

        var obj     = result.ToActionResult() as ObjectResult;
        var problem = obj!.Value as ProblemDetails;
        problem!.Extensions.Should().ContainKey("errors");
    }

    [Fact]
    public void ToActionResult_WithSingleError_ShouldNotAddExtensionsErrors()
    {
        var result = Result<string>.Failure(Error.Validation("Required"));

        var obj     = result.ToActionResult() as ObjectResult;
        var problem = obj!.Value as ProblemDetails;
        problem!.Extensions.Should().NotContainKey("errors");
    }

    // ── Response body contains ProblemDetails ─────────────────────────────────

    [Fact]
    public void ToActionResult_Failure_ShouldReturnProblemDetails()
    {
        var result = Result<string>.Failure(Error.NotFound("Item not found"));

        var obj     = result.ToActionResult() as ObjectResult;
        var problem = obj!.Value as ProblemDetails;
        problem.Should().NotBeNull();
        problem!.Status.Should().Be(404);
        problem.Detail.Should().Be("Item not found");
    }
}

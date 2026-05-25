using FluentAssertions;
using FluentValidation.Results;
using Microservice.API.ExceptionHandling;
using Microservice.Application.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace Microservice.Test.API.ExceptionHandling;

public class GlobalExceptionHandlerTests
{
    private readonly GlobalExceptionHandler _handler;

    public GlobalExceptionHandlerTests()
    {
        var mockLogger = new Mock<ILogger<GlobalExceptionHandler>>();
        _handler = new GlobalExceptionHandler(mockLogger.Object);
    }

    private static DefaultHttpContext CreateContext(string? correlationId = null)
    {
        var ctx = new DefaultHttpContext();
        ctx.Response.Body = new MemoryStream();
        if (correlationId is not null)
            ctx.Items["X-Correlation-Id"] = correlationId;
        return ctx;
    }

    // ── Always returns true (exception handled) ───────────────────────────────

    [Fact]
    public async Task TryHandleAsync_ShouldAlwaysReturnTrue()
    {
        var handled = await _handler.TryHandleAsync(
            CreateContext(), new Exception("any"), CancellationToken.None);

        handled.Should().BeTrue();
    }

    // ── Exception → HTTP status mapping ──────────────────────────────────────

    [Fact]
    public async Task TryHandleAsync_ValidationException_Returns400()
    {
        var ex = new ValidationException(new[] { new ValidationFailure("Name", "Required") });

        await _handler.TryHandleAsync(CreateContext(), ex, CancellationToken.None);

        // The response status is set synchronously before WriteAsJsonAsync
        // We verify via a context that captures the status
        var ctx = CreateContext();
        await _handler.TryHandleAsync(ctx, ex, CancellationToken.None);
        ctx.Response.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task TryHandleAsync_KeyNotFoundException_Returns404()
    {
        var ctx = CreateContext();
        await _handler.TryHandleAsync(ctx, new KeyNotFoundException("Not found"), CancellationToken.None);
        ctx.Response.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task TryHandleAsync_InvalidOperationException_Returns409()
    {
        var ctx = CreateContext();
        await _handler.TryHandleAsync(ctx, new InvalidOperationException("Conflict"), CancellationToken.None);
        ctx.Response.StatusCode.Should().Be(409);
    }

    [Fact]
    public async Task TryHandleAsync_UnauthorizedAccessException_Returns401()
    {
        var ctx = CreateContext();
        await _handler.TryHandleAsync(ctx, new UnauthorizedAccessException("Denied"), CancellationToken.None);
        ctx.Response.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task TryHandleAsync_NotImplementedException_Returns501()
    {
        var ctx = CreateContext();
        await _handler.TryHandleAsync(ctx, new NotImplementedException("Not done"), CancellationToken.None);
        ctx.Response.StatusCode.Should().Be(501);
    }

    [Fact]
    public async Task TryHandleAsync_ArgumentException_Returns400()
    {
        var ctx = CreateContext();
        await _handler.TryHandleAsync(ctx, new ArgumentException("Bad arg"), CancellationToken.None);
        ctx.Response.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task TryHandleAsync_GenericException_Returns500()
    {
        var ctx = CreateContext();
        await _handler.TryHandleAsync(ctx, new Exception("Unexpected"), CancellationToken.None);
        ctx.Response.StatusCode.Should().Be(500);
    }

    // ── Content-type is application/problem+json ──────────────────────────────

    [Fact]
    public async Task TryHandleAsync_ShouldSetProblemJsonContentType()
    {
        var ctx = CreateContext();
        await _handler.TryHandleAsync(ctx, new Exception("err"), CancellationToken.None);
        ctx.Response.ContentType.Should().Be("application/problem+json");
    }

    // ── CorrelationId from HttpContext.Items is included ─────────────────────

    [Fact]
    public async Task TryHandleAsync_WithCorrelationId_ShouldNotThrow()
    {
        // The key assertion here is that having a correlationId in Items
        // doesn't break the handler — it's used to enrich the response body.
        var ctx = CreateContext(correlationId: "agent-task-001");
        var act = async () => await _handler.TryHandleAsync(ctx, new Exception("err"), CancellationToken.None);

        await act.Should().NotThrowAsync();
        ctx.Response.StatusCode.Should().Be(500);
    }

    // ── Response body is written ──────────────────────────────────────────────

    [Fact]
    public async Task TryHandleAsync_ShouldWriteNonEmptyResponseBody()
    {
        var ctx = CreateContext();
        await _handler.TryHandleAsync(ctx, new Exception("err"), CancellationToken.None);

        ctx.Response.Body.Position = 0;
        var body = await new StreamReader(ctx.Response.Body).ReadToEndAsync();
        body.Should().NotBeNullOrWhiteSpace();
    }
}

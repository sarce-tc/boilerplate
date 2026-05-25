using FluentAssertions;
using Microservice.API.Middleware;
using Microsoft.AspNetCore.Http;

namespace Microservice.Test.API.Middleware;

public class CorrelationIdMiddlewareTests
{
    private const string Header = "X-Correlation-Id";

    private static DefaultHttpContext CreateContext(string? correlationId = null)
    {
        var ctx = new DefaultHttpContext();
        ctx.Response.Body = new MemoryStream();
        if (correlationId is not null)
            ctx.Request.Headers[Header] = correlationId;
        return ctx;
    }

    // ── Always calls next ─────────────────────────────────────────────────────
    [Fact]
    public async Task InvokeAsync_ShouldAlwaysCallNext()
    {
        var ctx        = CreateContext();
        var nextCalled = false;
        var middleware = new CorrelationIdMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });

        await middleware.InvokeAsync(ctx);

        nextCalled.Should().BeTrue();
    }

    // ── No header provided: generates a new ID ────────────────────────────────
    [Fact]
    public async Task InvokeAsync_WithoutHeader_ShouldGenerateIdAndAddToResponse()
    {
        var ctx        = CreateContext();
        var middleware = new CorrelationIdMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(ctx);

        ctx.Response.Headers.Should().ContainKey(Header);
        ctx.Response.Headers[Header].ToString().Should().NotBeNullOrWhiteSpace();
    }

    // ── Header provided: echoes the client-supplied value ────────────────────
    [Fact]
    public async Task InvokeAsync_WithProvidedHeader_ShouldPreserveItInResponse()
    {
        var supplied   = "agent-task-abc123";
        var ctx        = CreateContext(supplied);
        var middleware = new CorrelationIdMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(ctx);

        ctx.Response.Headers[Header].ToString().Should().Be(supplied);
    }

    // ── ID is stored in HttpContext.Items for downstream use ─────────────────
    [Fact]
    public async Task InvokeAsync_ShouldStoreCorrelationIdInHttpContextItems()
    {
        var ctx        = CreateContext();
        var middleware = new CorrelationIdMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(ctx);

        ctx.Items.Should().ContainKey(Header);
        ctx.Items[Header]!.ToString().Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task InvokeAsync_WithProvidedId_ShouldStoreItInItems()
    {
        var supplied   = "my-id-999";
        var ctx        = CreateContext(supplied);
        var middleware = new CorrelationIdMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(ctx);

        ctx.Items[Header].Should().Be(supplied);
    }

    // ── Response header and Items value must be consistent ───────────────────
    [Fact]
    public async Task InvokeAsync_ResponseHeaderAndItemsShouldMatch()
    {
        var ctx        = CreateContext();
        var middleware = new CorrelationIdMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(ctx);

        var headerValue = ctx.Response.Headers[Header].ToString();
        var itemValue   = ctx.Items[Header]?.ToString();
        headerValue.Should().Be(itemValue);
    }

    // ── Two requests without headers get different IDs ────────────────────────
    [Fact]
    public async Task InvokeAsync_TwoRequestsWithoutHeader_ShouldGetDistinctIds()
    {
        var middleware = new CorrelationIdMiddleware(_ => Task.CompletedTask);

        var ctx1 = CreateContext();
        var ctx2 = CreateContext();

        await middleware.InvokeAsync(ctx1);
        await middleware.InvokeAsync(ctx2);

        var id1 = ctx1.Response.Headers[Header].ToString();
        var id2 = ctx2.Response.Headers[Header].ToString();
        id1.Should().NotBe(id2);
    }
}

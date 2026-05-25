using FluentAssertions;
using Microservice.API.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;

namespace Microservice.Test.API.Middleware;

public class IdempotencyMiddlewareTests
{
    private static IMemoryCache CreateCache() =>
        new MemoryCache(new MemoryCacheOptions());

    private static DefaultHttpContext CreateContext(string method, string? idempotencyKey = null)
    {
        var ctx = new DefaultHttpContext();
        ctx.Request.Method   = method;
        ctx.Response.Body    = new MemoryStream();
        if (idempotencyKey is not null)
            ctx.Request.Headers["Idempotency-Key"] = idempotencyKey;
        return ctx;
    }

    // ── Read-only methods pass through unchanged ──────────────────────────────
    [Theory]
    [InlineData("GET")]
    [InlineData("DELETE")]
    [InlineData("HEAD")]
    [InlineData("OPTIONS")]
    public async Task InvokeAsync_WithNonWriteMethod_ShouldPassThrough(string method)
    {
        var cache      = CreateCache();
        var nextCalled = false;
        var middleware = new IdempotencyMiddleware(_ => { nextCalled = true; return Task.CompletedTask; }, cache);

        await middleware.InvokeAsync(CreateContext(method));

        nextCalled.Should().BeTrue();
    }

    // ── Write methods without key pass through ────────────────────────────────
    [Theory]
    [InlineData("POST")]
    [InlineData("PUT")]
    [InlineData("PATCH")]
    public async Task InvokeAsync_WithWriteMethodButNoKey_ShouldPassThrough(string method)
    {
        var cache      = CreateCache();
        var nextCalled = false;
        var middleware = new IdempotencyMiddleware(_ => { nextCalled = true; return Task.CompletedTask; }, cache);

        await middleware.InvokeAsync(CreateContext(method)); // no key

        nextCalled.Should().BeTrue();
    }

    // ── First call with new key: executes and caches ──────────────────────────
    [Fact]
    public async Task InvokeAsync_PostWithNewKey_ShouldExecuteNextAndSetStatusCode()
    {
        var cache = CreateCache();
        var key   = Guid.NewGuid().ToString();

        var middleware = new IdempotencyMiddleware(ctx =>
        {
            ctx.Response.StatusCode  = 201;
            ctx.Response.ContentType = "application/json";
            return ctx.Response.WriteAsync("{\"id\":\"1\"}");
        }, cache);

        var ctx = CreateContext("POST", key);
        await middleware.InvokeAsync(ctx);

        ctx.Response.StatusCode.Should().Be(201);
    }

    // ── Second call with same key: returns cached, skips next ─────────────────
    [Fact]
    public async Task InvokeAsync_SecondCallWithSameKey_ShouldNotCallNextAgain()
    {
        var cache     = CreateCache();
        var key       = Guid.NewGuid().ToString();
        var callCount = 0;

        var middleware = new IdempotencyMiddleware(ctx =>
        {
            callCount++;
            ctx.Response.StatusCode  = 200;
            ctx.Response.ContentType = "application/json";
            return ctx.Response.WriteAsync("{}");
        }, cache);

        await middleware.InvokeAsync(CreateContext("POST", key));
        await middleware.InvokeAsync(CreateContext("POST", key));

        callCount.Should().Be(1);
    }

    // ── Replayed response has marker header ───────────────────────────────────
    [Fact]
    public async Task InvokeAsync_ReplayedRequest_ShouldContainReplayedHeader()
    {
        var cache = CreateCache();
        var key   = Guid.NewGuid().ToString();

        var middleware = new IdempotencyMiddleware(ctx =>
        {
            ctx.Response.StatusCode  = 200;
            ctx.Response.ContentType = "application/json";
            return ctx.Response.WriteAsync("{}");
        }, cache);

        await middleware.InvokeAsync(CreateContext("POST", key));

        var ctx2 = CreateContext("POST", key);
        await middleware.InvokeAsync(ctx2);

        ctx2.Response.Headers.Should().ContainKey("X-Idempotent-Replayed");
        ctx2.Response.Headers["X-Idempotent-Replayed"].ToString().Should().Be("true");
    }

    // ── Failed responses are NOT cached ──────────────────────────────────────
    [Fact]
    public async Task InvokeAsync_FailedResponse_ShouldNotBeCached()
    {
        var cache     = CreateCache();
        var key       = Guid.NewGuid().ToString();
        var callCount = 0;

        var middleware = new IdempotencyMiddleware(ctx =>
        {
            callCount++;
            ctx.Response.StatusCode  = 400;
            ctx.Response.ContentType = "application/json";
            return ctx.Response.WriteAsync("{\"error\":\"bad request\"}");
        }, cache);

        await middleware.InvokeAsync(CreateContext("POST", key));
        await middleware.InvokeAsync(CreateContext("POST", key));

        callCount.Should().Be(2); // both requests executed — error not cached
    }

    // ── PUT and PATCH are also covered ────────────────────────────────────────
    [Theory]
    [InlineData("PUT")]
    [InlineData("PATCH")]
    public async Task InvokeAsync_PutAndPatch_AreIdempotent(string method)
    {
        var cache     = CreateCache();
        var key       = Guid.NewGuid().ToString();
        var callCount = 0;

        var middleware = new IdempotencyMiddleware(ctx =>
        {
            callCount++;
            ctx.Response.StatusCode  = 200;
            ctx.Response.ContentType = "application/json";
            return ctx.Response.WriteAsync("{}");
        }, cache);

        await middleware.InvokeAsync(CreateContext(method, key));
        await middleware.InvokeAsync(CreateContext(method, key));

        callCount.Should().Be(1);
    }

    // ── Different keys are independent ───────────────────────────────────────
    [Fact]
    public async Task InvokeAsync_DifferentKeys_ShouldBeIndependent()
    {
        var cache     = CreateCache();
        var callCount = 0;

        var middleware = new IdempotencyMiddleware(ctx =>
        {
            callCount++;
            ctx.Response.StatusCode  = 200;
            return ctx.Response.WriteAsync("{}");
        }, cache);

        await middleware.InvokeAsync(CreateContext("POST", "key-A"));
        await middleware.InvokeAsync(CreateContext("POST", "key-B"));

        callCount.Should().Be(2); // each key executes independently
    }
}

using FluentAssertions;
using MediatR;
using Microservice.Application.Behaviours;
using Microservice.Application.Common.Interfaces;
using Microservice.Application.Contracts.Infrastructure;
using Moq;

namespace Microservice.Test.Application.Behaviours;

public class CachingBehaviorTests
{
    // ── Internal test request types ──────────────────────────────────────────
    private record NonCacheableQuery : IRequest<string>;

    private record CacheableQuery(string Key, TimeSpan? Ttl = null)
        : IRequest<string>, ICacheableQuery
    {
        public string CacheKey  => Key;
        public TimeSpan? Expiration => Ttl;
    }

    // ── Non-cacheable request: always passes through ──────────────────────────
    [Fact]
    public async Task Handle_WithNonCacheableRequest_ShouldPassThroughWithoutTouchingCache()
    {
        var mockCache = new Mock<ICacheService>();
        var behavior  = new CachingBehavior<NonCacheableQuery, string>(mockCache.Object);
        var expected  = "fresh";

        var result = await behavior.Handle(
            new NonCacheableQuery(),
            ct => Task.FromResult(expected),
            CancellationToken.None);

        result.Should().Be(expected);
        mockCache.Verify(c => c.GetAsync<string>(It.IsAny<string>()), Times.Never);
        mockCache.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>()), Times.Never);
    }

    // ── Cache HIT: returns cached value, skips next ───────────────────────────
    [Fact]
    public async Task Handle_WithCacheHit_ShouldReturnCachedValueWithoutCallingNext()
    {
        var mockCache = new Mock<ICacheService>();
        mockCache.Setup(c => c.GetAsync<string>("my-key")).ReturnsAsync("cached");

        var behavior   = new CachingBehavior<CacheableQuery, string>(mockCache.Object);
        var nextCalled = false;

        var result = await behavior.Handle(
            new CacheableQuery("my-key"),
            ct => { nextCalled = true; return Task.FromResult("fresh"); },
            CancellationToken.None);

        result.Should().Be("cached");
        nextCalled.Should().BeFalse();
    }

    // ── Cache MISS: calls next and stores result ──────────────────────────────
    [Fact]
    public async Task Handle_WithCacheMiss_ShouldCallNextAndStoreResult()
    {
        var mockCache = new Mock<ICacheService>();
        mockCache.Setup(c => c.GetAsync<string>("my-key")).ReturnsAsync((string?)null);

        var behavior = new CachingBehavior<CacheableQuery, string>(mockCache.Object);

        var result = await behavior.Handle(
            new CacheableQuery("my-key"),
            ct => Task.FromResult("fresh"),
            CancellationToken.None);

        result.Should().Be("fresh");
        mockCache.Verify(
            c => c.SetAsync("my-key", "fresh", It.IsAny<TimeSpan>()),
            Times.Once);
    }

    // ── Custom TTL is respected ───────────────────────────────────────────────
    [Fact]
    public async Task Handle_WithCacheMissAndCustomExpiration_ShouldUseProvidedTtl()
    {
        var mockCache = new Mock<ICacheService>();
        mockCache.Setup(c => c.GetAsync<string>("key")).ReturnsAsync((string?)null);

        var customTtl = TimeSpan.FromHours(2);
        var behavior  = new CachingBehavior<CacheableQuery, string>(mockCache.Object);

        await behavior.Handle(
            new CacheableQuery("key", customTtl),
            ct => Task.FromResult("result"),
            CancellationToken.None);

        mockCache.Verify(
            c => c.SetAsync("key", It.IsAny<string>(), customTtl),
            Times.Once);
    }

    // ── Null TTL defaults to 5 minutes ───────────────────────────────────────
    [Fact]
    public async Task Handle_WithNullExpiration_ShouldUseDefaultFiveMinutes()
    {
        var mockCache = new Mock<ICacheService>();
        mockCache.Setup(c => c.GetAsync<string>("key")).ReturnsAsync((string?)null);

        var behavior = new CachingBehavior<CacheableQuery, string>(mockCache.Object);

        await behavior.Handle(
            new CacheableQuery("key", null),
            ct => Task.FromResult("result"),
            CancellationToken.None);

        mockCache.Verify(
            c => c.SetAsync("key", It.IsAny<string>(), TimeSpan.FromMinutes(5)),
            Times.Once);
    }

    // ── Different cache keys use independent slots ────────────────────────────
    [Fact]
    public async Task Handle_DifferentCacheKeys_ShouldBeStoredIndependently()
    {
        var mockCache = new Mock<ICacheService>();
        mockCache.Setup(c => c.GetAsync<string>(It.IsAny<string>())).ReturnsAsync((string?)null);

        var behavior = new CachingBehavior<CacheableQuery, string>(mockCache.Object);

        await behavior.Handle(new CacheableQuery("key-A"), ct => Task.FromResult("A"), CancellationToken.None);
        await behavior.Handle(new CacheableQuery("key-B"), ct => Task.FromResult("B"), CancellationToken.None);

        mockCache.Verify(c => c.SetAsync("key-A", "A", It.IsAny<TimeSpan>()), Times.Once);
        mockCache.Verify(c => c.SetAsync("key-B", "B", It.IsAny<TimeSpan>()), Times.Once);
    }
}

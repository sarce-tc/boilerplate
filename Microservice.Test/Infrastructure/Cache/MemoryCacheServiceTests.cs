using FluentAssertions;
using Microservice.Infrastructure.Cache;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace Microservice.Test.Infrastructure.Cache;
public class MemoryCacheServiceTests
{
    private readonly IMemoryCache _memoryCache;
    private readonly MemoryCacheService _cacheService;

    public MemoryCacheServiceTests()
    {
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _cacheService = new MemoryCacheService(_memoryCache);
    }

    [Fact]
    public async Task GetAsync_WithExistingKey_ShouldReturnValue()
    {
        // Arrange
        var key = "test-key";
        var value = "test-value";
        _memoryCache.Set(key, value);

        // Act
        var result = await _cacheService.GetAsync<string>(key);

        // Assert
        result.Should().Be(value);
    }

    [Fact]
    public async Task GetAsync_WithNonExistentKey_ShouldReturnNull()
    {
        // Arrange
        var key = "non-existent-key";

        // Act
        var result = await _cacheService.GetAsync<string>(key);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task SetAsync_ShouldStoreValue()
    {
        // Arrange
        var key = "test-key";
        var value = "test-value";
        var ttl = TimeSpan.FromMinutes(5);

        // Act
        await _cacheService.SetAsync(key, value, ttl);

        // Assert
        var result = await _cacheService.GetAsync<string>(key);
        result.Should().Be(value);
    }

    [Fact]
    public async Task RemoveAsync_ShouldRemoveValue()
    {
        // Arrange
        var key = "test-key";
        var value = "test-value";
        _memoryCache.Set(key, value);

        // Act
        await _cacheService.RemoveAsync(key);

        // Assert
        var result = await _cacheService.GetAsync<string>(key);
        result.Should().BeNull();
    }

    [Fact]
    public async Task SetAsync_WithExpiration_ShouldExpireValue()
    {
        // Arrange
        var key = "test-key";
        var value = "test-value";
        var ttl = TimeSpan.FromMilliseconds(100);

        // Act
        await _cacheService.SetAsync(key, value, ttl);
        await Task.Delay(150, TestContext.Current.CancellationToken); // Esperar a que expire

        // Assert
        var result = await _cacheService.GetAsync<string>(key);
        result.Should().BeNull();
    }
}

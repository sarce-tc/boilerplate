using Microservice.Application.Contracts.Infrastructure;
using Microsoft.Extensions.Caching.Memory;

namespace Microservice.Infrastructure.Cache
{
    public class MemoryCacheService(IMemoryCache memoryCache) : ICacheService
    {
        private readonly IMemoryCache _memoryCache = memoryCache;
        public Task<T?> GetAsync<T>(string key)
        {
            _memoryCache.TryGetValue(key, out T? value);
            return Task.FromResult(value);
        }
        public Task SetAsync<T>(string key, T value, TimeSpan ttl)
        {
            _memoryCache.Set(key, value, ttl);
            return Task.CompletedTask;
        }
        public Task RemoveAsync(string key)
        {
            _memoryCache.Remove(key);
            return Task.CompletedTask;
        }
    }
}
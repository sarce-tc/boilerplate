using MediatR;
using Microservice.Application.Common.Interfaces;
using Microservice.Application.Contracts.Infrastructure;

namespace Microservice.Application.Behaviours
{
    public class CachingBehavior<TRequest, TResponse>(ICacheService cache) : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            if (request is not ICacheableQuery cacheable)
                return await next(cancellationToken);

            var cached = await cache.GetAsync<TResponse>(cacheable.CacheKey);
            if (cached is not null)
                return cached;

            var response = await next(cancellationToken);

            await cache.SetAsync(
                cacheable.CacheKey,
                response,
                cacheable.Expiration ?? TimeSpan.FromMinutes(5));

            return response;
        }
    }
}
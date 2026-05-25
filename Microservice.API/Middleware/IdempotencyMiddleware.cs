using Microsoft.Extensions.Caching.Memory;
using System.Text;

namespace Microservice.API.Middleware;

/// <summary>
/// Protects POST/PUT/PATCH endpoints from duplicate execution when agents retry.
/// Clients send an "Idempotency-Key" header; subsequent requests with the same key
/// receive the cached response without re-executing the handler.
/// Successful responses are cached for 24 h. Non-2xx responses are never cached.
/// </summary>
public sealed class IdempotencyMiddleware(RequestDelegate next, IMemoryCache cache)
{
    private const string IdempotencyKeyHeader = "Idempotency-Key";
    private static readonly HashSet<string> Methods =
        new(StringComparer.OrdinalIgnoreCase) { "POST", "PUT", "PATCH" };

    public async Task InvokeAsync(HttpContext context)
    {
        if (!Methods.Contains(context.Request.Method) ||
            !context.Request.Headers.TryGetValue(IdempotencyKeyHeader, out var keyValue) ||
            string.IsNullOrWhiteSpace(keyValue))
        {
            await next(context);
            return;
        }

        var cacheKey = $"idempotency:{keyValue}";

        if (cache.TryGetValue(cacheKey, out CachedResponse? cached) && cached is not null)
        {
            context.Response.StatusCode = cached.StatusCode;
            context.Response.ContentType = cached.ContentType;
            context.Response.Headers.Append("X-Idempotent-Replayed", "true");
            await context.Response.WriteAsync(cached.Body);
            return;
        }

        var originalBody = context.Response.Body;
        using var buffer = new MemoryStream();
        context.Response.Body = buffer;

        try
        {
            await next(context);
        }
        finally
        {
            buffer.Position = 0;
            var body = await new StreamReader(buffer, Encoding.UTF8, leaveOpen: true).ReadToEndAsync();
            buffer.Position = 0;
            await buffer.CopyToAsync(originalBody);
            context.Response.Body = originalBody;

            if (context.Response.StatusCode is >= 200 and < 300)
                cache.Set(
                    cacheKey,
                    new CachedResponse(context.Response.StatusCode, context.Response.ContentType ?? "application/json", body),
                    TimeSpan.FromHours(24));
        }
    }

    private sealed record CachedResponse(int StatusCode, string ContentType, string Body);
}

public static class IdempotencyMiddlewareExtensions
{
    public static IApplicationBuilder UseIdempotency(this IApplicationBuilder app)
        => app.UseMiddleware<IdempotencyMiddleware>();
}

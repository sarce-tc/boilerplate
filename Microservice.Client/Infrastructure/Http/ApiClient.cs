using System.Net.Http.Json;
using System.Text.Json;
using Microservice.Client.Shared.Results;

namespace Microservice.Client.Infrastructure.Http;

/// <summary>
/// Thin typed wrapper over <see cref="HttpClient"/> that every feature gateway depends on.
/// Returns <see cref="UiResult{T}"/> — never throws for expected failures (validation,
/// not-found, conflict, offline). Centralizes JSON options, error translation, and the
/// idempotency-key plumbing so gateways stay declarative.
///
/// Direct HttpClient use in components/gateways is forbidden by the architecture rules;
/// this is the only sanctioned entry point.
/// </summary>
public sealed class ApiClient(HttpClient http)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public Task<UiResult<T>> GetAsync<T>(string url, CancellationToken ct = default) =>
        SendAsync<T>(() => new HttpRequestMessage(HttpMethod.Get, url), ct);

    public Task<UiResult<TResponse>> PostAsync<TResponse>(
        string url, object body, string? idempotencyKey = null, CancellationToken ct = default) =>
        SendAsync<TResponse>(() => BuildJson(HttpMethod.Post, url, body, idempotencyKey), ct);

    public Task<UiResult<TResponse>> PutAsync<TResponse>(
        string url, object body, string? idempotencyKey = null, CancellationToken ct = default) =>
        SendAsync<TResponse>(() => BuildJson(HttpMethod.Put, url, body, idempotencyKey), ct);

    /// <summary>POST with no request body (e.g. /sales/{id}/confirm).</summary>
    public Task<UiResult<TResponse>> PostEmptyAsync<TResponse>(
        string url, string? idempotencyKey = null, CancellationToken ct = default) =>
        SendAsync<TResponse>(() => WithKey(new HttpRequestMessage(HttpMethod.Post, url), idempotencyKey), ct);

    public Task<UiResult> DeleteAsync(string url, string? idempotencyKey = null, CancellationToken ct = default) =>
        SendUnitAsync(() => WithKey(new HttpRequestMessage(HttpMethod.Delete, url), idempotencyKey), ct);

    /// <summary>DELETE that returns a payload (the backend echoes the deleted PublicId).</summary>
    public Task<UiResult<TResponse>> DeleteAsync<TResponse>(
        string url, string? idempotencyKey = null, CancellationToken ct = default) =>
        SendAsync<TResponse>(() => WithKey(new HttpRequestMessage(HttpMethod.Delete, url), idempotencyKey), ct);

    /// <summary>
    /// Replays a persisted mutation (used by the SyncProcessor). Body is already-serialized
    /// JSON; the idempotency key is mandatory so backend dedup makes replays exactly-once.
    /// </summary>
    public Task<UiResult> ReplayAsync(
        string method, string url, string? jsonBody, string idempotencyKey, CancellationToken ct = default) =>
        SendUnitAsync(() =>
        {
            var request = new HttpRequestMessage(new HttpMethod(method), url);
            if (jsonBody is not null)
                request.Content = new StringContent(jsonBody, System.Text.Encoding.UTF8, "application/json");
            return WithKey(request, idempotencyKey);
        }, ct);

    // ── Core send loop ────────────────────────────────────────────────────────

    private async Task<UiResult<T>> SendAsync<T>(Func<HttpRequestMessage> factory, CancellationToken ct)
    {
        try
        {
            using var response = await http.SendAsync(factory(), ct);
            if (!response.IsSuccessStatusCode)
                return UiResult<T>.Failure(await ProblemDetailsReader.ReadAsync(response, ct));

            // 204 / empty body → default (valid for value types via JSON null handling).
            if (response.Content.Headers.ContentLength is 0)
                return UiResult<T>.Success(default!);

            var value = await response.Content.ReadFromJsonAsync<T>(JsonOptions, ct);
            return UiResult<T>.Success(value!);
        }
        catch (HttpRequestException)
        {
            return UiResult<T>.Failure(UiError.Network());
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            return UiResult<T>.Failure(UiError.Network());
        }
    }

    private async Task<UiResult> SendUnitAsync(Func<HttpRequestMessage> factory, CancellationToken ct)
    {
        try
        {
            using var response = await http.SendAsync(factory(), ct);
            return response.IsSuccessStatusCode
                ? UiResult.Success()
                : UiResult.Failure(await ProblemDetailsReader.ReadAsync(response, ct));
        }
        catch (HttpRequestException)
        {
            return UiResult.Failure(UiError.Network());
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            return UiResult.Failure(UiError.Network());
        }
    }

    private static HttpRequestMessage BuildJson(HttpMethod method, string url, object body, string? key)
    {
        var request = new HttpRequestMessage(method, url) { Content = JsonContent.Create(body, options: JsonOptions) };
        return WithKey(request, key);
    }

    private static HttpRequestMessage WithKey(HttpRequestMessage request, string? key)
    {
        if (!string.IsNullOrWhiteSpace(key))
            request.Options.Set(RequestOptions.IdempotencyKey, key);
        return request;
    }
}

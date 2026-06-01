using System.Text.Json;
using Microsoft.JSInterop;

namespace Microservice.Client.Infrastructure.Offline.IndexedDb;

/// <summary>
/// Lazy-initialized IndexedDB facade. Serializes values to JSON in C# (one JSON contract,
/// trim-safe) and stores strings via the interop module. The DB schema (stores) is created
/// on first open by the JS module.
/// </summary>
public sealed class IndexedDb(IJSRuntime js) : IIndexedDb, IAsyncDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private IJSObjectReference? _module;

    private async ValueTask<IJSObjectReference> ModuleAsync() =>
        _module ??= await js.InvokeAsync<IJSObjectReference>("import", "./js/indexeddb.js");

    public async ValueTask PutAsync<T>(string store, string key, T value)
    {
        var module = await ModuleAsync();
        var json = JsonSerializer.Serialize(value, JsonOptions);
        await module.InvokeVoidAsync("put", store, key, json);
    }

    public async ValueTask<T?> GetAsync<T>(string store, string key)
    {
        var module = await ModuleAsync();
        var json = await module.InvokeAsync<string?>("get", store, key);
        return json is null ? default : JsonSerializer.Deserialize<T>(json, JsonOptions);
    }

    public async ValueTask<IReadOnlyList<T>> GetAllAsync<T>(string store)
    {
        var module = await ModuleAsync();
        var jsons = await module.InvokeAsync<string[]>("getAll", store);
        return jsons
            .Select(j => JsonSerializer.Deserialize<T>(j, JsonOptions))
            .Where(v => v is not null)
            .Select(v => v!)
            .ToList();
    }

    public async ValueTask DeleteAsync(string store, string key)
    {
        var module = await ModuleAsync();
        await module.InvokeVoidAsync("remove", store, key);
    }

    public async ValueTask ClearAsync(string store)
    {
        var module = await ModuleAsync();
        await module.InvokeVoidAsync("clear", store);
    }

    public async ValueTask DisposeAsync()
    {
        if (_module is not null)
            await _module.DisposeAsync();
    }
}

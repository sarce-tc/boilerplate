using Microsoft.JSInterop;

namespace Microservice.Client.Infrastructure.Offline;

/// <summary>
/// Bridges the browser's connectivity events into C#. JS calls back into
/// <see cref="OnStatusChanged"/> via a DotNetObjectReference whenever online/offline fires.
/// </summary>
public sealed class Connectivity(IJSRuntime js) : IConnectivity
{
    private IJSObjectReference? _module;
    private DotNetObjectReference<Connectivity>? _selfRef;

    public bool IsOnline { get; private set; } = true;
    public event Action<bool>? StatusChanged;

    public async Task InitializeAsync()
    {
        _module = await js.InvokeAsync<IJSObjectReference>("import", "./js/connectivity.js");
        _selfRef = DotNetObjectReference.Create(this);
        IsOnline = await _module.InvokeAsync<bool>("initialize", _selfRef);
    }

    [JSInvokable]
    public void OnStatusChanged(bool isOnline)
    {
        if (IsOnline == isOnline)
            return;
        IsOnline = isOnline;
        StatusChanged?.Invoke(isOnline);
    }

    public async ValueTask DisposeAsync()
    {
        if (_module is not null)
        {
            await _module.InvokeVoidAsync("dispose");
            await _module.DisposeAsync();
        }
        _selfRef?.Dispose();
    }
}

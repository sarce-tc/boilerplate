using Microsoft.JSInterop;

namespace Microservice.Client.Features.Sales.Services;

/// <summary>
/// JS-interop implementation of the keyboard-wedge scanner. DI-owned (singleton in WASM): the JS
/// module is imported once and reused across Start/Stop cycles. Components call Start/Stop — never
/// DisposeAsync (the container owns the lifetime and disposes the module at app shutdown).
/// </summary>
public sealed class BarcodeScanner(IJSRuntime js) : IBarcodeScanner
{
    private IJSObjectReference? _module;
    private DotNetObjectReference<BarcodeScanner>? _selfRef;
    private bool _running;

    public event Action<string>? Scanned;

    public async Task StartAsync()
    {
        if (_running)
            return;

        _module ??= await js.InvokeAsync<IJSObjectReference>("import", "./js/barcode-scanner.js");
        _selfRef ??= DotNetObjectReference.Create(this);
        await _module.InvokeVoidAsync("start", _selfRef);
        _running = true;
    }

    public async Task StopAsync()
    {
        if (!_running || _module is null)
            return;
        _running = false;
        await SafeStopAsync();
    }

    [JSInvokable]
    public void OnScan(string code)
    {
        if (!string.IsNullOrWhiteSpace(code))
            Scanned?.Invoke(code.Trim());
    }

    public async ValueTask DisposeAsync()
    {
        _running = false;
        if (_module is not null)
        {
            await SafeStopAsync();
            try { await _module.DisposeAsync(); }
            catch (JSDisconnectedException) { }
            catch (ObjectDisposedException) { }
            _module = null;
        }
        _selfRef?.Dispose();
        _selfRef = null;
    }

    // Calling JS during teardown can race the renderer/circuit disposing the JS object reference.
    private async Task SafeStopAsync()
    {
        try { await _module!.InvokeVoidAsync("stop"); }
        catch (JSDisconnectedException) { }
        catch (ObjectDisposedException) { }
    }
}

namespace Microservice.Client.Features.Sales.Services;

/// <summary>
/// Global keyboard-wedge barcode capture. A hardware scanner emits keystrokes very fast and
/// ends with Enter; the JS layer distinguishes that burst from human typing and reports the
/// decoded code here. The POS page subscribes while active and starts/stops capture.
/// </summary>
public interface IBarcodeScanner : IAsyncDisposable
{
    /// <summary>Raised with the decoded code when a scan burst completes.</summary>
    event Action<string>? Scanned;

    Task StartAsync();
    Task StopAsync();
}

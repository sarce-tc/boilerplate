// Connectivity interop module. Reports online/offline transitions back to .NET.
let handlers = null;

export function initialize(dotNetRef) {
    handlers = {
        online: () => dotNetRef.invokeMethodAsync('OnStatusChanged', true),
        offline: () => dotNetRef.invokeMethodAsync('OnStatusChanged', false),
    };
    window.addEventListener('online', handlers.online);
    window.addEventListener('offline', handlers.offline);
    return navigator.onLine;
}

export function dispose() {
    if (!handlers) return;
    window.removeEventListener('online', handlers.online);
    window.removeEventListener('offline', handlers.offline);
    handlers = null;
}

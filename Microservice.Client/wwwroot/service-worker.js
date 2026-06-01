// Development service worker — intentionally a no-op so the dev loop is not cached.
// The real offline behaviour lives in service-worker.published.js (used on `dotnet publish`).
self.addEventListener('install', () => self.skipWaiting());
self.addEventListener('activate', () => self.clients.claim());
self.addEventListener('fetch', () => { /* pass-through */ });

// Production service worker — app-shell precache + offline navigation fallback.
//
// Strategy (POS-aware):
//   • App shell (html/js/wasm/css/fonts/icons) → precache, cache-first.
//   • Navigation requests → serve cached index.html when offline (SPA shell).
//   • API calls (/api/...) → NEVER cached here. Reads are cached in IndexedDB by the
//     feature gateways; writes go through the SyncQueue. The SW must not interfere.
self.importScripts('./service-worker-assets.js');

const CACHE_PREFIX = 'pos-cache-';
const CACHE_NAME = CACHE_PREFIX + self.assetsManifest.version;

// Only assets that make up the offline app shell.
const offlineAssetsInclude = [/\.dll$/, /\.pdb$/, /\.wasm$/, /\.html$/, /\.js$/, /\.json$/, /\.css$/, /\.woff2?$/, /\.png$/, /\.ico$/, /\.blat$/, /\.dat$/];
const offlineAssetsExclude = [/^service-worker\.js$/];

self.addEventListener('install', event => event.waitUntil(onInstall()));
self.addEventListener('activate', event => event.waitUntil(onActivate()));
self.addEventListener('fetch', event => event.respondWith(onFetch(event)));

async function onInstall() {
    const assets = self.assetsManifest.assets
        .filter(a => offlineAssetsInclude.some(p => p.test(a.url)))
        .filter(a => !offlineAssetsExclude.some(p => p.test(a.url)))
        .map(a => new Request(a.url, { integrity: a.hash, cache: 'no-cache' }));
    await caches.open(CACHE_NAME).then(c => c.addAll(assets));
    self.skipWaiting();
}

async function onActivate() {
    const keys = await caches.keys();
    await Promise.all(keys.filter(k => k.startsWith(CACHE_PREFIX) && k !== CACHE_NAME).map(k => caches.delete(k)));
    self.clients.claim();
}

async function onFetch(event) {
    const req = event.request;

    // Never intercept API traffic — gateways + SyncQueue own that contract.
    const url = new URL(req.url);
    if (url.pathname.startsWith('/api/')) {
        return fetch(req);
    }

    if (req.method !== 'GET') {
        return fetch(req);
    }

    // SPA navigation → serve the cached shell when the network is unavailable.
    const isNavigation = req.mode === 'navigate';
    const cache = await caches.open(CACHE_NAME);
    const cachedShell = isNavigation ? await cache.match('index.html') : await cache.match(req);

    try {
        return cachedShell ?? await fetch(req);
    } catch {
        return cachedShell ?? Response.error();
    }
}

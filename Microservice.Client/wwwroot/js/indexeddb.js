// IndexedDB interop module. Values are opaque JSON strings (serialized in C#).
// One database, two object stores created on upgrade. Bump DB_VERSION to evolve the schema.
const DB_NAME = 'pos-db';
const DB_VERSION = 1;
const STORES = ['read-cache', 'sync-queue'];

let dbPromise = null;

function openDb() {
    if (dbPromise) return dbPromise;
    dbPromise = new Promise((resolve, reject) => {
        const req = indexedDB.open(DB_NAME, DB_VERSION);
        req.onupgradeneeded = () => {
            const db = req.result;
            for (const name of STORES) {
                if (!db.objectStoreNames.contains(name)) {
                    db.createObjectStore(name);
                }
            }
        };
        req.onsuccess = () => resolve(req.result);
        req.onerror = () => reject(req.error);
    });
    return dbPromise;
}

function tx(db, store, mode) {
    return db.transaction(store, mode).objectStore(store);
}

function asPromise(request) {
    return new Promise((resolve, reject) => {
        request.onsuccess = () => resolve(request.result);
        request.onerror = () => reject(request.error);
    });
}

export async function put(store, key, json) {
    const db = await openDb();
    await asPromise(tx(db, store, 'readwrite').put(json, key));
}

export async function get(store, key) {
    const db = await openDb();
    const result = await asPromise(tx(db, store, 'readonly').get(key));
    return result ?? null;
}

export async function getAll(store) {
    const db = await openDb();
    const result = await asPromise(tx(db, store, 'readonly').getAll());
    return result ?? [];
}

export async function remove(store, key) {
    const db = await openDb();
    await asPromise(tx(db, store, 'readwrite').delete(key));
}

export async function clear(store) {
    const db = await openDb();
    await asPromise(tx(db, store, 'readwrite').clear());
}

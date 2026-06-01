// Keyboard-wedge barcode capture.
// A hardware scanner types the code as fast keystrokes terminated by Enter. We buffer keydowns
// and only treat the buffer as a scan when the characters arrived faster than a human could type
// (INTERVAL_MS) and the burst ends with Enter. Human typing into inputs is left untouched.
const INTERVAL_MS = 35;   // max gap between keystrokes to be considered "scanned"
const MIN_LENGTH = 3;     // ignore stray fast keypresses

let dotNetRef = null;
let buffer = '';
let lastTime = 0;
let handler = null;

function onKeyDown(e) {
    const now = Date.now();

    // Reset the buffer if too much time passed since the last key (human pace).
    if (now - lastTime > INTERVAL_MS) {
        buffer = '';
    }
    lastTime = now;

    if (e.key === 'Enter') {
        if (buffer.length >= MIN_LENGTH) {
            const code = buffer;
            buffer = '';
            e.preventDefault(); // don't submit a form on the scan's terminating Enter
            dotNetRef.invokeMethodAsync('OnScan', code);
        }
        return;
    }

    // Only printable single characters form a barcode.
    if (e.key.length === 1) {
        buffer += e.key;
    }
}

export function start(ref) {
    dotNetRef = ref;
    buffer = '';
    handler = onKeyDown;
    document.addEventListener('keydown', handler, true);
}

export function stop() {
    if (handler) {
        document.removeEventListener('keydown', handler, true);
        handler = null;
    }
    dotNetRef = null;
    buffer = '';
}

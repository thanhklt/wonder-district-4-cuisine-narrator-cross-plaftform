// Booth manager module (extracted from app.js)
(function () {
    // Wait until the main app has defined helper functions used by this module
    function ensureReady() {
        return !!(window.getBooths && window.saveBooths && window.showToast && window.initBoothModule);
    }

    function boot() {
        if (typeof window.initBoothModule === 'function') {
            try { window.initBoothModule(); } catch (e) { console.error('initBoothModule failed', e); }
        }
    }

    function waitForMain() {
        if (ensureReady()) return boot();
        let tries = 0;
        const t = setInterval(() => {
            if (ensureReady()) {
                clearInterval(t);
                boot();
            } else if (++tries > 20) {
                clearInterval(t);
                console.warn('booth-manager: main app did not become ready in time');
            }
        }, 150);
    }

    // Auto-run
    if (document.readyState === 'complete' || document.readyState === 'interactive') {
        waitForMain();
    } else {
        document.addEventListener('DOMContentLoaded', waitForMain);
    }
})();

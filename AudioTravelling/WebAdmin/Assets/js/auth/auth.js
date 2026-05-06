// Auth module bootstrap
(function () {
    function ensureReady() {
        return !!(window.doLogin && window.getAccounts && document.getElementById('form-login'));
    }

    function boot() {
        window.initAuthModule = function () {
            if (window._authBooted) return;
            window._authBooted = true;

            // Ensure the auth tab is in a consistent state
            try {
                if (typeof switchAuthTab === 'function') switchAuthTab('login');
            } catch (e) { /* ignore */ }

            // If a session exists, ensure UI reflects it
            try {
                const sess = localStorage.getItem('exhib_session');
                if (sess) {
                    const s = JSON.parse(sess);
                    if (s && s.loggedIn) {
                        document.getElementById('sidebar-name').textContent = s.name || 'Admin';
                    }
                }
            } catch (e) { }
        };
    }

    if (document.readyState === 'complete' || document.readyState === 'interactive') boot();
    else document.addEventListener('DOMContentLoaded', boot);
})();

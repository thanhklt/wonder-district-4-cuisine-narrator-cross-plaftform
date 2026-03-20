// Auth module bootstrap
(function () {
    let standaloneLoginBound = false;

    function bindStandaloneLogin() {
        const formLogin = document.getElementById('form-login');
        if (!formLogin || standaloneLoginBound) return;

        standaloneLoginBound = true;

        formLogin.addEventListener('submit', function (e) {
            e.preventDefault();

            const email = (document.getElementById('login-email')?.value || '').trim().toLowerCase();
            const password = document.getElementById('login-password')?.value || '';
            const errorEl = document.getElementById('login-error');
            const errorText = document.getElementById('login-error-text');

            if (email === 'owner@gmail.com' && password === 'owner123') {
                window.location.href = './Views/Owner/DashboardOwner.html';
                return;
            }
            else if (email === 'admin@gmail.com' && password === 'admin123') {
                window.location.href = './Views/Admin/DashboardAdmin.html';
                return;
            }
            if (errorText) errorText.textContent = 'Sai email hoặc mật khẩu.';
            if (errorEl) errorEl.classList.remove('hidden');
        });
    }

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

            // Standalone login page flow
            bindStandaloneLogin();
        };

        // Ensure standalone page can work immediately.
        bindStandaloneLogin();
    }

    if (document.readyState === 'complete' || document.readyState === 'interactive') boot();
    else document.addEventListener('DOMContentLoaded', boot);
})();

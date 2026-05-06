/**
 * ═══════════════════════════════════════════════════
 * Audio Travelling — Auth Page Bootstrap
 *
 * Runs on the login page (Home/Index with _AuthLayout).
 * - Auto-redirects to correct portal if already logged in.
 * - Initialises theme, binds login form, handles login flow.
 * ═══════════════════════════════════════════════════
 */
document.addEventListener('DOMContentLoaded', function () {
    'use strict';

    var AT = window.AudioTravelling;
    if (!AT || !AT.Core) {
        console.error('AudioTravelling modules not loaded. Check script order in _AuthLayout.cshtml.');
        return;
    }

    // 1. Initialise theme
    AT.Core.UI.initTheme();

    // 2. Auto-redirect if already logged in
    var session = AT.Core.Storage.getSession();
    if (session && session.loggedIn && session.role) {
        var role = String(session.role).toLowerCase();
        if (role === 'admin') {
            window.location.replace('/admin');
            return;
        }
        if (role === 'owner') {
            window.location.replace('/owner');
            return;
        }
    }

    // 3. Bind password toggle
    document.querySelectorAll('.auth-toggle-pw').forEach(function (btn) {
        btn.addEventListener('click', function () {
            var targetId = btn.getAttribute('data-target');
            var input = document.getElementById(targetId);
            var icon = btn.querySelector('i');
            if (!input || !icon) return;
            if (input.type === 'password') {
                input.type = 'text';
                icon.className = 'fa-solid fa-eye-slash';
            } else {
                input.type = 'password';
                icon.className = 'fa-solid fa-eye';
            }
        });
    });

    // 4. Restore remembered email
    var savedEmail = AT.Core.Storage.getRememberEmail();
    var emailInput = document.getElementById('login-email');
    var rememberCheck = document.getElementById('login-remember');
    if (savedEmail && emailInput) {
        emailInput.value = savedEmail;
        if (rememberCheck) rememberCheck.checked = true;
    }

    // 5. Bind login form
    var form = document.getElementById('form-login');
    if (!form) return;

    form.addEventListener('submit', async function (e) {
        e.preventDefault();

        var emailEl = document.getElementById('login-email');
        var passwordEl = document.getElementById('login-password');
        var rememberEl = document.getElementById('login-remember');
        var errorEl = document.getElementById('login-error');
        var errorText = document.getElementById('login-error-text');
        var btn = document.getElementById('btn-login');

        var email = emailEl ? emailEl.value.trim() : '';
        var password = passwordEl ? passwordEl.value : '';
        var remember = rememberEl ? rememberEl.checked : false;

        if (errorEl) errorEl.classList.add('hidden');

        // Show loading state
        if (btn) {
            btn.disabled = true;
            var btnTextEl = btn.querySelector('.auth-btn-text');
            if (btnTextEl) btnTextEl.textContent = 'Đang đăng nhập...';
        }

        // MUST await the async login call
        var result = await AT.Core.Auth.login(email, password);

        console.log('[appAuth] Login result:', result);

        if (!result || !result.success) {
            // Login failed — show error
            if (btn) {
                btn.disabled = false;
                var failBtnText = btn.querySelector('.auth-btn-text');
                if (failBtnText) failBtnText.textContent = 'Đăng nhập';
            }
            if (errorText) errorText.textContent = (result && result.error) ? result.error : 'Đăng nhập thất bại.';
            if (errorEl) errorEl.classList.remove('hidden');
            return;
        }

        // Login success — save remember email
        AT.Core.Storage.setRememberEmail(remember ? email : null);

        var loginSession = result.session;

        // Reset button state
        if (btn) {
            btn.disabled = false;
            var successBtnText = btn.querySelector('.auth-btn-text');
            if (successBtnText) successBtnText.textContent = 'Đăng nhập';
        }

        // Redirect to correct portal based on role — IMMEDIATELY, no setTimeout
        var role = String(loginSession.role || '').toLowerCase();
        console.log('[appAuth] Redirecting role:', role);

        if (role === 'admin') {
            window.location.href = '/admin';
        } else if (role === 'owner') {
            window.location.href = '/owner';
        } else {
            // Unknown role
            if (errorText) errorText.textContent = 'Vai trò không hợp lệ: ' + loginSession.role;
            if (errorEl) errorEl.classList.remove('hidden');
            AT.Core.Storage.clearSession();
        }
    });

    // 6. Ripple effect for buttons
    document.addEventListener('click', function (e) {
        var btn = e.target.closest('.btn-ripple');
        if (btn) {
            var ripple = document.createElement('span');
            ripple.className = 'ripple-effect';
            var rect = btn.getBoundingClientRect();
            var size = Math.max(rect.width, rect.height);
            ripple.style.width = ripple.style.height = size + 'px';
            ripple.style.left = (e.clientX - rect.left - size / 2) + 'px';
            ripple.style.top = (e.clientY - rect.top - size / 2) + 'px';
            btn.appendChild(ripple);
            setTimeout(function () { ripple.remove(); }, 600);
        }
    });
});

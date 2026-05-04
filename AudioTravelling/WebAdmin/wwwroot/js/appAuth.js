/**
 * ═══════════════════════════════════════════════════
 * Audio Travelling — Auth Page Bootstrap
 *
 * Runs on the login page (Home/Index with _AuthLayout).
 * - Auto-redirects to correct portal if already logged in.
 * - Initialises theme, binds login form, handles login flow.
 *
 * TODO: Replace mock login with real API call:
 *   POST /api/auth/login { email, password }
 *   Response: { token, role, userId, name }
 *   Then store in at_session (or migrate to access_token, user_role, user_id).
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
        if (session.role === 'Admin') {
            window.location.replace('/admin');
            return;
        }
        if (session.role === 'Owner') {
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

    form.addEventListener('submit', function (e) {
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

        /**
         * TODO: Replace this mock login with real API call:
         *   fetch('/api/auth/login', {
         *       method: 'POST',
         *       headers: { 'Content-Type': 'application/json' },
         *       body: JSON.stringify({ email: email, password: password })
         *   })
         *   .then(function(res) { return res.json(); })
         *   .then(function(data) {
         *       if (data.token) {
         *           AT.Core.Storage.setSession({
         *               name: data.name,
         *               email: email,
         *               role: data.role,
         *               loggedIn: true,
         *               token: data.token,
         *               userId: data.userId
         *           });
         *           // redirect based on role
         *       }
         *   });
         */
        var result = AT.Core.Auth.login(email, password);

        if (!result.success) {
            if (errorText) errorText.textContent = result.error;
            if (errorEl) errorEl.classList.remove('hidden');
            return;
        }

        // Remember email
        AT.Core.Storage.setRememberEmail(remember ? email : null);

        // Show loading state
        if (btn) {
            btn.disabled = true;
            var btnText = btn.querySelector('.auth-btn-text');
            if (btnText) btnText.textContent = 'Đang đăng nhập...';
        }

        setTimeout(function () {
            var session = result.session;

            // Show toast before redirect
            if (typeof Toastify !== 'undefined') {
                Toastify({
                    text: 'Chào mừng trở lại, ' + session.name.split(' ')[0] + '!',
                    duration: 2000,
                    gravity: 'bottom',
                    position: 'right',
                    style: {
                        background: 'linear-gradient(135deg, #059669, #22c55e)',
                        borderRadius: '12px',
                        fontSize: '13px',
                        padding: '12px 20px'
                    }
                }).showToast();
            }

            // Redirect to correct portal based on role
            setTimeout(function () {
                if (session.role === 'Admin') {
                    window.location.href = '/admin';
                } else if (session.role === 'Owner') {
                    window.location.href = '/owner';
                } else {
                    // Unknown role
                    if (errorText) errorText.textContent = 'Vai trò không hợp lệ.';
                    if (errorEl) errorEl.classList.remove('hidden');
                    AT.Core.Storage.clearSession();
                    if (btn) {
                        btn.disabled = false;
                        var btnText2 = btn.querySelector('.auth-btn-text');
                        if (btnText2) btnText2.textContent = 'Đăng nhập';
                    }
                }
            }, 400);
        }, 600);
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

/**
 * ═══════════════════════════════════════════════════
 * Audio Travelling — Auth Service
 * Login / Logout / Session Management
 * ═══════════════════════════════════════════════════
 */
(function () {
    'use strict';

    var AT = window.AudioTravelling = window.AudioTravelling || {};
    AT.Core = AT.Core || {};

    var Roles = AT.Config.Roles;
    var Storage = AT.Core.Storage;
    var UI = AT.Core.UI;

    /**
     * TODO: Replace mock accounts with real API authentication.
     * When backend is ready, doLogin should call POST /api/auth/login
     * and parse JWT token for role information.
     */
    var MOCK_ACCOUNTS = [
        { name: 'Admin User', email: 'admin@audiotravelling.com', password: btoa('admin123'), role: Roles.ADMIN },
        { name: 'POI Owner', email: 'owner@audiotravelling.com', password: btoa('owner123'), role: Roles.OWNER }
    ];

    function determineRole(email) {
        var account = MOCK_ACCOUNTS.find(function (a) {
            return a.email.toLowerCase() === email.toLowerCase();
        });
        return account ? account.role : null;
    }

    AT.Core.Auth = {
        /** Initialise auth — check existing session, bind login/logout forms */
        init: function () {
            this._bindLoginForm();
            this._bindLogout();
            this._bindPasswordToggle();
            this._restoreRememberedEmail();
            this._checkExistingSession();
        },

        /** Attempt login with email + password */
        login: function (email, password) {
            var account = MOCK_ACCOUNTS.find(function (a) {
                return a.email.toLowerCase() === email.toLowerCase();
            });

            if (!account) {
                return { success: false, error: 'Không tìm thấy tài khoản với email này.' };
            }

            if (account.password !== btoa(password)) {
                return { success: false, error: 'Sai mật khẩu.' };
            }

            // Check if role is allowed on portal
            if (AT.Config.ALLOWED_ROLES.indexOf(account.role) === -1) {
                return { success: false, error: 'Tài khoản của bạn không có quyền truy cập portal.' };
            }

            // Create session
            var session = {
                name: account.name,
                email: account.email,
                role: account.role,
                loggedIn: true,
                token: 'mock-jwt-' + Date.now() // TODO: Replace with real JWT
            };

            Storage.setSession(session);
            return { success: true, session: session };
        },

        /** Logout — clear session, show auth screen */
        logout: function () {
            Storage.clearSession();
            UI.showAuthScreen();
            var loginForm = document.getElementById('form-login');
            if (loginForm) loginForm.reset();
        },

        /** Get current session */
        getSession: function () {
            return Storage.getSession();
        },

        /** Get current role or null */
        getCurrentRole: function () {
            var session = Storage.getSession();
            return session && session.loggedIn ? session.role : null;
        },

        /** Check if currently authenticated */
        isAuthenticated: function () {
            var session = Storage.getSession();
            return !!(session && session.loggedIn);
        },

        // ── Private Methods ──

        _checkExistingSession: function () {
            var session = Storage.getSession();
            if (session && session.loggedIn) {
                var role = session.role;
                if (AT.Config.ALLOWED_ROLES.indexOf(role) === -1) {
                    this.logout();
                    return;
                }
                UI.showDashboardApp();
                UI.updateSidebarUser(session.name);
                UI.renderSidebar(role);

                var defaultView = AT.Config.getDefaultView(role);
                setTimeout(function () {
                    AT.Core.Router.navigate(defaultView);
                }, 50);
            }
        },

        _bindLoginForm: function () {
            var self = this;
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

                var result = self.login(email, password);

                if (!result.success) {
                    if (errorText) errorText.textContent = result.error;
                    if (errorEl) errorEl.classList.remove('hidden');
                    return;
                }

                // Remember email
                Storage.setRememberEmail(remember ? email : null);

                // Show loading state
                if (btn) {
                    btn.disabled = true;
                    var btnText = btn.querySelector('.auth-btn-text');
                    if (btnText) btnText.textContent = 'Đang đăng nhập...';
                }

                setTimeout(function () {
                    if (btn) {
                        btn.disabled = false;
                        var btnText = btn.querySelector('.auth-btn-text');
                        if (btnText) btnText.textContent = 'Đăng nhập';
                    }

                    var session = result.session;
                    UI.showDashboardApp();
                    UI.updateSidebarUser(session.name);
                    UI.renderSidebar(session.role);

                    var defaultView = AT.Config.getDefaultView(session.role);
                    AT.Core.Router.navigate(defaultView);

                    UI.showToast('Chào mừng trở lại, ' + session.name.split(' ')[0] + '!', 'success');
                }, 600);
            });
        },

        _bindLogout: function () {
            var self = this;
            var btn = document.getElementById('btn-logout');
            if (btn) {
                btn.addEventListener('click', function () {
                    self.logout();
                });
            }
        },

        _bindPasswordToggle: function () {
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
        },

        _restoreRememberedEmail: function () {
            var savedEmail = Storage.getRememberEmail();
            var emailInput = document.getElementById('login-email');
            var rememberCheck = document.getElementById('login-remember');
            if (savedEmail && emailInput) {
                emailInput.value = savedEmail;
                if (rememberCheck) rememberCheck.checked = true;
            }
        }
    };
})();

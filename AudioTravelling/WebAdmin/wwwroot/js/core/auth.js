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
     * API base URL
     * Vì API của bạn đang chạy ở port 5000
     */
    var API_BASE_URL = 'http://localhost:5264/api';

    /**
     * Dev offline mode
     * false = dùng API thật
     * true  = dùng mock account
     */
    window.AT_USE_MOCK = window.AT_USE_MOCK || false;

    /**
     * Mock accounts chỉ dùng khi window.AT_USE_MOCK = true
     */
    var MOCK_ACCOUNTS = [
        {
            userId: 1,
            name: 'Admin User',
            email: 'admin@audiotravelling.com',
            password: btoa('admin123'),
            role: Roles.ADMIN
        },
        {
            userId: 2,
            name: 'POI Owner',
            email: 'owner@audiotravelling.com',
            password: btoa('owner123'),
            role: Roles.OWNER
        }
    ];

    /**
     * Login bằng mock data
     */
    function loginWithMock(email, password) {
        var account = MOCK_ACCOUNTS.find(function (a) {
            return a.email.toLowerCase() === email.toLowerCase();
        });

        if (!account) {
            return {
                success: false,
                error: 'Không tìm thấy tài khoản với email này.'
            };
        }

        if (account.password !== btoa(password)) {
            return {
                success: false,
                error: 'Sai mật khẩu.'
            };
        }

        if (AT.Config.ALLOWED_ROLES.indexOf(account.role) === -1) {
            return {
                success: false,
                error: 'Tài khoản của bạn không có quyền truy cập portal.'
            };
        }

        var session = {
            userId: account.userId,
            name: account.name,
            email: account.email,
            role: account.role,
            loggedIn: true,
            token: 'mock-jwt-' + Date.now()
        };

        Storage.setSession(session);

        return {
            success: true,
            session: session
        };
    }

    /**
     * Login bằng API thật
     */
    async function loginWithApi(email, password) {
        try {
            var response = await fetch(API_BASE_URL + '/auth/login', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    email: email,
                    password: password
                })
            });

            if (response.status === 401) {
                return {
                    success: false,
                    error: 'Sai email hoặc mật khẩu.'
                };
            }

            if (response.status === 403) {
                return {
                    success: false,
                    error: 'Tài khoản của bạn không có quyền truy cập portal.'
                };
            }

            if (!response.ok) {
                return {
                    success: false,
                    error: 'Đăng nhập thất bại. Vui lòng thử lại.'
                };
            }

            var data = await response.json();

            /**
             * Backend nên trả về:
             * {
             *   userId: 1,
             *   fullName: "Admin User",
             *   email: "admin@gmail.com",
             *   roleName: "Admin",
             *   token: "..."
             * }
             */
            var role = data.roleName;

            if (!role) {
                return {
                    success: false,
                    error: 'API chưa trả về roleName.'
                };
            }

            if (AT.Config.ALLOWED_ROLES.indexOf(role) === -1) {
                return {
                    success: false,
                    error: 'Tài khoản của bạn không có quyền truy cập portal.'
                };
            }

            var session = {
                userId: data.userId,
                name: data.fullName || data.name || data.email,
                email: data.email,
                role: role,
                loggedIn: true,
                token: data.token
            };

            Storage.setSession(session);

            return {
                success: true,
                session: session
            };
        } catch (err) {
            console.error('Login API error:', err);

            return {
                success: false,
                error: 'Không thể kết nối đến API. Hãy kiểm tra API đã chạy ở port 5000 chưa.'
            };
        }
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
        login: async function (email, password) {
            if (!email || !password) {
                return {
                    success: false,
                    error: 'Vui lòng nhập email và mật khẩu.'
                };
            }

            if (window.AT_USE_MOCK) {
                return loginWithMock(email, password);
            }

            return await loginWithApi(email, password);
        },

        /** Logout — clear session, show auth screen */
        logout: function () {
            Storage.clearSession();
            UI.showAuthScreen();

            var loginForm = document.getElementById('form-login');
            if (loginForm) {
                loginForm.reset();
            }
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

                if (errorEl) {
                    errorEl.classList.add('hidden');
                }

                if (btn) {
                    btn.disabled = true;

                    var btnText = btn.querySelector('.auth-btn-text');
                    if (btnText) {
                        btnText.textContent = 'Đang đăng nhập...';
                    }
                }

                var result = await self.login(email, password);

                if (!result.success) {
                    if (btn) {
                        btn.disabled = false;

                        var failBtnText = btn.querySelector('.auth-btn-text');
                        if (failBtnText) {
                            failBtnText.textContent = 'Đăng nhập';
                        }
                    }

                    if (errorText) {
                        errorText.textContent = result.error;
                    }

                    if (errorEl) {
                        errorEl.classList.remove('hidden');
                    }

                    return;
                }

                Storage.setRememberEmail(remember ? email : null);

                if (btn) {
                    btn.disabled = false;

                    var successBtnText = btn.querySelector('.auth-btn-text');
                    if (successBtnText) {
                        successBtnText.textContent = 'Đăng nhập';
                    }
                }

                var session = result.session;

                UI.showDashboardApp();
                UI.updateSidebarUser(session.name);
                UI.renderSidebar(session.role);

                var defaultView = AT.Config.getDefaultView(session.role);
                AT.Core.Router.navigate(defaultView);

                var firstName = session.name ? session.name.split(' ')[0] : 'bạn';
                UI.showToast('Chào mừng trở lại, ' + firstName + '!', 'success');
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

                if (rememberCheck) {
                    rememberCheck.checked = true;
                }
            }
        }
    };
})();
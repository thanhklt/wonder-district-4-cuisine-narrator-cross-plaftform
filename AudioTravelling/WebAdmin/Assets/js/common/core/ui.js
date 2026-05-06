/**
 * ═══════════════════════════════════════════════════
 * Audio Travelling — UI Utilities
 * Toast, Modal, Loading, Sidebar, Theme Toggle
 * ═══════════════════════════════════════════════════
 */
(function () {
    'use strict';

    var AT = window.AudioTravelling = window.AudioTravelling || {};
    AT.Core = AT.Core || {};

    AT.Core.UI = {
        // ── Toast Notifications ──
        showToast: function (message, type) {
            type = type || 'success';
            if (typeof Toastify === 'undefined') {
                alert(message);
                return;
            }
            var bgs = {
                success: 'linear-gradient(135deg, #059669, #22c55e)',
                error: 'linear-gradient(135deg, #dc2626, #ef4444)',
                warning: 'linear-gradient(135deg, #d97706, #fbbf24)',
                info: 'linear-gradient(135deg, #2563eb, #60a5fa)'
            };
            Toastify({
                text: message,
                duration: 3000,
                gravity: 'bottom',
                position: 'right',
                style: {
                    background: bgs[type] || bgs.success,
                    borderRadius: '12px',
                    fontSize: '13px',
                    padding: '12px 20px'
                }
            }).showToast();
        },

        // ── Modal ──
        showModal: function (modalId) {
            var modal = document.getElementById(modalId);
            if (!modal) return;
            modal.classList.remove('hidden');
            modal.style.pointerEvents = 'auto';
            setTimeout(function () {
                modal.style.opacity = '1';
                var card = modal.querySelector('.glass-card, .modal-card');
                if (card) card.style.transform = 'translateY(0)';
            }, 10);
        },

        hideModal: function (modalId) {
            var modal = document.getElementById(modalId);
            if (!modal) return;
            modal.style.opacity = '0';
            var card = modal.querySelector('.glass-card, .modal-card');
            if (card) card.style.transform = 'translateY(20px)';
            modal.style.pointerEvents = 'none';
            setTimeout(function () {
                modal.classList.add('hidden');
            }, 300);
        },

        // ── Confirm Dialog ──
        confirm: function (message, onConfirm, onCancel) {
            if (window.confirm(message)) {
                if (onConfirm) onConfirm();
            } else {
                if (onCancel) onCancel();
            }
        },

        // ── Loading State ──
        setLoading: function (elementId, isLoading) {
            var el = document.getElementById(elementId);
            if (!el) return;
            if (isLoading) {
                el.classList.add('is-loading');
                el.disabled = true;
            } else {
                el.classList.remove('is-loading');
                el.disabled = false;
            }
        },

        // ── Sidebar ──
        renderSidebar: function (role) {
            var navContainer = document.getElementById('sidebar-nav');
            if (!navContainer) return;

            var routes = AT.Config.getRoutesForRole(role);
            var html = '';

            routes.forEach(function (route) {
                html += '<a class="nav-link" data-target="' + route.viewId + '" href="#">';
                html += '<i class="' + route.navIcon + '"></i> ' + route.navLabel;
                html += '</a>';
            });

            // Divider + Settings placeholder (could be added later)
            // html += '<div class="nav-divider"></div>';

            navContainer.innerHTML = html;

            // Bind click events
            navContainer.querySelectorAll('.nav-link[data-target]').forEach(function (link) {
                link.addEventListener('click', function (e) {
                    e.preventDefault();
                    AT.Core.Router.navigate(link.getAttribute('data-target'));
                });
            });
        },

        updateSidebarActive: function (viewId) {
            var navContainer = document.getElementById('sidebar-nav');
            if (!navContainer) return;
            navContainer.querySelectorAll('.nav-link').forEach(function (link) {
                link.classList.remove('active');
                if (link.getAttribute('data-target') === viewId) {
                    link.classList.add('active');
                }
            });
        },

        updateSidebarUser: function (name) {
            var nameEl = document.getElementById('sidebar-name');
            if (nameEl) nameEl.textContent = name || 'User';
        },

        // ── Theme Toggle ──
        initTheme: function () {
            var theme = AT.Core.Storage.getTheme();
            var themeBtn = document.getElementById('theme-toggle');

            if (theme === 'dark') {
                document.documentElement.removeAttribute('data-theme');
                if (themeBtn) themeBtn.querySelector('i').className = 'fa-solid fa-moon';
            } else {
                document.documentElement.setAttribute('data-theme', 'light');
                if (themeBtn) themeBtn.querySelector('i').className = 'fa-solid fa-sun';
            }

            if (themeBtn) {
                themeBtn.addEventListener('click', function () {
                    var isDark = !document.documentElement.hasAttribute('data-theme');
                    if (isDark) {
                        document.documentElement.setAttribute('data-theme', 'light');
                        AT.Core.Storage.setTheme('light');
                        themeBtn.querySelector('i').className = 'fa-solid fa-sun';
                    } else {
                        document.documentElement.removeAttribute('data-theme');
                        AT.Core.Storage.setTheme('dark');
                        themeBtn.querySelector('i').className = 'fa-solid fa-moon';
                    }
                });
            }
        },

        // ── Header ──
        setHeaderTitle: function (title) {
            var el = document.getElementById('header-title');
            if (el) el.textContent = title || 'Portal';
        },

        // ── Show / Hide Auth vs Dashboard ──
        showAuthScreen: function () {
            var auth = document.getElementById('auth-screen');
            var app = document.getElementById('dashboard-app');
            if (auth) auth.style.display = '';
            if (app) app.style.display = 'none';
        },

        showDashboardApp: function () {
            var auth = document.getElementById('auth-screen');
            var app = document.getElementById('dashboard-app');
            if (auth) auth.style.display = 'none';
            if (app) app.style.display = 'block';
        }
    };
})();

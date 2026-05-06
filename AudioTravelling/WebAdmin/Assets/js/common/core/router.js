/**
 * ═══════════════════════════════════════════════════
 * Audio Travelling — SPA Router
 * Handles view switching with role guard
 * ═══════════════════════════════════════════════════
 */
(function () {
    'use strict';

    var AT = window.AudioTravelling = window.AudioTravelling || {};
    AT.Core = AT.Core || {};

    var Routes = AT.Config.Routes;

    AT.Core.Router = {
        /** Navigate to a view by viewId */
        navigate: function (viewId) {
            var role = AT.Core.Auth.getCurrentRole();

            if (!role) {
                AT.Core.UI.showAuthScreen();
                return;
            }

            // Role guard check
            if (!AT.Core.RoleGuard.canAccess(viewId, role)) {
                AT.Core.RoleGuard.showForbiddenPage();
                return;
            }

            var route = Routes[viewId];
            if (!route) {
                console.warn('Router: Unknown view:', viewId);
                return;
            }

            // 1. Hide all views
            document.querySelectorAll('.view-section').forEach(function (el) {
                el.classList.remove('active');
            });

            // 2. Show target view
            var targetEl = document.getElementById(route.viewId);
            if (targetEl) {
                targetEl.classList.add('active');
            }

            // 3. Update header title
            AT.Core.UI.setHeaderTitle(route.title);

            // 4. Update sidebar active state
            AT.Core.UI.updateSidebarActive(viewId);

            // 5. Call module init function if defined
            if (route.initFn && AT.Modules && typeof AT.Modules[route.initFn] === 'function') {
                try {
                    AT.Modules[route.initFn]();
                } catch (e) {
                    console.error('Router: Error initialising module ' + route.initFn, e);
                }
            }
        }
    };
})();

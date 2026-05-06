/**
 * ═══════════════════════════════════════════════════
 * Audio Travelling — Role Guard
 * Enforces role-based access to views
 * ═══════════════════════════════════════════════════
 */
(function () {
    'use strict';

    var AT = window.AudioTravelling = window.AudioTravelling || {};
    AT.Core = AT.Core || {};

    var Routes = AT.Config.Routes;
    var Roles = AT.Config.Roles;

    AT.Core.RoleGuard = {
        /**
         * Check if the current role can access a given view.
         * Returns true if allowed.
         */
        canAccess: function (viewId, role) {
            var route = Routes[viewId];

            // Unknown view
            if (!route) return false;

            // Shared views (e.g. 403)
            if (route.role === null) return true;

            // Must match the view's required role
            return route.role === role;
        },

        /**
         * Redirect to the default dashboard for the current role.
         */
        redirectToDefaultDashboard: function (role) {
            var defaultView = AT.Config.getDefaultView(role);
            if (defaultView) {
                AT.Core.Router.navigate(defaultView);
            }
        },

        /**
         * Show the 403 Forbidden page.
         */
        showForbiddenPage: function () {
            // Hide all views first
            document.querySelectorAll('.view-section').forEach(function (el) {
                el.classList.remove('active');
            });

            var forbidden = document.getElementById('view-403');
            if (forbidden) {
                forbidden.classList.add('active');
            }

            AT.Core.UI.setHeaderTitle('Truy cập bị từ chối');
            AT.Core.UI.updateSidebarActive('');
        }
    };
})();

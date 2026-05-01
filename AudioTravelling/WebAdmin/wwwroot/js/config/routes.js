/**
 * ═══════════════════════════════════════════════════
 * Audio Travelling — Route / View Configuration
 * ═══════════════════════════════════════════════════
 */
(function () {
    'use strict';

    var AT = window.AudioTravelling = window.AudioTravelling || {};
    AT.Config = AT.Config || {};

    var Roles = AT.Config.Roles;

    /**
     * Each route entry:
     *   viewId   – id of the .view-section div
     *   role     – which role can access this view
     *   title    – header title when view is active
     *   initFn   – name of the module init function on AT.Modules
     *   navIcon  – FontAwesome icon class
     *   navLabel – sidebar label
     */
    AT.Config.Routes = Object.freeze({
        // ── Admin Routes ──
        'view-dashboard-admin': {
            viewId: 'view-dashboard-admin',
            role: Roles.ADMIN,
            title: 'Tổng quan',
            initFn: 'initAdminDashboard',
            navIcon: 'fa-solid fa-chart-pie',
            navLabel: 'Tổng quan',
            navKey: 'nav-dashboard'
        },
        'view-heatmap': {
            viewId: 'view-heatmap',
            role: Roles.ADMIN,
            title: 'Heatmap khu vực',
            initFn: 'initHeatmap',
            navIcon: 'fa-solid fa-map-location-dot',
            navLabel: 'Heatmap',
            navKey: 'nav-heatmap'
        },
        'view-qr-management': {
            viewId: 'view-qr-management',
            role: Roles.ADMIN,
            title: 'Quản lý QR',
            initFn: 'initQRManagement',
            navIcon: 'fa-solid fa-qrcode',
            navLabel: 'Quản lý QR',
            navKey: 'nav-qr'
        },
        'view-poi-approval': {
            viewId: 'view-poi-approval',
            role: Roles.ADMIN,
            title: 'Duyệt POI',
            initFn: 'initPOIApproval',
            navIcon: 'fa-solid fa-clipboard-check',
            navLabel: 'Duyệt POI',
            navKey: 'nav-poi-approval'
        },


        // ── Owner Routes ──
        'view-dashboard-owner': {
            viewId: 'view-dashboard-owner',
            role: Roles.OWNER,
            title: 'Tổng quan',
            initFn: 'initOwnerDashboard',
            navIcon: 'fa-solid fa-chart-pie',
            navLabel: 'Tổng quan',
            navKey: 'nav-dashboard'
        },
        'view-my-pois': {
            viewId: 'view-my-pois',
            role: Roles.OWNER,
            title: 'POI của tôi',
            initFn: 'initMyPOIs',
            navIcon: 'fa-solid fa-map-pin',
            navLabel: 'POI của tôi',
            navKey: 'nav-my-pois'
        },
        'view-create-poi': {
            viewId: 'view-create-poi',
            role: Roles.OWNER,
            title: 'Tạo POI',
            initFn: 'initCreatePOI',
            navIcon: 'fa-solid fa-plus-circle',
            navLabel: 'Tạo POI',
            navKey: 'nav-create-poi'
        },
        'view-poi-status': {
            viewId: 'view-poi-status',
            role: Roles.OWNER,
            title: 'Trạng thái duyệt',
            initFn: 'initPOIStatus',
            navIcon: 'fa-solid fa-list-check',
            navLabel: 'Trạng thái duyệt',
            navKey: 'nav-poi-status'
        },

        // ── Shared ──
        'view-403': {
            viewId: 'view-403',
            role: null, // accessible by any authenticated user
            title: 'Truy cập bị từ chối',
            initFn: null,
            navIcon: null,
            navLabel: null,
            navKey: null
        }
    });

    /** Get the default dashboard view for a role */
    AT.Config.getDefaultView = function (role) {
        if (role === Roles.ADMIN) return 'view-dashboard-admin';
        if (role === Roles.OWNER) return 'view-dashboard-owner';
        return null;
    };

    /** Get all routes for a given role (for sidebar rendering) */
    AT.Config.getRoutesForRole = function (role) {
        var routes = AT.Config.Routes;
        var result = [];
        for (var key in routes) {
            if (routes.hasOwnProperty(key) && routes[key].role === role && routes[key].navLabel) {
                result.push(routes[key]);
            }
        }
        return result;
    };
})();

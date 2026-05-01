/**
 * ═══════════════════════════════════════════════════
 * Audio Travelling — Access Stats Service
 * TODO: Replace mock calls with real API endpoints.
 * ═══════════════════════════════════════════════════
 */
(function () {
    'use strict';

    var AT = window.AudioTravelling = window.AudioTravelling || {};
    AT.Services = AT.Services || {};

    var Mocks = AT.Mocks;

    AT.Services.AccessStats = {
        /** Get active user count */
        getActiveUserCount: function () {
            // TODO: return AT.Core.ApiClient.get('/admin/dashboard/active-users');
            return Promise.resolve(Mocks.DashboardStats.activeUsers);
        },



        /** Get dashboard stats */
        getDashboardStats: function () {
            // TODO: return AT.Core.ApiClient.get('/admin/dashboard/stats');
            return Promise.resolve(Object.assign({}, Mocks.DashboardStats));
        }
    };
})();

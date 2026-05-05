/**
 * Audio Travelling — Admin Dashboard Module
 */
(function () {
    'use strict';
    var AT = window.AudioTravelling = window.AudioTravelling || {};
    AT.Modules = AT.Modules || {};

    AT.Modules.initAdminDashboard = function () {
        AT.Services.AccessStats.getDashboardStats().then(function (stats) {
            console.log('[AdminDashboard] Stats:', stats);
            var el = document.getElementById('stat-active-users');
            if (el) el.textContent = stats.activeUsers || 0;
            var elScans = document.getElementById('stat-total-scans');
            if (elScans) elScans.textContent = stats.totalScansToday || 0;
            var elPois = document.getElementById('stat-total-pois');
            if (elPois) elPois.textContent = stats.totalPOIs || 0;
            var elPending = document.getElementById('stat-pending-pois');
            if (elPending) elPending.textContent = stats.pendingPOIs || 0;
        }).catch(function (err) {
            console.error('[AdminDashboard] Error loading stats:', err);
        });
    };
})();

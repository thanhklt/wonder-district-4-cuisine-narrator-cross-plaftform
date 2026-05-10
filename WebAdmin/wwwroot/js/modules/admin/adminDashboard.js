/**
 * Audio Travelling — Admin Dashboard Module
 */
(function () {
    'use strict';
    var AT = window.AudioTravelling = window.AudioTravelling || {};
    AT.Modules = AT.Modules || {};

    var _heatmapInitialized = false;

    AT.Modules.initAdminDashboard = function () {
        AT.Services.AccessStats.getDashboardStats().then(function (stats) {
            console.log('[AdminDashboard] Stats:', stats);
            var el = document.getElementById('stat-active-users');
            if (el) el.textContent = stats.activeUsers || 0;
            var elScans = document.getElementById('stat-total-scans');
            if (elScans) elScans.textContent = stats.rejectedPOIs || 0;
            var elPois = document.getElementById('stat-total-pois');
            if (elPois) elPois.textContent = stats.approvedPOIs || 0;
            var elPending = document.getElementById('stat-pending-pois');
            if (elPending) elPending.textContent = stats.pendingPOIs || 0;
        }).catch(function (err) {
            console.error('[AdminDashboard] Error loading stats:', err);
        });

        // Initialise heatmap only once and only if container exists
        if (!_heatmapInitialized) {
            var container = document.getElementById('heatmap-container');
            if (container && AT.Modules.initHeatmap) {
                _heatmapInitialized = true;
                AT.Modules.initHeatmap();
            } else if (!container) {
                // Show empty state if container is missing
                var emptyEl = document.getElementById('heatmap-empty-state');
                if (emptyEl) emptyEl.classList.remove('hidden');
            }
        }
    };
})();

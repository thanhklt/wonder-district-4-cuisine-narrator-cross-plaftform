/**
 * Audio Travelling — Admin Dashboard Module
 */
(function () {
    'use strict';
    var AT = window.AudioTravelling = window.AudioTravelling || {};
    AT.Modules = AT.Modules || {};
    var Fmt = AT.Utils.Formatters;

    AT.Modules.initAdminDashboard = function () {
        AT.Services.AccessStats.getDashboardStats().then(function (stats) {
            var el = document.getElementById('stat-active-users');
            if (el) el.textContent = stats.activeUsers;
            var elScans = document.getElementById('stat-total-scans');
            if (elScans) elScans.textContent = stats.totalScansToday;
            var elPois = document.getElementById('stat-total-pois');
            if (elPois) elPois.textContent = stats.totalPOIs;
            var elPending = document.getElementById('stat-pending-pois');
            if (elPending) elPending.textContent = stats.pendingPOIs;
        });


    };
})();

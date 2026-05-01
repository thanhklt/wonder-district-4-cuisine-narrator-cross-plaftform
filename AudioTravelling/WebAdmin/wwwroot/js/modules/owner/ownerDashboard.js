/**
 * Audio Travelling — Owner Dashboard Module
 */
(function () {
    'use strict';
    var AT = window.AudioTravelling = window.AudioTravelling || {};
    AT.Modules = AT.Modules || {};
    var Fmt = AT.Utils.Formatters;

    AT.Modules.initOwnerDashboard = function () {
        var session = AT.Core.Auth.getSession();
        if (!session) return;

        AT.Services.POI.getByOwner(session.email).then(function (pois) {
            var total = pois.length;
            var approved = pois.filter(function (p) { return p.status === 'approved'; }).length;
            var pending = pois.filter(function (p) { return p.status === 'pending'; }).length;
            var rejected = pois.filter(function (p) { return p.status === 'rejected'; }).length;

            var el = function (id) { return document.getElementById(id); };
            if (el('stat-owner-total')) el('stat-owner-total').textContent = total;
            if (el('stat-owner-approved')) el('stat-owner-approved').textContent = approved;
            if (el('stat-owner-pending')) el('stat-owner-pending').textContent = pending;
            if (el('stat-owner-rejected')) el('stat-owner-rejected').textContent = rejected;

            // Render recent POI list
            var tbody = document.getElementById('owner-pois-preview');
            if (!tbody) return;
            if (pois.length === 0) {
                tbody.innerHTML = '<tr><td colspan="3" style="text-align:center;padding:20px;color:var(--text-dim);">Bạn chưa có POI nào</td></tr>';
                return;
            }
            tbody.innerHTML = pois.slice(0, 5).map(function (p) {
                return '<tr style="border-bottom:1px solid var(--border);">' +
                    '<td style="padding:10px;font-size:13px;font-weight:500;">' + p.name + '</td>' +
                    '<td style="padding:10px;"><span class="status-badge status-' + p.status + '">' + p.status + '</span></td>' +
                    '<td style="padding:10px;font-size:13px;">' + Fmt.date(p.updatedAt) + '</td></tr>';
            }).join('');
        });
    };
})();

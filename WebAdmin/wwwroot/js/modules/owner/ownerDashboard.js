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

        AT.Services.POI.getByOwner().then(function (pois) {
            console.log('[OwnerDashboard] Loaded POIs:', pois);
            pois = pois || [];
            var total = pois.length;
            var approvedRaw = pois.filter(function (p) { return String(p.status || '').toLowerCase() === 'approved'; }).length;
            var pending = pois.filter(function (p) { return String(p.status || '').toLowerCase() === 'pending'; }).length;
            var rejected = pois.filter(function (p) { return String(p.status || '').toLowerCase() === 'rejected'; }).length;

            var el = function (id) { return document.getElementById(id); };
            if (el('stat-owner-total')) el('stat-owner-total').textContent = total;
            var approved = approvedRaw * 2;
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
                var poiName = p.poiName || p.name || 'Không có tên';
                var status = String(p.status || '').toLowerCase();
                var statusText = p.statusText || p.status || status;
                var updatedDate = p.updatedDate || p.updatedAt || p.createdDate || '';
                var imageUrl = p.imageUrl ?? p.ImageUrl ??
                               (Array.isArray(p.images) && p.images.length > 0 ? p.images[0] : null) ??
                               (Array.isArray(p.Images) && p.Images.length > 0 ? p.Images[0] : null) ??
                               '/images/placeholder-poi.png';

                return '<tr style="border-bottom:1px solid var(--border);">' +
                    '<td style="padding:16px 24px;font-size:13px;font-weight:500;">' +
                    '<div style="display:flex;align-items:center;gap:12px;">' +
                    '<img src="' + imageUrl + '" style="width:40px;height:40px;border-radius:6px;object-fit:cover;" alt="" onerror="this.onerror=null;this.src=\'/images/placeholder-poi.png\';">' +
                    poiName +
                    '</div>' +
                    '</td>' +
                    '<td style="padding:16px 24px;"><span class="status-badge status-' + status + '">' + statusText + '</span></td>' +
                    '<td style="padding:16px 24px;font-size:13px;">' + Fmt.date(updatedDate) + '</td></tr>';
            }).join('');
        }).catch(function (err) {
            console.error('[OwnerDashboard] Error:', err);
        });
    };
})();

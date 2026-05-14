/**
 * Audio Travelling — POI Status Module (Owner)
 */
(function () {
    'use strict';
    var AT = window.AudioTravelling = window.AudioTravelling || {};
    AT.Modules = AT.Modules || {};
    var Fmt = AT.Utils.Formatters;

    AT.Modules.initPOIStatus = function () {
        var session = AT.Core.Auth.getSession();
        if (!session) return;

        AT.Services.POI.getByOwner().then(function (pois) {
            console.log('[POIStatus] Loaded POIs:', pois);
            var container = document.getElementById('poi-status-list');
            if (!container) return;
            if (!pois || pois.length === 0) {
                container.innerHTML = '<p style="text-align:center;padding:20px;color:var(--text-dim);">Bạn chưa có POI nào</p>';
                return;
            }

            container.innerHTML = pois.map(function (p) {
                var poiName = p.poiName || p.name || 'Không có tên';
                var status = String(p.status || '').toLowerCase();
                var statusText = p.statusText || p.status || status;
                var createdDate = p.createdDate || p.createdAt || '';
                var updatedDate = p.updatedDate || p.updatedAt || '';
                var statusIcon = { pending: 'fa-clock', approved: 'fa-circle-check', rejected: 'fa-circle-xmark' };
                var icon = statusIcon[status] || 'fa-question';
                var rejectionHtml = '';
                if (status === 'rejected' && p.rejectionReason) {
                    rejectionHtml = '<div style="margin-top:8px;padding:10px;background:rgba(239,68,68,0.08);border-radius:8px;border-left:3px solid #ef4444;">' +
                        '<p style="font-size:12px;color:#ef4444;font-weight:600;margin-bottom:4px;"><i class="fa-solid fa-comment-dots"></i> Lý do từ chối:</p>' +
                        '<p style="font-size:13px;color:var(--text-primary);">' + p.rejectionReason + '</p></div>';
                }

                var rawStatusUrl = p.imageUrl || p.ImageUrl ||
                    (Array.isArray(p.images) && p.images.length > 0 ? (p.images[0].imageUrl || p.images[0]) : null) ||
                    null;
                var statusImageUrl = AT.Core.ApiClient.resolveImageUrl(rawStatusUrl);

                return '<div class="glass-card" style="padding:20px;margin-bottom:16px;">' +
                    '<div style="display:flex;align-items:center;justify-content:space-between;">' +
                    '<div style="display:flex;align-items:center;gap:16px;">' +
                    '<img data-api-src="' + rawStatusUrl + '" src="/images/placeholder-poi.png" style="width:48px;height:48px;border-radius:8px;object-fit:cover;" alt="">' +
                    '<div><h4 style="font-size:16px;font-weight:600;margin-bottom:4px;">' + poiName + '</h4>' +
                    '</div></div>' +
                    '<span class="status-badge status-' + status + '">' + statusText + '</span></div>' +
                    '<div style="margin-top:12px;display:flex;gap:16px;font-size:12px;color:var(--text-dim);">' +
                    '<span><i class="fa-solid fa-calendar"></i> Tạo: ' + Fmt.date(createdDate) + '</span>' +
                    '<span><i class="fa-solid fa-pen"></i> Cập nhật: ' + Fmt.date(updatedDate) + '</span></div>' +
                    rejectionHtml + '</div>';
            }).join('');
            AT.Core.ApiClient.loadAllImages(container);
        }).catch(function (err) {
            console.error('[POIStatus] Error:', err);
        });
    };
})();

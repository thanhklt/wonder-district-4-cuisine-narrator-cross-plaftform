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

        AT.Services.POI.getByOwner(session.email).then(function (pois) {
            var container = document.getElementById('poi-status-list');
            if (!container) return;
            if (pois.length === 0) {
                container.innerHTML = '<p style="text-align:center;padding:20px;color:var(--text-dim);">Bạn chưa có POI nào</p>';
                return;
            }

            container.innerHTML = pois.map(function (p) {
                var statusIcon = { draft: 'fa-file-pen', pending: 'fa-clock', approved: 'fa-circle-check', rejected: 'fa-circle-xmark' };
                var icon = statusIcon[p.status] || 'fa-question';
                var rejectionHtml = '';
                if (p.status === 'rejected' && p.rejectionReason) {
                    rejectionHtml = '<div style="margin-top:8px;padding:10px;background:rgba(239,68,68,0.08);border-radius:8px;border-left:3px solid #ef4444;">' +
                        '<p style="font-size:12px;color:#ef4444;font-weight:600;margin-bottom:4px;"><i class="fa-solid fa-comment-dots"></i> Lý do từ chối:</p>' +
                        '<p style="font-size:13px;color:var(--text-primary);">' + p.rejectionReason + '</p></div>';
                }

                return '<div class="glass-card" style="padding:20px;margin-bottom:16px;">' +
                    '<div style="display:flex;align-items:center;justify-content:space-between;">' +
                    '<div style="display:flex;align-items:center;gap:12px;">' +
                    '<i class="fa-solid ' + icon + '" style="font-size:20px;color:var(--text-muted);"></i>' +
                    '<div><h4 style="font-size:15px;font-weight:600;">' + p.name + '</h4>' +
                    '</div></div>' +
                    '<span class="status-badge status-' + p.status + '">' + p.status + '</span></div>' +
                    '<div style="margin-top:12px;display:flex;gap:16px;font-size:12px;color:var(--text-dim);">' +
                    '<span><i class="fa-solid fa-calendar"></i> Tạo: ' + Fmt.date(p.createdAt) + '</span>' +
                    '<span><i class="fa-solid fa-pen"></i> Cập nhật: ' + Fmt.date(p.updatedAt) + '</span></div>' +
                    rejectionHtml + '</div>';
            }).join('');
        });
    };
})();

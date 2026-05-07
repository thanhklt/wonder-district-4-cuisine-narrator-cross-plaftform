/**
 * Audio Travelling — My POIs Module (Owner)
 */
(function () {
    'use strict';
    var AT = window.AudioTravelling = window.AudioTravelling || {};
    AT.Modules = AT.Modules || {};
    var Fmt = AT.Utils.Formatters;
    var UI = AT.Core.UI;

    AT.Modules.initMyPOIs = function () {
        renderMyPOIs();
    };

    function renderMyPOIs() {
        var session = AT.Core.Auth.getSession();
        if (!session) return;

        AT.Services.POI.getByOwner().then(function (pois) {
            console.log('[MyPOIs] Loaded POIs:', pois);
            var tbody = document.getElementById('my-pois-body');
            if (!tbody) return;
            if (!pois || pois.length === 0) {
                tbody.innerHTML = '<tr><td colspan="5" style="text-align:center;padding:20px;color:var(--text-dim);">Bạn chưa có POI nào. Hãy tạo POI mới!</td></tr>';
                return;
            }
            tbody.innerHTML = pois.map(function (p) {
                var poiId = p.poiId || p.id || '';
                var poiName = p.poiName || p.name || 'Không có tên';
                var status = String(p.status || '').toLowerCase();
                var statusText = p.statusText || p.status || status;
                var updatedDate = p.updatedDate || p.updatedAt || p.createdDate || '';
                var canSubmit = status === 'rejected' || status === 'pending';
                var canDelete = false; // Owner cannot delete POI

                var imageUrl = p.imageUrl ?? p.ImageUrl ??
                               (Array.isArray(p.images) && p.images.length > 0 ? p.images[0] : null) ??
                               (Array.isArray(p.Images) && p.Images.length > 0 ? p.Images[0] : null) ??
                               '/images/placeholder-poi.png';

                return '<tr style="border-bottom:1px solid var(--border);">' +
                    '<td style="padding:16px 24px;font-weight:600;font-size:13px;">' + poiId + '</td>' +
                    '<td style="padding:16px 24px;font-size:13px;font-weight:500;">' +
                        '<div style="display:flex;align-items:center;gap:12px;">' +
                            '<img src="' + imageUrl + '" style="width:40px;height:40px;border-radius:6px;object-fit:cover;" alt="" onerror="this.onerror=null;this.src=\'/images/placeholder-poi.png\';">' +
                            poiName +
                        '</div>' +
                    '</td>' +
                    '<td style="padding:16px 24px;"><span class="status-badge status-' + status + '">' + statusText + '</span></td>' +
                    '<td style="padding:16px 24px;font-size:13px;">' + Fmt.date(updatedDate) + '</td>' +
                    '<td style="padding:16px 24px;text-align:center;">' +
                    '<button class="btn-ghost btn-edit-poi" data-poi-id="' + poiId + '" style="font-size:11px;padding:5px 10px;margin-right:4px;" title="Sửa">' +
                    '<i class="fa-solid fa-pen"></i></button>' +
                    (canSubmit ? '<button class="btn-primary btn-submit-poi" data-poi-id="' + poiId + '" style="font-size:11px;padding:5px 10px;margin-right:4px;" title="Gửi duyệt">' +
                    '<i class="fa-solid fa-paper-plane"></i></button>' : '') +
                    '</td></tr>';
            }).join('');

            // Bind edit
            tbody.querySelectorAll('.btn-edit-poi').forEach(function (btn) {
                btn.addEventListener('click', function () {
                    var poiId = btn.getAttribute('data-poi-id');
                    window.location.href = '/owner/pois/edit/' + poiId;
                });
            });

            // Bind submit
            tbody.querySelectorAll('.btn-submit-poi').forEach(function (btn) {
                btn.addEventListener('click', function () {
                    var poiId = btn.getAttribute('data-poi-id');
                    AT.Services.POI.submitForApproval(poiId).then(function () {
                        UI.showToast('Đã gửi yêu cầu duyệt POI ' + poiId, 'success');
                        renderMyPOIs();
                    });
                });
            });

        }).catch(function (err) {
            console.error('[MyPOIs] Error:', err);
        });
    }
})();

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

        AT.Services.POI.getByOwner(session.email).then(function (pois) {
            var tbody = document.getElementById('my-pois-body');
            if (!tbody) return;
            if (pois.length === 0) {
                tbody.innerHTML = '<tr><td colspan="6" style="text-align:center;padding:20px;color:var(--text-dim);">Bạn chưa có POI nào. Hãy tạo POI mới!</td></tr>';
                return;
            }
            tbody.innerHTML = pois.map(function (p) {
                var canSubmit = p.status === 'draft' || p.status === 'rejected';
                var canDelete = true;
                return '<tr style="border-bottom:1px solid var(--border);">' +
                    '<td style="padding:10px;font-weight:600;font-size:13px;">' + p.id + '</td>' +
                    '<td style="padding:10px;font-size:13px;font-weight:500;">' + p.name + '</td>' +
                    '<td style="padding:10px;"><span class="status-badge status-' + p.status + '">' + p.status + '</span></td>' +
                    '<td style="padding:10px;font-size:13px;">' + Fmt.date(p.updatedAt) + '</td>' +
                    '<td style="padding:10px;text-align:center;">' +
                    '<button class="btn-ghost btn-edit-poi" data-poi-id="' + p.id + '" style="font-size:11px;padding:5px 10px;margin-right:4px;" title="Sửa">' +
                    '<i class="fa-solid fa-pen"></i></button>' +
                    (canSubmit ? '<button class="btn-primary btn-submit-poi" data-poi-id="' + p.id + '" style="font-size:11px;padding:5px 10px;margin-right:4px;" title="Gửi duyệt">' +
                    '<i class="fa-solid fa-paper-plane"></i></button>' : '') +
                    (canDelete ? '<button class="btn-danger-ghost btn-delete-poi" data-poi-id="' + p.id + '" style="font-size:11px;padding:5px 10px;" title="Xóa">' +
                    '<i class="fa-solid fa-trash"></i></button>' : '') +
                    '</td></tr>';
            }).join('');

            // Bind edit
            tbody.querySelectorAll('.btn-edit-poi').forEach(function (btn) {
                btn.addEventListener('click', function () {
                    var poiId = btn.getAttribute('data-poi-id');
                    // Navigate to create-poi view with edit mode
                    AT.Modules._editPoiId = poiId;
                    AT.Core.Router.navigate('view-create-poi');
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

            // Bind delete
            tbody.querySelectorAll('.btn-delete-poi').forEach(function (btn) {
                btn.addEventListener('click', function () {
                    var poiId = btn.getAttribute('data-poi-id');
                    UI.confirm('Bạn có chắc muốn xóa POI ' + poiId + '?', function () {
                        AT.Services.POI.delete(poiId).then(function () {
                            UI.showToast('Đã xóa POI ' + poiId, 'success');
                            renderMyPOIs();
                        });
                    });
                });
            });
        });
    }
})();

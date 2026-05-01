                        /**
 * Audio Travelling — QR Management Module (Admin)
 */
(function () {
    'use strict';
    var AT = window.AudioTravelling = window.AudioTravelling || {};
    AT.Modules = AT.Modules || {};
    var Fmt = AT.Utils.Formatters;
    var UI = AT.Core.UI;

    AT.Modules.initQRManagement = function () {
        renderQRList();
        bindEvents();
    };

    function renderQRList() {
        AT.Services.QR.getAll().then(function (qrs) {
            var tbody = document.getElementById('qr-table-body');
            if (!tbody) return;
            if (qrs.length === 0) {
                tbody.innerHTML = '<tr><td colspan="6" style="text-align:center;padding:20px;color:var(--text-dim);">Chưa có mã QR nào</td></tr>';
                return;
            }
            tbody.innerHTML = qrs.map(function (qr) {
                var statusClass = qr.status === 'active' ? 'status-approved' : 'status-rejected';
                var statusText = qr.status === 'active' ? 'Hoạt động' : 'Đã tắt';
                var toggleText = qr.status === 'active' ? 'Tắt' : 'Bật';
                var toggleIcon = qr.status === 'active' ? 'fa-toggle-on' : 'fa-toggle-off';
                return '<tr style="border-bottom:1px solid var(--border);">' +
                    '<td style="padding:10px;font-weight:600;font-size:13px;">' + qr.id + '</td>' +
                    '<td style="padding:10px;"><span class="status-badge ' + statusClass + '">' + statusText + '</span></td>' +
                    '<td style="padding:10px;font-size:13px;text-align:center;">' + qr.scanCount + '</td>' +
                    '<td style="padding:10px;text-align:center;">' +
                    '<button class="btn-ghost btn-toggle-qr" data-qr-id="' + qr.id + '" style="font-size:12px;padding:6px 12px;">' +
                    '<i class="fa-solid ' + toggleIcon + '"></i> ' + toggleText +
                    '</button></td></tr>';
            }).join('');

            // Bind toggle buttons
            tbody.querySelectorAll('.btn-toggle-qr').forEach(function (btn) {
                btn.addEventListener('click', function () {
                    var qrId = btn.getAttribute('data-qr-id');
                    AT.Services.QR.toggleStatus(qrId).then(function (qr) {
                        UI.showToast('QR ' + qr.id + ' đã ' + (qr.status === 'active' ? 'bật' : 'tắt'), 'success');
                        renderQRList();
                    });
                });
            });
        });
    }

    var _eventsBound = false;
    function bindEvents() {
        if (_eventsBound) return;
        _eventsBound = true;

        var btnCreate = document.getElementById('btn-create-qr');
        if (btnCreate) {
            btnCreate.addEventListener('click', function () {
                UI.showModal('modal-create-qr');
            });
        }

        var btnSaveQR = document.getElementById('btn-save-new-qr');
        if (btnSaveQR) {
            btnSaveQR.addEventListener('click', function () {
                var label = document.getElementById('new-qr-label');
                var poiId = document.getElementById('new-qr-poi');
                if (!label || !label.value.trim()) {
                    UI.showToast('Vui lòng nhập nhãn QR', 'error');
                    return;
                }
                AT.Services.QR.create({
                    label: label.value.trim(),
                    poiId: poiId ? poiId.value.trim() : null
                }).then(function (qr) {
                    UI.showToast('Đã tạo QR ' + qr.id, 'success');
                    UI.hideModal('modal-create-qr');
                    if (label) label.value = '';
                    if (poiId) poiId.value = '';
                    renderQRList();
                });
            });
        }

        var btnCancelQR = document.getElementById('btn-cancel-create-qr');
        if (btnCancelQR) {
            btnCancelQR.addEventListener('click', function () {
                UI.hideModal('modal-create-qr');
            });
        }
    }
})();

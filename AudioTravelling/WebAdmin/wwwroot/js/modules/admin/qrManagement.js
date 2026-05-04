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
                tbody.innerHTML = '<tr><td colspan="5" style="text-align:center;padding:20px;color:var(--text-dim);">Chưa có mã QR nào</td></tr>';
                return;
            }
            tbody.innerHTML = qrs.map(function (qr) {
                var statusClass = qr.status === 'active' ? 'status-approved' : 'status-rejected';
                var statusText = qr.status === 'active' ? 'Hoạt động' : 'Đã tắt';
                var toggleText = qr.status === 'active' ? 'Tắt' : 'Bật';
                var toggleIcon = qr.status === 'active' ? 'fa-toggle-on' : 'fa-toggle-off';
                return '<tr style="border-bottom:1px solid var(--border);" class="qr-row" data-qr-id="' + qr.id + '">' +
                    '<td style="padding:10px;font-weight:600;font-size:13px;cursor:pointer;" class="qr-id-cell" data-qr-id="' + qr.id + '">' +
                        '<i class="fa-solid fa-qrcode" style="margin-right:4px;color:var(--text-dim);"></i>' + qr.id +
                    '</td>' +
                    '<td style="padding:10px;"><span class="status-badge ' + statusClass + '">' + statusText + '</span></td>' +
                    '<td style="padding:10px;font-size:13px;text-align:center;">' + qr.scanCount + '</td>' +
                    '<td style="padding:10px;text-align:center;">' +
                        '<div style="display:flex;gap:4px;justify-content:center;">' +
                            '<button class="btn-ghost btn-view-qr" data-qr-id="' + qr.id + '" style="font-size:12px;padding:6px 10px;" title="Xem mã QR">' +
                                '<i class="fa-solid fa-eye"></i>' +
                            '</button>' +
                            '<button class="btn-ghost btn-toggle-qr" data-qr-id="' + qr.id + '" style="font-size:12px;padding:6px 10px;" title="' + toggleText + '">' +
                                '<i class="fa-solid ' + toggleIcon + '"></i> ' + toggleText +
                            '</button>' +
                        '</div>' +
                    '</td></tr>';
            }).join('');

            // Bind toggle buttons
            tbody.querySelectorAll('.btn-toggle-qr').forEach(function (btn) {
                btn.addEventListener('click', function (e) {
                    e.stopPropagation();
                    var qrId = btn.getAttribute('data-qr-id');
                    AT.Services.QR.toggleStatus(qrId).then(function (qr) {
                        UI.showToast('QR ' + qr.id + ' đã ' + (qr.status === 'active' ? 'bật' : 'tắt'), 'success');
                        renderQRList();
                    });
                });
            });

            // Bind view QR buttons
            tbody.querySelectorAll('.btn-view-qr').forEach(function (btn) {
                btn.addEventListener('click', function (e) {
                    e.stopPropagation();
                    showQRDetail(btn.getAttribute('data-qr-id'));
                });
            });

            // Bind QR ID cell clicks (click on ID to view QR)
            tbody.querySelectorAll('.qr-id-cell').forEach(function (cell) {
                cell.addEventListener('click', function (e) {
                    e.stopPropagation();
                    showQRDetail(cell.getAttribute('data-qr-id'));
                });
            });
        });
    }

    /** Show QR detail popup with generated QR image */
    function showQRDetail(qrId) {
        AT.Services.QR.getStats(qrId).then(function (qr) {
            if (!qr) return;

            var container = document.getElementById('qr-detail-content');
            if (!container) return;

            // Generate QR code URL using public API
            // TODO: Replace with real QR image from backend if available
            var qrData = encodeURIComponent(window.location.origin + '/scan/' + qr.id);
            var qrImageUrl = 'https://api.qrserver.com/v1/create-qr-code/?size=200x200&data=' + qrData;
            var qrLink = window.location.origin + '/scan/' + qr.id;

            var statusClass = qr.status === 'active' ? 'status-approved' : 'status-rejected';
            var statusText = qr.status === 'active' ? 'Hoạt động' : 'Đã tắt';

            container.innerHTML =
                '<div style="text-align:center;margin-bottom:20px;">' +
                    '<img src="' + qrImageUrl + '" alt="QR Code ' + qr.id + '" style="width:200px;height:200px;border-radius:8px;border:1px solid var(--border);padding:8px;background:#fff;" id="qr-detail-image">' +
                '</div>' +
                '<div style="font-size:13px;display:grid;grid-template-columns:1fr 1fr;gap:10px;">' +
                    '<div><strong style="color:var(--text-dim);display:block;margin-bottom:2px;">Mã QR</strong>' + qr.id + '</div>' +
                    '<div><strong style="color:var(--text-dim);display:block;margin-bottom:2px;">Trạng thái</strong><span class="status-badge ' + statusClass + '">' + statusText + '</span></div>' +
                    '<div><strong style="color:var(--text-dim);display:block;margin-bottom:2px;">Lượt quét</strong>' + qr.scanCount + '</div>' +
                    '<div><strong style="color:var(--text-dim);display:block;margin-bottom:2px;">Ngày tạo</strong>' + (qr.createdAt || '—') + '</div>' +
                    '<div style="grid-column:1/-1;"><strong style="color:var(--text-dim);display:block;margin-bottom:2px;">Quét lần cuối</strong>' + (qr.lastScanned ? Fmt.date(qr.lastScanned) : 'Chưa quét') + '</div>' +
                '</div>' +
                '<div style="display:flex;gap:8px;margin-top:16px;justify-content:center;">' +
                    '<a href="' + qrImageUrl + '" download="' + qr.id + '.png" class="btn-primary" style="font-size:12px;padding:8px 16px;text-decoration:none;">' +
                        '<i class="fa-solid fa-download"></i> Tải QR' +
                    '</a>' +
                    '<button class="btn-ghost" id="btn-copy-qr-link" data-link="' + qrLink + '" style="font-size:12px;padding:8px 16px;">' +
                        '<i class="fa-solid fa-copy"></i> Copy link' +
                    '</button>' +
                '</div>';

            // Bind copy link
            var btnCopy = document.getElementById('btn-copy-qr-link');
            if (btnCopy) {
                btnCopy.addEventListener('click', function () {
                    var link = btnCopy.getAttribute('data-link');
                    if (navigator.clipboard) {
                        navigator.clipboard.writeText(link).then(function () {
                            UI.showToast('Đã copy link QR', 'success');
                        });
                    } else {
                        // Fallback
                        var temp = document.createElement('textarea');
                        temp.value = link;
                        document.body.appendChild(temp);
                        temp.select();
                        document.execCommand('copy');
                        document.body.removeChild(temp);
                        UI.showToast('Đã copy link QR', 'success');
                    }
                });
            }

            UI.showModal('modal-view-qr');
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

        // Close QR view modal
        var btnCloseViewQR = document.getElementById('btn-close-view-qr');
        if (btnCloseViewQR) {
            btnCloseViewQR.addEventListener('click', function () {
                UI.hideModal('modal-view-qr');
            });
        }

        var modalViewQR = document.getElementById('modal-view-qr');
        if (modalViewQR) {
            modalViewQR.addEventListener('click', function (e) {
                if (e.target === modalViewQR) {
                    UI.hideModal('modal-view-qr');
                }
            });
        }
    }
})();


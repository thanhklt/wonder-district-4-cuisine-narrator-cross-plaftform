/**
 * Audio Travelling — QR Management Module (Admin)
 */
(function () {
    'use strict';
    var AT = window.AudioTravelling = window.AudioTravelling || {};
    AT.Modules = AT.Modules || {};
    var Fmt = AT.Utils.Formatters;
    var UI = AT.Core.UI;

    var _qrCache = [];

    AT.Modules.initQRManagement = function () {
        renderQRList();
        bindEvents();
    };

    function renderQRList() {
        AT.Services.QR.getAll().then(function (qrs) {
            console.log('[QRManagement] Loaded QR codes:', qrs);
            _qrCache = qrs || [];
            var tbody = document.getElementById('qr-table-body');
            if (!tbody) return;
            if (!qrs || qrs.length === 0) {
                tbody.innerHTML = '<tr><td colspan="4" style="text-align:center;padding:20px;color:var(--text-dim);">Chưa có mã QR nào</td></tr>';
                return;
            }
            tbody.innerHTML = qrs.map(function (qr) {
                var qrId = qr.qrId || qr.id || '';
                var code = qr.code || qr.qrCode || '';
                var status = String(qr.status || '').toLowerCase();
                var scanCount = qr.scanCount || 0;
                var createdDate = qr.createdDate || qr.createdAt || '';
                var statusClass = status === 'active' ? 'status-approved' : 'status-rejected';
                var statusText = status === 'active' ? 'Hoạt động' : 'Đã tắt';
                var toggleText = status === 'active' ? 'Tắt' : 'Bật lại';
                var toggleIcon = status === 'active' ? 'fa-toggle-on' : 'fa-toggle-off';
                return '<tr style="border-bottom:1px solid var(--border);" class="qr-row" data-qr-id="' + qrId + '">' +
                    '<td style="padding:10px;font-weight:600;font-size:13px;cursor:pointer;" class="qr-id-cell" data-qr-id="' + qrId + '">' +
                    '<i class="fa-solid fa-qrcode" style="margin-right:4px;color:var(--text-dim);"></i>' + (code || qrId) +
                    '</td>' +
                    '<td style="padding:10px;"><span class="status-badge ' + statusClass + '">' + statusText + '</span></td>' +
                    '<td style="padding:10px;font-size:13px;text-align:center;">' + scanCount + '</td>' +
                    '<td style="padding:10px;text-align:center;">' +
                    '<div style="display:flex;gap:4px;justify-content:center;">' +
                    '<button class="btn-ghost btn-view-qr" data-qr-id="' + qrId + '" style="font-size:12px;padding:6px 10px;" title="Xem mã QR">' +
                    '<i class="fa-solid fa-eye"></i>' +
                    '</button>' +
                    '<button class="btn-ghost btn-toggle-qr" data-qr-id="' + qrId + '" style="font-size:12px;padding:6px 10px;" title="' + toggleText + '">' +
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
                    AT.Services.QR.toggleStatus(qrId).then(function (result) {
                        var isActive = result && result.isActive;
                        UI.showToast('QR ' + qrId + ' đã ' + (isActive ? 'bật' : 'tắt'), 'success');
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
        }).catch(function (err) {
            console.error('[QRManagement] Error loading QR codes:', err);
        });
    }

    /** Show QR detail popup with generated QR image */
    function showQRDetail(qrId) {
        AT.Services.QR.getStats(qrId).then(function (qr) {
            console.log('[QRManagement] QR detail:', qr);
            if (!qr) return;

            var container = document.getElementById('qr-detail-content');
            if (!container) return;

            var id = qr.qrId || qr.id || qrId;
            var code = qr.code || qr.qrCode || '';
            var qrPayload = qr.qrPayload || qr.targetUrl || ('/access?code=' + code);
            var status = String(qr.status || '').toLowerCase();
            var scanCount = qr.scanCount || 0;
            var createdDate = qr.createdDate || qr.createdAt || '';

            // Generate QR code URL using public API
            var qrData = encodeURIComponent(window.location.origin + qrPayload);
            var qrImageUrl = 'https://api.qrserver.com/v1/create-qr-code/?size=200x200&data=' + qrData;
            var qrLink = window.location.origin + qrPayload;

            var statusClass = status === 'active' ? 'status-approved' : 'status-rejected';
            var statusText = status === 'active' ? 'Hoạt động' : 'Đã tắt';

            container.innerHTML =
                '<div style="text-align:center;margin-bottom:20px;">' +
                '<img src="' + qrImageUrl + '" alt="QR Code ' + code + '" style="width:200px;height:200px;border-radius:8px;border:1px solid var(--border);padding:8px;background:#fff;" id="qr-detail-image">' +
                '</div>' +
                '<div style="font-size:13px;display:grid;grid-template-columns:1fr 1fr;gap:10px;">' +
                '<div><strong style="color:var(--text-dim);display:block;margin-bottom:2px;">Mã QR</strong>' + code + '</div>' +
                '<div><strong style="color:var(--text-dim);display:block;margin-bottom:2px;">Trạng thái</strong><span class="status-badge ' + statusClass + '">' + statusText + '</span></div>' +
                '<div><strong style="color:var(--text-dim);display:block;margin-bottom:2px;">Lượt quét</strong>' + scanCount + '</div>' +
                '<div><strong style="color:var(--text-dim);display:block;margin-bottom:2px;">Ngày tạo</strong>' + Fmt.date(createdDate) + '</div>' +
                '</div>' +
                '<div style="display:flex;gap:8px;margin-top:16px;justify-content:center;">' +
                '<a href="' + qrImageUrl + '" download="QR-' + code + '.png" class="btn-primary" style="font-size:12px;padding:8px 16px;text-decoration:none;">' +
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
        }).catch(function (err) {
            console.error('[QRManagement] Error loading QR detail:', err);
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
                AT.Services.QR.create({}).then(function (qr) {
                    var qrCode = qr.code || qr.qrId || '';
                    UI.showToast('Đã tạo QR ' + qrCode, 'success');
                    UI.hideModal('modal-create-qr');
                    renderQRList();
                }).catch(function (err) {
                    UI.showToast('Lỗi khi tạo QR', 'error');
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

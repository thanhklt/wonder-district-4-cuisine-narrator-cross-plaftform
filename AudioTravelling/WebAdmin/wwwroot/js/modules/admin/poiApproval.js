/**
 * Audio Travelling — POI Approval Module (Admin)
 */
(function () {
    'use strict';
    var AT = window.AudioTravelling = window.AudioTravelling || {};
    AT.Modules = AT.Modules || {};
    var Fmt = AT.Utils.Formatters;
    var UI = AT.Core.UI;

    var _rejectTargetId = null;

    AT.Modules.initPOIApproval = function () {
        renderPendingPOIs();
        bindEvents();
    };

    function renderPendingPOIs() {
        AT.Services.POI.getPending().then(function (pois) {
            var tbody = document.getElementById('poi-approval-body');
            if (!tbody) return;
            if (pois.length === 0) {
                tbody.innerHTML = '<tr><td colspan="6" style="text-align:center;padding:20px;color:var(--text-dim);">Không có POI nào đang chờ duyệt</td></tr>';
                return;
            }
            tbody.innerHTML = pois.map(function (poi) {
                return '<tr style="border-bottom:1px solid var(--border);">' +
                    '<td style="padding:10px;font-weight:600;font-size:13px;">' + poi.id + '</td>' +
                    '<td style="padding:10px;font-size:13px;font-weight:500;">' + poi.name + '</td>' +
                    '<td style="padding:10px;font-size:13px;">' + poi.owner + '</td>' +
                    '<td style="padding:10px;font-size:13px;">' + Fmt.dateTime(poi.updatedAt) + '</td>' +
                    '<td style="padding:10px;text-align:center;">' +
                    '<button class="btn-primary btn-approve-poi" data-poi-id="' + poi.id + '" style="font-size:12px;padding:6px 14px;margin-right:6px;">' +
                    '<i class="fa-solid fa-check"></i> Duyệt</button>' +
                    '<button class="btn-danger-ghost btn-reject-poi" data-poi-id="' + poi.id + '" style="font-size:12px;padding:6px 14px;">' +
                    '<i class="fa-solid fa-xmark"></i> Từ chối</button>' +
                    '</td></tr>';
            }).join('');

            // Bind approve buttons
            tbody.querySelectorAll('.btn-approve-poi').forEach(function (btn) {
                btn.addEventListener('click', function () {
                    var poiId = btn.getAttribute('data-poi-id');
                    AT.Services.POI.approve(poiId).then(function () {
                        UI.showToast('Đã duyệt POI ' + poiId, 'success');
                        renderPendingPOIs();
                    });
                });
            });

            // Bind reject buttons
            tbody.querySelectorAll('.btn-reject-poi').forEach(function (btn) {
                btn.addEventListener('click', function () {
                    _rejectTargetId = btn.getAttribute('data-poi-id');
                    var reasonInput = document.getElementById('reject-reason');
                    if (reasonInput) reasonInput.value = '';
                    UI.showModal('modal-reject-poi');
                });
            });
        });
    }

    var _eventsBound = false;
    function bindEvents() {
        if (_eventsBound) return;
        _eventsBound = true;

        var btnConfirmReject = document.getElementById('btn-confirm-reject');
        if (btnConfirmReject) {
            btnConfirmReject.addEventListener('click', function () {
                if (!_rejectTargetId) return;
                var reason = document.getElementById('reject-reason');
                AT.Services.POI.reject(_rejectTargetId, reason ? reason.value : '').then(function () {
                    UI.showToast('Đã từ chối POI ' + _rejectTargetId, 'warning');
                    UI.hideModal('modal-reject-poi');
                    _rejectTargetId = null;
                    renderPendingPOIs();
                });
            });
        }

        var btnCancelReject = document.getElementById('btn-cancel-reject');
        if (btnCancelReject) {
            btnCancelReject.addEventListener('click', function () {
                UI.hideModal('modal-reject-poi');
                _rejectTargetId = null;
            });
        }
    }
})();

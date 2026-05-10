/**
 * Audio Travelling — Access History Module (Admin)
 */
(function () {
    'use strict';
    var AT = window.AudioTravelling = window.AudioTravelling || {};
    AT.Modules = AT.Modules || {};
    var Fmt = AT.Utils.Formatters;
    var UI = AT.Core.UI;

    AT.Modules.initAccessHistory = function () {
        renderAccessHistory();
        bindFilter();
    };

    function renderAccessHistory(fromDate, toDate) {
        var tbody = document.getElementById('access-history-body');
        var countEl = document.getElementById('history-count');

        // Show loading state
        if (tbody) {
            tbody.innerHTML = '<tr><td colspan="4" style="text-align:center;padding:30px;color:var(--text-dim);">' +
                '<i class="fa-solid fa-spinner fa-spin" style="font-size:20px;margin-bottom:8px;display:block;"></i>' +
                'Đang tải dữ liệu...</td></tr>';
        }

        AT.Services.AccessStats.getAccessHistory(fromDate || null, toDate || null).then(function (sessions) {
            console.log('[AccessHistory] Loaded sessions:', sessions);
            if (countEl) countEl.textContent = (sessions ? sessions.length : 0) + ' bản ghi';

            if (!tbody) return;
            if (!sessions || sessions.length === 0) {
                tbody.innerHTML = '<tr><td colspan="4" style="text-align:center;padding:30px;color:var(--text-dim);">' +
                    '<i class="fa-solid fa-inbox" style="font-size:28px;margin-bottom:8px;display:block;"></i>' +
                    'Không có lịch sử truy cập' +
                    (fromDate || toDate ? ' trong khoảng thời gian đã chọn' : '') +
                    '</td></tr>';
                return;
            }
            tbody.innerHTML = sessions.map(function (s) {
                var sessionId = s.sessionId || s.id || s.sessionID || '-';
                var qrCode = s.code || s.qrCode || s.qrName || s.qrCodeValue || '-';
                var deviceId = s.deviceId || s.device || s.deviceCode || s.deviceIdentifier || '-';
                var scanTime = s.issuedAt || s.timestamp || s.createdDate || s.scanTime || s.scannedAt;
                
                var formattedTime = '-';
                if (scanTime) {
                    var d = new Date(scanTime);
                    if (!isNaN(d.getTime())) {
                        formattedTime = 
                            String(d.getDate()).padStart(2, '0') + '/' + 
                            String(d.getMonth() + 1).padStart(2, '0') + '/' + 
                            d.getFullYear() + ' ' + 
                            String(d.getHours()).padStart(2, '0') + ':' + 
                            String(d.getMinutes()).padStart(2, '0') + ':' + 
                            String(d.getSeconds()).padStart(2, '0');
                    }
                }

                return '<tr style="border-bottom:1px solid var(--border);">' +
                    '<td style="padding:10px;font-size:13px;">' + sessionId + '</td>' +
                    '<td style="padding:10px;font-size:13px;">' + qrCode + '</td>' +
                    '<td style="padding:10px;font-size:13px;">' + deviceId + '</td>' +
                    '<td style="padding:10px;font-size:13px;">' + formattedTime + '</td>' +
                    '</tr>';
            }).join('');
        }).catch(function (err) {
            console.error('[AccessHistory] Error:', err);
            if (countEl) countEl.textContent = '0 bản ghi';
            if (tbody) {
                tbody.innerHTML = '<tr><td colspan="4" style="text-align:center;padding:30px;color:red;">' +
                    '<i class="fa-solid fa-circle-exclamation" style="font-size:24px;margin-bottom:8px;display:block;"></i>' +
                    'Không thể tải dữ liệu. Lỗi: ' + (err.message || err) + '</td></tr>';
            }
        });
    }

    var _filterBound = false;
    function bindFilter() {
        if (_filterBound) return;
        _filterBound = true;

        var btnFilter = document.getElementById('btn-filter-history');
        if (btnFilter) {
            btnFilter.addEventListener('click', function () {
                var fromEl = document.getElementById('filter-history-from');
                var toEl = document.getElementById('filter-history-to');
                var fromDate = fromEl ? fromEl.value : '';
                var toDate = toEl ? toEl.value : '';

                // Validate: fromDate must not be greater than toDate
                if (fromDate && toDate && fromDate > toDate) {
                    UI.showToast('Ngày bắt đầu không được lớn hơn ngày kết thúc', 'error');
                    return;
                }

                renderAccessHistory(fromDate, toDate);
            });
        }

        var btnReset = document.getElementById('btn-reset-filter-history');
        if (btnReset) {
            btnReset.addEventListener('click', function () {
                var fromEl = document.getElementById('filter-history-from');
                var toEl = document.getElementById('filter-history-to');
                if (fromEl) fromEl.value = '';
                if (toEl) toEl.value = '';
                renderAccessHistory();
            });
        }
    }
})();

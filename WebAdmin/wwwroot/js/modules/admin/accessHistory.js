/**
 * Audio Travelling — Access History Module (Admin)
 */
(function () {
    'use strict';
    var AT = window.AudioTravelling = window.AudioTravelling || {};
    AT.Modules = AT.Modules || {};
    var Fmt = AT.Utils.Formatters;

    AT.Modules.initAccessHistory = function () {
        renderAccessHistory();
        bindFilter();
    };

    function renderAccessHistory(fromDate, toDate) {
        AT.Services.AccessStats.getAccessHistory(fromDate, toDate).then(function (sessions) {
            console.log('[AccessHistory] Loaded sessions:', sessions);
            var countEl = document.getElementById('history-count');
            if (countEl) countEl.textContent = (sessions ? sessions.length : 0) + ' bản ghi';

            var tbody = document.getElementById('access-history-body');
            if (!tbody) return;
            if (!sessions || sessions.length === 0) {
                tbody.innerHTML = '<tr><td colspan="6" style="text-align:center;padding:20px;color:var(--text-dim);">Không có lịch sử truy cập</td></tr>';
                return;
            }
            tbody.innerHTML = sessions.map(function (s) {
                var sessionId = s.sessionId || s.id || '—';
                var qrCode = s.code || s.qrCode || '—';
                var deviceId = s.deviceId || s.device || '—';
                var issuedAt = s.issuedAt || s.timestamp || '';
                var expiredAt = s.expiredAt || '';
                var isRevoked = s.isRevoked ? 'Đã thu hồi' : 'Hoạt động';
                return '<tr style="border-bottom:1px solid var(--border);">' +
                    '<td style="padding:10px;font-size:13px;">' + sessionId + '</td>' +
                    '<td style="padding:10px;font-size:13px;">' + qrCode + '</td>' +
                    '<td style="padding:10px;font-size:13px;">' + deviceId + '</td>' +
                    '<td style="padding:10px;font-size:13px;">' + Fmt.dateTime(issuedAt) + '</td>' +
                    '<td style="padding:10px;font-size:13px;">' + Fmt.dateTime(expiredAt) + '</td>' +
                    '<td style="padding:10px;font-size:13px;">' + isRevoked + '</td>' +
                    '</tr>';
            }).join('');
        }).catch(function (err) {
            console.error('[AccessHistory] Error:', err);
        });
    }

    var _filterBound = false;
    function bindFilter() {
        if (_filterBound) return;
        _filterBound = true;
        var btnFilter = document.getElementById('btn-filter-history');
        if (btnFilter) {
            btnFilter.addEventListener('click', function () {
                var from = document.getElementById('filter-history-from');
                var to = document.getElementById('filter-history-to');
                renderAccessHistory(from ? from.value : null, to ? to.value : null);
            });
        }
    }
})();

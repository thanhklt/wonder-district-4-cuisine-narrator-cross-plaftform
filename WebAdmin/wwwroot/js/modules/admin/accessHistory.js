/**
 * Audio Travelling — Access Sessions Module (Admin)
 */
(function () {
    'use strict';
    var AT = window.AudioTravelling = window.AudioTravelling || {};
    AT.Modules = AT.Modules || {};
    var Fmt = AT.Utils.Formatters;

    AT.Modules.initAccessHistory = function () {
        loadSessions();
        bindEvents();
    };

    function getFilters() {
        return {
            from:   (document.getElementById('filter-history-from')   || {}).value || '',
            to:     (document.getElementById('filter-history-to')     || {}).value || '',
            status: (document.getElementById('filter-history-status') || {}).value || 'all'
        };
    }

    function loadSessions() {
        var f = getFilters();
        var tbody = document.getElementById('access-history-body');
        if (tbody) tbody.innerHTML = '<tr><td colspan="6" style="text-align:center;padding:20px;color:var(--text-dim);"><i class="fa-solid fa-spinner fa-spin"></i> Đang tải...</td></tr>';

        AT.Services.AccessStats.getSessionsList(f.from, f.to, f.status)
            .then(function (sessions) {
                var countEl = document.getElementById('history-count');
                if (countEl) countEl.textContent = (sessions ? sessions.length : 0) + ' bản ghi';

                if (!tbody) return;
                if (!sessions || sessions.length === 0) {
                    tbody.innerHTML = '<tr><td colspan="6" style="text-align:center;padding:24px;color:var(--text-dim);">Không có phiên truy cập nào</td></tr>';
                    return;
                }

                tbody.innerHTML = sessions.map(function (s) {
                    var expired  = s.isExpired;
                    var badge    = expired
                        ? '<span class="status-badge status-rejected">Hết hạn</span>'
                        : '<span class="status-badge status-approved">Hoạt động</span>';

                    // Rút gọn deviceId để không chiếm quá nhiều chỗ
                    var device = s.deviceId || '—';
                    var deviceShort = device.length > 20 ? device.substring(0, 20) + '…' : device;

                    return '<tr style="border-bottom:1px solid var(--border);">' +
                        '<td style="padding:12px 16px;font-size:13px;font-weight:600;">#' + (s.sessionId || '—') + '</td>' +
                        '<td style="padding:12px 16px;font-size:12px;font-family:monospace;" title="' + device + '">' + deviceShort + '</td>' +
                        '<td style="padding:12px 16px;font-size:13px;">' + (s.qrCodeName || '—') + '</td>' +
                        '<td style="padding:12px 16px;font-size:13px;">' + Fmt.dateTime(s.issuedAt) + '</td>' +
                        '<td style="padding:12px 16px;font-size:13px;">' + Fmt.dateTime(s.expiredAt) + '</td>' +
                        '<td style="padding:12px 16px;text-align:center;">' + badge + '</td>' +
                        '</tr>';
                }).join('');
            })
            .catch(function (err) {
                console.error('[AccessHistory] Error:', err);
                var tbody2 = document.getElementById('access-history-body');
                if (tbody2) tbody2.innerHTML = '<tr><td colspan="6" style="text-align:center;padding:20px;color:#ef4444;">Lỗi tải dữ liệu</td></tr>';
            });
    }

    var _bound = false;
    function bindEvents() {
        if (_bound) return;
        _bound = true;

        var btnFilter = document.getElementById('btn-filter-history');
        if (btnFilter) btnFilter.addEventListener('click', loadSessions);

        var btnReset = document.getElementById('btn-reset-history');
        if (btnReset) {
            btnReset.addEventListener('click', function () {
                var el;
                el = document.getElementById('filter-history-from');   if (el) el.value = '';
                el = document.getElementById('filter-history-to');     if (el) el.value = '';
                el = document.getElementById('filter-history-status'); if (el) el.value = 'all';
                loadSessions();
            });
        }
    }
})();

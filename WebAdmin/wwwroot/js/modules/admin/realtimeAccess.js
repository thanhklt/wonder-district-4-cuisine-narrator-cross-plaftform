/**
 * Audio Travelling — Realtime Access Module (Admin)
 */
(function () {
    'use strict';
    var AT = window.AudioTravelling = window.AudioTravelling || {};
    AT.Modules = AT.Modules || {};
    var Fmt = AT.Utils.Formatters;

    AT.Modules.initRealtimeAccess = function () {
        renderRealtimeSessions();
        // Tự động làm mới mỗi 5 giây
        setInterval(renderRealtimeSessions, 5000);
    };

    function renderRealtimeSessions() {
        // Get realtime count
        AT.Services.AccessStats.getRealtimeCount().then(function (count) {
            var countEl = document.getElementById('realtime-session-count');
            if (countEl) countEl.textContent = count;
        }).catch(function () {});

        // Get recent sessions for the table
        AT.Services.AccessStats.getRealtimeSessions().then(function (sessions) {
            console.log('[RealtimeAccess] Sessions:', sessions);
            var tbody = document.getElementById('realtime-sessions-body');
            if (!tbody) return;
            if (!sessions || sessions.length === 0) {
                tbody.innerHTML = '<tr><td colspan="5" style="text-align:center;padding:20px;color:var(--text-dim);">Không có phiên truy cập nào đang hoạt động</td></tr>';
                return;
            }
            tbody.innerHTML = sessions.map(function (s) {
                var sessionId = s.sessionId || s.id || '—';
                var qrCode = s.code || s.qrCode || '—';
                var deviceId = s.deviceId || '—';
                var issuedAt = s.issuedAt || '';
                var expiredAt = s.expiredAt || '';
                var isRevoked = s.isRevoked;
                var statusText = isRevoked ? 'Kết thúc' : 'Đang hoạt động';
                var statusClass = isRevoked ? 'status-rejected' : 'status-approved';
                return '<tr style="border-bottom:1px solid var(--border);">' +
                    '<td style="padding:10px;font-weight:500;font-size:13px;">' + sessionId + '</td>' +
                    '<td style="padding:10px;font-size:13px;">' + deviceId + '</td>' +
                    '<td style="padding:10px;font-size:13px;">' + qrCode + '</td>' +
                    '<td style="padding:10px;font-size:13px;">' + Fmt.dateTime(issuedAt) + '</td>' +
                    '<td style="padding:10px;"><span class="status-badge ' + statusClass + '">' + statusText + '</span></td>' +
                    '</tr>';
            }).join('');
        }).catch(function (err) {
            console.error('[RealtimeAccess] Error:', err);
        });
    }
})();

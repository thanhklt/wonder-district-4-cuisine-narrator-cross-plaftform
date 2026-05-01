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
    };

    function renderRealtimeSessions() {
        AT.Services.AccessStats.getRealtimeSessions().then(function (sessions) {
            var countEl = document.getElementById('realtime-session-count');
            if (countEl) countEl.textContent = sessions.length;

            var tbody = document.getElementById('realtime-sessions-body');
            if (!tbody) return;
            if (sessions.length === 0) {
                tbody.innerHTML = '<tr><td colspan="6" style="text-align:center;padding:20px;color:var(--text-dim);">Không có phiên truy cập nào đang hoạt động</td></tr>';
                return;
            }
            tbody.innerHTML = sessions.map(function (s) {
                var statusMap = { active: 'Đang xem', listening: 'Đang nghe', browsing: 'Đang duyệt', ended: 'Kết thúc' };
                return '<tr style="border-bottom:1px solid var(--border);">' +
                    '<td style="padding:10px;font-weight:500;font-size:13px;">' + s.sessionId + '</td>' +
                    '<td style="padding:10px;font-size:13px;">' + s.qrId + '</td>' +
                    '<td style="padding:10px;font-size:13px;">' + (s.poiName || '—') + '</td>' +
                    '<td style="padding:10px;font-size:13px;">' + Fmt.dateTime(s.startTime) + '</td>' +
                    '<td style="padding:10px;font-size:13px;">' + Fmt.relativeTime(s.lastActive) + '</td>' +
                    '<td style="padding:10px;"><span class="status-badge status-' + s.status + '">' + (statusMap[s.status] || s.status) + '</span></td>' +
                    '</tr>';
            }).join('');
        });
    }
})();

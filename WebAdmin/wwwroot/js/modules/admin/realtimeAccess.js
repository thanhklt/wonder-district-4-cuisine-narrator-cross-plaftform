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
                tbody.innerHTML = '<tr><td colspan="4" style="text-align:center;padding:20px;color:var(--text-dim);">Không có phiên truy cập nào đang hoạt động</td></tr>';
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
                    '<td style="padding:10px;font-weight:500;font-size:13px;">' + sessionId + '</td>' +
                    '<td style="padding:10px;font-size:13px;">' + qrCode + '</td>' +
                    '<td style="padding:10px;font-size:13px;">' + deviceId + '</td>' +
                    '<td style="padding:10px;font-size:13px;">' + formattedTime + '</td>' +
                    '</tr>';
            }).join('');
        }).catch(function (err) {
            console.error('[RealtimeAccess] Error:', err);
        });
    }
})();

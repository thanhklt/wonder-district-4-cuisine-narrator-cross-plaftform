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

    function getDeviceProfileBadge(profile) {
        if (profile === 0) {
            return '<span style="background:#dcfce7; color:#166534; padding:4px 8px; border-radius:4px; font-weight:600; font-size:12px;">Mạnh</span>';
        }
        if (profile === 1) {
            return '<span style="background:#fef08a; color:#854d0e; padding:4px 8px; border-radius:4px; font-weight:600; font-size:12px;">Yếu</span>';
        }
        return '<span style="background:#e5e7eb; color:#6b7280; padding:4px 8px; border-radius:4px; font-weight:600; font-size:12px;">Không xác định</span>';
    }

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

                var configBadge = getDeviceProfileBadge(s.deviceProfile);

                return '<tr style="border-bottom:1px solid var(--border);">' +
                    '<td style="padding:10px;font-weight:500;font-size:13px;">' + sessionId + '</td>' +
                    '<td style="padding:10px;font-size:13px;">' + qrCode + '</td>' +
                    '<td style="padding:10px;font-size:13px;">' + deviceId + '</td>' +
                    '<td style="padding:10px;font-size:13px;">' + configBadge + '</td>' +
                    '<td style="padding:10px;font-size:13px;">' + formattedTime + '</td>' +
                    '</tr>';
            }).join('');
        }).catch(function (err) {
            console.error('[RealtimeAccess] Error:', err);
        });
    }
})();

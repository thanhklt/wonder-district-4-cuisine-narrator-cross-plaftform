/**
 * ═══════════════════════════════════════════════════
 * Audio Travelling — Access Stats Service
 * ═══════════════════════════════════════════════════
 */
(function () {
    'use strict';

    var AT = window.AudioTravelling = window.AudioTravelling || {};
    AT.Services = AT.Services || {};

    AT.Services.AccessStats = {
        /** Get access stats for a specific period (today, week, month) */
        getStats: function (period) {
            return AT.Core.ApiClient.get('/admin/stats/sessions?period=' + (period || 'today'));
        },

        /** Get dashboard stats — combines session stats + POI counts */
        getDashboardStats: function () {
            return Promise.all([
                AT.Core.ApiClient.get('/admin/stats/sessions?period=today'),
                AT.Core.ApiClient.get('/admin/pois')
            ]).then(function (results) {
                var stats = results[0] || {};
                var pois = results[1] || [];
                return {
                    activeUsers: stats.activeUsers || 0,
                    rejectedPOIs: pois.filter(function (p) { return (p.status || '').toLowerCase() === 'rejected'; }).length,
                    approvedPOIs: pois.filter(function (p) { return (p.status || '').toLowerCase() === 'approved'; }).length,
                    pendingPOIs: pois.filter(function (p) { return (p.status || '').toLowerCase() === 'pending'; }).length
                };
            }).catch(function (err) {
                console.warn('[AccessStats] getDashboardStats error:', err);
                return { activeUsers: 0, rejectedPOIs: 0, approvedPOIs: 0, pendingPOIs: 0 };
            });
        },

        /** Get access history (recent sessions) for a date range */
        getAccessHistory: function (fromDate, toDate) {
            var period = 'all';
            if (fromDate || toDate) {
                // Use 'all' and filter client-side if needed
                period = 'all';
            }
            return AT.Core.ApiClient.get('/admin/stats/sessions?period=' + period).then(function (data) {
                return (data && data.recentSessions) || [];
            }).catch(function (err) {
                console.warn('[AccessStats] getAccessHistory error:', err);
                return [];
            });
        },

        /** Get realtime active user count */
        getRealtimeCount: function () {
            return AT.Core.ApiClient.get('/admin/stats/realtime').then(function (data) {
                return data.activeCount || 0;
            });
        },

        /** Get realtime sessions (returns array with activeCount info) */
        getRealtimeSessions: function () {
            return AT.Core.ApiClient.get('/admin/stats/sessions?period=today').then(function (data) {
                return (data && data.recentSessions) || [];
            }).catch(function (err) {
                console.warn('[AccessStats] getRealtimeSessions error:', err);
                return [];
            });
        },

        /** Get sessions table with filters (from, to: YYYY-MM-DD; status: active|expired|all) */
        getSessionsList: function (from, to, status) {
            return AT.Core.ApiClient.get('/admin/stats/sessions?period=all')
                .then(function (data) {
                    var sessions = (data && data.recentSessions) || [];
                    var now = new Date();

                    // Lọc theo ngày issuedAt
                    if (from) {
                        var fromDate = new Date(from);
                        sessions = sessions.filter(function (s) {
                            return new Date(s.issuedAt) >= fromDate;
                        });
                    }
                    if (to) {
                        var toDate = new Date(to);
                        toDate.setHours(23, 59, 59, 999);
                        sessions = sessions.filter(function (s) {
                            return new Date(s.issuedAt) <= toDate;
                        });
                    }

                    // Map + tính trạng thái + lọc theo status
                    var result = [];
                    sessions.forEach(function (s) {
                        var isExpired = new Date(s.expiredAt) <= now || !!s.isRevoked;
                        if (status === 'active'  &&  isExpired) return;
                        if (status === 'expired' && !isExpired) return;
                        result.push({
                            sessionId:   s.sessionId,
                            deviceId:    s.deviceId,
                            qrCodeName:  s.code || '—',
                            issuedAt:    s.issuedAt,
                            expiredAt:   s.expiredAt,
                            isExpired:   isExpired
                        });
                    });
                    return result;
                });
        },

        /** Lượt quét QR theo ngày (N ngày gần nhất) */
        getDailyScans: function (days) {
            return AT.Core.ApiClient.get('/admin/stats/daily-scans?days=' + (days || 7));
        },

        /** Get heatmap data */
        getHeatmapData: function (period) {
            return AT.Core.ApiClient.get('/admin/stats/heatmap?period=' + (period || 'today'));
        }
    };
})();


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
            var params = [];

            if (fromDate) {
                params.push('fromDate=' + fromDate);
            }
            if (toDate) {
                params.push('toDate=' + toDate);
            }

            // If no dates provided, use period=all
            if (params.length === 0) {
                params.push('period=all');
            }

            var queryString = '?' + params.join('&');

            return AT.Core.ApiClient.get('/admin/stats/sessions' + queryString).then(function (data) {
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

        /** Get heatmap data */
        getHeatmapData: function (period) {
            return AT.Core.ApiClient.get('/admin/stats/heatmap?period=' + (period || 'today'));
        }
    };
})();

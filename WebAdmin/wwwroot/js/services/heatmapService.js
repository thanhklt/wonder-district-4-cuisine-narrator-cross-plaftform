/**
 * Audio Travelling — Heatmap Service
 */
(function () {
    'use strict';
    var AT = window.AudioTravelling = window.AudioTravelling || {};
    AT.Services = AT.Services || {};

    AT.Services.Heatmap = {
        getData: function () {
            return AT.Core.ApiClient.get('/admin/stats/heatmap?period=today').then(function (data) {
                // API returns empty array when no heatmap data
                return data || [];
            }).catch(function (err) {
                console.warn('[Heatmap] getData error:', err);
                return [];
            });
        }
    };
})();

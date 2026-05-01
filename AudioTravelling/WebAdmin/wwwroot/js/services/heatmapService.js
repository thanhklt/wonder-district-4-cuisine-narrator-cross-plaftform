/**
 * Audio Travelling — Heatmap Service
 * TODO: Replace mock calls with real API endpoints.
 */
(function () {
    'use strict';
    var AT = window.AudioTravelling = window.AudioTravelling || {};
    AT.Services = AT.Services || {};

    AT.Services.Heatmap = {
        getData: function () {
            // TODO: return AT.Core.ApiClient.get('/admin/heatmap');
            return Promise.resolve(AT.Mocks.HeatmapData.slice());
        }
    };
})();

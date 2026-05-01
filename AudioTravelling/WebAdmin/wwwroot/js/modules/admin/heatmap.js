/**
 * Audio Travelling — Heatmap Module (Admin)
 */
(function () {
    'use strict';
    var AT = window.AudioTravelling = window.AudioTravelling || {};
    AT.Modules = AT.Modules || {};

    var _mapInstance = null;
    var _heatLayer = null;

    AT.Modules.initHeatmap = function () {
        var container = document.getElementById('heatmap-container');
        if (!container) return;

        // Wait for Leaflet
        if (typeof L === 'undefined') {
            setTimeout(AT.Modules.initHeatmap, 200);
            return;
        }

        // Initialise or reuse map
        if (_mapInstance) {
            _mapInstance.invalidateSize();
            refreshHeatData();
            return;
        }

        _mapInstance = L.map(container, { zoomControl: true }).setView([10.7750, 106.6930], 14);

        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            attribution: '&copy; OpenStreetMap'
        }).addTo(_mapInstance);

        refreshHeatData();

        // Fix map size after animation
        setTimeout(function () { _mapInstance.invalidateSize(); }, 400);
    };

    function refreshHeatData() {
        AT.Services.Heatmap.getData().then(function (data) {
            if (!_mapInstance) return;
            if (_heatLayer) _mapInstance.removeLayer(_heatLayer);

            // If leaflet-heat plugin is available, use it
            if (typeof L.heatLayer === 'function') {
                _heatLayer = L.heatLayer(data, {
                    radius: 35,
                    blur: 25,
                    maxZoom: 17,
                    gradient: { 0.2: '#22c55e', 0.5: '#fbbf24', 0.8: '#ef4444', 1.0: '#dc2626' }
                }).addTo(_mapInstance);
            } else {
                // Fallback: render circles for each data point
                data.forEach(function (point) {
                    var intensity = point[2] || 0.5;
                    var color = intensity > 0.7 ? '#ef4444' : intensity > 0.4 ? '#fbbf24' : '#22c55e';
                    L.circle([point[0], point[1]], {
                        radius: 80 * intensity,
                        color: color,
                        fillColor: color,
                        fillOpacity: 0.3 + intensity * 0.3,
                        weight: 1
                    }).addTo(_mapInstance);
                });
            }
        });
    }
})();

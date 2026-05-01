/**
 * ═══════════════════════════════════════════════════
 * Audio Travelling — QR Service
 * TODO: Replace mock calls with real API endpoints.
 * ═══════════════════════════════════════════════════
 */
(function () {
    'use strict';

    var AT = window.AudioTravelling = window.AudioTravelling || {};
    AT.Services = AT.Services || {};

    var Mocks = AT.Mocks;

    AT.Services.QR = {
        /** Get all QR codes */
        getAll: function () {
            // TODO: return AT.Core.ApiClient.get('/admin/qr');
            return Promise.resolve(Mocks.QRCodes.slice());
        },

        /** Create a new QR code */
        create: function (qrData) {
            // TODO: return AT.Core.ApiClient.post('/admin/qr', qrData);
            var newQR = Object.assign({}, qrData, {
                id: 'QR-' + String(Mocks.QRCodes.length + 1).padStart(3, '0'),
                status: 'active',
                scanCount: 0,
                createdAt: new Date().toISOString().split('T')[0],
                lastScanned: null
            });
            Mocks.QRCodes.push(newQR);
            return Promise.resolve(newQR);
        },

        /** Toggle QR status (active/disabled) */
        toggleStatus: function (qrId) {
            // TODO: return AT.Core.ApiClient.patch('/admin/qr/' + qrId + '/status');
            var qr = Mocks.QRCodes.find(function (q) { return q.id === qrId; });
            if (!qr) return Promise.reject(new Error('QR not found'));
            qr.status = qr.status === 'active' ? 'disabled' : 'active';
            return Promise.resolve(qr);
        },

        /** Get QR scan statistics */
        getStats: function (qrId) {
            // TODO: return AT.Core.ApiClient.get('/admin/qr/' + qrId + '/stats');
            var qr = Mocks.QRCodes.find(function (q) { return q.id === qrId; });
            return Promise.resolve(qr || null);
        }
    };
})();

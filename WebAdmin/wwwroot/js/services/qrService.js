/**
 * ═══════════════════════════════════════════════════
 * Audio Travelling — QR Service
 * ═══════════════════════════════════════════════════
 */
(function () {
    'use strict';

    var AT = window.AudioTravelling = window.AudioTravelling || {};
    AT.Services = AT.Services || {};

    AT.Services.QR = {
        /** Get all QR codes */
        getAll: function () {
            return AT.Core.ApiClient.get('/admin/qr');
        },

        /** Get a single QR code by ID */
        getStats: function (qrId) {
            return AT.Core.ApiClient.get('/admin/qr/' + qrId);
        },

        /** Generate a new QR code */
        generate: function (settings) {
            return AT.Core.ApiClient.post('/admin/qr', settings);
        },

        /** Alias for generate */
        create: function (settings) {
            return AT.Core.ApiClient.post('/admin/qr', settings);
        },

        /** Regenerate existing QR code */
        regenerate: function (qrId) {
            return AT.Core.ApiClient.post('/admin/qr/' + qrId + '/regenerate');
        },

        /** Toggle active/inactive status */
        toggleStatus: function (qrId) {
            return AT.Core.ApiClient.patch('/admin/qr/' + qrId + '/toggle');
        },

        /** Delete QR code */
        delete: function (qrId) {
            return AT.Core.ApiClient.del('/admin/qr/' + qrId);
        }
    };
})();

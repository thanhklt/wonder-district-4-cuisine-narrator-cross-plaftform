/**
 * ═══════════════════════════════════════════════════
 * Audio Travelling — POI Service
 * ═══════════════════════════════════════════════════
 */
(function () {
    'use strict';

    var AT = window.AudioTravelling = window.AudioTravelling || {};
    AT.Services = AT.Services || {};

    AT.Services.POI = {
        /** Get all POIs (admin) */
        getAll: function (status) {
            var query = status !== undefined && status !== null ? '?status=' + status : '';
            return AT.Core.ApiClient.get('/admin/pois' + query);
        },

        /** Get POIs by owner (Owner API inherently filters by jwt token) */
        getByOwner: function () {
            return AT.Core.ApiClient.get('/owner/pois');
        },

        /** Get pending POIs (admin) */
        getPending: function () {
            return AT.Core.ApiClient.get('/admin/pois/pending');
        },

        /** Create a new POI (owner) */
        create: function (poiData) {
            return AT.Core.ApiClient.post('/owner/pois', poiData);
        },

        /** Update a POI (owner) */
        update: function (poiId, poiData) {
            return AT.Core.ApiClient.put('/owner/pois/' + poiId, poiData);
        },

        /** Delete a POI (owner) */
        delete: function (poiId) {
            return AT.Core.ApiClient.del('/owner/pois/' + poiId);
        },

        /** Submit POI for approval (owner) */
        submitForApproval: function (poiId) {
            return AT.Core.ApiClient.patch('/owner/pois/' + poiId + '/submit');
        },

        /** Approve POI (admin) */
        approve: function (poiId) {
            return AT.Core.ApiClient.patch('/admin/pois/' + poiId + '/approve');
        },

        /** Reject POI (admin) */
        reject: function (poiId, reason) {
            return AT.Core.ApiClient.patch('/admin/pois/' + poiId + '/reject', { note: reason });
        }
    };
})();

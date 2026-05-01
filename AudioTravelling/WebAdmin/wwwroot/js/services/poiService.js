/**
 * ═══════════════════════════════════════════════════
 * Audio Travelling — POI Service
 * TODO: Replace mock calls with real API endpoints.
 * ═══════════════════════════════════════════════════
 */
(function () {
    'use strict';

    var AT = window.AudioTravelling = window.AudioTravelling || {};
    AT.Services = AT.Services || {};

    var Mocks = AT.Mocks;

    AT.Services.POI = {
        /** Get all POIs (admin) */
        getAll: function () {
            // TODO: return AT.Core.ApiClient.get('/admin/pois');
            return Promise.resolve(Mocks.POIs.slice());
        },

        /** Get POIs by owner email */
        getByOwner: function (ownerEmail) {
            // TODO: return AT.Core.ApiClient.get('/owner/pois');
            var filtered = Mocks.POIs.filter(function (p) { return p.owner === ownerEmail; });
            return Promise.resolve(filtered);
        },

        /** Get pending POIs (admin) */
        getPending: function () {
            // TODO: return AT.Core.ApiClient.get('/admin/pois/pending');
            var pending = Mocks.POIs.filter(function (p) { return p.status === 'pending'; });
            return Promise.resolve(pending);
        },

        /** Create a new POI (owner) */
        create: function (poiData) {
            // TODO: return AT.Core.ApiClient.post('/owner/pois', poiData);
            var newPoi = Object.assign({}, poiData, {
                id: 'POI-' + String(Mocks.POIs.length + 1).padStart(3, '0'),
                status: 'draft',
                createdAt: new Date().toISOString(),
                updatedAt: new Date().toISOString(),
                rejectionReason: null
            });
            Mocks.POIs.push(newPoi);
            return Promise.resolve(newPoi);
        },

        /** Update a POI (owner) */
        update: function (poiId, poiData) {
            // TODO: return AT.Core.ApiClient.put('/owner/pois/' + poiId, poiData);
            var idx = Mocks.POIs.findIndex(function (p) { return p.id === poiId; });
            if (idx === -1) return Promise.reject(new Error('POI not found'));
            Object.assign(Mocks.POIs[idx], poiData, { updatedAt: new Date().toISOString() });
            // If editing an approved POI, set back to pending
            if (Mocks.POIs[idx].status === 'approved') {
                Mocks.POIs[idx].status = 'pending';
            }
            return Promise.resolve(Mocks.POIs[idx]);
        },

        /** Delete a POI (owner) */
        delete: function (poiId) {
            // TODO: return AT.Core.ApiClient.del('/owner/pois/' + poiId);
            var idx = Mocks.POIs.findIndex(function (p) { return p.id === poiId; });
            if (idx === -1) return Promise.reject(new Error('POI not found'));
            Mocks.POIs.splice(idx, 1);
            return Promise.resolve({ success: true });
        },

        /** Submit POI for approval (owner) */
        submitForApproval: function (poiId) {
            // TODO: return AT.Core.ApiClient.post('/owner/pois/' + poiId + '/submit');
            var poi = Mocks.POIs.find(function (p) { return p.id === poiId; });
            if (!poi) return Promise.reject(new Error('POI not found'));
            poi.status = 'pending';
            poi.updatedAt = new Date().toISOString();
            return Promise.resolve(poi);
        },

        /** Approve POI (admin) */
        approve: function (poiId) {
            // TODO: return AT.Core.ApiClient.post('/admin/pois/' + poiId + '/approve');
            var poi = Mocks.POIs.find(function (p) { return p.id === poiId; });
            if (!poi) return Promise.reject(new Error('POI not found'));
            poi.status = 'approved';
            poi.updatedAt = new Date().toISOString();
            poi.rejectionReason = null;
            return Promise.resolve(poi);
        },

        /** Reject POI (admin) */
        reject: function (poiId, reason) {
            // TODO: return AT.Core.ApiClient.post('/admin/pois/' + poiId + '/reject', { reason: reason });
            var poi = Mocks.POIs.find(function (p) { return p.id === poiId; });
            if (!poi) return Promise.reject(new Error('POI not found'));
            poi.status = 'rejected';
            poi.rejectionReason = reason || '';
            poi.updatedAt = new Date().toISOString();
            return Promise.resolve(poi);
        }
    };
})();

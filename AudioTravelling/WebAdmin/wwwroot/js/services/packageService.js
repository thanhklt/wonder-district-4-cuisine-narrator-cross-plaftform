(function () {
    'use strict';

    var AT = window.AudioTravelling = window.AudioTravelling || {};
    AT.Services = AT.Services || {};

    AT.Services.Package = {
        getPackages: function () {
            return AT.Core.ApiClient.get('/packages');
        },

        getPackageById: function (id) {
            return AT.Core.ApiClient.get('/packages/' + id);
        },

        createPackage: function (data) {
            return AT.Core.ApiClient.post('/packages', data);
        },

        updatePackage: function (id, data) {
            return AT.Core.ApiClient.put('/packages/' + id, data);
        },

        deletePackage: function (id) {
            return AT.Core.ApiClient.del ? AT.Core.ApiClient.del('/packages/' + id) : AT.Core.ApiClient.delete('/packages/' + id);
        }
    };
})();

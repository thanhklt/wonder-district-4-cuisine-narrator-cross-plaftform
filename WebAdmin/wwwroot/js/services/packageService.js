/**
 * ═══════════════════════════════════════════════════
 * Audio Travelling — Package Service
 * ═══════════════════════════════════════════════════
 */
(function () {
    'use strict';

    var AT = window.AudioTravelling = window.AudioTravelling || {};
    AT.Services = AT.Services || {};

    AT.Services.Package = {
        /** Get all packages */
        getAll: function () {
            return AT.Core.ApiClient.get('/packages');
        },

        /** Create a new package (Admin) */
        create: function (packageData) {
            return AT.Core.ApiClient.post('/admin/packages', packageData);
        },

        /** Update an existing package (Admin) */
        update: function (packageId, packageData) {
            return AT.Core.ApiClient.put('/admin/packages/' + packageId, packageData);
        },

        /** Delete a package (Admin) */
        delete: function (packageId) {
            return AT.Core.ApiClient.del('/admin/packages/' + packageId);
        }
    };

    // Aliases for backward compatibility with packageManagement.js
    AT.Services.Package.getPackages = AT.Services.Package.getAll;
    AT.Services.Package.createPackage = AT.Services.Package.create;
    AT.Services.Package.updatePackage = AT.Services.Package.update;
    AT.Services.Package.deletePackage = AT.Services.Package.delete;
})();

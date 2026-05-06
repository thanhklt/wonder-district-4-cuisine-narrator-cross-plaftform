/**
 * ═══════════════════════════════════════════════════
 * Audio Travelling — User Service
 * ═══════════════════════════════════════════════════
 */
(function () {
    'use strict';

    var AT = window.AudioTravelling = window.AudioTravelling || {};
    AT.Services = AT.Services || {};

    AT.Services.User = {
        // ── User Profiles (Hồ sơ người dùng) ──

        /** Get all user profiles */
        getAllProfiles: function () {
            return AT.Core.ApiClient.get('/admin/profiles');
        },

        /** Get a single user profile by ID */
        getProfileById: function (id) {
            return AT.Core.ApiClient.get('/admin/profiles/' + id);
        },

        // ── User Management (Quản lý người dùng) ──

        /** Get all user accounts */
        getAllUsers: function () {
            return AT.Core.ApiClient.get('/admin/users');
        },

        /** Get a single user account by ID */
        getUserById: function (id) {
            return AT.Core.ApiClient.get('/admin/users/' + id);
        }
    };
})();

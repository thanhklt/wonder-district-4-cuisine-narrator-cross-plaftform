/**
 * ═══════════════════════════════════════════════════
 * Audio Travelling — User Service
 * TODO: Replace all mock calls with real API endpoints.
 * ═══════════════════════════════════════════════════
 */
(function () {
    'use strict';

    var AT = window.AudioTravelling = window.AudioTravelling || {};
    AT.Services = AT.Services || {};

    var Mocks = AT.Mocks;

    AT.Services.User = {
        // ── User Profiles (Hồ sơ người dùng) ──

        /** Get all user profiles */
        getAllProfiles: function () {
            // TODO: return AT.Core.ApiClient.get('/api/admin/user-profiles');
            return Promise.resolve(Mocks.UserProfiles.slice());
        },

        /** Get a single user profile by ID */
        getProfileById: function (id) {
            // TODO: return AT.Core.ApiClient.get('/api/admin/user-profiles/' + id);
            var profile = Mocks.UserProfiles.find(function (p) { return p.id === id; });
            return Promise.resolve(profile || null);
        },

        // ── User Management (Quản lý người dùng) ──

        /** Get all user accounts */
        getAllUsers: function () {
            // TODO: return AT.Core.ApiClient.get('/api/admin/users');
            return Promise.resolve(Mocks.Users.slice());
        },

        /** Get a single user account by ID */
        getUserById: function (id) {
            // TODO: return AT.Core.ApiClient.get('/api/admin/users/' + id);
            var user = Mocks.Users.find(function (u) { return u.id === id; });
            return Promise.resolve(user || null);
        },

        /** Toggle lock/unlock a user account */
        toggleLock: function (userId) {
            // TODO: return AT.Core.ApiClient.patch('/api/admin/users/' + userId + '/toggle-lock');
            var user = Mocks.Users.find(function (u) { return u.id === userId; });
            if (!user) return Promise.reject(new Error('User not found'));
            user.status = user.status === 'active' ? 'locked' : 'active';
            // Also sync with profiles mock
            var profile = Mocks.UserProfiles.find(function (p) { return p.id === userId; });
            if (profile) profile.status = user.status;
            return Promise.resolve(user);
        },

        /** Change user role */
        changeRole: function (userId, newRole) {
            // TODO: return AT.Core.ApiClient.patch('/api/admin/users/' + userId + '/role', { role: newRole });
            var user = Mocks.Users.find(function (u) { return u.id === userId; });
            if (!user) return Promise.reject(new Error('User not found'));
            user.role = newRole;
            var profile = Mocks.UserProfiles.find(function (p) { return p.id === userId; });
            if (profile) profile.role = newRole;
            return Promise.resolve(user);
        },

        /** Reset user password */
        resetPassword: function (userId) {
            // TODO: return AT.Core.ApiClient.post('/api/admin/users/' + userId + '/reset-password');
            // Mock: just return success
            return Promise.resolve({ success: true, message: 'Mật khẩu đã được reset. Email thông báo đã gửi.' });
        }
    };
})();

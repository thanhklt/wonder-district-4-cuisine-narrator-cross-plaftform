/**
 * ═══════════════════════════════════════════════════
 * Audio Travelling — API Client Wrapper
 * ═══════════════════════════════════════════════════
 *
 * Wraps fetch() with automatic Bearer token injection
 * and standardised error handling. When real API endpoints
 * are ready, services only need to change the URL — the
 * auth header / error flow stays the same.
 */
(function () {
    'use strict';

    var AT = window.AudioTravelling = window.AudioTravelling || {};
    AT.Core = AT.Core || {};

    // TODO: Replace with real API base URL from config
    var BASE_URL = '/api';

    function getAuthHeaders() {
        var session = AT.Core.Storage.getSession();
        var headers = { 'Content-Type': 'application/json' };
        if (session && session.token) {
            headers['Authorization'] = 'Bearer ' + session.token;
        }
        return headers;
    }

    function handleResponse(response) {
        if (response.status === 401) {
            AT.Core.Auth.logout();
            throw new Error('Unauthorized');
        }
        if (response.status === 403) {
            if (AT.Core.RoleGuard) AT.Core.RoleGuard.showForbiddenPage();
            throw new Error('Forbidden');
        }
        if (!response.ok) {
            throw new Error('API Error: ' + response.status);
        }
        // 204 No Content
        if (response.status === 204) return null;
        return response.json();
    }

    AT.Core.ApiClient = {
        get: function (url) {
            return fetch(BASE_URL + url, {
                method: 'GET',
                headers: getAuthHeaders()
            }).then(handleResponse);
        },

        post: function (url, data) {
            return fetch(BASE_URL + url, {
                method: 'POST',
                headers: getAuthHeaders(),
                body: JSON.stringify(data)
            }).then(handleResponse);
        },

        put: function (url, data) {
            return fetch(BASE_URL + url, {
                method: 'PUT',
                headers: getAuthHeaders(),
                body: JSON.stringify(data)
            }).then(handleResponse);
        },

        patch: function (url, data) {
            return fetch(BASE_URL + url, {
                method: 'PATCH',
                headers: getAuthHeaders(),
                body: JSON.stringify(data)
            }).then(handleResponse);
        },

        del: function (url) {
            return fetch(BASE_URL + url, {
                method: 'DELETE',
                headers: getAuthHeaders()
            }).then(handleResponse);
        }
    };
})();

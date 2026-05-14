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

    // Replace with real API base URL from config
    // var BASE_URL = 'http://localhost:5184/api';
    var BASE_URL = 'https://scraggly-plausibly-synthesis.ngrok-free.dev/api';

    function getAuthHeaders() {
        var session = AT.Core.Storage.getSession();
        var headers = {
            'Content-Type': 'application/json',
            'ngrok-skip-browser-warning': 'true'
        };
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

    /**
     * Resolve an image URL returned by the API.
     * Always points to the API origin (ngrok or localhost).
     */
    var API_LOCAL_ORIGIN = 'http://localhost:5184';

    function resolveImageUrl(url) {
        if (!url || url === 'null' || url === 'undefined') return '/images/placeholder-poi.png';

        var origin = BASE_URL.replace(/\/api\/?$/, '');

        // Already a full online URL (not localhost) → keep as-is
        if (/^https?:\/\//i.test(url) && !/localhost/i.test(url) && !/127\.0\.0\.1/i.test(url)) {
            // Force https for ngrok URLs to avoid redirect dropping the skip-browser-warning header
            if (url.indexOf('ngrok-free.dev') !== -1 && url.startsWith('http://')) {
                return url.replace(/^http:\/\//i, 'https://');
            }
            return url;
        }
        // Absolute path like /images/pois/xxx.webp
        if (url.charAt(0) === '/') {
            return origin + url;
        }
        // Full localhost URL → strip origin, re-prefix with API origin
        try {
            var parsed = new URL(url);
            return origin + parsed.pathname;
        } catch (_) {
            return origin + '/' + url;
        }
    }

    /**
     * Load an image through fetch() with ngrok-skip-browser-warning header,
     * then convert to a blob URL. This bypasses ngrok's interstitial page
     * that blocks normal <img> tag requests.
     *
     * Usage: AT.Core.ApiClient.loadNgrokImage(imgElement, imageUrl)
     */
    function loadNgrokImage(imgElement, url) {
        if (!imgElement) return;
        var resolved = resolveImageUrl(url);
        if (!resolved || resolved === '/images/placeholder-poi.png') {
            imgElement.src = '/images/placeholder-poi.png';
            return;
        }

        fetch(resolved, {
            headers: { 'ngrok-skip-browser-warning': 'true' }
        }).then(function (res) {
            if (!res.ok) throw new Error('Image fetch failed');
            return res.blob();
        }).then(function (blob) {
            imgElement.src = URL.createObjectURL(blob);
        }).catch(function () {
            imgElement.src = '/images/placeholder-poi.png';
        });
    }

    /**
     * Process all <img> tags with data-api-src attribute inside a container.
     * Loads each image via fetch with ngrok header.
     *
     * Usage: AT.Core.ApiClient.loadAllImages(containerElement)
     * HTML:  <img data-api-src="http://..." src="/images/placeholder-poi.png">
     */
    function loadAllImages(container) {
        if (!container) return;
        var imgs = container.querySelectorAll('img[data-api-src]');
        imgs.forEach(function (img) {
            loadNgrokImage(img, img.getAttribute('data-api-src'));
        });
    }

    AT.Core.ApiClient = {
        /** Get the API origin (e.g. http://localhost:5184) without the /api path */
        getBaseOrigin: function () {
            return BASE_URL.replace(/\/api\/?$/, '');
        },

        /** Resolve an image URL to be loadable from the browser */
        resolveImageUrl: resolveImageUrl,

        /** Load a single image via fetch (bypasses ngrok warning) */
        loadNgrokImage: loadNgrokImage,

        /** Load all images with data-api-src in a container */
        loadAllImages: loadAllImages,

        get: function (url) {
            return fetch(BASE_URL + url, {
                method: 'GET',
                headers: getAuthHeaders()
            }).then(handleResponse);
        },

        post: function (url, data) {
            var isFormData = data instanceof FormData;
            var headers = getAuthHeaders();
            if (isFormData) delete headers['Content-Type'];
            return fetch(BASE_URL + url, {
                method: 'POST',
                headers: headers,
                body: isFormData ? data : JSON.stringify(data)
            }).then(handleResponse);
        },

        put: function (url, data) {
            var isFormData = data instanceof FormData;
            var headers = getAuthHeaders();
            if (isFormData) delete headers['Content-Type'];
            return fetch(BASE_URL + url, {
                method: 'PUT',
                headers: headers,
                body: isFormData ? data : JSON.stringify(data)
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

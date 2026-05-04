/**
 * ═══════════════════════════════════════════════════
 * Audio Travelling — Portal Guard
 * Runs at the top of <head> BEFORE any other scripts.
 * Checks session & role, redirects if unauthorized.
 *
 * Usage: <script src="~/js/core/portalGuard.js" data-portal="admin"></script>
 *        <script src="~/js/core/portalGuard.js" data-portal="owner"></script>
 *
 * NOTE: This uses the same session key "at_session" stored by storage.js.
 * TODO: When integrating real auth, consider migrating session storage
 *       to use separate keys: access_token, user_role, user_id
 *       instead of the single at_session JSON object.
 * ═══════════════════════════════════════════════════
 */
(function () {
    'use strict';

    // Determine which portal this guard is protecting
    var scripts = document.querySelectorAll('script[data-portal]');
    var portalType = null;
    for (var i = 0; i < scripts.length; i++) {
        var p = scripts[i].getAttribute('data-portal');
        if (p) { portalType = p.toLowerCase(); break; }
    }

    // Read session from localStorage
    var session = null;
    try {
        var raw = localStorage.getItem('at_session');
        session = raw ? JSON.parse(raw) : null;
    } catch (e) {
        session = null;
    }

    var path = window.location.pathname.toLowerCase();

    // ── Not logged in → redirect to login ──
    if (!session || !session.loggedIn) {
        if (portalType === 'admin' || portalType === 'owner') {
            window.location.replace('/');
            // Stop further page rendering
            document.documentElement.innerHTML = '';
            return;
        }
    }

    // ── Logged in but wrong portal → redirect to correct portal ──
    if (session && session.loggedIn && session.role) {
        var role = session.role;

        if (portalType === 'admin' && role !== 'Admin') {
            // Non-admin trying to access admin portal
            window.location.replace('/owner');
            document.documentElement.innerHTML = '';
            return;
        }

        if (portalType === 'owner' && role !== 'Owner') {
            // Non-owner trying to access owner portal
            window.location.replace('/admin');
            document.documentElement.innerHTML = '';
            return;
        }
    }

    // If we get here, access is allowed — page continues loading normally
})();

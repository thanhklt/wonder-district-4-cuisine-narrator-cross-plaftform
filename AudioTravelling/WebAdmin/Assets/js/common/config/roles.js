/**
 * ═══════════════════════════════════════════════════
 * Audio Travelling — Role Constants
 * ═══════════════════════════════════════════════════
 */
(function () {
    'use strict';

    window.AudioTravelling = window.AudioTravelling || {};
    window.AudioTravelling.Config = window.AudioTravelling.Config || {};

    window.AudioTravelling.Config.Roles = Object.freeze({
        ADMIN: 'Admin',
        OWNER: 'Owner'
    });

    /** List of roles allowed to access the portal */
    window.AudioTravelling.Config.ALLOWED_ROLES = Object.freeze([
        window.AudioTravelling.Config.Roles.ADMIN,
        window.AudioTravelling.Config.Roles.OWNER
    ]);
})();

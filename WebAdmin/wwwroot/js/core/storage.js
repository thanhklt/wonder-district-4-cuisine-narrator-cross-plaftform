/**
 * ═══════════════════════════════════════════════════
 * Audio Travelling — Storage Wrapper
 * ═══════════════════════════════════════════════════
 */
(function () {
    'use strict';

    var AT = window.AudioTravelling = window.AudioTravelling || {};
    AT.Core = AT.Core || {};

    var KEYS = {
        SESSION: 'at_session',
        THEME: 'at_theme',
        REMEMBER_EMAIL: 'at_remember_email'
    };

    AT.Core.Storage = {
        KEYS: KEYS,

        // ── Session ──
        getSession: function () {
            try {
                var raw = localStorage.getItem(KEYS.SESSION);
                return raw ? JSON.parse(raw) : null;
            } catch (e) {
                return null;
            }
        },

        setSession: function (session) {
            localStorage.setItem(KEYS.SESSION, JSON.stringify(session));
        },

        clearSession: function () {
            localStorage.removeItem(KEYS.SESSION);
        },

        // ── Theme ──
        getTheme: function () {
            return localStorage.getItem(KEYS.THEME) || 'light';
        },

        setTheme: function (theme) {
            localStorage.setItem(KEYS.THEME, theme);
        },

        // ── Remember Email ──
        getRememberEmail: function () {
            return localStorage.getItem(KEYS.REMEMBER_EMAIL) || '';
        },

        setRememberEmail: function (email) {
            if (email) {
                localStorage.setItem(KEYS.REMEMBER_EMAIL, email);
            } else {
                localStorage.removeItem(KEYS.REMEMBER_EMAIL);
            }
        }
    };
})();

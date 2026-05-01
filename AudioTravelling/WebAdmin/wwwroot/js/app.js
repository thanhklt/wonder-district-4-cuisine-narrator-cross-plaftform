/**
 * ═══════════════════════════════════════════════════
 * Audio Travelling — App Bootstrap
 *
 * This file ONLY bootstraps the application.
 * All business logic lives in the modular JS files.
 * ═══════════════════════════════════════════════════
 */
document.addEventListener('DOMContentLoaded', function () {
    'use strict';

    var AT = window.AudioTravelling;
    if (!AT || !AT.Core) {
        console.error('AudioTravelling modules not loaded. Check script order in _Layout.cshtml.');
        return;
    }

    // 1. Initialise theme
    AT.Core.UI.initTheme();

    // 2. Initialise auth (checks existing session, binds login form)
    AT.Core.Auth.init();

    // 3. Ripple effect for buttons
    document.addEventListener('click', function (e) {
        var btn = e.target.closest('.btn-ripple');
        if (btn) {
            var ripple = document.createElement('span');
            ripple.className = 'ripple-effect';
            var rect = btn.getBoundingClientRect();
            var size = Math.max(rect.width, rect.height);
            ripple.style.width = ripple.style.height = size + 'px';
            ripple.style.left = (e.clientX - rect.left - size / 2) + 'px';
            ripple.style.top = (e.clientY - rect.top - size / 2) + 'px';
            btn.appendChild(ripple);
            setTimeout(function () { ripple.remove(); }, 600);
        }
    });
});
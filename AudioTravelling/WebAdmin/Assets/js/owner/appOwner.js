/**
 * ═══════════════════════════════════════════════════
 * Audio Travelling — Owner Portal Bootstrap
 *
 * Runs on all Owner portal pages (_OwnerLayout).
 * - Initialises theme
 * - Updates sidebar user info
 * - Sets sidebar active state based on current URL
 * - Binds logout button
 * - Ripple effect for buttons
 * ═══════════════════════════════════════════════════
 */
document.addEventListener('DOMContentLoaded', function () {
    'use strict';

    var AT = window.AudioTravelling;
    if (!AT || !AT.Core) {
        console.error('AudioTravelling modules not loaded. Check script order in _OwnerLayout.cshtml.');
        return;
    }

    // 1. Initialise theme
    AT.Core.UI.initTheme();

    // 2. Update sidebar user info from session
    var session = AT.Core.Storage.getSession();
    if (session) {
        AT.Core.UI.updateSidebarUser(session.name);
    }

    // 3. Set sidebar active state based on current URL
    var currentPath = window.location.pathname.toLowerCase().replace(/\/$/, '') || '/owner';
    var navLinks = document.querySelectorAll('#sidebar-nav .nav-link');
    navLinks.forEach(function (link) {
        var linkPath = (link.getAttribute('data-path') || link.getAttribute('href') || '').toLowerCase().replace(/\/$/, '');
        link.classList.remove('active');

        // Exact match or default dashboard match
        if (linkPath === currentPath) {
            link.classList.add('active');
        } else if (currentPath === '/owner' && linkPath === '/owner') {
            link.classList.add('active');
        } else if (currentPath === '/owner/dashboard' && linkPath === '/owner') {
            link.classList.add('active');
        }
    });

    // 4. Bind logout button
    var logoutBtn = document.getElementById('btn-logout');
    if (logoutBtn) {
        logoutBtn.addEventListener('click', function () {
            AT.Core.Storage.clearSession();
            window.location.href = '/';
        });
    }

    // 5. Ripple effect for buttons
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

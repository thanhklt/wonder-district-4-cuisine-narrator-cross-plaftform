// Settings module bootstrap
(function () {
    function boot() {
        window.initSettingsModule = function () {
            try {
                if (typeof window.loadProfileData === 'function') window.loadProfileData();
                if (typeof window.loadTierConfigUI === 'function') window.loadTierConfigUI();
            } catch (e) { console.warn('initSettingsModule error', e); }
        };
    }

    if (document.readyState === 'complete' || document.readyState === 'interactive') boot();
    else document.addEventListener('DOMContentLoaded', boot);
})();

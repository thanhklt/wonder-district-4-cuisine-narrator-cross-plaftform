// Subscription (plans) module bootstrap
(function () {
    function boot() {
        window.initSubscriptionModule = function () {
            try {
                if (typeof window.renderPlans === 'function') window.renderPlans();
            } catch (e) { console.warn('initSubscriptionModule error', e); }
        };
    }

    if (document.readyState === 'complete' || document.readyState === 'interactive') boot();
    else document.addEventListener('DOMContentLoaded', boot);
})();

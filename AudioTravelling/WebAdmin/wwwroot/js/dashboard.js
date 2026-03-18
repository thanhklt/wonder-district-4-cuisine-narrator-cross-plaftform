// Dashboard module bootstrap
(function () {
    function ensureReady() {
        return !!(window.renderDashboard || window.initRevenueChart);
    }

    function boot() {
        // expose init that the main app can call
        window.initDashboardModule = function () {
            try {
                if (typeof window.renderDashboard === 'function') {
                    window.renderDashboard();
                } else if (typeof window.initRevenueChart === 'function') {
                    // fallback: initialize revenue chart directly
                    const booths = window.getBooths ? window.getBooths() : [];
                    const orders = window.getOrders ? window.getOrders() : [];
                    const tierCfg = window.getTierConfig ? window.getTierConfig() : {};
                    window.initRevenueChart && window.initRevenueChart(orders, tierCfg, booths);
                }
            } catch (e) { console.warn('initDashboardModule error', e); }
        };
    }

    if (document.readyState === 'complete' || document.readyState === 'interactive') boot();
    else document.addEventListener('DOMContentLoaded', boot);
})();

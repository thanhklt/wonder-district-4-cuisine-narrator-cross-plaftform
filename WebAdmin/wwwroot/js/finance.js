// Finance module bootstrap
(function () {
    function ensureReady() {
        return !!(window.getBooths && window.getOrders && window.renderFinanceData);
    }

    function boot() {
        // expose a helper that the app can call when navigating to finance view
        window._financeBoot = function () {
            const financeSearchInput = document.getElementById('finance-booth-search');
            const financeSearchResults = document.getElementById('finance-search-results');
            const financeSearchClear = document.getElementById('finance-search-clear');
            let selectedFinanceBoothId = 'all';

            function populateFinanceBoothSelect() {
                selectedFinanceBoothId = 'all';
                if (financeSearchInput) financeSearchInput.value = '';
                financeSearchClear?.classList.add('hidden');
                financeSearchResults?.classList.add('hidden');
                renderFinanceData('all');
            }

            function escapeHtml(str) {
                return (str || '').replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;');
            }

            function escapeRegex(str) {
                return (str || '').replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
            }

            function showSearchResults(query) {
                const booths = getBooths();
                const orders = getOrders();
                if (!financeSearchResults) return;

                let html = '';
                html += `\n                    <div class="booth-search-all" data-booth-id="all">\n                        <i class="fa-solid fa-layer-group"></i> Show All Booths\n                    </div>\n                `;

                const filtered = query
                    ? booths.filter(b => (b.name || '').toLowerCase().includes(query) || (b.company || '').toLowerCase().includes(query))
                    : booths;

                if (filtered.length === 0) {
                    html += `\n                        <div class="booth-search-empty">\n                            <i class="fa-solid fa-magnifying-glass"></i>\n                            No booths found for "${escapeHtml(query)}"\n                        </div>\n                    `;
                } else {
                    filtered.forEach(b => {
                        const boothOrders = orders.filter(o => o.boothId === b.id && o.status === 'success');
                        const tierClass = 'tier-' + (b.tier || 'basic').toLowerCase();
                        let nameHtml = escapeHtml(b.name);
                        if (query) {
                            const regex = new RegExp(`(${escapeRegex(query)})`, 'gi');
                            nameHtml = escapeHtml(b.name).replace(regex, '<mark>$1</mark>');
                        }
                        const isActive = selectedFinanceBoothId === b.id ? ' active' : '';
                        html += `\n                            <div class="booth-search-item${isActive}" data-booth-id="${b.id}">\n                                <div class="booth-search-item-info">\n                                    <div class="booth-search-item-name">${nameHtml}</div>\n                                    <div class="booth-search-item-company">${escapeHtml(b.company)} · ${boothOrders.length} orders</div>\n                                </div>\n                                <span class="tier-badge ${tierClass}" style="font-size:10px; padding:2px 8px;">${b.tier}</span>\n                            </div>\n                        `;
                    });
                }

                financeSearchResults.innerHTML = html;
                financeSearchResults.classList.remove('hidden');

                financeSearchResults.querySelectorAll('[data-booth-id]').forEach(el => {
                    el.addEventListener('click', () => {
                        const id = el.getAttribute('data-booth-id');
                        selectFinanceBooth(id);
                    });
                });
            }

            function selectFinanceBooth(id) {
                selectedFinanceBoothId = id;
                financeSearchResults.classList.add('hidden');
                if (id === 'all') {
                    if (financeSearchInput) financeSearchInput.value = '';
                    financeSearchClear?.classList.add('hidden');
                } else {
                    const booths = getBooths();
                    const booth = booths.find(b => b.id === id);
                    if (booth) {
                        if (financeSearchInput) financeSearchInput.value = booth.name;
                        financeSearchClear?.classList.remove('hidden');
                    }
                }
                renderFinanceData(id);
            }

            if (financeSearchInput) {
                financeSearchInput.addEventListener('input', () => {
                    const query = (financeSearchInput.value || '').trim().toLowerCase();
                    if (query.length > 0) financeSearchClear?.classList.remove('hidden'); else financeSearchClear?.classList.add('hidden');
                    showSearchResults(query);
                });
                financeSearchInput.addEventListener('focus', () => showSearchResults((financeSearchInput.value || '').trim().toLowerCase()));
            }

            financeSearchClear?.addEventListener('click', () => {
                if (financeSearchInput) financeSearchInput.value = '';
                financeSearchClear.classList.add('hidden');
                renderFinanceData('all');
                financeSearchResults?.classList.add('hidden');
            });

            document.addEventListener('click', e => {
                if (!e.target.closest('.booth-search-wrapper')) {
                    financeSearchResults?.classList.add('hidden');
                }
            });

            // initial populate when finance view is opened
            populateFinanceBoothSelect();
        };

        // Auto-run when DOM ready
        if (document.readyState === 'complete' || document.readyState === 'interactive') {
            // nothing — wait for app to call _financeBoot when appropriate
        } else {
            document.addEventListener('DOMContentLoaded', function () { /* no-op */ });
        }
    }

    // Wait until renderFinanceData exists
    (function waitForApp() {
        let tries = 0;
        const t = setInterval(() => {
            if (window.renderFinanceData && window.getBooths && window.getOrders) {
                clearInterval(t);
                boot();
            } else if (++tries > 20) {
                clearInterval(t);
                console.warn('finance.js: main app did not expose required functions in time');
            }
        }, 150);
    })();
})();

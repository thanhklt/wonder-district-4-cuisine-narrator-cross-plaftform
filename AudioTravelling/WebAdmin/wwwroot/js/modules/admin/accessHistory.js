/**
 * Audio Travelling — Access History Module (Admin)
 */
(function () {
    'use strict';
    var AT = window.AudioTravelling = window.AudioTravelling || {};
    AT.Modules = AT.Modules || {};
    var Fmt = AT.Utils.Formatters;

    AT.Modules.initAccessHistory = function () {
        renderAccessHistory();
        bindFilter();
    };

    function renderAccessHistory(fromDate, toDate) {
        AT.Services.AccessStats.getAccessHistory(fromDate, toDate).then(function (history) {
            var countEl = document.getElementById('history-count');
            if (countEl) countEl.textContent = history.length + ' bản ghi';

            var tbody = document.getElementById('access-history-body');
            if (!tbody) return;
            if (history.length === 0) {
                tbody.innerHTML = '<tr><td colspan="6" style="text-align:center;padding:20px;color:var(--text-dim);">Không có lịch sử truy cập</td></tr>';
                return;
            }
            tbody.innerHTML = history.map(function (h) {
                return '<tr style="border-bottom:1px solid var(--border);">' +
                    '<td style="padding:10px;font-size:13px;">' + Fmt.dateTime(h.timestamp) + '</td>' +
                    '<td style="padding:10px;font-size:13px;">' + h.qrLabel + '</td>' +
                    '<td style="padding:10px;font-size:13px;">' + (h.poiName || '—') + '</td>' +
                    '<td style="padding:10px;font-size:13px;">' + h.area + '</td>' +
                    '<td style="padding:10px;font-size:13px;">' + h.device + '</td>' +
                    '<td style="padding:10px;font-size:13px;">' + (h.duration || '—') + '</td>' +
                    '</tr>';
            }).join('');
        });
    }

    var _filterBound = false;
    function bindFilter() {
        if (_filterBound) return;
        _filterBound = true;
        var btnFilter = document.getElementById('btn-filter-history');
        if (btnFilter) {
            btnFilter.addEventListener('click', function () {
                var from = document.getElementById('filter-history-from');
                var to = document.getElementById('filter-history-to');
                renderAccessHistory(from ? from.value : null, to ? to.value : null);
            });
        }
    }
})();

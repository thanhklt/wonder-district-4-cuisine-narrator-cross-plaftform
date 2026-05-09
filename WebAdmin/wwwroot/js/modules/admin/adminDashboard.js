/**
 * Audio Travelling — Admin Dashboard Module
 */
(function () {
    'use strict';
    var AT = window.AudioTravelling = window.AudioTravelling || {};
    AT.Modules = AT.Modules || {};

    var _chartInstance = null;

    AT.Modules.initAdminDashboard = function () {
        loadSummaryStats();
        loadDailyScansChart();
    };

    function loadSummaryStats() {
        AT.Services.AccessStats.getDashboardStats().then(function (stats) {
            var el;
            el = document.getElementById('stat-active-users');  if (el) el.textContent = stats.activeUsers  || 0;
            el = document.getElementById('stat-total-scans');   if (el) el.textContent = stats.rejectedPOIs || 0;
            el = document.getElementById('stat-total-pois');    if (el) el.textContent = stats.approvedPOIs || 0;
            el = document.getElementById('stat-pending-pois');  if (el) el.textContent = stats.pendingPOIs  || 0;
        }).catch(function (err) {
            console.error('[AdminDashboard] Stats error:', err);
        });
    }

    function loadDailyScansChart() {
        AT.Services.AccessStats.getDailyScans(7).then(function (data) {
            if (!data || !data.length) return;

            var labels = data.map(function (d) { return d.date; });
            var counts = data.map(function (d) { return d.count; });
            var total  = counts.reduce(function (a, b) { return a + b; }, 0);

            var badge = document.getElementById('chart-total-badge');
            if (badge) badge.textContent = total + ' lượt';

            var canvas = document.getElementById('daily-scans-chart');
            if (!canvas) return;

            if (_chartInstance) { _chartInstance.destroy(); _chartInstance = null; }

            var ctx = canvas.getContext('2d');

            // Gradient fill dưới đường
            var gradient = ctx.createLinearGradient(0, 0, 0, 260);
            gradient.addColorStop(0,   'rgba(74,144,226,0.35)');
            gradient.addColorStop(1,   'rgba(74,144,226,0.01)');

            _chartInstance = new Chart(ctx, {
                type: 'line',
                data: {
                    labels: labels,
                    datasets: [{
                        label: 'Lượt quét',
                        data:  counts,
                        fill:  true,
                        backgroundColor: gradient,
                        borderColor:     '#4A90E2',
                        borderWidth:     2.5,
                        pointBackgroundColor: '#4A90E2',
                        pointBorderColor:     '#fff',
                        pointBorderWidth:     2,
                        pointRadius:          5,
                        pointHoverRadius:     7,
                        tension:              0.35
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    interaction: { mode: 'index', intersect: false },
                    plugins: {
                        legend: { display: false },
                        tooltip: {
                            backgroundColor: '#1e2a3a',
                            titleColor: '#aaa',
                            bodyColor:  '#fff',
                            borderColor: '#4A90E2',
                            borderWidth: 1,
                            padding: 10,
                            callbacks: {
                                label: function (ctx) {
                                    return '  ' + ctx.parsed.y + ' phiên';
                                }
                            }
                        }
                    },
                    scales: {
                        x: {
                            grid: { color: 'rgba(255,255,255,0.06)' },
                            ticks: { color: '#888', font: { size: 12 } }
                        },
                        y: {
                            beginAtZero: true,
                            grid: { color: 'rgba(255,255,255,0.06)' },
                            ticks: {
                                color: '#888',
                                font: { size: 12 },
                                stepSize: 1,
                                precision: 0
                            }
                        }
                    }
                }
            });
        }).catch(function (err) {
            console.error('[AdminDashboard] Chart error:', err);
        });
    }
})();

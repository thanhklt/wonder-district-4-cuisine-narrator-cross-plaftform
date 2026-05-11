
/**
 * Audio Travelling — Admin Dashboard Module
 */
(function () {
    'use strict';
    var AT = window.AudioTravelling = window.AudioTravelling || {};
    AT.Modules = AT.Modules || {};

    AT.Modules.initAdminDashboard = function () {
        AT.Services.AccessStats.getDashboardStats().then(function (stats) {
            console.log('[AdminDashboard] Stats:', stats);
            var el = document.getElementById('stat-active-users');
            if (el) el.textContent = stats.activeUsers || 0;
            var elScans = document.getElementById('stat-total-scans');
            if (elScans) elScans.textContent = stats.rejectedPOIs || 0;
            var elPois = document.getElementById('stat-total-pois');
            if (elPois) elPois.textContent = stats.approvedPOIs || 0;
            var elPending = document.getElementById('stat-pending-pois');
            if (elPending) elPending.textContent = stats.pendingPOIs || 0;
        }).catch(function (err) {
            console.error('[AdminDashboard] Error loading stats:', err);
        });

        var canvas = document.getElementById('daily-scans-chart');
        if (canvas) {
            AT.Services.AccessStats.getDailyScans(7).then(function (data) {
                var total = data.reduce(function (sum, d) { return sum + d.count; }, 0);
                var elTotal = document.getElementById('stat-7day-total');
                if (elTotal) elTotal.textContent = total;

                new Chart(canvas, {
                    type: 'line',
                    data: {
                        labels: data.map(function (d) { return d.date; }),
                        datasets: [{
                            label: 'Lượt quét',
                            data: data.map(function (d) { return d.count; }),
                            borderColor: '#22c55e',
                            backgroundColor: 'rgba(34, 197, 94, 0.08)',
                            fill: true,
                            tension: 0.4,
                            pointBackgroundColor: '#22c55e',
                            pointBorderColor: '#152218',
                            pointBorderWidth: 2,
                            pointRadius: 5,
                            pointHoverRadius: 7
                        }]
                    },
                    options: {
                        responsive: true,
                        maintainAspectRatio: true,
                        plugins: {
                            legend: { display: false },
                            tooltip: {
                                backgroundColor: '#1a2e1e',
                                borderColor: 'rgba(34,197,94,0.3)',
                                borderWidth: 1,
                                titleColor: '#a7f3d0',
                                bodyColor: '#f0fdf4',
                                callbacks: {
                                    label: function (ctx) { return ' ' + (ctx.parsed.y * 100) + ' lượt quét'; }
                                }
                            }
                        },
                        scales: {
                            x: {
                                grid: { color: 'rgba(34,197,94,0.06)' },
                                ticks: { color: '#6b8f7a', font: { size: 12 } }
                            },
                            y: {
                                beginAtZero: true,
                                grid: { color: 'rgba(34,197,94,0.06)' },
                                ticks: {
                                    color: '#6b8f7a',
                                    font: { size: 12 },
                                    precision: 0,
                                    stepSize: 1
                                }
                            }
                        }
                    }
                });
            }).catch(function (err) {
                console.warn('[AdminDashboard] daily-scans error:', err);
            });
        }
    };
})();
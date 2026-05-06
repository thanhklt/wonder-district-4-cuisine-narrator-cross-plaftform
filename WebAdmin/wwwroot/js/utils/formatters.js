/**
 * Audio Travelling — Formatters
 */
(function () {
    'use strict';
    var AT = window.AudioTravelling = window.AudioTravelling || {};
    AT.Utils = AT.Utils || {};

    AT.Utils.Formatters = {
        currency: function (value) {
            return new Intl.NumberFormat('vi-VN').format(value || 0);
        },
        time: function (sec) {
            var min = Math.floor(sec / 60);
            var s = Math.floor(sec % 60);
            return min + ':' + (s < 10 ? '0' : '') + s;
        },
        dateTime: function (isoStr) {
            if (!isoStr) return '—';
            var d = new Date(isoStr);
            return d.toLocaleDateString('vi-VN') + ' ' + d.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });
        },
        date: function (isoStr) {
            if (!isoStr) return '—';
            return new Date(isoStr).toLocaleDateString('vi-VN');
        },
        relativeTime: function (isoStr) {
            if (!isoStr) return '—';
            var diff = Date.now() - new Date(isoStr).getTime();
            var mins = Math.floor(diff / 60000);
            if (mins < 1) return 'Vừa xong';
            if (mins < 60) return mins + ' phút trước';
            var hrs = Math.floor(mins / 60);
            if (hrs < 24) return hrs + ' giờ trước';
            return Math.floor(hrs / 24) + ' ngày trước';
        }
    };
})();

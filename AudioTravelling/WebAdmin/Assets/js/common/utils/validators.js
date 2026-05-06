/**
 * Audio Travelling — Validators
 */
(function () {
    'use strict';
    var AT = window.AudioTravelling = window.AudioTravelling || {};
    AT.Utils = AT.Utils || {};

    AT.Utils.Validators = {
        isEmail: function (str) {
            return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(str);
        },
        isNotEmpty: function (str) {
            return !!(str && str.trim().length > 0);
        },
        isValidLatLng: function (lat, lng) {
            return lat >= -90 && lat <= 90 && lng >= -180 && lng <= 180;
        }
    };
})();

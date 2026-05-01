/**
 * Audio Travelling — Create / Edit POI Module (Owner)
 */
(function () {
    'use strict';
    var AT = window.AudioTravelling = window.AudioTravelling || {};
    AT.Modules = AT.Modules || {};
    var UI = AT.Core.UI;

    AT.Modules._editPoiId = null;

    AT.Modules.initCreatePOI = function () {
        var editId = AT.Modules._editPoiId;
        var formTitle = document.getElementById('create-poi-title');

        if (editId) {
            // Edit mode — load existing POI data
            if (formTitle) formTitle.textContent = 'Chỉnh sửa POI';
            var poi = AT.Mocks.POIs.find(function (p) { return p.id === editId; });
            if (poi) {
                setField('poi-name', poi.name);
                setField('poi-description', poi.description);
                setField('poi-address', poi.address);
                setField('poi-lat', poi.lat);
                setField('poi-lng', poi.lng);
            }
        } else {
            // Create mode — clear form
            if (formTitle) formTitle.textContent = 'Tạo POI mới';
            clearForm();
        }

        bindEvents();
    };

    function setField(id, value) {
        var el = document.getElementById(id);
        if (el) el.value = value || '';
    }

    function clearForm() {
        ['poi-name', 'poi-description', 'poi-address', 'poi-lat', 'poi-lng'].forEach(function (id) {
            setField(id, '');
        });
    }

    function getFormData() {
        var session = AT.Core.Auth.getSession();
        return {
            name: (document.getElementById('poi-name') || {}).value || '',
            description: (document.getElementById('poi-description') || {}).value || '',
            address: (document.getElementById('poi-address') || {}).value || '',
            lat: parseFloat((document.getElementById('poi-lat') || {}).value) || 0,
            lng: parseFloat((document.getElementById('poi-lng') || {}).value) || 0,
            owner: session ? session.email : ''
        };
    }

    var _eventsBound = false;
    function bindEvents() {
        if (_eventsBound) return;
        _eventsBound = true;

        var btnSave = document.getElementById('btn-save-poi');
        if (btnSave) {
            btnSave.addEventListener('click', function () {
                var data = getFormData();
                if (!data.name.trim()) {
                    UI.showToast('Vui lòng nhập tên POI', 'error');
                    return;
                }
                var editId = AT.Modules._editPoiId;
                if (editId) {
                    AT.Services.POI.update(editId, data).then(function () {
                        UI.showToast('Đã cập nhật POI', 'success');
                        AT.Modules._editPoiId = null;
                        AT.Core.Router.navigate('view-my-pois');
                    });
                } else {
                    AT.Services.POI.create(data).then(function (poi) {
                        UI.showToast('Đã tạo POI ' + poi.id, 'success');
                        clearForm();
                        AT.Core.Router.navigate('view-my-pois');
                    });
                }
            });
        }

        var btnSubmit = document.getElementById('btn-save-and-submit-poi');
        if (btnSubmit) {
            btnSubmit.addEventListener('click', function () {
                var data = getFormData();
                if (!data.name.trim()) {
                    UI.showToast('Vui lòng nhập tên POI', 'error');
                    return;
                }
                var editId = AT.Modules._editPoiId;
                var savePromise = editId
                    ? AT.Services.POI.update(editId, data)
                    : AT.Services.POI.create(data);

                savePromise.then(function (poi) {
                    return AT.Services.POI.submitForApproval(poi.id);
                }).then(function () {
                    UI.showToast('Đã lưu và gửi yêu cầu duyệt!', 'success');
                    AT.Modules._editPoiId = null;
                    clearForm();
                    AT.Core.Router.navigate('view-my-pois');
                });
            });
        }

        var btnCancel = document.getElementById('btn-cancel-poi');
        if (btnCancel) {
            btnCancel.addEventListener('click', function () {
                AT.Modules._editPoiId = null;
                clearForm();
                AT.Core.Router.navigate('view-my-pois');
            });
        }
    }
})();

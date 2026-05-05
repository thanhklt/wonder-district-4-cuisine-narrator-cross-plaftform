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

        // Load packages for dropdown
        loadPackages();

        if (editId) {
            // Edit mode — load existing POI data from API
            if (formTitle) formTitle.textContent = 'Chỉnh sửa POI';
            AT.Services.POI.getByOwner().then(function (pois) {
                var poi = (pois || []).find(function (p) { return (p.poiId || p.id) == editId; });
                if (poi) {
                    setField('poi-name', poi.poiName || poi.name);
                    setField('poi-description', poi.descriptionVi || poi.description);
                    setField('poi-image', poi.imageUrl || '');
                    setField('poi-lat', poi.latitude || poi.lat);
                    setField('poi-lng', poi.longitude || poi.lng);
                    // Set package dropdown
                    var pkgSelect = document.getElementById('poi-package');
                    if (pkgSelect) pkgSelect.value = poi.packageId || '';
                }
            });
        } else {
            // Create mode — clear form
            if (formTitle) formTitle.textContent = 'Tạo POI mới';
            clearForm();
        }

        bindEvents();
    };

    function loadPackages() {
        var pkgSelect = document.getElementById('poi-package');
        if (!pkgSelect) return;

        AT.Services.Package.getAll().then(function (packages) {
            if (!packages || packages.length === 0) return;
            // Keep existing options (like a placeholder) and add packages
            var existingValue = pkgSelect.value;
            var optionsHtml = '<option value="">-- Chọn gói --</option>';
            packages.forEach(function (pkg) {
                var pkgId = pkg.packageId || pkg.id;
                var name = pkg.name || '';
                var radius = pkg.radius || 0;
                optionsHtml += '<option value="' + pkgId + '">' + name + ' (' + radius + 'm)</option>';
            });
            pkgSelect.innerHTML = optionsHtml;
            if (existingValue) pkgSelect.value = existingValue;
        }).catch(function (err) {
            console.warn('[CreatePOI] Error loading packages:', err);
        });
    }

    function setField(id, value) {
        var el = document.getElementById(id);
        if (el) el.value = value || '';
    }

    function clearForm() {
        ['poi-name', 'poi-description', 'poi-image', 'poi-lat', 'poi-lng'].forEach(function (id) {
            setField(id, '');
        });
        var pkgSelect = document.getElementById('poi-package');
        if (pkgSelect) pkgSelect.value = '';
    }

    function getFormData() {
        var formData = new FormData();
        var name = (document.getElementById('poi-name') || {}).value || '';
        var packageId = parseInt((document.getElementById('poi-package') || {}).value, 10) || 0;
        
        formData.append('poiName', name);
        formData.append('descriptionVi', (document.getElementById('poi-description') || {}).value || '');
        formData.append('latitude', parseFloat((document.getElementById('poi-lat') || {}).value) || 0);
        formData.append('longitude', parseFloat((document.getElementById('poi-lng') || {}).value) || 0);
        formData.append('packageId', packageId);
        
        var imgFile = document.getElementById('poi-image');
        if (imgFile && imgFile.files && imgFile.files[0]) {
            formData.append('imageFile', imgFile.files[0]);
        }
        
        return formData;
    }

    var _eventsBound = false;
    function bindEvents() {
        if (_eventsBound) return;
        _eventsBound = true;

        var btnSave = document.getElementById('btn-save-poi');
        if (btnSave) {
            btnSave.addEventListener('click', function () {
                var data = getFormData();
                if (!data.get('poiName').trim()) {
                    UI.showToast('Vui lòng nhập tên POI', 'error');
                    return;
                }
                if (!data.get('packageId') || data.get('packageId') === '0') {
                    UI.showToast('Vui lòng chọn gói', 'error');
                    return;
                }
                var editId = AT.Modules._editPoiId;
                if (editId) {
                    AT.Services.POI.update(editId, data).then(function () {
                        UI.showToast('Đã cập nhật POI', 'success');
                        AT.Modules._editPoiId = null;
                        AT.Core.Router.navigate('view-my-pois');
                    }).catch(function (err) {
                        UI.showToast('Lỗi khi cập nhật POI', 'error');
                    });
                } else {
                    AT.Services.POI.create(data).then(function (result) {
                        var poiId = result && (result.poiId || result.id) || '';
                        UI.showToast('Đã tạo POI ' + poiId, 'success');
                        clearForm();
                        AT.Core.Router.navigate('view-my-pois');
                    }).catch(function (err) {
                        UI.showToast('Lỗi khi tạo POI', 'error');
                    });
                }
            });
        }

        var btnSubmit = document.getElementById('btn-save-and-submit-poi');
        if (btnSubmit) {
            btnSubmit.addEventListener('click', function () {
                var data = getFormData();
                if (!data.get('poiName').trim()) {
                    UI.showToast('Vui lòng nhập tên POI', 'error');
                    return;
                }
                if (!data.get('packageId') || data.get('packageId') === '0') {
                    UI.showToast('Vui lòng chọn gói', 'error');
                    return;
                }
                var editId = AT.Modules._editPoiId;
                var savePromise = editId
                    ? AT.Services.POI.update(editId, data)
                    : AT.Services.POI.create(data);

                savePromise.then(function (result) {
                    var poiId = editId || (result && (result.poiId || result.id)) || '';
                    return AT.Services.POI.submitForApproval(poiId);
                }).then(function () {
                    UI.showToast('Đã lưu và gửi yêu cầu duyệt!', 'success');
                    AT.Modules._editPoiId = null;
                    clearForm();
                    AT.Core.Router.navigate('view-my-pois');
                }).catch(function (err) {
                    UI.showToast('Lỗi: ' + (err.message || 'Không thể lưu POI'), 'error');
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

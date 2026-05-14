/**
 * Audio Travelling — Admin Manage POIs Module
 */
(function () {
    'use strict';
    var AT = window.AudioTravelling = window.AudioTravelling || {};
    AT.Modules = AT.Modules || {};
    var Fmt = AT.Utils.Formatters;
    var UI = AT.Core.UI;

    var _poiCache = [];
    var _packagesCache = [];

    AT.Modules.initManagePois = function () {
        loadAdminPois();
        loadPackagesForDropdown();
        bindEvents();
    };

    function loadPackagesForDropdown() {
        AT.Services.Package.getAll().then(function (packages) {
            _packagesCache = packages || [];
            var select = document.getElementById('edit-poi-package');
            if (!select) return;
            var html = '<option value="">-- Chọn gói --</option>';
            _packagesCache.forEach(function (pkg) {
                var id   = pkg.packageId || pkg.id || pkg.PackageId;
                var name = pkg.name || pkg.Name || '';
                var radius = pkg.radius || pkg.Radius || 0;
                html += '<option value="' + id + '">' + name + ' (' + radius + 'm)</option>';
            });
            select.innerHTML = html;
        }).catch(function (err) {
            console.warn('[ManagePois] Failed to load packages:', err);
        });
    }

    function loadAdminPois() {
        AT.Services.POI.getAll().then(function (pois) {
            console.log('[ManagePois] Loaded POIs:', pois);
            _poiCache = pois || [];
            renderAdminPoisTable(_poiCache);
        }).catch(function (err) {
            console.error('[ManagePois] Error loading POIs:', err);
            UI.showToast('Lỗi khi tải danh sách POI', 'error');
        });
    }

    function renderAdminPoisTable(pois) {
        var tbody = document.getElementById('manage-pois-body');
        if (!tbody) return;

        var statusFilter = document.getElementById('filter-poi-status').value;
        var activeFilter = document.getElementById('filter-poi-active') ? document.getElementById('filter-poi-active').value : '';
        var searchInput = document.getElementById('search-poi').value.toLowerCase();

        var filtered = pois.filter(function (p) {
            var matchStatus = !statusFilter || String(p.status || p.Status || '').toLowerCase() === statusFilter.toLowerCase();
            var isActive = p.isActive ?? p.IsActive;
            var matchActive = !activeFilter || (activeFilter === 'active' && isActive) || (activeFilter === 'inactive' && !isActive);
            var searchStr = (p.poiName || p.PoiName || '') + ' ' + (p.ownerName || p.OwnerName || '') + ' ' + (p.poiId || p.PoiId || '');
            var matchSearch = !searchInput || searchStr.toLowerCase().indexOf(searchInput) > -1;
            return matchStatus && matchSearch && matchActive;
        });

        if (filtered.length === 0) {
            tbody.innerHTML = '<tr><td colspan="7" style="text-align:center;padding:20px;color:var(--text-dim);">Không có POI nào</td></tr>';
            return;
        }

        tbody.innerHTML = filtered.map(function (p) {
            var poiId = p.poiId ?? p.PoiId ?? p.id ?? '';
            var poiName = p.poiName ?? p.PoiName ?? p.name ?? 'Không có tên';
            var desc = p.descriptionVi ?? p.DescriptionVi ?? '';
            var ownerName = p.ownerName ?? p.OwnerName ?? ('ID: ' + (p.ownerId ?? p.OwnerId));
            var packageName = p.packageName ?? p.PackageName ?? ('ID: ' + (p.packageId ?? p.PackageId));
            var status = String(p.status ?? p.Status ?? '').toLowerCase();
            var statusText = p.statusText ?? p.StatusText ?? p.status ?? p.Status ?? status;
            var isActive = p.isActive ?? p.IsActive;
            var lat = p.latitude ?? p.Latitude ?? 0;
            var lng = p.longitude ?? p.Longitude ?? 0;
            var radius = p.radius ?? p.Radius ?? 0;
            var priority = p.priority ?? p.Priority ?? 0;
            var createdDate = p.createdDate ?? p.CreatedDate ?? '';
            var updatedDate = p.updatedDate ?? p.UpdatedDate ?? p.updatedAt ?? '';

            var activeBadge = isActive ? '<span class="status-badge status-approved">Đang hoạt động</span>' : '<span class="status-badge status-rejected">Đã tắt</span>';
            var toggleText = isActive ? 'Tắt' : 'Bật';
            var toggleIcon = isActive ? 'fa-toggle-on' : 'fa-toggle-off';

            var rawUrl = p.imageUrl || p.ImageUrl ||
                           (Array.isArray(p.images) && p.images.length > 0 ? (p.images[0].imageUrl || p.images[0]) : null) ||
                           (Array.isArray(p.Images) && p.Images.length > 0 ? (p.Images[0].imageUrl || p.Images[0]) : null) ||
                           null;
            var imageUrl = AT.Core.ApiClient.resolveImageUrl(rawUrl);

            return '<tr style="border-bottom:1px solid var(--border);">' +
                '<td style="padding:16px 12px;font-weight:600;font-size:13px;">' + poiId + '</td>' +
                '<td style="padding:16px 12px;font-size:13px;font-weight:500;">' +
                    '<div style="display:flex;align-items:center;gap:12px;">' +
                        '<img data-api-src="' + rawUrl + '" src="/images/placeholder-poi.png" style="width:40px;height:40px;border-radius:6px;object-fit:cover;" alt="">' +
                        poiName +
                    '</div>' +
                '</td>' +
                '<td style="padding:16px 12px;font-size:13px;max-width:150px;white-space:nowrap;overflow:hidden;text-overflow:ellipsis;" title="' + desc + '">' + desc + '</td>' +
                '<td style="padding:16px 12px;font-size:13px;">' + ownerName + '</td>' +
                '<td style="padding:16px 12px;font-size:13px;">' + packageName + '</td>' +
                '<td style="padding:16px 12px;"><span class="status-badge status-' + status + '">' + statusText + '</span></td>' +
                '<td style="padding:16px 12px;">' + activeBadge + '</td>' +
                '<td style="padding:16px 12px;font-size:13px;">' + lat + ',<br>' + lng + '</td>' +
                '<td style="padding:16px 12px;font-size:13px;">' + radius + 'm / P' + priority + '</td>' +
                '<td style="padding:16px 12px;font-size:13px;">' + Fmt.date(createdDate) + '</td>' +
                '<td style="padding:16px 12px;font-size:13px;">' + Fmt.date(updatedDate) + '</td>' +
                '<td style="padding:16px 12px;text-align:center;">' +
                    '<button class="btn-ghost btn-edit-poi" data-poi-id="' + poiId + '" style="font-size:11px;padding:5px 8px;margin-right:2px;" title="Sửa">' +
                        '<i class="fa-solid fa-pen"></i></button>' +
                    '<button class="btn-ghost btn-toggle-poi" data-poi-id="' + poiId + '" style="font-size:11px;padding:5px 8px;margin-right:2px;" title="' + toggleText + '">' +
                        '<i class="fa-solid ' + toggleIcon + '"></i></button>' +
                    '<button class="btn-danger-ghost btn-delete-poi" data-poi-id="' + poiId + '" style="font-size:11px;padding:5px 8px;" title="Xóa">' +
                        '<i class="fa-solid fa-trash"></i></button>' +
                '</td></tr>';
        }).join('');

        bindTableEvents(tbody);
        AT.Core.ApiClient.loadAllImages(tbody);
    }

    function bindTableEvents(tbody) {
        tbody.querySelectorAll('.btn-edit-poi').forEach(function (btn) {
            btn.addEventListener('click', function () {
                var poiId = btn.getAttribute('data-poi-id');
                openEditPoiModal(poiId);
            });
        });

        tbody.querySelectorAll('.btn-toggle-poi').forEach(function (btn) {
            btn.addEventListener('click', function () {
                var poiId = btn.getAttribute('data-poi-id');
                togglePoiActive(poiId);
            });
        });

        tbody.querySelectorAll('.btn-delete-poi').forEach(function (btn) {
            btn.addEventListener('click', function () {
                var poiId = btn.getAttribute('data-poi-id');
                if (confirm('Bạn có chắc muốn xóa POI ' + poiId + ' không? Thao tác này không thể hoàn tác.')) {
                    deletePoi(poiId);
                }
            });
        });
    }

    function renderAdminGallery(images, poiId) {
        var gallery = document.getElementById('admin-image-gallery');
        if (!gallery) return;
        var imgs = Array.isArray(images) ? images : [];
        var canAdd = imgs.length < 4;
        var html = imgs.map(function (img) {
            var rawGalleryUrl = (img && img.imageUrl) ? img.imageUrl : null;
            var isCover = img && img.isCover;
            var coverBadge = isCover ? '<span style="position:absolute;top:3px;left:3px;background:#f59e0b;color:#000;font-size:9px;font-weight:700;padding:1px 5px;border-radius:3px;">Bìa</span>' : '';
            var setCoverBtn = isCover ? '' : '<button type="button" onclick="adminSetCover(' + poiId + ',' + img.imageID + ')" style="background:#3b82f6;color:#fff;border:none;border-radius:3px;padding:2px 6px;font-size:10px;cursor:pointer;" title="Đặt bìa"><i class="fa-solid fa-star"></i></button>';
            return '<div style="position:relative;border:1px solid var(--border);border-radius:6px;overflow:hidden;">' +
                '<img data-api-src="' + rawGalleryUrl + '" src="/images/placeholder-poi.png" style="width:100%;height:80px;object-fit:cover;display:block;">' +
                coverBadge +
                '<div style="display:flex;gap:3px;padding:4px;background:rgba(0,0,0,.5);">' +
                setCoverBtn +
                '<button type="button" onclick="adminDeleteImage(' + poiId + ',' + img.imageID + ')" style="background:#ef4444;color:#fff;border:none;border-radius:3px;padding:2px 6px;font-size:10px;cursor:pointer;"><i class="fa-solid fa-trash"></i></button>' +
                '</div></div>';
        }).join('');
        if (canAdd) {
            html += '<div style="border:2px dashed var(--border);border-radius:6px;display:flex;align-items:center;justify-content:center;height:110px;cursor:pointer;" onclick="adminAddImageTrigger(' + poiId + ')">' +
                '<div style="text-align:center;color:var(--text-dim);"><i class="fa-solid fa-plus" style="font-size:18px;"></i><br><span style="font-size:11px;">Thêm ảnh</span></div></div>';
        }
        gallery.innerHTML = html;
        AT.Core.ApiClient.loadAllImages(gallery);
    }

    window.adminDeleteImage = function (poiId, imageId) {
        if (!confirm('Xóa ảnh này?')) return;
        AT.Services.POI.deleteImage('admin', poiId, imageId).then(function () {
            UI.showToast('Đã xóa ảnh', 'success');
            return AT.Services.POI.getById(poiId);
        }).then(function (data) { renderAdminGallery(data.images, poiId); });
    };

    window.adminSetCover = function (poiId, imageId) {
        AT.Services.POI.setCoverImage('admin', poiId, imageId).then(function () {
            UI.showToast('Đã đặt ảnh bìa', 'success');
            return AT.Services.POI.getById(poiId);
        }).then(function (data) { renderAdminGallery(data.images, poiId); });
    };

    window.adminAddImageTrigger = function (poiId) {
        var input = document.getElementById('edit-poi-image-add');
        if (!input) return;
        input._targetPoiId = poiId;
        input.click();
    };

    function openEditPoiModal(id) {
        var imgSection = document.getElementById('admin-image-section');
        var createSection = document.getElementById('admin-create-image-section');
        var createUrlSection = document.getElementById('admin-create-url-section');

        if (!id) {
            document.getElementById('poi-modal-title').innerHTML = '<i class="fa-solid fa-map-location-dot text-accent" style="margin-right:8px;"></i>Tạo POI mới';
            document.getElementById('edit-poi-id').value = '';
            document.getElementById('form-edit-poi').reset();
            document.getElementById('edit-poi-image-url').value = '';
            if (imgSection) imgSection.style.display = 'none';
            if (createSection) createSection.style.display = '';
            if (createUrlSection) createUrlSection.style.display = '';
            UI.showModal('modal-edit-poi');
            return;
        }

        if (imgSection) imgSection.style.display = '';
        if (createSection) createSection.style.display = 'none';
        if (createUrlSection) createUrlSection.style.display = 'none';

        document.getElementById('poi-modal-title').innerHTML = '<i class="fa-solid fa-map-location-dot text-accent" style="margin-right:8px;"></i>Sửa POI';

        function fillModal(data) {
            document.getElementById('edit-poi-id').value = data.poiId || data.id;
            document.getElementById('edit-poi-name').value = data.poiName || data.name || '';
            document.getElementById('edit-poi-desc').value = data.descriptionVi || '';
            document.getElementById('edit-poi-lat').value = data.latitude || '';
            document.getElementById('edit-poi-lng').value = data.longitude || '';
            document.getElementById('edit-poi-package').value = data.packageId || '';
            document.getElementById('edit-poi-owner').value = data.ownerId || '';
            renderAdminGallery(data.images || [], data.poiId || data.id);
            UI.showModal('modal-edit-poi');
        }

        var poi = _poiCache.find(function (p) { return p.poiId == id || p.id == id; });
        if (poi) { fillModal(poi); }
        else { AT.Services.POI.getById(id).then(fillModal); }
    }

    function deletePoi(id) {
        AT.Core.Auth.authFetch('/admin/pois/' + id, {
            method: 'DELETE'
        }).then(function () {
            UI.showToast('Đã xóa POI', 'success');
            loadAdminPois();
        }).catch(function (err) {
            console.error('Delete error', err);
            UI.showToast('Lỗi khi xóa POI', 'error');
        });
    }

    function togglePoiActive(id) {
        AT.Core.Auth.authFetch('/admin/pois/' + id + '/toggle-active', {
            method: 'PATCH'
        }).then(function () {
            UI.showToast('Đã thay đổi trạng thái hoạt động', 'success');
            loadAdminPois();
        }).catch(function (err) {
            console.error('Toggle error', err);
            UI.showToast('Lỗi khi thay đổi trạng thái', 'error');
        });
    }

    function bindEvents() {
        document.getElementById('filter-poi-status').addEventListener('change', function () {
            renderAdminPoisTable(_poiCache);
        });

        document.getElementById('search-poi').addEventListener('input', function () {
            renderAdminPoisTable(_poiCache);
        });

        var filterActive = document.getElementById('filter-poi-active');
        if (filterActive) {
            filterActive.addEventListener('change', function () {
                renderAdminPoisTable(_poiCache);
            });
        }

        document.getElementById('btn-create-poi').addEventListener('click', function () {
            openEditPoiModal(null);
        });

        var addImageInput = document.getElementById('edit-poi-image-add');
        if (addImageInput) {
            addImageInput.addEventListener('change', function () {
                var file = this.files[0];
                var poiId = this._targetPoiId;
                if (!file || !poiId) return;
                var fd = new FormData();
                fd.append('ImageFile', file);
                fd.append('IsCover', 'false');
                AT.Services.POI.addImage('admin', poiId, fd).then(function () {
                    UI.showToast('Đã thêm ảnh', 'success');
                    return AT.Services.POI.getById(poiId);
                }).then(function (data) {
                    renderAdminGallery(data.images || [], poiId);
                }).catch(function (err) {
                    UI.showToast((err && err.message) || 'Lỗi khi thêm ảnh', 'error');
                });
                addImageInput.value = '';
            });
        }

        document.getElementById('btn-close-edit-poi').addEventListener('click', function () {
            UI.hideModal('modal-edit-poi');
        });

        document.getElementById('btn-cancel-edit-poi').addEventListener('click', function () {
            UI.hideModal('modal-edit-poi');
        });

        document.getElementById('form-edit-poi').addEventListener('submit', function (e) {
            e.preventDefault();
            var id = document.getElementById('edit-poi-id').value;

            // Khi tạo mới (id rỗng): bắt buộc phải có ảnh bìa
            if (!id) {
                var fileInput = document.getElementById('edit-poi-image');
                var imageUrlInput = document.getElementById('edit-poi-image-url').value || '';
                var hasImage = (fileInput && fileInput.files.length > 0) || imageUrlInput.trim();
                if (!hasImage) {
                    UI.showToast('Vui lòng chọn ảnh bìa cho POI', 'error');
                    return;
                }
            }

            var formData = new FormData();
            formData.append('PoiName', document.getElementById('edit-poi-name').value);
            formData.append('DescriptionVi', document.getElementById('edit-poi-desc').value);
            formData.append('Latitude', document.getElementById('edit-poi-lat').value);
            formData.append('Longitude', document.getElementById('edit-poi-lng').value);
            formData.append('PackageId', document.getElementById('edit-poi-package').value);

            var ownerId = document.getElementById('edit-poi-owner').value;
            if (ownerId) formData.append('OwnerId', ownerId);

            var fileInput2 = document.getElementById('edit-poi-image');
            if (fileInput2 && fileInput2.files.length > 0)
                formData.append('ImageFile', fileInput2.files[0]);

            var imageUrlVal = document.getElementById('edit-poi-image-url').value;
            if (imageUrlVal) formData.append('ImageUrl', imageUrlVal);

            var url = id ? '/admin/pois/' + id : '/admin/pois';
            var method = id ? 'PUT' : 'POST';

            // Need custom fetch for FormData because authFetch assumes JSON by default unless modified
            var token = AT.Core.Auth.getToken();
            fetch(AT.Core.Config.API_URL + url, {
                method: method,
                headers: {
                    'Authorization': 'Bearer ' + token
                },
                body: formData
            }).then(function (res) {
                if (!res.ok) throw new Error('Lỗi cập nhật POI');
                return res.json();
            }).then(function (data) {
                UI.showToast('Lưu POI thành công', 'success');
                UI.hideModal('modal-edit-poi');
                loadAdminPois();
            }).catch(function (err) {
                console.error('Save error', err);
                UI.showToast('Lỗi khi lưu POI', 'error');
            });
        });
    }

})();

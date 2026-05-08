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

    AT.Modules.initManagePois = function () {
        loadAdminPois();
        bindEvents();
    };

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

            var imageUrl = p.imageUrl ?? p.ImageUrl ??
                           (Array.isArray(p.images) && p.images.length > 0 ? p.images[0] : null) ??
                           (Array.isArray(p.Images) && p.Images.length > 0 ? p.Images[0] : null) ??
                           '/images/placeholder-poi.png';

            return '<tr style="border-bottom:1px solid var(--border);">' +
                '<td style="padding:16px 12px;font-weight:600;font-size:13px;">' + poiId + '</td>' +
                '<td style="padding:16px 12px;font-size:13px;font-weight:500;">' +
                    '<div style="display:flex;align-items:center;gap:12px;">' +
                        '<img src="' + imageUrl + '" style="width:40px;height:40px;border-radius:6px;object-fit:cover;" alt="" onerror="this.onerror=null;this.src=\'/images/placeholder-poi.png\';">' +
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

    function openEditPoiModal(id) {
        if (!id) {
            // Create
            document.getElementById('poi-modal-title').innerHTML = '<i class="fa-solid fa-map-location-dot text-accent" style="margin-right:8px;"></i>Tạo POI mới';
            document.getElementById('edit-poi-id').value = '';
            document.getElementById('form-edit-poi').reset();
            document.getElementById('edit-poi-image-url').value = '';
            UI.showModal('modal-edit-poi');
            return;
        }

        // Edit
        document.getElementById('poi-modal-title').innerHTML = '<i class="fa-solid fa-map-location-dot text-accent" style="margin-right:8px;"></i>Sửa POI';
        var poi = _poiCache.find(function(p) { return p.poiId == id || p.id == id; });
        if (poi) {
            document.getElementById('edit-poi-id').value = poi.poiId || poi.id;
            document.getElementById('edit-poi-name').value = poi.poiName || poi.name || '';
            document.getElementById('edit-poi-desc').value = poi.descriptionVi || '';
            document.getElementById('edit-poi-lat').value = poi.latitude || '';
            document.getElementById('edit-poi-lng').value = poi.longitude || '';
            document.getElementById('edit-poi-package').value = poi.packageId || '';
            document.getElementById('edit-poi-owner').value = poi.ownerId || '';
            document.getElementById('edit-poi-image').value = '';
            document.getElementById('edit-poi-image-url').value = '';
            UI.showModal('modal-edit-poi');
        } else {
            AT.Services.POI.getById(id).then(function(data) {
                document.getElementById('edit-poi-id').value = data.poiId || data.id;
                document.getElementById('edit-poi-name').value = data.poiName || data.name || '';
                document.getElementById('edit-poi-desc').value = data.descriptionVi || '';
                document.getElementById('edit-poi-lat').value = data.latitude || '';
                document.getElementById('edit-poi-lng').value = data.longitude || '';
                document.getElementById('edit-poi-package').value = data.packageId || '';
                document.getElementById('edit-poi-owner').value = data.ownerId || '';
                document.getElementById('edit-poi-image').value = '';
                document.getElementById('edit-poi-image-url').value = '';
                UI.showModal('modal-edit-poi');
            });
        }
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

        document.getElementById('btn-close-edit-poi').addEventListener('click', function () {
            UI.hideModal('modal-edit-poi');
        });

        document.getElementById('btn-cancel-edit-poi').addEventListener('click', function () {
            UI.hideModal('modal-edit-poi');
        });

        document.getElementById('form-edit-poi').addEventListener('submit', function (e) {
            e.preventDefault();
            var id = document.getElementById('edit-poi-id').value;
            
            var formData = new FormData();
            formData.append('PoiName', document.getElementById('edit-poi-name').value);
            formData.append('DescriptionVi', document.getElementById('edit-poi-desc').value);
            formData.append('Latitude', document.getElementById('edit-poi-lat').value);
            formData.append('Longitude', document.getElementById('edit-poi-lng').value);
            formData.append('PackageId', document.getElementById('edit-poi-package').value);
            
            var ownerId = document.getElementById('edit-poi-owner').value;
            if (ownerId) {
                formData.append('OwnerId', ownerId);
            }

            var fileInput = document.getElementById('edit-poi-image');
            if (fileInput.files.length > 0) {
                formData.append('ImageFile', fileInput.files[0]);
            }

            var imageUrlInput = document.getElementById('edit-poi-image-url').value;
            if (imageUrlInput) {
                formData.append('ImageUrl', imageUrlInput);
            }

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

/**
 * Audio Travelling — Package Management Module (Admin)
 */
(function () {
    'use strict';
    var AT = window.AudioTravelling = window.AudioTravelling || {};
    AT.Modules = AT.Modules || {};
    var UI = AT.Core.UI;
    var currentEditingId = null;

    AT.Modules.initPackageManagement = function () {
        renderPackageList();
        bindEvents();
    };

    function renderPackageList() {
        var tbody = document.getElementById('package-table-body');
        if (!tbody) return;
        
        tbody.innerHTML = '<tr><td colspan="7" style="text-align:center;padding:20px;color:var(--text-dim);">Đang tải dữ liệu...</td></tr>';

        AT.Services.Package.getPackages().then(function (packages) {
            if (!packages || packages.length === 0) {
                tbody.innerHTML = '<tr><td colspan="7" style="text-align:center;padding:20px;color:var(--text-dim);">Chưa có gói đăng ký nào</td></tr>';
                return;
            }
            tbody.innerHTML = packages.map(function (pkg) {
                return '<tr style="border-bottom:1px solid var(--border);">' +
                    '<td style="padding:10px;font-weight:600;font-size:13px;">' + pkg.id + '</td>' +
                    '<td style="padding:10px;">' + (pkg.name || '') + '</td>' +
                    '<td style="padding:10px;text-align:center;">' + (pkg.radiusMeters || 0) + '</td>' +
                    '<td style="padding:10px;text-align:center;">' + (pkg.priority || 0) + '</td>' +
                    '<td style="padding:10px;text-align:right;">' + (pkg.price || 0) + '</td>' +
                    '<td style="padding:10px;">' + (pkg.description || '') + '</td>' +
                    '<td style="padding:10px;text-align:center;">' +
                    '<button class="btn-ghost btn-edit-package" data-id="' + pkg.id + '" style="font-size:12px;padding:6px;margin-right:5px;" title="Sửa">' +
                    '<i class="fa-solid fa-pen"></i></button>' +
                    '<button class="btn-ghost btn-delete-package" data-id="' + pkg.id + '" style="font-size:12px;padding:6px;color:red;" title="Xóa">' +
                    '<i class="fa-solid fa-trash"></i></button>' +
                    '</td></tr>';
            }).join('');

            // Bind edit/delete buttons
            tbody.querySelectorAll('.btn-edit-package').forEach(function (btn) {
                btn.addEventListener('click', function () {
                    var id = btn.getAttribute('data-id');
                    var pkg = packages.find(function(p) { return p.id == id; });
                    if (pkg) {
                        openModal(pkg);
                    }
                });
            });

            tbody.querySelectorAll('.btn-delete-package').forEach(function (btn) {
                btn.addEventListener('click', function () {
                    var id = btn.getAttribute('data-id');
                    if (confirm('Bạn có chắc chắn muốn xóa gói này?')) {
                        AT.Services.Package.deletePackage(id).then(function () {
                            UI.showToast('Xóa gói thành công', 'success');
                            renderPackageList();
                        }).catch(function(err) {
                            UI.showToast('Lỗi khi xóa gói', 'error');
                        });
                    }
                });
            });
        }).catch(function(err) {
            tbody.innerHTML = '<tr><td colspan="7" style="text-align:center;padding:20px;color:red;">Lỗi tải dữ liệu</td></tr>';
            UI.showToast('Không thể tải danh sách gói', 'error');
        });
    }

    function openModal(pkg) {
        var form = document.getElementById('package-form');
        if (!form) return;
        
        form.reset();
        if (pkg) {
            currentEditingId = pkg.id;
            document.getElementById('modal-package-title').innerText = 'Sửa gói đăng ký';
            document.getElementById('pkg-name').value = pkg.name || '';
            document.getElementById('pkg-radius').value = pkg.radiusMeters || 0;
            document.getElementById('pkg-priority').value = pkg.priority || 0;
            document.getElementById('pkg-price').value = pkg.price || 0;
            document.getElementById('pkg-description').value = pkg.description || '';
        } else {
            currentEditingId = null;
            document.getElementById('modal-package-title').innerText = 'Tạo gói mới';
        }
        
        UI.showModal('modal-package');
    }

    var _eventsBound = false;
    function bindEvents() {
        if (_eventsBound) return;
        _eventsBound = true;

        var btnCreate = document.getElementById('btn-create-package');
        if (btnCreate) {
            btnCreate.addEventListener('click', function () {
                openModal(null);
            });
        }

        var btnSave = document.getElementById('btn-save-package');
        if (btnSave) {
            btnSave.addEventListener('click', function () {
                var nameInput = document.getElementById('pkg-name');
                var radiusInput = document.getElementById('pkg-radius');
                var priorityInput = document.getElementById('pkg-priority');
                var priceInput = document.getElementById('pkg-price');
                var descInput = document.getElementById('pkg-description');

                var data = {
                    name: nameInput ? nameInput.value.trim() : '',
                    radiusMeters: radiusInput ? parseInt(radiusInput.value, 10) : 0,
                    priority: priorityInput ? parseInt(priorityInput.value, 10) : 0,
                    price: priceInput ? parseFloat(priceInput.value) : 0,
                    description: descInput ? descInput.value.trim() : ''
                };

                // Validate
                if (!data.name) {
                    UI.showToast('Tên gói không được rỗng', 'error');
                    return;
                }
                if (isNaN(data.radiusMeters) || data.radiusMeters <= 0) {
                    UI.showToast('Bán kính phải là số nguyên dương', 'error');
                    return;
                }
                if (isNaN(data.priority) || data.priority < 0) {
                    UI.showToast('Độ ưu tiên phải là số nguyên >= 0', 'error');
                    return;
                }
                if (isNaN(data.price) || data.price < 0) {
                    UI.showToast('Giá phải >= 0', 'error');
                    return;
                }

                btnSave.disabled = true;
                btnSave.innerText = 'Đang lưu...';

                var req = currentEditingId 
                    ? AT.Services.Package.updatePackage(currentEditingId, data)
                    : AT.Services.Package.createPackage(data);

                req.then(function () {
                    UI.showToast(currentEditingId ? 'Cập nhật gói thành công' : 'Tạo gói thành công', 'success');
                    UI.hideModal('modal-package');
                    renderPackageList();
                }).catch(function (err) {
                    UI.showToast('Có lỗi xảy ra khi lưu gói', 'error');
                }).finally(function() {
                    btnSave.disabled = false;
                    btnSave.innerText = 'Lưu';
                });
            });
        }

        var btnCancel = document.getElementById('btn-cancel-package');
        if (btnCancel) {
            btnCancel.addEventListener('click', function () {
                UI.hideModal('modal-package');
            });
        }
    }
})();

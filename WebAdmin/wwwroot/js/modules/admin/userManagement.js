/**
 * ═══════════════════════════════════════════════════
 * Audio Travelling — User Management Module (Admin)
 * Quản lý người dùng — tài khoản, role, trạng thái
 * ═══════════════════════════════════════════════════
 */
(function () {
    'use strict';
    var AT = window.AudioTravelling = window.AudioTravelling || {};
    AT.Modules = AT.Modules || {};
    var Fmt = AT.Utils.Formatters;
    var UI = AT.Core.UI;

    var _allUsers = [];

    AT.Modules.initUserManagement = function () {
        loadUsers();
        bindEvents();
    };

    function loadUsers() {
        var tbody = document.getElementById('users-table-body');
        if (tbody) {
            tbody.innerHTML = '<tr><td colspan="7" style="text-align:center;padding:40px;color:var(--text-dim);">' +
                '<i class="fa-solid fa-spinner fa-spin" style="font-size:24px;margin-bottom:12px;display:block;"></i>' +
                'Đang tải dữ liệu...</td></tr>';
        }

        AT.Services.User.getAllUsers().then(function (users) {
            console.log('[UserManagement] Loaded users:', users);
            _allUsers = users || [];
            renderUsers(_allUsers);
        }).catch(function (err) {
            console.error('[UserManagement] Error loading users:', err);
            if (tbody) {
                tbody.innerHTML = '<tr><td colspan="7" style="text-align:center;padding:40px;color:red;">' +
                    '<i class="fa-solid fa-circle-exclamation" style="font-size:24px;margin-bottom:12px;display:block;"></i>' +
                    'Không thể tải dữ liệu. Lỗi: ' + (err.message || err) + '</td></tr>';
            }
        });
    }

    function renderUsers(users) {
        var tbody = document.getElementById('users-table-body');
        if (!tbody) return;

        if (!users || users.length === 0) {
            tbody.innerHTML = '<tr><td colspan="7" style="text-align:center;padding:40px;color:var(--text-dim);">' +
                '<i class="fa-solid fa-users" style="font-size:32px;margin-bottom:12px;display:block;"></i>' +
                'Không tìm thấy người dùng nào</td></tr>';
            return;
        }

        tbody.innerHTML = users.map(function (u) {
            var userId = u.userId ?? u.UserId ?? '—';
            var fullName = u.fullName ?? u.FullName ?? '—';
            var email = u.email ?? u.Email ?? '—';
            var role = u.role ?? u.Role ?? '—';
            var isActive = u.isActive ?? u.IsActive ?? false;
            var statusText = u.statusText ?? u.StatusText ?? (isActive ? 'Hoạt động' : 'Không hoạt động');
            var statusClass = isActive ? 'status-approved' : 'status-rejected';
            var roleBadge = role === 'Admin'
                ? '<span class="status-badge status-listening">' + role + '</span>'
                : '<span class="status-badge status-browsing">' + role + '</span>';

            return '<tr style="border-bottom:1px solid var(--border);">' +
                '<td style="padding:12px 10px;font-weight:600;font-size:13px;">' + userId + '</td>' +
                '<td style="padding:12px 10px;font-size:13px;font-weight:500;">' + fullName + '</td>' +
                '<td style="padding:12px 10px;font-size:13px;">' + email + '</td>' +
                '<td style="padding:12px 10px;">' + roleBadge + '</td>' +
                '<td style="padding:12px 10px;"><span class="status-badge ' + statusClass + '">' + statusText + '</span></td>' +
                '<td style="padding:12px 10px;font-size:13px;">' + Fmt.date(u.createdDate ?? u.CreatedDate) + '</td>' +
                '<td style="padding:12px 10px;text-align:center;">' +
                    '<div style="display:flex;gap:4px;justify-content:center;flex-wrap:wrap;">' +
                        '<button class="btn-ghost btn-view-user" data-user-id="' + u.userId + '" style="font-size:11px;padding:5px 8px;" title="Xem chi tiết">' +
                            '<i class="fa-solid fa-eye"></i>' +
                        '</button>' +
                        '<button class="btn-ghost btn-edit-user" data-user-id="' + u.userId + '" style="font-size:11px;padding:5px 8px;" title="Sửa">' +
                            '<i class="fa-solid fa-pen"></i>' +
                        '</button>' +
                        '<button class="btn-ghost btn-toggle-user" data-user-id="' + u.userId + '" style="font-size:11px;padding:5px 8px;" title="Khóa/Mở">' +
                            '<i class="fa-solid fa-toggle-on"></i>' +
                        '</button>' +
                        '<button class="btn-danger-ghost btn-delete-user" data-user-id="' + u.userId + '" style="font-size:11px;padding:5px 8px;" title="Xóa">' +
                            '<i class="fa-solid fa-trash"></i>' +
                        '</button>' +
                    '</div>' +
                '</td>' +
                '</tr>';
        }).join('');

        bindTableEvents(tbody);
    }

    function bindTableEvents(tbody) {
        tbody.querySelectorAll('.btn-view-user').forEach(function (btn) {
            btn.addEventListener('click', function () {
                showUserDetail(btn.getAttribute('data-user-id'));
            });
        });

        tbody.querySelectorAll('.btn-edit-user').forEach(function (btn) {
            btn.addEventListener('click', function () {
                openEditUserModal(btn.getAttribute('data-user-id'));
            });
        });

        tbody.querySelectorAll('.btn-toggle-user').forEach(function (btn) {
            btn.addEventListener('click', function () {
                toggleUserStatus(btn.getAttribute('data-user-id'));
            });
        });

        tbody.querySelectorAll('.btn-delete-user').forEach(function (btn) {
            btn.addEventListener('click', function () {
                var userId = btn.getAttribute('data-user-id');
                if (confirm('Bạn có chắc muốn xóa hoặc vô hiệu hóa người dùng ' + userId + '?')) {
                    deleteUser(userId);
                }
            });
        });
    }

    function openEditUserModal(userId) {
        if (!userId) {
            document.getElementById('user-modal-title').innerHTML = '<i class="fa-solid fa-user-plus text-accent" style="margin-right:8px;"></i>Tạo Tài Khoản';
            document.getElementById('edit-user-id').value = '';
            document.getElementById('form-edit-user').reset();
            UI.showModal('modal-edit-user');
            return;
        }

        document.getElementById('user-modal-title').innerHTML = '<i class="fa-solid fa-user-pen text-accent" style="margin-right:8px;"></i>Sửa Tài Khoản';
        var u = _allUsers.find(function(user) { return (user.userId ?? user.UserId) == userId || user.id == userId; });
        if (u) {
            document.getElementById('edit-user-id').value = u.userId ?? u.UserId ?? '';
            document.getElementById('edit-user-fullname').value = u.fullName ?? u.FullName ?? '';
            document.getElementById('edit-user-email').value = u.email ?? u.Email ?? '';
            document.getElementById('edit-user-phone').value = u.phoneNumber ?? u.PhoneNumber ?? '';
            document.getElementById('edit-user-role').value = u.role ?? u.Role ?? 'Owner';
            document.getElementById('edit-user-password').value = '';
            UI.showModal('modal-edit-user');
        }
    }

    function toggleUserStatus(userId) {
        AT.Core.Auth.authFetch('/admin/users/' + userId + '/toggle-active', {
            method: 'PATCH'
        }).then(function () {
            UI.showToast('Đã cập nhật trạng thái', 'success');
            loadUsers();
        }).catch(function (err) {
            UI.showToast('Lỗi: ' + err.message, 'error');
        });
    }

    function deleteUser(userId) {
        AT.Core.Auth.authFetch('/admin/users/' + userId, {
            method: 'DELETE'
        }).then(function (res) {
            UI.showToast(res.message || 'Đã xử lý xóa', 'success');
            loadUsers();
        }).catch(function (err) {
            UI.showToast('Lỗi: ' + err.message, 'error');
        });
    }

    function showUserDetail(userId) {
        AT.Services.User.getUserById(userId).then(function (u) {
            if (!u) return;
            var container = document.getElementById('user-detail-content');
            if (!container) return;

            var userId = u.userId ?? u.UserId ?? '—';
            var fullName = u.fullName ?? u.FullName ?? '—';
            var email = u.email ?? u.Email ?? '—';
            var role = u.role ?? u.Role ?? '—';
            var phoneNumber = u.phoneNumber ?? u.PhoneNumber ?? '—';
            var isActive = u.isActive ?? u.IsActive ?? false;
            var statusText = u.statusText ?? u.StatusText ?? (isActive ? 'Hoạt động' : 'Không hoạt động');
            var statusClass = isActive ? 'status-approved' : 'status-rejected';
            var createdDate = u.createdDate ?? u.CreatedDate;
            var lastLoginAt = u.lastLoginAt ?? u.LastLoginAt;

            container.innerHTML =
                '<div style="margin-bottom:16px;">' +
                    '<h4 style="font-size:18px;font-weight:700;margin:0 0 4px;">' + fullName + '</h4>' +
                    '<p style="font-size:13px;color:var(--text-dim);margin:0;">' + email + '</p>' +
                '</div>' +
                '<div style="display:grid;grid-template-columns:1fr 1fr;gap:12px;font-size:13px;">' +
                    '<div><strong style="color:var(--text-dim);display:block;margin-bottom:2px;">ID</strong>' + userId + '</div>' +
                    '<div><strong style="color:var(--text-dim);display:block;margin-bottom:2px;">Vai trò</strong>' + role + '</div>' +
                    '<div><strong style="color:var(--text-dim);display:block;margin-bottom:2px;">Số điện thoại</strong>' + phoneNumber + '</div>' +
                    '<div><strong style="color:var(--text-dim);display:block;margin-bottom:2px;">Trạng thái</strong><span class="status-badge ' + statusClass + '">' + statusText + '</span></div>' +
                    '<div><strong style="color:var(--text-dim);display:block;margin-bottom:2px;">Ngày tạo</strong>' + Fmt.date(createdDate) + '</div>' +
                    '<div><strong style="color:var(--text-dim);display:block;margin-bottom:2px;">Đăng nhập gần nhất</strong>' + (lastLoginAt ? Fmt.dateTime(lastLoginAt) : '—') + '</div>' +
                '</div>';

            UI.showModal('modal-user-detail');
        });
    }

    function filterUsers() {
        var roleFilter = document.getElementById('filter-user-role');
        var statusFilter = document.getElementById('filter-user-status');
        var searchInput = document.getElementById('search-user');
        var role = roleFilter ? roleFilter.value : '';
        var statusVal = statusFilter ? statusFilter.value : '';
        var query = searchInput ? searchInput.value.toLowerCase().trim() : '';

        var filtered = _allUsers.filter(function (u) {
            var uRole = u.role ?? u.Role;
            var uIsActive = u.isActive ?? u.IsActive;
            if (role && uRole !== role) return false;
            if (statusVal) {
                if (statusVal === 'active' && !uIsActive) return false;
                if (statusVal === 'locked' && uIsActive) return false;
            }
            var fullName = ((u.fullName ?? u.FullName) || '').toLowerCase();
            var email = ((u.email ?? u.Email) || '').toLowerCase();
            if (query && fullName.indexOf(query) === -1 && email.indexOf(query) === -1) return false;
            return true;
        });

        renderUsers(filtered);
    }

    var _eventsBound = false;
    function bindEvents() {
        if (_eventsBound) return;
        _eventsBound = true;

        var roleFilter = document.getElementById('filter-user-role');
        if (roleFilter) roleFilter.addEventListener('change', filterUsers);

        var statusFilter = document.getElementById('filter-user-status');
        if (statusFilter) statusFilter.addEventListener('change', filterUsers);

        var searchInput = document.getElementById('search-user');
        if (searchInput) searchInput.addEventListener('input', filterUsers);

        var btnCreate = document.getElementById('btn-create-user');
        if (btnCreate) btnCreate.addEventListener('click', function () { openEditUserModal(null); });

        var btnCloseEdit = document.getElementById('btn-close-edit-user');
        if (btnCloseEdit) btnCloseEdit.addEventListener('click', function () { UI.hideModal('modal-edit-user'); });

        var btnCancelEdit = document.getElementById('btn-cancel-edit-user');
        if (btnCancelEdit) btnCancelEdit.addEventListener('click', function () { UI.hideModal('modal-edit-user'); });

        var formEdit = document.getElementById('form-edit-user');
        if (formEdit) {
            formEdit.addEventListener('submit', function (e) {
                e.preventDefault();
                var id = document.getElementById('edit-user-id').value;
                var method = id ? 'PUT' : 'POST';
                var url = id ? '/admin/users/' + id : '/admin/users';

                var data = {
                    FullName: document.getElementById('edit-user-fullname').value,
                    Email: document.getElementById('edit-user-email').value,
                    PhoneNumber: document.getElementById('edit-user-phone').value,
                    Password: document.getElementById('edit-user-password').value,
                    Role: document.getElementById('edit-user-role').value
                };

                if (!id && !data.Password) {
                    UI.showToast('Vui lòng nhập mật khẩu cho người dùng mới', 'error');
                    return;
                }

                AT.Core.Auth.authFetch(url, {
                    method: method,
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(data)
                }).then(function () {
                    UI.showToast('Đã lưu thông tin người dùng', 'success');
                    UI.hideModal('modal-edit-user');
                    loadUsers();
                }).catch(function (err) {
                    UI.showToast('Lỗi: ' + err.message, 'error');
                });
            });
        }

        var btnCloseModal = document.getElementById('btn-close-user-detail');
        if (btnCloseModal) {
            btnCloseModal.addEventListener('click', function () {
                UI.hideModal('modal-user-detail');
            });
        }

        var modalOverlay = document.getElementById('modal-user-detail');
        if (modalOverlay) {
            modalOverlay.addEventListener('click', function (e) {
                if (e.target === modalOverlay) {
                    UI.hideModal('modal-user-detail');
                }
            });
        }
    }
})();

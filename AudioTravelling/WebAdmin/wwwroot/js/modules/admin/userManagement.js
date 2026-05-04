/**
 * ═══════════════════════════════════════════════════
 * Audio Travelling — User Management Module (Admin)
 * Quản lý người dùng — tài khoản, role, trạng thái
 * TODO: Replace mock data with real API calls.
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
        AT.Services.User.getAllUsers().then(function (users) {
            _allUsers = users;
            renderUsers(users);
        });
    }

    function renderUsers(users) {
        var tbody = document.getElementById('users-table-body');
        if (!tbody) return;

        if (users.length === 0) {
            tbody.innerHTML = '<tr><td colspan="7" style="text-align:center;padding:40px;color:var(--text-dim);">' +
                '<i class="fa-solid fa-users" style="font-size:32px;margin-bottom:12px;display:block;"></i>' +
                'Không tìm thấy người dùng nào</td></tr>';
            return;
        }

        tbody.innerHTML = users.map(function (u) {
            var statusClass = u.status === 'active' ? 'status-approved' : 'status-rejected';
            var statusText = u.status === 'active' ? 'Hoạt động' : 'Đã khóa';
            var roleBadge = u.role === 'Admin'
                ? '<span class="status-badge status-listening">' + u.role + '</span>'
                : '<span class="status-badge status-browsing">' + u.role + '</span>';
            var lockIcon = u.status === 'active' ? 'fa-lock' : 'fa-lock-open';
            var lockTitle = u.status === 'active' ? 'Khóa tài khoản' : 'Mở khóa tài khoản';

            return '<tr style="border-bottom:1px solid var(--border);">' +
                '<td style="padding:10px;font-weight:600;font-size:13px;">' + u.id + '</td>' +
                '<td style="padding:10px;font-size:13px;font-weight:500;">' + u.fullName + '</td>' +
                '<td style="padding:10px;font-size:13px;">' + u.email + '</td>' +
                '<td style="padding:10px;">' + roleBadge + '</td>' +
                '<td style="padding:10px;"><span class="status-badge ' + statusClass + '">' + statusText + '</span></td>' +
                '<td style="padding:10px;font-size:13px;">' + Fmt.date(u.createdAt) + '</td>' +
                '<td style="padding:10px;text-align:center;">' +
                    '<div style="display:flex;gap:4px;justify-content:center;flex-wrap:wrap;">' +
                        '<button class="btn-ghost btn-view-user" data-user-id="' + u.id + '" style="font-size:11px;padding:5px 8px;" title="Xem chi tiết">' +
                            '<i class="fa-solid fa-eye"></i>' +
                        '</button>' +
                        '<button class="btn-ghost btn-toggle-lock" data-user-id="' + u.id + '" style="font-size:11px;padding:5px 8px;" title="' + lockTitle + '">' +
                            '<i class="fa-solid ' + lockIcon + '"></i>' +
                        '</button>' +
                        '<button class="btn-ghost btn-change-role" data-user-id="' + u.id + '" data-role="' + u.role + '" style="font-size:11px;padding:5px 8px;" title="Đổi vai trò">' +
                            '<i class="fa-solid fa-user-gear"></i>' +
                        '</button>' +
                        '<button class="btn-ghost btn-reset-pw" data-user-id="' + u.id + '" style="font-size:11px;padding:5px 8px;" title="Reset mật khẩu">' +
                            '<i class="fa-solid fa-key"></i>' +
                        '</button>' +
                    '</div>' +
                '</td>' +
                '</tr>';
        }).join('');

        // ── Bind action buttons ──

        // View detail
        tbody.querySelectorAll('.btn-view-user').forEach(function (btn) {
            btn.addEventListener('click', function () {
                showUserDetail(btn.getAttribute('data-user-id'));
            });
        });

        // Toggle lock
        tbody.querySelectorAll('.btn-toggle-lock').forEach(function (btn) {
            btn.addEventListener('click', function () {
                var userId = btn.getAttribute('data-user-id');
                AT.Services.User.toggleLock(userId).then(function (u) {
                    var msg = u.status === 'active' ? 'Đã mở khóa tài khoản ' : 'Đã khóa tài khoản ';
                    UI.showToast(msg + u.fullName, 'success');
                    loadUsers(); // Re-render with filters
                });
            });
        });

        // Change role
        tbody.querySelectorAll('.btn-change-role').forEach(function (btn) {
            btn.addEventListener('click', function () {
                var userId = btn.getAttribute('data-user-id');
                var currentRole = btn.getAttribute('data-role');
                var newRole = currentRole === 'Admin' ? 'Owner' : 'Admin';
                UI.confirm('Đổi vai trò của user ' + userId + ' từ ' + currentRole + ' thành ' + newRole + '?', function () {
                    AT.Services.User.changeRole(userId, newRole).then(function (u) {
                        UI.showToast('Đã đổi vai trò ' + u.fullName + ' thành ' + newRole, 'success');
                        loadUsers();
                    });
                });
            });
        });

        // Reset password
        tbody.querySelectorAll('.btn-reset-pw').forEach(function (btn) {
            btn.addEventListener('click', function () {
                var userId = btn.getAttribute('data-user-id');
                UI.confirm('Reset mật khẩu cho user ' + userId + '? Một email thông báo sẽ được gửi.', function () {
                    AT.Services.User.resetPassword(userId).then(function (result) {
                        UI.showToast(result.message, 'success');
                    });
                });
            });
        });
    }

    function showUserDetail(userId) {
        AT.Services.User.getUserById(userId).then(function (u) {
            if (!u) return;
            var container = document.getElementById('user-detail-content');
            if (!container) return;

            var statusClass = u.status === 'active' ? 'status-approved' : 'status-rejected';
            var statusText = u.status === 'active' ? 'Hoạt động' : 'Đã khóa';

            container.innerHTML =
                '<div style="margin-bottom:16px;">' +
                    '<h4 style="font-size:18px;font-weight:700;margin:0 0 4px;">' + u.fullName + '</h4>' +
                    '<p style="font-size:13px;color:var(--text-dim);margin:0;">' + u.email + '</p>' +
                '</div>' +
                '<div style="display:grid;grid-template-columns:1fr 1fr;gap:12px;font-size:13px;">' +
                    '<div><strong style="color:var(--text-dim);display:block;margin-bottom:2px;">ID</strong>' + u.id + '</div>' +
                    '<div><strong style="color:var(--text-dim);display:block;margin-bottom:2px;">Vai trò</strong>' + u.role + '</div>' +
                    '<div><strong style="color:var(--text-dim);display:block;margin-bottom:2px;">Trạng thái</strong><span class="status-badge ' + statusClass + '">' + statusText + '</span></div>' +
                    '<div><strong style="color:var(--text-dim);display:block;margin-bottom:2px;">Ngày tạo</strong>' + Fmt.date(u.createdAt) + '</div>' +
                    '<div style="grid-column:1/-1;"><strong style="color:var(--text-dim);display:block;margin-bottom:2px;">Đăng nhập gần nhất</strong>' + (u.lastLogin ? Fmt.date(u.lastLogin) : '—') + '</div>' +
                '</div>';

            UI.showModal('modal-user-detail');
        });
    }

    function filterUsers() {
        var roleFilter = document.getElementById('filter-user-role');
        var statusFilter = document.getElementById('filter-user-status');
        var searchInput = document.getElementById('search-user');
        var role = roleFilter ? roleFilter.value : '';
        var status = statusFilter ? statusFilter.value : '';
        var query = searchInput ? searchInput.value.toLowerCase().trim() : '';

        var filtered = _allUsers.filter(function (u) {
            if (role && u.role !== role) return false;
            if (status && u.status !== status) return false;
            if (query && u.fullName.toLowerCase().indexOf(query) === -1 && u.email.toLowerCase().indexOf(query) === -1) return false;
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

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
            var status = String(u.status || '').toLowerCase();
            var statusClass = status === 'active' ? 'status-approved' : 'status-rejected';
            var statusText = status === 'active' ? 'Hoạt động' : 'Không hoạt động';
            var roleBadge = (u.role || '') === 'Admin'
                ? '<span class="status-badge status-listening">' + u.role + '</span>'
                : '<span class="status-badge status-browsing">' + u.role + '</span>';

            return '<tr style="border-bottom:1px solid var(--border);">' +
                '<td style="padding:12px 10px;font-weight:600;font-size:13px;">' + (u.userId || '—') + '</td>' +
                '<td style="padding:12px 10px;font-size:13px;font-weight:500;">' + (u.fullName || '—') + '</td>' +
                '<td style="padding:12px 10px;font-size:13px;">' + (u.email || '—') + '</td>' +
                '<td style="padding:12px 10px;">' + roleBadge + '</td>' +
                '<td style="padding:12px 10px;"><span class="status-badge ' + statusClass + '">' + statusText + '</span></td>' +
                '<td style="padding:12px 10px;font-size:13px;">' + Fmt.date(u.createdDate) + '</td>' +
                '<td style="padding:12px 10px;text-align:center;">' +
                    '<div style="display:flex;gap:4px;justify-content:center;flex-wrap:wrap;">' +
                        '<button class="btn-ghost btn-view-user" data-user-id="' + u.userId + '" style="font-size:11px;padding:5px 8px;" title="Xem chi tiết">' +
                            '<i class="fa-solid fa-eye"></i>' +
                        '</button>' +
                    '</div>' +
                '</td>' +
                '</tr>';
        }).join('');

        // Bind view detail
        tbody.querySelectorAll('.btn-view-user').forEach(function (btn) {
            btn.addEventListener('click', function () {
                showUserDetail(btn.getAttribute('data-user-id'));
            });
        });
    }

    function showUserDetail(userId) {
        AT.Services.User.getUserById(userId).then(function (u) {
            if (!u) return;
            var container = document.getElementById('user-detail-content');
            if (!container) return;

            var status = String(u.status || '').toLowerCase();
            var statusClass = status === 'active' ? 'status-approved' : 'status-rejected';
            var statusText = status === 'active' ? 'Hoạt động' : 'Không hoạt động';

            container.innerHTML =
                '<div style="margin-bottom:16px;">' +
                    '<h4 style="font-size:18px;font-weight:700;margin:0 0 4px;">' + (u.fullName || '—') + '</h4>' +
                    '<p style="font-size:13px;color:var(--text-dim);margin:0;">' + (u.email || '—') + '</p>' +
                '</div>' +
                '<div style="display:grid;grid-template-columns:1fr 1fr;gap:12px;font-size:13px;">' +
                    '<div><strong style="color:var(--text-dim);display:block;margin-bottom:2px;">ID</strong>' + (u.userId || '—') + '</div>' +
                    '<div><strong style="color:var(--text-dim);display:block;margin-bottom:2px;">Vai trò</strong>' + (u.role || '—') + '</div>' +
                    '<div><strong style="color:var(--text-dim);display:block;margin-bottom:2px;">Số điện thoại</strong>' + (u.phoneNumber || '—') + '</div>' +
                    '<div><strong style="color:var(--text-dim);display:block;margin-bottom:2px;">Trạng thái</strong><span class="status-badge ' + statusClass + '">' + statusText + '</span></div>' +
                    '<div><strong style="color:var(--text-dim);display:block;margin-bottom:2px;">Ngày tạo</strong>' + Fmt.date(u.createdDate) + '</div>' +
                    '<div><strong style="color:var(--text-dim);display:block;margin-bottom:2px;">Đăng nhập gần nhất</strong>' + (u.lastLoginAt ? Fmt.dateTime(u.lastLoginAt) : '—') + '</div>' +
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
            if (role && u.role !== role) return false;
            if (statusVal) {
                var uStatus = String(u.status || '').toLowerCase();
                // Map filter values: "active" => "active", "locked" => "inactive"
                if (statusVal === 'active' && uStatus !== 'active') return false;
                if (statusVal === 'locked' && uStatus !== 'inactive') return false;
            }
            var fullName = (u.fullName || '').toLowerCase();
            var email = (u.email || '').toLowerCase();
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

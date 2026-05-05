/**
 * ═══════════════════════════════════════════════════
 * Audio Travelling — User Profiles Module (Admin)
 * Hồ sơ người dùng — quản lý hồ sơ Admin & Owner
 * ═══════════════════════════════════════════════════
 */
(function () {
    'use strict';
    var AT = window.AudioTravelling = window.AudioTravelling || {};
    AT.Modules = AT.Modules || {};
    var Fmt = AT.Utils.Formatters;
    var UI = AT.Core.UI;

    var _allProfiles = [];

    AT.Modules.initUserProfiles = function () {
        loadProfiles();
        bindEvents();
    };

    function loadProfiles() {
        var tbody = document.getElementById('profiles-table-body');
        if (tbody) {
            tbody.innerHTML = '<tr><td colspan="8" style="text-align:center;padding:40px;color:var(--text-dim);">' +
                '<i class="fa-solid fa-spinner fa-spin" style="font-size:24px;margin-bottom:12px;display:block;"></i>' +
                'Đang tải hồ sơ...</td></tr>';
        }

        AT.Services.User.getAllProfiles().then(function (profiles) {
            console.log('[UserProfiles] Loaded profiles:', profiles);
            _allProfiles = profiles || [];
            renderProfiles(_allProfiles);
        }).catch(function (err) {
            console.error('[UserProfiles] Error loading profiles:', err);
            if (tbody) {
                tbody.innerHTML = '<tr><td colspan="8" style="text-align:center;padding:40px;color:red;">' +
                    '<i class="fa-solid fa-circle-exclamation" style="font-size:24px;margin-bottom:12px;display:block;"></i>' +
                    'Không thể tải dữ liệu hồ sơ. Lỗi: ' + (err.message || err) + '</td></tr>';
            }
        });
    }

    function renderProfiles(profiles) {
        var tbody = document.getElementById('profiles-table-body');
        if (!tbody) return;

        if (!profiles || profiles.length === 0) {
            tbody.innerHTML = '<tr><td colspan="8" style="text-align:center;padding:40px;color:var(--text-dim);">' +
                '<i class="fa-solid fa-id-card" style="font-size:32px;margin-bottom:12px;display:block;"></i>' +
                'Không tìm thấy hồ sơ nào</td></tr>';
            return;
        }

        tbody.innerHTML = profiles.map(function (p) {
            var status = String(p.status || '').toLowerCase();
            var statusClass = status === 'active' ? 'status-approved' : 'status-rejected';
            var statusText = status === 'active' ? 'Hoạt động' : 'Không hoạt động';
            var roleBadge = (p.role || '') === 'Admin'
                ? '<span class="status-badge status-listening">' + p.role + '</span>'
                : '<span class="status-badge status-browsing">' + p.role + '</span>';

            return '<tr style="border-bottom:1px solid var(--border);cursor:pointer;" class="profile-row" data-profile-id="' + p.userId + '">' +
                '<td style="padding:12px 10px;font-weight:600;font-size:13px;">' + (p.userId || '—') + '</td>' +
                '<td style="padding:12px 10px;font-size:13px;font-weight:500;">' +
                    '<div style="display:flex;align-items:center;gap:8px;">' +
                        (p.fullName || '—') +
                    '</div>' +
                '</td>' +
                '<td style="padding:12px 10px;font-size:13px;">' + (p.email || '—') + '</td>' +
                '<td style="padding:12px 10px;">' + roleBadge + '</td>' +
                '<td style="padding:12px 10px;font-size:13px;">' + (p.phoneNumber || '—') + '</td>' +
                '<td style="padding:12px 10px;"><span class="status-badge ' + statusClass + '">' + statusText + '</span></td>' +
                '<td style="padding:12px 10px;font-size:13px;">' + Fmt.date(p.createdDate) + '</td>' +
                '<td style="padding:12px 10px;text-align:center;">' +
                    '<button class="btn-ghost btn-view-profile" data-profile-id="' + p.userId + '" style="font-size:12px;padding:6px 12px;" title="Xem chi tiết">' +
                        '<i class="fa-solid fa-eye"></i> Xem' +
                    '</button>' +
                '</td>' +
                '</tr>';
        }).join('');

        // Bind view buttons
        tbody.querySelectorAll('.btn-view-profile').forEach(function (btn) {
            btn.addEventListener('click', function (e) {
                e.stopPropagation();
                showProfileDetail(btn.getAttribute('data-profile-id'));
            });
        });

        // Bind row clicks
        tbody.querySelectorAll('.profile-row').forEach(function (row) {
            row.addEventListener('click', function () {
                showProfileDetail(row.getAttribute('data-profile-id'));
            });
        });
    }

    function showProfileDetail(profileId) {
        AT.Services.User.getProfileById(profileId).then(function (p) {
            if (!p) return;
            var container = document.getElementById('profile-detail-content');
            if (!container) return;

            var status = String(p.status || '').toLowerCase();
            var statusClass = status === 'active' ? 'status-approved' : 'status-rejected';
            var statusText = status === 'active' ? 'Hoạt động' : 'Không hoạt động';

            container.innerHTML =
                '<div style="text-align:center;margin-bottom:20px;">' +
                    '<img src="' + (p.avatarUrl || 'https://i.pravatar.cc/80?u=' + (p.userId || '')) + '" style="width:80px;height:80px;border-radius:50%;object-fit:cover;margin-bottom:12px;" alt="">' +
                    '<h4 style="font-size:18px;font-weight:700;margin:0;">' + (p.fullName || '—') + '</h4>' +
                    '<p style="font-size:13px;color:var(--text-dim);margin:4px 0 8px;">' + (p.email || '—') + '</p>' +
                    '<span class="status-badge ' + statusClass + '">' + statusText + '</span>' +
                '</div>' +
                '<div style="display:grid;grid-template-columns:1fr 1fr;gap:12px;font-size:13px;">' +
                    '<div><strong style="color:var(--text-dim);display:block;margin-bottom:2px;">ID</strong>' + (p.userId || '—') + '</div>' +
                    '<div><strong style="color:var(--text-dim);display:block;margin-bottom:2px;">Vai trò</strong>' + (p.role || '—') + '</div>' +
                    '<div><strong style="color:var(--text-dim);display:block;margin-bottom:2px;">Số điện thoại</strong>' + (p.phoneNumber || '—') + '</div>' +
                    '<div><strong style="color:var(--text-dim);display:block;margin-bottom:2px;">Ngày tạo</strong>' + Fmt.date(p.createdDate) + '</div>' +
                    '<div style="grid-column:1/-1;"><strong style="color:var(--text-dim);display:block;margin-bottom:2px;">Địa chỉ</strong>' + (p.address || '—') + '</div>' +
                '</div>';

            UI.showModal('modal-profile-detail');
        });
    }

    function filterProfiles() {
        var roleFilter = document.getElementById('filter-profile-role');
        var searchInput = document.getElementById('search-profile');
        var role = roleFilter ? roleFilter.value : '';
        var query = searchInput ? searchInput.value.toLowerCase().trim() : '';

        var filtered = _allProfiles.filter(function (p) {
            if (role && p.role !== role) return false;
            var fullName = (p.fullName || '').toLowerCase();
            var email = (p.email || '').toLowerCase();
            if (query && fullName.indexOf(query) === -1 && email.indexOf(query) === -1) return false;
            return true;
        });

        renderProfiles(filtered);
    }

    var _eventsBound = false;
    function bindEvents() {
        if (_eventsBound) return;
        _eventsBound = true;

        var roleFilter = document.getElementById('filter-profile-role');
        if (roleFilter) roleFilter.addEventListener('change', filterProfiles);

        var searchInput = document.getElementById('search-profile');
        if (searchInput) searchInput.addEventListener('input', filterProfiles);

        var btnCloseModal = document.getElementById('btn-close-profile-detail');
        if (btnCloseModal) {
            btnCloseModal.addEventListener('click', function () {
                UI.hideModal('modal-profile-detail');
            });
        }

        var modalOverlay = document.getElementById('modal-profile-detail');
        if (modalOverlay) {
            modalOverlay.addEventListener('click', function (e) {
                if (e.target === modalOverlay) {
                    UI.hideModal('modal-profile-detail');
                }
            });
        }
    }
})();

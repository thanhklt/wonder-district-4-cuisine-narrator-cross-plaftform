
/**
 * Audio Travelling — POI Approval Module (Admin)
 */
(function () {
    'use strict';
    var AT = window.AudioTravelling = window.AudioTravelling || {};
    AT.Modules = AT.Modules || {};
    var Fmt = AT.Utils.Formatters;
    var UI = AT.Core.UI;

    var _rejectTargetId = null;

    AT.Modules.initPOIApproval = function () {
        renderPendingPOIs();
        bindEvents();
    };

    /** Get API origin for resolving relative image URLs */
    function getApiOrigin() {
        if (AT.Core.ApiClient && typeof AT.Core.ApiClient.getBaseOrigin === 'function') {
            return AT.Core.ApiClient.getBaseOrigin();
        }
        return '';
    }

    /** Resolve an image URL — delegates to the centralized resolver */
    function resolveImageUrl(url) {
        if (!url) return null;
        if (typeof url === 'object' && url.imageUrl) {
            url = url.imageUrl;
        }
        if (typeof url !== 'string') return null;
        return AT.Core.ApiClient.resolveImageUrl(url);
    }

    function renderPendingPOIs() {
        AT.Services.POI.getPending().then(function (pois) {
            console.log('[POIApproval] Loaded pending POIs:', pois);
            var tbody = document.getElementById('poi-approval-body');
            if (!tbody) return;
            if (!pois || pois.length === 0) {
                tbody.innerHTML = '<tr><td colspan="5" style="text-align:center;padding:20px;color:var(--text-dim);">Không có POI nào đang chờ duyệt</td></tr>';
                return;
            }
            tbody.innerHTML = pois.map(function (poi) {
                var poiId = poi.poiId || poi.id || '';
                var poiName = poi.poiName || poi.name || 'Không có tên';
                var ownerName = poi.ownerName || poi.owner || '—';
                var updatedDate = poi.updatedDate || poi.updatedAt || poi.createdDate || '';
                return '<tr style="border-bottom:1px solid var(--border);">' +
                    '<td style="padding:10px;font-weight:600;font-size:13px;">' + poiId + '</td>' +
                    '<td style="padding:10px;font-size:13px;font-weight:500;">' + poiName + '</td>' +
                    '<td style="padding:10px;font-size:13px;">' + ownerName + '</td>' +
                    '<td style="padding:10px;font-size:13px;">' + Fmt.dateTime(updatedDate) + '</td>' +
                    '<td style="padding:10px;text-align:center;">' +
                    '<button class="btn-ghost btn-detail-poi" data-poi-id="' + poiId + '" style="font-size:12px;padding:6px 14px;margin-right:4px;" title="Chi tiết">' +
                    '<i class="fa-solid fa-eye"></i> Chi tiết</button>' +
                    '<button class="btn-primary btn-approve-poi" data-poi-id="' + poiId + '" style="font-size:12px;padding:6px 14px;margin-right:4px;">' +
                    '<i class="fa-solid fa-check"></i> Duyệt</button>' +
                    '<button class="btn-danger-ghost btn-reject-poi" data-poi-id="' + poiId + '" style="font-size:12px;padding:6px 14px;">' +
                    '<i class="fa-solid fa-xmark"></i> Từ chối</button>' +
                    '</td></tr>';
            }).join('');

            bindTableButtons(tbody);
        }).catch(function (err) {
            console.error('[POIApproval] Error loading POIs:', err);
            var tbody = document.getElementById('poi-approval-body');
            if (tbody) {
                tbody.innerHTML = '<tr><td colspan="5" style="text-align:center;padding:20px;color:red;">' +
                    '<i class="fa-solid fa-circle-exclamation" style="font-size:24px;margin-bottom:8px;display:block;"></i>' +
                    'Không thể tải danh sách POI. Lỗi: ' + (err.message || err) + '</td></tr>';
            }
        });
    }

    function bindTableButtons(tbody) {
        // Detail buttons
        tbody.querySelectorAll('.btn-detail-poi').forEach(function (btn) {
            btn.addEventListener('click', function () {
                showPoiDetail(btn.getAttribute('data-poi-id'));
            });
        });

        // Approve buttons
        tbody.querySelectorAll('.btn-approve-poi').forEach(function (btn) {
            btn.addEventListener('click', function () {
                var poiId = btn.getAttribute('data-poi-id');
                AT.Services.POI.approve(poiId).then(function () {
                    UI.showToast('Đã duyệt POI ' + poiId, 'success');
                    renderPendingPOIs();
                }).catch(function (err) {
                    UI.showToast('Lỗi duyệt POI: ' + (err.message || err), 'error');
                });
            });
        });

        // Reject buttons
        tbody.querySelectorAll('.btn-reject-poi').forEach(function (btn) {
            btn.addEventListener('click', function () {
                _rejectTargetId = btn.getAttribute('data-poi-id');
                var reasonInput = document.getElementById('reject-reason');
                if (reasonInput) reasonInput.value = '';
                UI.showModal('modal-reject-poi');
            });
        });
    }

    /** Show POI detail modal — fetches fresh data from API */
    function showPoiDetail(poiId) {
        AT.Services.POI.getById(poiId).then(function (poi) {
            if (!poi) {
                UI.showToast('Không tìm thấy thông tin POI', 'error');
                return;
            }

            // --- Cover Image ---
            var coverEl = document.getElementById('poi-detail-cover');
            if (coverEl) {
                var coverRawUrl = poi.imageUrl;
                if (coverRawUrl) {
                    coverEl.innerHTML = '';
                    var img = document.createElement('img');
                    img.src = '/images/placeholder-poi.png';
                    img.alt = poi.poiName || 'POI';
                    img.style.cssText = 'max-width:100%;max-height:300px;border-radius:12px;object-fit:cover;';
                    coverEl.appendChild(img);
                    AT.Core.ApiClient.loadNgrokImage(img, coverRawUrl);
                } else {
                    coverEl.innerHTML = '<div style="padding:30px;color:var(--text-dim);background:var(--card-bg);border-radius:12px;">' +
                        '<i class="fa-solid fa-image" style="font-size:32px;display:block;margin-bottom:8px;"></i>Chưa có hình ảnh</div>';
                }
            }

            // --- Info Grid (using textContent for safety) ---
            var infoEl = document.getElementById('poi-detail-info');
            if (infoEl) {
                infoEl.innerHTML = '';
                var fields = [
                    { label: 'Mã POI', value: poi.poiId || '—' },
                    { label: 'Tên POI', value: poi.poiName || '—' },
                    { label: 'Trạng thái', value: poi.statusText || poi.status || '—', badge: true },
                    { label: 'Hoạt động', value: poi.isActive ? 'Có' : 'Không' },
                    { label: 'Vĩ độ (Lat)', value: poi.latitude != null ? poi.latitude : '—' },
                    { label: 'Kinh độ (Lng)', value: poi.longitude != null ? poi.longitude : '—' },
                    { label: 'Gói đăng ký', value: poi.packageName || ('ID: ' + (poi.packageId || '—')) },
                    { label: 'Chủ sở hữu', value: poi.ownerName || ('ID: ' + (poi.ownerId || '—')) },
                    { label: 'Email chủ', value: poi.ownerEmail || '—' },
                    { label: 'Ngày tạo', value: Fmt.dateTime(poi.createdDate) },
                    { label: 'Ngày cập nhật', value: Fmt.dateTime(poi.updatedDate) }
                ];

                fields.forEach(function (f) {
                    var div = document.createElement('div');
                    var strong = document.createElement('strong');
                    strong.style.cssText = 'color:var(--text-dim);display:block;margin-bottom:2px;';
                    strong.textContent = f.label;
                    div.appendChild(strong);

                    if (f.badge) {
                        var span = document.createElement('span');
                        var st = (poi.status || '').toLowerCase();
                        var cls = st === 'approved' ? 'status-approved' : st === 'rejected' ? 'status-rejected' : 'status-browsing';
                        span.className = 'status-badge ' + cls;
                        span.textContent = f.value;
                        div.appendChild(span);
                    } else {
                        var text = document.createTextNode(String(f.value));
                        div.appendChild(text);
                    }
                    infoEl.appendChild(div);
                });
            }

            // --- Description ---
            var descEl = document.getElementById('poi-detail-description');
            if (descEl) {
                descEl.textContent = poi.descriptionVi || 'Chưa có mô tả';
            }

            // --- Image Gallery ---
            var gallerySection = document.getElementById('poi-detail-gallery-section');
            var galleryEl = document.getElementById('poi-detail-gallery');
            if (galleryEl && gallerySection) {
                galleryEl.innerHTML = '';
                var images = poi.images || [];
                if (images.length === 0) {
                    gallerySection.style.display = 'none';
                } else {
                    gallerySection.style.display = '';
                    images.forEach(function (imgData) {
                        var rawUrl = (typeof imgData === 'object' && imgData.imageUrl) ? imgData.imageUrl : imgData;
                        if (!rawUrl) return;
                        var resolvedUrl = resolveImageUrl(rawUrl);
                        var img = document.createElement('img');
                        img.src = '/images/placeholder-poi.png';
                        img.alt = 'POI Image';
                        img.style.cssText = 'width:100px;height:80px;object-fit:cover;border-radius:8px;border:1px solid var(--border);cursor:pointer;';
                        img.addEventListener('click', function () {
                            window.open(resolvedUrl, '_blank');
                        });
                        galleryEl.appendChild(img);
                        AT.Core.ApiClient.loadNgrokImage(img, rawUrl);
                    });
                }
            }

            // --- Action Buttons (only for pending) ---
            var actionsEl = document.getElementById('poi-detail-actions');
            if (actionsEl) {
                actionsEl.innerHTML = '';
                var status = (poi.status || '').toLowerCase();
                if (status === 'pending') {
                    var btnApprove = document.createElement('button');
                    btnApprove.className = 'btn-primary';
                    btnApprove.style.cssText = 'padding:10px 24px;font-size:13px;';
                    btnApprove.innerHTML = '<i class="fa-solid fa-check"></i> Duyệt';
                    btnApprove.addEventListener('click', function () {
                        AT.Services.POI.approve(poi.poiId).then(function () {
                            UI.showToast('Đã duyệt POI ' + poi.poiId, 'success');
                            UI.hideModal('modal-poi-detail');
                            renderPendingPOIs();
                        }).catch(function (err) {
                            UI.showToast('Lỗi duyệt: ' + (err.message || err), 'error');
                        });
                    });

                    var btnReject = document.createElement('button');
                    btnReject.className = 'btn-danger-ghost';
                    btnReject.style.cssText = 'padding:10px 24px;font-size:13px;';
                    btnReject.innerHTML = '<i class="fa-solid fa-xmark"></i> Từ chối';
                    btnReject.addEventListener('click', function () {
                        UI.hideModal('modal-poi-detail');
                        _rejectTargetId = poi.poiId;
                        var reasonInput = document.getElementById('reject-reason');
                        if (reasonInput) reasonInput.value = '';
                        UI.showModal('modal-reject-poi');
                    });

                    actionsEl.appendChild(btnApprove);
                    actionsEl.appendChild(btnReject);
                } else {
                    var infoSpan = document.createElement('span');
                    infoSpan.style.cssText = 'font-size:13px;color:var(--text-dim);';
                    infoSpan.textContent = 'POI đã được xử lý (' + (poi.statusText || status) + ')';
                    actionsEl.appendChild(infoSpan);
                }
            }

            UI.showModal('modal-poi-detail');
        }).catch(function (err) {
            console.error('[POIApproval] Error loading POI detail:', err);
            UI.showToast('Không thể tải chi tiết POI: ' + (err.message || err), 'error');
        });
    }

    var _eventsBound = false;
    function bindEvents() {
        if (_eventsBound) return;
        _eventsBound = true;

        // Reject confirm
        var btnConfirmReject = document.getElementById('btn-confirm-reject');
        if (btnConfirmReject) {
            btnConfirmReject.addEventListener('click', function () {
                if (!_rejectTargetId) return;
                var reason = document.getElementById('reject-reason');
                AT.Services.POI.reject(_rejectTargetId, reason ? reason.value : '').then(function () {
                    UI.showToast('Đã từ chối POI ' + _rejectTargetId, 'warning');
                    UI.hideModal('modal-reject-poi');
                    _rejectTargetId = null;
                    renderPendingPOIs();
                }).catch(function (err) {
                    UI.showToast('Lỗi từ chối POI: ' + (err.message || err), 'error');
                });
            });
        }

        // Reject cancel
        var btnCancelReject = document.getElementById('btn-cancel-reject');
        if (btnCancelReject) {
            btnCancelReject.addEventListener('click', function () {
                UI.hideModal('modal-reject-poi');
                _rejectTargetId = null;
            });
        }

        // POI detail close
        var btnCloseDetail = document.getElementById('btn-close-poi-detail');
        if (btnCloseDetail) {
            btnCloseDetail.addEventListener('click', function () {
                UI.hideModal('modal-poi-detail');
            });
        }

        // Close modal on overlay click
        var detailOverlay = document.getElementById('modal-poi-detail');
        if (detailOverlay) {
            detailOverlay.addEventListener('click', function (e) {
                if (e.target === detailOverlay) {
                    UI.hideModal('modal-poi-detail');
                }
            });
        }
    }
})();

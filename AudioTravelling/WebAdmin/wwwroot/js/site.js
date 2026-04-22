/* ══════════════════════════════════════
   Audio Travelling — Site JS Utilities
   Modal, Toast, Drawer, Confirmation
   ══════════════════════════════════════ */

// ── Modal Functions ──
function openModal(modalId) {
    const modal = document.getElementById(modalId);
    if (modal) {
        modal.classList.add('show');
        document.body.style.overflow = 'hidden';
    }
}

function closeModal(modalId) {
    const modal = document.getElementById(modalId);
    if (modal) {
        modal.classList.remove('show');
        document.body.style.overflow = '';
    }
}

// Close modal when clicking overlay
document.addEventListener('click', function (e) {
    if (e.target.classList.contains('modal-overlay') && e.target.classList.contains('show')) {
        e.target.classList.remove('show');
        document.body.style.overflow = '';
    }
});

// ── Drawer Functions ──
function openDrawer(drawerId) {
    const overlay = document.getElementById(drawerId + '-overlay');
    const panel = document.getElementById(drawerId);
    if (overlay) overlay.classList.add('show');
    if (panel) panel.classList.add('show');
    document.body.style.overflow = 'hidden';
}

function closeDrawer(drawerId) {
    const overlay = document.getElementById(drawerId + '-overlay');
    const panel = document.getElementById(drawerId);
    if (panel) panel.classList.remove('show');
    setTimeout(() => {
        if (overlay) overlay.classList.remove('show');
        document.body.style.overflow = '';
    }, 350);
}

// Close drawer when clicking overlay
document.addEventListener('click', function (e) {
    if (e.target.classList.contains('drawer-overlay') && e.target.classList.contains('show')) {
        const drawerId = e.target.id.replace('-overlay', '');
        closeDrawer(drawerId);
    }
});

// ── Toast Notification ──
function showToast(message, type = 'success', duration = 3000) {
    const toast = document.createElement('div');
    toast.className = `toast ${type}`;
    const icon = type === 'success' ? 'fa-circle-check' : 'fa-circle-exclamation';
    toast.innerHTML = `<i class="fa-solid ${icon}"></i> <span>${message}</span>`;
    document.body.appendChild(toast);

    setTimeout(() => {
        toast.style.animation = 'toastSlideIn 0.3s ease-out reverse';
        setTimeout(() => toast.remove(), 300);
    }, duration);
}

// ── Confirm Dialog ──
function confirmAction(message, onConfirm) {
    // Create a simple confirm modal
    const id = 'confirm-dialog-' + Date.now();
    const html = `
        <div class="modal-overlay show" id="${id}">
            <div class="modal-content" style="max-width:420px;">
                <div class="modal-header">
                    <h3 class="modal-title">
                        <i class="fa-solid fa-triangle-exclamation" style="color:#fbbf24;"></i>
                        Xác nhận
                    </h3>
                    <button class="modal-close" onclick="closeModal('${id}');document.getElementById('${id}').remove();">
                        <i class="fa-solid fa-times"></i>
                    </button>
                </div>
                <div class="modal-body">
                    <p style="font-size:14px; color:var(--text-primary); line-height:1.6;">${message}</p>
                </div>
                <div class="modal-footer">
                    <button class="btn-ghost" onclick="closeModal('${id}');document.getElementById('${id}').remove();">
                        Hủy
                    </button>
                    <button class="btn-primary" id="${id}-confirm">
                        <i class="fa-solid fa-check"></i> Xác nhận
                    </button>
                </div>
            </div>
        </div>
    `;
    document.body.insertAdjacentHTML('beforeend', html);
    document.getElementById(`${id}-confirm`).addEventListener('click', function () {
        closeModal(id);
        document.getElementById(id).remove();
        if (typeof onConfirm === 'function') onConfirm();
    });
}

// ── Toggle Active Status ──
function toggleActive(entityType, id, currentStatus) {
    const action = currentStatus ? 'tắt' : 'bật';
    confirmAction(
        `Bạn có chắc muốn ${action} trạng thái hoạt động?`,
        function () {
            showToast(`Đã ${action} thành công!`, 'success');
            // In real app: call API here
        }
    );
}

// ── Escape key closes modals/drawers ──
document.addEventListener('keydown', function (e) {
    if (e.key === 'Escape') {
        // Close any open modals
        document.querySelectorAll('.modal-overlay.show').forEach(m => {
            m.classList.remove('show');
        });
        // Close any open drawers
        document.querySelectorAll('.drawer-panel.show').forEach(d => {
            closeDrawer(d.id);
        });
        document.body.style.overflow = '';
    }
});

// ── Table Row Selection ──
document.addEventListener('DOMContentLoaded', function () {
    // Add click handler for table rows
    document.querySelectorAll('.data-table tbody tr[data-id]').forEach(row => {
        row.style.cursor = 'pointer';
    });
});

console.log('✅ Audio Travelling — Site JS loaded');

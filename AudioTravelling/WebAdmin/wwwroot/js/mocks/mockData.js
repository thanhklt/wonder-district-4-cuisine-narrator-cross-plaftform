/**
 * ═══════════════════════════════════════════════════
 * Audio Travelling — Mock Data
 * TODO: Replace mock data with real API calls.
 * ═══════════════════════════════════════════════════
 */
(function () {
    'use strict';

    var AT = window.AudioTravelling = window.AudioTravelling || {};
    AT.Mocks = AT.Mocks || {};

    // ── Mock POIs ──
    // TODO: Replace mock POI data with real API (GET /api/owner/pois, GET /api/admin/pois/pending)
    AT.Mocks.POIs = [
        { id: 'POI-001', name: 'Phở Hòa Pasteur', description: 'Quán phở nổi tiếng nhất Sài Gòn từ năm 1968.', lat: 10.7845, lng: 106.6912, owner: 'owner@audiotravelling.com', status: 'approved', audioUrl: null, createdAt: '2026-03-10T08:00:00', updatedAt: '2026-03-12T10:00:00', rejectionReason: null },
        { id: 'POI-002', name: 'Bánh Mì 362', description: 'Bánh mì nổi tiếng đường Nguyễn Trãi.', lat: 10.7555, lng: 106.6673, owner: 'owner@audiotravelling.com', status: 'pending', audioUrl: null, createdAt: '2026-03-15T09:00:00', updatedAt: '2026-03-15T09:00:00', rejectionReason: null },
        { id: 'POI-003', name: 'Cơm Tấm Bụi', description: 'Cơm tấm sườn bì chả truyền thống.', lat: 10.7980, lng: 106.6820, owner: 'owner@audiotravelling.com', status: 'draft', audioUrl: null, createdAt: '2026-03-20T14:00:00', updatedAt: '2026-03-20T14:00:00', rejectionReason: null },
        { id: 'POI-004', name: 'Bún Chả Hà Nội', description: 'Bún chả chuẩn Hà Nội giữa lòng Sài Gòn.', lat: 10.7760, lng: 106.7005, owner: 'owner@audiotravelling.com', status: 'rejected', audioUrl: null, createdAt: '2026-03-08T11:00:00', updatedAt: '2026-03-10T15:00:00', rejectionReason: 'Thông tin mô tả chưa đầy đủ, vui lòng bổ sung hình ảnh và script audio.' },
        { id: 'POI-005', name: 'Chè Thái Mango', description: 'Chè Thái và các món tráng miệng đặc sắc.', lat: 10.7680, lng: 106.6940, owner: 'owner2@audiotravelling.com', status: 'pending', audioUrl: null, createdAt: '2026-03-22T10:00:00', updatedAt: '2026-03-22T10:00:00', rejectionReason: null },
        { id: 'POI-006', name: 'Hủ Tiếu Nam Vang', description: 'Hủ tiếu truyền thống với nước dùng đậm đà.', lat: 10.7830, lng: 106.6960, owner: 'owner2@audiotravelling.com', status: 'approved', audioUrl: null, createdAt: '2026-02-28T08:00:00', updatedAt: '2026-03-05T10:00:00', rejectionReason: null }
    ];

    // ── Mock QR Codes ──
    // TODO: Replace mock QR data with real API (GET /api/admin/qr)
    AT.Mocks.QRCodes = [
        { id: 'QR-001', status: 'active', scanCount: 342, createdAt: '2026-02-01', lastScanned: '2026-04-30T15:30:00' },
        { id: 'QR-002', status: 'active', scanCount: 189, createdAt: '2026-02-15', lastScanned: '2026-04-30T14:20:00' },
        { id: 'QR-003', status: 'active', scanCount: 567, createdAt: '2026-01-20', lastScanned: '2026-04-30T16:00:00' },
        { id: 'QR-004', status: 'disabled', scanCount: 78, createdAt: '2026-03-01', lastScanned: '2026-03-28T09:00:00' },
        { id: 'QR-005', status: 'active', scanCount: 423, createdAt: '2026-01-10', lastScanned: '2026-04-30T12:45:00' }
    ];



    // ── Mock Heatmap Data (lat, lng, intensity) ──
    // TODO: Replace mock heatmap data with real API (GET /api/admin/heatmap)
    AT.Mocks.HeatmapData = [
        [10.7845, 106.6912, 0.9],  // Phở Hòa - hot spot
        [10.7555, 106.6673, 0.6],  // Bánh Mì 362
        [10.7980, 106.6820, 0.3],  // Cơm Tấm Bụi
        [10.7760, 106.7005, 0.4],  // Bún Chả
        [10.7680, 106.6940, 0.7],  // Chè Thái
        [10.7830, 106.6960, 0.8],  // Hủ Tiếu Nam Vang
        [10.7700, 106.6950, 0.5],  // Additional area
        [10.7750, 106.6900, 0.6],
        [10.7800, 106.6880, 0.4],
        [10.7820, 106.6930, 0.7],
        [10.7860, 106.6950, 0.5],
        [10.7780, 106.6970, 0.3],
        [10.7730, 106.6890, 0.8],
        [10.7710, 106.6920, 0.6],
        [10.7850, 106.6940, 0.9]
    ];

    // ── Mock Dashboard Stats ──
    // TODO: Replace with real API (GET /api/admin/dashboard/active-users)
    AT.Mocks.DashboardStats = {
        activeUsers: 23,
        totalScansToday: 145,
        totalPOIs: 6,
        pendingPOIs: 2
    };

})();

/**
* ═══════════════════════════════════════════════════
* Audio Travelling — Exhibition Edition
* Application Logic: SPA, Booths, Finance, WaveSurfer, Leaflet
* ═══════════════════════════════════════════════════
*/
document.addEventListener('DOMContentLoaded', () => {
    // ── State Trackers ──
    let mapInitialized = false;
    let chartInitialized = false;
    let selectedBoothId = null;
    const emerald = '#22c55e';
    window.myMaps = {}; // Track map instances to prevent re-initialization crashes

    // Hàm tiện ích đặc biệt cho SPA: Lấy đúng Element của màn hình đang Active
    function getActiveEl(id) {
        const activeView = document.querySelector('.view-section.active');
        if (activeView) {
            const el = activeView.querySelector(`[id="${id}"]`);
            if (el) return el;
        }
        return document.getElementById(id); // Dự phòng
    }

    // Clear old mock data for VNĐ migration validation
    if (!localStorage.getItem('exhib_vn_migrated')) {
        localStorage.removeItem('exhib_tierConfig');
        localStorage.removeItem('exhib_booths');
        localStorage.removeItem('exhib_orders');
        localStorage.removeItem('exhib_fairs');
        localStorage.removeItem('exhib_plans');
        localStorage.setItem('exhib_vn_migrated', 'true');
    }

    /* ══════════════════════════════════════════════
       1. MOCK DATA — Fairs, Booths, Orders, Tiers
       ══════════════════════════════════════════════ */

    const defaultTierConfig = {
        Basic: { rc: 0.10, fee: 35000, weight: 1 },
        Gold: { rc: 0.08, fee: 25000, weight: 2 },
        Diamond: { rc: 0.05, fee: 15000, weight: 3 }
    };

    function getTierConfig() {
        const raw = localStorage.getItem('exhib_tierConfig');
        if (raw) try { return JSON.parse(raw); } catch (e) { }
        return { ...defaultTierConfig };
    }

    function saveTierConfig(cfg) {
        localStorage.setItem('exhib_tierConfig', JSON.stringify(cfg));
    }

    const defaultBooths = [
        { id: 'B001', name: 'TechVision Pro', company: 'VisionTech Corp', category: 'Technology', tier: 'Diamond', lat: 10.7721, lng: 106.6980, radius: 25, script: 'Welcome to TechVision Pro — the future of immersive display technology! Our booth showcases the latest 8K MicroLED panels and holographic projection systems. Step closer to experience crystal-clear visuals that redefine what screens can do.', engagement: 4200 },
        { id: 'B002', name: 'Green Harvest Organics', company: 'EcoFarms Ltd.', category: 'Food & Beverage', tier: 'Gold', lat: 10.7715, lng: 106.6975, radius: 20, script: 'Discover the taste of nature at Green Harvest Organics! We partner with local Vietnamese farms to bring you 100% certified organic produce, cold-pressed juices, and sustainable snacks. Try our signature Dragon Fruit Elixir!', engagement: 3100 },
        { id: 'B003', name: 'Silk & Thread Atelier', company: 'Viet Couture', category: 'Fashion', tier: 'Gold', lat: 10.7718, lng: 106.6985, radius: 18, script: 'Step into the world of Silk & Thread Atelier by Viet Couture. Our collection blends traditional Vietnamese silk weaving with modern European cuts. Each piece tells a story of heritage meeting contemporary elegance.', engagement: 2800 },
        { id: 'B004', name: 'MindWell Therapy', company: 'NeuroCalm Inc.', category: 'Health & Wellness', tier: 'Basic', lat: 10.7725, lng: 106.6990, radius: 15, script: 'Welcome to MindWell Therapy by NeuroCalm. Experience our guided meditation pods powered by binaural audio technology. Take a 5-minute session and feel the difference in your stress levels instantly.', engagement: 1500 },
        { id: 'B005', name: 'EduSpark Academy', company: 'LearnBright Global', category: 'Education', tier: 'Basic', lat: 10.7712, lng: 106.6978, radius: 22, script: 'EduSpark Academy presents interactive STEM learning stations for all ages. Build a robot, code a game, or explore virtual chemistry labs. Education has never been this much fun!', engagement: 1900 },
        { id: 'B006', name: 'AutoDrive Concept', company: 'ElectraMotors', category: 'Automotive', tier: 'Diamond', lat: 10.7728, lng: 106.6970, radius: 30, script: 'Experience the future of mobility at AutoDrive Concept by ElectraMotors. Sit inside our fully autonomous EV prototype and take a virtual test drive through Ho Chi Minh City streets. Zero emissions, infinite possibilities.', engagement: 5100 },
        { id: 'B007', name: 'Artisan Gallery', company: 'Mekong Arts Collective', category: 'Art & Culture', tier: 'Gold', lat: 10.7720, lng: 106.6965, radius: 20, script: 'The Artisan Gallery by Mekong Arts Collective features handcrafted lacquerware, woodblock prints, and contemporary Vietnamese art. Meet the artists and see live demonstrations of traditional techniques passed down through generations.', engagement: 2400 },
        { id: 'B008', name: 'CloudSync Solutions', company: 'DataBridge Tech', category: 'Technology', tier: 'Basic', lat: 10.7710, lng: 106.6988, radius: 15, script: 'CloudSync Solutions helps businesses migrate to the cloud seamlessly. Visit our booth to see live demos of our zero-downtime migration tools and talk to our enterprise solutions team.', engagement: 1200 }
    ];

    const defaultOrders = [
        { id: 'ORD-1001', boothId: 'B001', product: 'Màn hình 8K MicroLED 32"', qty: 2, price: 4500000, status: 'success', date: '2026-03-10' },
        { id: 'ORD-1002', boothId: 'B001', product: 'Máy chiếu Holo Mini', qty: 5, price: 890000, status: 'pending', date: '2026-03-10' },
        { id: 'ORD-1003', boothId: 'B002', product: 'Giỏ quà hữu cơ', qty: 15, price: 450000, status: 'canceled', date: '2026-03-09' }
    ];

    const defaultFairs = [
        { id: 'F001', name: 'Saigon Tech Expo 2026', status: 'active', date: '2026-03-01' },
        { id: 'F002', name: 'Vietnam Food Festival', status: 'active', date: '2026-03-05' }
    ];

    function getBooths() {
        const raw = localStorage.getItem('exhib_booths');
        if (raw) try { return JSON.parse(raw); } catch (e) { }
        localStorage.setItem('exhib_booths', JSON.stringify(defaultBooths));
        return [...defaultBooths];
    }

    function saveBooths(booths) {
        localStorage.setItem('exhib_booths', JSON.stringify(booths));
    }

    function getOrders() {
        const raw = localStorage.getItem('exhib_orders');
        if (raw) try { return JSON.parse(raw); } catch (e) { }
        localStorage.setItem('exhib_orders', JSON.stringify(defaultOrders));
        return [...defaultOrders];
    }

    function getFairs() {
        const raw = localStorage.getItem('exhib_fairs');
        if (raw) try { return JSON.parse(raw); } catch (e) { }
        localStorage.setItem('exhib_fairs', JSON.stringify(defaultFairs));
        return [...defaultFairs];
    }

    const defaultPlans = [
        {
            id: 'basic',
            name: 'Gói BASIC (Khởi nghiệp)',
            price: 0, // Giá mẫu
            target: 'Sạp hàng nhỏ, mới tham gia.',
            features: [
                { text: 'Phí hoa hồng: 10% (Cao nhất)', active: true },
                { text: 'Phí giao dịch: 5,000 VND', active: true },
                { text: 'Vị trí hiển thị: Không ưu tiên', active: true },
                { text: 'Thuyết minh Audio: Tiêu chuẩn (1 phút)', active: true },
                { text: 'Hỗ trợ AI: Tối ưu kịch bản 10 lần/tháng', active: true },
                { text: 'Bán kính GPS: 15m (Phải đứng sát mới hiện)', active: true }
            ]
        },
        {
            id: 'gold',
            name: 'Gói GOLD (Kinh doanh)',
            price: 499000, // Giá mẫu
            target: 'Hộ kinh doanh chuyên nghiệp.',
            features: [
                { text: 'Phí hoa hồng: 8% (Trung bình)', active: true },
                { text: 'Phí giao dịch: 3,000 VND', active: true },
                { text: 'Vị trí hiển thị: Ưu tiên Top 10', active: true },
                { text: 'Thuyết minh Audio: Tiêu chuẩn (2 phút)', active: true },
                { text: 'Hỗ trợ AI: Tối ưu kịch bản 15 lần/tháng', active: true },
                { text: 'Bán kính GPS: 30m (Phát hiện từ xa)', active: true }
            ]
        },
        {
            id: 'diamond',
            name: 'Gói DIAMOND (Đẳng cấp)',
            price: 999000, // Giá mẫu
            target: 'Doanh nghiệp lớn, thương hiệu.',
            features: [
                { text: 'Phí hoa hồng: 5% (Thấp nhất)', active: true },
                { text: 'Phí giao dịch: 1,000 VND', active: true },
                { text: 'Vị trí hiển thị: Luôn nằm trong Top 3', active: true },
                { text: 'Thuyết minh Audio: Chất lượng cao (Không giới hạn)', active: true },
                { text: 'Hỗ trợ AI: Tối ưu không giới hạn', active: true },
                { text: 'Bán kính GPS: 50m (Kéo khách từ sảnh hội chợ)', active: true }
            ]
        }
    ];

    function getPlans() {
        // CẬP NHẬT MỚI: Bỏ qua việc check LocalStorage cũ, trực tiếp ghi đè dữ liệu mới vào luôn
        localStorage.setItem('exhib_plans', JSON.stringify(defaultPlans));
        return [...defaultPlans];
    }

    function savePlans(plans) {
        localStorage.setItem('exhib_plans', JSON.stringify(plans));
    }

    /* ══════════════════════════════════════════════
       2. AUTH — Login / Register / Logout
       ══════════════════════════════════════════════ */
    const authScreen = document.getElementById('auth-screen');
    const dashboardApp = document.getElementById('dashboard-app');
    const btnLogout = document.getElementById('btn-logout');

    function getAccounts() {
        const defaults = [
            { name: 'Admin User', email: 'admin@audiotravelling.com', phone: '+84 123 456 789', password: btoa('admin123') },
            { name: 'Booth Owner', email: 'owner@audiotravelling.com', phone: '+84 987 654 321', password: btoa('owner123') }
        ];

        const raw = localStorage.getItem('exhib_accounts');
        let accounts = [];
        if (raw) {
            try {
                accounts = JSON.parse(raw);
                // Ensure defaults exist and have correct passwords in the cached accounts
                defaults.forEach(def => {
                    const existing = accounts.find(a => a.email.toLowerCase() === def.email.toLowerCase());
                    if (!existing) {
                        accounts.push(def);
                    } else {
                        existing.password = def.password; // Force reset password to default
                    }
                });
                localStorage.setItem('exhib_accounts', JSON.stringify(accounts));
                return accounts;
            } catch (e) { }
        }

        localStorage.setItem('exhib_accounts', JSON.stringify(defaults));
        return defaults;
    }

    function saveAccounts(accounts) {
        localStorage.setItem('exhib_accounts', JSON.stringify(accounts));
    }

    const tabLogin = document.getElementById('tab-login');
    const tabRegister = document.getElementById('tab-register');
    const tabIndicator = document.getElementById('auth-tab-indicator');
    const formLogin = document.getElementById('form-login');
    const formRegister = document.getElementById('form-register');

    function switchAuthTab(tab) {
        const isLogin = tab === 'login';
        if (tabLogin) tabLogin.classList.toggle('active', isLogin);
        if (tabRegister) tabRegister.classList.toggle('active', !isLogin);
        if (tabIndicator) tabIndicator.classList.toggle('right', !isLogin);
        if (formLogin) formLogin.classList.toggle('active', isLogin);
        if (formRegister) formRegister.classList.toggle('active', !isLogin);
        document.getElementById('login-error')?.classList.add('hidden');
        document.getElementById('register-error')?.classList.add('hidden');
        document.getElementById('register-success')?.classList.add('hidden');
    }

    tabLogin?.addEventListener('click', () => switchAuthTab('login'));
    tabRegister?.addEventListener('click', () => switchAuthTab('register'));

    document.querySelectorAll('.auth-toggle-pw').forEach(btn => {
        btn.addEventListener('click', () => {
            const targetId = btn.getAttribute('data-target');
            const input = document.getElementById(targetId);
            const icon = btn.querySelector('i');
            if (input.type === 'password') {
                input.type = 'text';
                icon.className = 'fa-solid fa-eye-slash';
            } else {
                input.type = 'password';
                icon.className = 'fa-solid fa-eye';
            }
        });
    });

    const savedEmail = localStorage.getItem('exhib_rememberEmail');
    if (savedEmail && document.getElementById('login-email')) {
        document.getElementById('login-email').value = savedEmail;
        document.getElementById('login-remember').checked = true;
    }

    function doLogin(name, email) {
        const role = email.toLowerCase() === 'admin@audiotravelling.com' ? 'Admin' : 'Owner';
        localStorage.setItem('exhib_session', JSON.stringify({ name, email, loggedIn: true, role }));
        if (document.getElementById('sidebar-name')) document.getElementById('sidebar-name').textContent = name;
        if (authScreen) authScreen.style.display = 'none';
        if (dashboardApp) dashboardApp.style.display = 'block';

        // Reset chart so it re-initialises on the correct canvas for this role
        chartInitialized = false;
        if (window.dashboardChart && typeof window.dashboardChart.destroy === 'function') {
            window.dashboardChart.destroy();
            window.dashboardChart = null;
        }

        // Hide/Show menu items based on role
        document.querySelectorAll('.nav-item').forEach(el => {
            if (el.getAttribute('data-role') && el.getAttribute('data-role') !== role && role !== 'Admin') {
                el.style.display = 'none';
            } else {
                el.style.display = 'block';
            }
        });

        switchView('view-dashboard');
        showToast(`Welcome back, ${name.split(' ')[0]}!`, 'success');
    }

    formLogin?.addEventListener('submit', e => {
        e.preventDefault();
        const email = document.getElementById('login-email').value.trim();
        const password = document.getElementById('login-password').value;
        const remember = document.getElementById('login-remember').checked;
        const errorEl = document.getElementById('login-error');
        const errorText = document.getElementById('login-error-text');
        const btn = document.getElementById('btn-login');

        errorEl.classList.add('hidden');

        const accounts = getAccounts();
        const account = accounts.find(a => a.email.toLowerCase() === email.toLowerCase());

        if (!account || account.password !== btoa(password)) {
            errorText.textContent = !account ? 'No account found with this email address.' : 'Incorrect password.';
            errorEl.classList.remove('hidden');
            return;
        }

        if (remember) localStorage.setItem('exhib_rememberEmail', email);
        else localStorage.removeItem('exhib_rememberEmail');

        btn.disabled = true;
        btn.querySelector('.auth-btn-text').textContent = 'Signing in...';
        setTimeout(() => {
            btn.disabled = false;
            btn.querySelector('.auth-btn-text').textContent = 'Sign In';
            doLogin(account.name, account.email);
        }, 800);
    });

    btnLogout?.addEventListener('click', () => {
        localStorage.removeItem('exhib_session');
        if (dashboardApp) dashboardApp.style.display = 'none';
        if (authScreen) authScreen.style.display = '';
        if (formLogin) formLogin.reset();
        switchAuthTab('login');
        if (wavesurfer && wavesurfer.isPlaying()) wavesurfer.stop();
    });

    const existingSession = localStorage.getItem('exhib_session');
    if (existingSession) {
        try {
            const session = JSON.parse(existingSession);
            if (session.loggedIn) {
                if (authScreen) authScreen.style.display = 'none';
                if (dashboardApp) dashboardApp.style.display = 'block';
                if (document.getElementById('sidebar-name')) document.getElementById('sidebar-name').textContent = session.name;

                // Role visibility
                document.querySelectorAll('.nav-item').forEach(el => {
                    if (el.getAttribute('data-role') && el.getAttribute('data-role') !== session.role && session.role !== 'Admin') {
                        el.style.display = 'none';
                    } else {
                        el.style.display = 'block';
                    }
                });

                setTimeout(() => switchView('view-dashboard'), 50);
            }
        } catch (e) { }
    }

    /* ══════════════════════════════════════════════
       3. SPA NAVIGATION
       ══════════════════════════════════════════════ */
    const navLinks = document.querySelectorAll('#sidebar-nav .nav-link[data-target]');
    const headerTitle = document.getElementById('header-title');

    // ==========================================
    // TÀI CHÍNH - OWNER
    // ==========================================
    function renderFinanceOwner() {
        const container = getActiveEl('view-finance-owner');
        if (!container) return;

        const ownerBoothIds = ['B001', 'B002'];
        const booths = getBooths();
        const orders = getOrders().filter(o => ownerBoothIds.includes(o.boothId) && o.status === 'success');
        const tierCfg = getTierConfig();

        let totalRevenue = 0;
        let totalCommission = 0;
        let rowsHtml = '';

        orders.forEach(o => {
            const booth = booths.find(b => b.id === o.boothId);
            const cfg = booth ? (tierCfg[booth.tier] || tierCfg.Basic) : tierCfg.Basic;

            const revenue = o.price * o.qty;
            const commission = (revenue * cfg.rc) + cfg.fee;
            const net = revenue - commission;

            totalRevenue += revenue;
            totalCommission += commission;

            rowsHtml += `
            <tr style="border-bottom: 1px solid var(--border);">
                <td style="padding:10px; font-weight:500;">${o.id}</td>
                <td style="padding:10px;">${o.product} <span style="color:var(--text-dim); font-size:11px;">(x${o.qty})</span></td>
                <td style="padding:10px; color:var(--emerald);">${formatCurrency(revenue)} ₫</td>
                <td style="padding:10px; color:#ef4444;">-${formatCurrency(commission)} ₫</td>
                <td style="padding:10px; font-weight:600;">${formatCurrency(net)} ₫</td>
                <td style="padding:10px; color:var(--text-dim); font-size:12px;">${o.date}</td>
            </tr>
        `;
        });

        const tbody = document.getElementById('finance-owner-orders-body');
        if (tbody) {
            tbody.innerHTML = orders.length === 0
                ? `<tr><td colspan="6" style="text-align:center; padding:20px; color:var(--text-dim);">Chưa có đơn hàng thành công nào</td></tr>`
                : rowsHtml;
        }

        const netIncome = totalRevenue - totalCommission;
        let withdrawals = JSON.parse(localStorage.getItem('exhib_withdrawals') || '[]');
        let pendingAmount = withdrawals.filter(w => w.status === 'pending').reduce((sum, w) => sum + w.amount, 0);
        let withdrawnAmount = withdrawals.filter(w => w.status === 'success').reduce((sum, w) => sum + w.amount, 0);
        let availableBalance = netIncome - pendingAmount - withdrawnAmount;

        document.getElementById('finance-owner-revenue').textContent = formatCurrency(totalRevenue) + ' ₫';
        document.getElementById('finance-owner-commission').textContent = formatCurrency(totalCommission) + ' ₫';
        document.getElementById('finance-owner-balance').textContent = formatCurrency(availableBalance) + ' ₫';
        document.getElementById('finance-owner-pending').textContent = formatCurrency(pendingAmount) + ' ₫';
        if (document.getElementById('withdraw-max-text')) document.getElementById('withdraw-max-text').textContent = formatCurrency(availableBalance) + ' ₫';

        const historyList = document.getElementById('withdraw-history-list');
        if (historyList) {
            if (withdrawals.length === 0) {
                historyList.innerHTML = `<p style="font-size:12px; color:var(--text-dim); text-align:center;">Chưa có lịch sử rút tiền</p>`;
            } else {
                historyList.innerHTML = withdrawals.map(w => `
                <div style="display:flex; justify-content:space-between; align-items:center; padding:10px 0; border-bottom:1px solid rgba(255,255,255,0.05);">
                    <div>
                        <p style="font-size:13px; font-weight:600;">${formatCurrency(w.amount)} ₫</p>
                        <p style="font-size:11px; color:var(--text-dim); margin-top:2px;">
                            ${w.date} • ${w.bank.toUpperCase()} (*${w.account.slice(-4)})
                        </p>
                    </div>
                    <span style="font-size:11px; padding:4px 8px; border-radius:6px; background:${w.status === 'pending' ? 'rgba(251,191,36,0.1)' : 'rgba(34,197,94,0.1)'}; color:${w.status === 'pending' ? '#fbbf24' : 'var(--emerald)'};">
                        ${w.status === 'pending' ? 'Đang chờ' : 'Thành công'}
                    </span>
                </div>
            `).join('');
            }
        }

        const btnWithdraw = document.getElementById('btn-withdraw');
        if (btnWithdraw) {
            const newBtn = btnWithdraw.cloneNode(true);
            btnWithdraw.parentNode.replaceChild(newBtn, btnWithdraw);

            newBtn.addEventListener('click', () => {
                const amount = parseFloat(document.getElementById('withdraw-amount').value);
                const bank = document.getElementById('withdraw-bank').value;
                const account = document.getElementById('withdraw-account').value;

                if (!account) return showToast('Vui lòng nhập số tài khoản hợp lệ.', 'error');
                if (isNaN(amount) || amount < 100000) return showToast('Số tiền rút tối thiểu là 100,000 ₫.', 'error');
                if (amount > availableBalance) return showToast('Số dư khả dụng không đủ để thực hiện.', 'error');

                withdrawals.unshift({
                    id: 'WD-' + Date.now(),
                    amount: amount,
                    bank: bank,
                    account: account,
                    status: 'pending',
                    date: new Date().toLocaleDateString('vi-VN', { hour: '2-digit', minute: '2-digit' })
                });

                localStorage.setItem('exhib_withdrawals', JSON.stringify(withdrawals));
                document.getElementById('withdraw-amount').value = '';
                document.getElementById('withdraw-account').value = '';

                showToast('Đã gửi yêu cầu rút tiền! Chờ quản trị viên duyệt.', 'success');
                renderFinanceOwner();
            });
        }
    }

    // ==========================================
    // TÀI CHÍNH - ADMIN
    // ==========================================
    let selectedAdminOrderId = null;

    window.initFinanceModule = function () {
        const tbody = document.getElementById('finance-admin-orders-body');
        const countSpan = document.getElementById('finance-admin-order-count');
        const searchInput = document.getElementById('finance-admin-search');
        if (!tbody) return;

        let orders = getOrders();
        let booths = getBooths();

        if (searchInput) {
            const searchTerm = searchInput.value.toLowerCase().trim();
            if (searchTerm) {
                orders = orders.filter(o => {
                    const b = booths.find(x => x.id === o.boothId);
                    const boothName = b ? b.name.toLowerCase() : '';
                    return o.id.toLowerCase().includes(searchTerm) || boothName.includes(searchTerm);
                });
            }
        }

        if (countSpan) countSpan.textContent = `${orders.length} đơn`;

        if (orders.length === 0) {
            tbody.innerHTML = `<tr><td colspan="5" style="text-align:center; padding:20px; color:var(--text-dim);">Không tìm thấy đơn hàng nào</td></tr>`;
            return;
        }

        orders.sort((a, b) => b.date.localeCompare(a.date));

        tbody.innerHTML = orders.map(o => {
            const b = booths.find(x => x.id === o.boothId);
            const boothName = b ? b.name : 'Không rõ';
            const total = o.price * o.qty;

            let statusColor = '#60a5fa';
            let statusText = 'Đang xử lý';
            if (o.status === 'success') { statusColor = 'var(--emerald)'; statusText = 'Thành công'; }
            if (o.status === 'cancelled' || o.status === 'canceled') { statusColor = '#ef4444'; statusText = 'Đã hủy'; }

            const isSelected = o.id === selectedAdminOrderId;

            return `
            <tr class="admin-order-row ${isSelected ? 'active-row' : ''}" data-id="${o.id}" style="border-bottom: 1px solid var(--border); cursor: pointer; background: ${isSelected ? 'rgba(34,197,94,0.1)' : 'transparent'};">
                <td style="padding:10px; font-weight:600;">${o.id}</td>
                <td style="padding:10px;">${boothName}</td>
                <td style="padding:10px; font-weight:600;">${formatCurrency(total)} ₫</td>
                <td style="padding:10px;">
                    <span style="font-size:11px; padding:4px 8px; border-radius:6px; background:${statusColor}20; color:${statusColor};">
                        ${statusText}
                    </span>
                </td>
                <td style="padding:10px; color:var(--text-dim); font-size:12px;">${o.date}</td>
            </tr>
        `;
        }).join('');

        document.querySelectorAll('.admin-order-row').forEach(row => {
            row.addEventListener('click', function () {
                selectedAdminOrderId = this.getAttribute('data-id');
                window.initFinanceModule();
                renderAdminOrderDetails(selectedAdminOrderId);
            });
        });

        if (!selectedAdminOrderId && orders.length > 0) {
            selectedAdminOrderId = orders[0].id;
            renderAdminOrderDetails(selectedAdminOrderId);
            const firstRow = document.querySelector('.admin-order-row');
            if (firstRow) firstRow.style.background = 'rgba(34,197,94,0.1)';
        }

        if (searchInput && !searchInput.dataset.bound) {
            searchInput.dataset.bound = "true";
            searchInput.addEventListener('input', window.initFinanceModule);
        }
    };

    function renderAdminOrderDetails(orderId) {
        const container = document.getElementById('finance-admin-order-details');
        if (!container) return;

        const order = getOrders().find(o => o.id === orderId);
        if (!order) return;

        const booth = getBooths().find(b => b.id === order.boothId);
        const tierCfg = getTierConfig();
        const cfg = booth ? (tierCfg[booth.tier] || tierCfg.Basic) : tierCfg.Basic;

        const revenue = order.price * order.qty;
        const commission = (revenue * cfg.rc) + cfg.fee;
        const net = revenue - commission;

        let statusColor = '#60a5fa';
        let statusText = 'Đang xử lý';
        if (order.status === 'success') { statusColor = 'var(--emerald)'; statusText = 'Thành công'; }
        if (order.status === 'cancelled' || order.status === 'canceled') { statusColor = '#ef4444'; statusText = 'Đã hủy'; }

        container.innerHTML = `
        <div style="margin-bottom: 20px; padding-bottom: 16px; border-bottom: 1px dashed var(--border);">
            <div class="flex-between mb-2">
                <span style="color:var(--text-dim); font-size:12px;">Mã đơn hàng:</span>
                <strong style="font-size:14px;">${order.id}</strong>
            </div>
            <div class="flex-between mb-2">
                <span style="color:var(--text-dim); font-size:12px;">Trạng thái:</span>
                <span style="font-size:12px; font-weight:600; color:${statusColor};">${statusText}</span>
            </div>
            <div class="flex-between mb-2">
                <span style="color:var(--text-dim); font-size:12px;">Ngày tạo:</span>
                <span style="font-size:13px;">${order.date}</span>
            </div>
        </div>

        <div style="margin-bottom: 20px; padding-bottom: 16px; border-bottom: 1px dashed var(--border);">
            <h4 style="font-size:13px; font-weight:600; color:var(--text-muted); margin-bottom:12px;">THÔNG TIN SẢN PHẨM</h4>
            <p style="font-weight:500; margin-bottom:4px;">${order.product}</p>
            <div class="flex-between">
                <span style="color:var(--text-dim); font-size:13px;">${formatCurrency(order.price)} ₫ x ${order.qty}</span>
                <strong>${formatCurrency(revenue)} ₫</strong>
            </div>
        </div>

        <div style="margin-bottom: 20px; padding-bottom: 16px; border-bottom: 1px dashed var(--border);">
            <h4 style="font-size:13px; font-weight:600; color:var(--text-muted); margin-bottom:12px;">ĐỐI SOÁT GIAN HÀNG</h4>
            <div class="flex-between mb-2">
                <span style="color:var(--text-dim); font-size:13px;">Gian hàng:</span>
                <span style="font-size:13px; font-weight:500;">${booth ? booth.name : 'N/A'}</span>
            </div>
            <div class="flex-between mb-2">
                <span style="color:var(--text-dim); font-size:13px;">Gói dịch vụ:</span>
                <span class="tier-badge tier-${booth ? booth.tier.toLowerCase() : 'basic'}" style="font-size:10px;">${booth ? booth.tier.toUpperCase() : 'BASIC'}</span>
            </div>
            <div class="flex-between mb-2">
                <span style="color:var(--text-dim); font-size:13px;">Hoa hồng (${cfg.rc * 100}%):</span>
                <span style="color:#ef4444; font-size:13px;">-${formatCurrency(revenue * cfg.rc)} ₫</span>
            </div>
            <div class="flex-between">
                <span style="color:var(--text-dim); font-size:13px;">Phí cố định:</span>
                <span style="color:#ef4444; font-size:13px;">-${formatCurrency(cfg.fee)} ₫</span>
            </div>
        </div>

        <div style="background: rgba(0,0,0,0.15); padding: 16px; border-radius: 8px;">
            <div class="flex-between mb-2">
                <span style="color:var(--text-dim); font-size:14px;">Tổng doanh thu:</span>
                <span style="font-weight:600;">${formatCurrency(revenue)} ₫</span>
            </div>
            <div class="flex-between mb-2">
                <span style="color:#ef4444; font-size:14px;">Phí sàn thu:</span>
                <span style="font-weight:600; color:#ef4444;">${formatCurrency(commission)} ₫</span>
            </div>
            <div class="flex-between mt-3 pt-3" style="border-top: 1px solid var(--border);">
                <span style="font-weight:600; font-size:14px;">Thực trả gian hàng:</span>
                <span style="font-weight:700; font-size:18px; color:var(--emerald);">${formatCurrency(net)} ₫</span>
            </div>
        </div>
    `;
    }

    function switchView(targetId) {
        const session = JSON.parse(localStorage.getItem('exhib_session') || '{}');
        const role = session.role || 'Admin';

        // 1. Xác định ID màn hình thực tế dựa theo Role
        let actualTargetId = targetId;
        if (targetId === 'view-dashboard') actualTargetId = `view-dashboard-${role.toLowerCase()}`;
        if (targetId === 'view-booths') actualTargetId = `view-booths-${role.toLowerCase()}`;
        if (targetId === 'view-finance') actualTargetId = role === 'Owner' ? 'view-finance-owner' : 'view-finance';

        // 2. Chuyển đổi class 'active' để hiển thị đúng màn hình
        document.querySelectorAll('.view-section').forEach(el => el.classList.remove('active'));
        const targetView = document.getElementById(actualTargetId);
        if (targetView) targetView.classList.add('active');

        // 3. Cập nhật Header
        if (typeof headerTitle !== 'undefined' && headerTitle) {
            const titles = {
                'view-dashboard': 'Dashboard Overview',
                'view-booths': 'Booth Manager',
                'view-finance': 'Finance & Commission',
                'view-plans': 'Subscription Plans',
                'view-settings': 'Admin Settings'
            };
            headerTitle.textContent = titles[targetId] || 'Portal';
        }

        // 4. Cập nhật Sidebar Navigation
        if (typeof navLinks !== 'undefined') {
            navLinks.forEach(link => {
                link.classList.remove('active');
                if (link.getAttribute('data-target') === targetId) link.classList.add('active');
            });
        }

        // 5. Gọi hàm load dữ liệu tương ứng cho màn hình
        if (targetId === 'view-dashboard') renderDashboard();
        if (targetId === 'view-booths') {
            try { if (typeof window.initBoothModule === 'function') window.initBoothModule(); } catch (e) { console.warn(e); }
            setTimeout(() => initMap(), 150);
        }
        if (targetId === 'view-finance') {
            if (role === 'Owner') {
                if (typeof renderFinanceOwner === 'function') renderFinanceOwner();
            } else {
                try { if (typeof window.initFinanceModule === 'function') window.initFinanceModule(); } catch (e) { console.warn(e); }
            }
        }
        if (targetId === 'view-plans') {
            if (typeof renderPlans === 'function') renderPlans();
        }
        if (targetId === 'view-settings') {
            try {
                if (typeof loadProfileData === 'function') loadProfileData();
                if (typeof loadTierConfigUI === 'function') loadTierConfigUI();
            } catch (e) { console.warn(e); }
        }
    }

    navLinks.forEach(link => {
        link.addEventListener('click', e => {
            e.preventDefault();
            switchView(link.getAttribute('data-target'));
        });
    });

    /* ══════════════════════════════════════════════
       4. THEME TOGGLE
       ══════════════════════════════════════════════ */
    const themeBtn = document.getElementById('theme-toggle');
    let darkMode = localStorage.getItem('exhib_theme') === 'dark';
    if (darkMode) {
        document.documentElement.removeAttribute('data-theme');
        if (themeBtn) themeBtn.querySelector('i').className = 'fa-solid fa-moon';
    } else {
        document.documentElement.setAttribute('data-theme', 'light');
        if (themeBtn) themeBtn.querySelector('i').className = 'fa-solid fa-sun';
    }

    themeBtn?.addEventListener('click', () => {
        darkMode = !darkMode;
        if (darkMode) {
            document.documentElement.removeAttribute('data-theme');
            localStorage.setItem('exhib_theme', 'dark');
            themeBtn.querySelector('i').className = 'fa-solid fa-moon';
        } else {
            document.documentElement.setAttribute('data-theme', 'light');
            localStorage.setItem('exhib_theme', 'light');
            themeBtn.querySelector('i').className = 'fa-solid fa-sun';
        }
    });

    function formatCurrency(value) {
        return new Intl.NumberFormat('vi-VN').format(value || 0);
    }

    /* ══════════════════════════════════════════════
       5. DASHBOARD - STATS, CHARTS & LEADERBOARD
       ══════════════════════════════════════════════ */
    function renderDashboard() {
        const session = JSON.parse(localStorage.getItem('exhib_session') || '{}');
        const role = session.role || 'Admin';
        const booths = getBooths();
        const orders = getOrders();
        const fairs = getFairs();
        const tierCfg = getTierConfig();

        const successOrders = orders.filter(o => o.status === 'success');
        const pendingOrders = orders.filter(o => o.status === 'pending');
        const cancelledOrders = orders.filter(o => o.status === 'cancelled');
        let totalRevenue = 0;
        successOrders.forEach(o => { totalRevenue += o.price * o.qty; });

        if (role === 'Owner') {
            // Owner dashboard stats
            const elRevenue = getActiveEl('stat-revenue-owner');
            const elSuccess = getActiveEl('stat-success-owner');
            const elPending = getActiveEl('stat-pending-owner');
            const elCancelled = getActiveEl('stat-cancelled-owner');
            if (elRevenue) elRevenue.textContent = formatCurrency(totalRevenue) + ' ₫';
            if (elSuccess) elSuccess.textContent = successOrders.length;
            if (elPending) elPending.textContent = pendingOrders.length;
            if (elCancelled) elCancelled.textContent = cancelledOrders.length;
        } else {
            // Admin dashboard stats
            const elFairs = getActiveEl('stat-fairs');
            const elBooths = getActiveEl('stat-booths');
            const elRevenue = getActiveEl('stat-revenue');
            const elVisitors = getActiveEl('stat-visitors');
            if (elFairs) elFairs.textContent = fairs.length;
            if (elBooths) elBooths.textContent = booths.length;
            if (elRevenue) elRevenue.textContent = formatCurrency(totalRevenue) + ' ₫';
            const totalEngagement = booths.reduce((sum, b) => sum + (b.engagement || 0), 0);
            if (elVisitors) elVisitors.textContent = totalEngagement >= 1000 ? (totalEngagement / 1000).toFixed(1) + 'k' : totalEngagement;
        }

        // Render Biểu đồ, Hoạt động gần đây và Bảng xếp hạng
        if (!chartInitialized) initRevenueChart(orders, tierCfg, booths);
        renderActivity(orders);
        if (role !== 'Owner') renderLeaderboard(booths, orders, tierCfg);
    }

    function initRevenueChart(orders, tierCfg, booths) {
        if (typeof Chart === 'undefined') {
            setTimeout(() => initRevenueChart(orders, tierCfg, booths), 200);
            return;
        }

        chartInitialized = true;
        const session = JSON.parse(localStorage.getItem('exhib_session') || '{}');
        const role = session.role || 'Admin';
        const canvasId = role === 'Owner' ? 'revenueChart-owner' : 'revenueChart';
        const canvas = getActiveEl(canvasId);
        if (!canvas) return;

        let ctx = canvas.getContext('2d');
        if (!ctx) return;

        if (window.dashboardChart && typeof window.dashboardChart.destroy === 'function') {
            window.dashboardChart.destroy();
            window.dashboardChart = null;
        }

        const dayLabels = [];
        const dayData = [];
        for (let i = 6; i >= 0; i--) {
            const d = new Date();
            d.setDate(d.getDate() - i);
            const dateStr = d.toISOString().split('T')[0];
            dayLabels.push(d.toLocaleDateString('en-US', { month: 'short', day: 'numeric' }));

            let dayCommission = 0;
            const dayOrders = orders.filter(o => o.date === dateStr && o.status === 'success');
            dayOrders.forEach(o => {
                const booth = booths.find(b => b.id === o.boothId);
                if (booth) {
                    const cfg = tierCfg[booth.tier] || tierCfg.Basic;
                    dayCommission += (o.price * o.qty * cfg.rc) + cfg.fee;
                }
            });
            dayData.push(dayCommission);
        }

        // Tạo dữ liệu mô phỏng nếu chưa có đơn hàng nào trong 7 ngày qua để đồ thị hiển thị đẹp mắt
        if (dayData.every(v => v === 0)) {
            const simulated = [320000, 480000, 580000, 420000, 750000, 890000, 1120000];
            simulated.forEach((v, i) => dayData[i] = v);
        }

        const gradient = ctx.createLinearGradient(0, 0, 0, 280);
        gradient.addColorStop(0, 'rgba(34, 197, 94, 0.2)');
        gradient.addColorStop(1, 'rgba(34, 197, 94, 0)');

        window.dashboardChart = new Chart(ctx, {
            type: 'line',
            data: {
                labels: dayLabels,
                datasets: [{
                    label: 'Hoa hồng (VNĐ)',
                    data: dayData,
                    borderColor: emerald,
                    backgroundColor: gradient,
                    borderWidth: 2.5,
                    fill: true,
                    tension: 0.4,
                    pointBackgroundColor: emerald,
                    pointBorderColor: '#0a1210',
                    pointBorderWidth: 2,
                    pointRadius: 4,
                    pointHoverRadius: 6
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { display: false },
                    tooltip: {
                        backgroundColor: 'rgba(10, 18, 16, 0.9)',
                        borderColor: 'rgba(34, 197, 94, 0.3)',
                        borderWidth: 1,
                        titleFont: { family: 'Inter', weight: '600' },
                        padding: 12,
                        cornerRadius: 10,
                        callbacks: {
                            label: (ctx) => `Hoa hồng: ${formatCurrency(ctx.parsed.y)} ₫`
                        }
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        grid: { color: 'rgba(255,255,255,0.04)' },
                        ticks: { color: '#6b8f7a', callback: v => formatCurrency(v) + ' ₫' }
                    },
                    x: {
                        grid: { display: false },
                        ticks: { color: '#6b8f7a' }
                    }
                }
            }
        });
    }

    function renderActivity(orders) {
        const session = JSON.parse(localStorage.getItem('exhib_session') || '{}');
        const role = session.role || 'Admin';
        const listId = role === 'Owner' ? 'activity-list-owner' : 'activity-list';
        const container = getActiveEl(listId);
        if (!container) return;

        // Lấy 5 đơn hàng mới nhất
        const recent = [...orders].sort((a, b) => b.date.localeCompare(a.date)).slice(0, 5);

        // Hàm helper để lấy màu theo trạng thái đơn hàng
        const getStatusColor = (status) => {
            if (status === 'success') return 'var(--emerald, #22c55e)'; // Xanh lá
            if (status === 'pending') return '#fbbf24'; // Vàng
            if (status === 'canceled' || status === 'cancelled') return '#ef4444'; // Đỏ
            return '#60a5fa'; // Mặc định (xanh dương)
        };

        // 1. Tạo HTML cho phần chú giải (Legend)
        const legendHTML = `
        <div style="display:flex; gap:12px; margin-bottom:16px; flex-wrap:wrap; font-size:11px; color:var(--text-muted); border-bottom:1px dashed var(--border); padding-bottom:12px;">
            <div style="display:flex; align-items:center; gap:6px;">
                <div style="width:8px; height:8px; border-radius:50%; background:var(--emerald, #22c55e);"></div> Thành công
            </div>
            <div style="display:flex; align-items:center; gap:6px;">
                <div style="width:8px; height:8px; border-radius:50%; background:#fbbf24;"></div> Đang xử lý
            </div>
            <div style="display:flex; align-items:center; gap:6px;">
                <div style="width:8px; height:8px; border-radius:50%; background:#ef4444;"></div> Bị hủy
            </div>
        </div>
    `;

        // 2. Tạo HTML cho danh sách đơn hàng
        const listHTML = recent.map((o) => {
            const statusColor = getStatusColor(o.status);

            // Em giữ lại thông tin sản phẩm và thêm ngày thực hiện xuống dưới cùng
            // để danh sách hiển thị đầy đủ thông tin nhất
            return `
            <div class="activity-item" style="display:flex; gap:12px; margin-bottom:12px; padding-bottom:12px; border-bottom:1px solid var(--border);">
                <div style="width:10px; height:10px; border-radius:50%; margin-top:5px; background:${statusColor}; box-shadow: 0 0 8px ${statusColor}40;"></div>
                <div>
                    <p style="font-size:13px; font-weight:500; margin-bottom:2px;">Đơn hàng <strong>${o.id}</strong></p>
                    <p style="font-size:12px; color:var(--text-dim); margin-bottom:4px;">${o.product} (×${o.qty})</p>
                    <p style="font-size:11px; color:var(--text-muted);">
                        <i class="fa-regular fa-calendar" style="margin-right:4px;"></i> ${o.date}
                    </p>
                </div>
            </div>
        `;
        }).join('');

        // Gộp chú giải và danh sách lại rồi render
        container.innerHTML = legendHTML + listHTML;
    }

    function renderLeaderboard(booths, orders, tierCfg) {
        const tbody = getActiveEl('leaderboard-body');
        if (!tbody) return;

        const boothStats = booths.map(b => {
            const boothOrders = orders.filter(o => o.boothId === b.id && o.status === 'success');
            const orderCount = boothOrders.length;
            const cfg = tierCfg[b.tier] || tierCfg.Basic;
            const priority = cfg.weight * (orderCount + (b.engagement || 0) / 100);
            return { ...b, orderCount, priority };
        });

        boothStats.sort((a, b) => b.priority - a.priority);

        tbody.innerHTML = boothStats.map((b, i) => {
            const rank = i + 1;
            const rankClass = rank <= 3 ? `color:var(--emerald); font-weight:bold;` : `color:var(--text-dim);`;
            return `
                <tr style="border-bottom: 1px solid var(--border);">
                    <td style="padding:10px; text-align:center;"><span style="${rankClass}">${rank}</span></td>
                    <td style="padding:10px; font-weight:600;">${b.name}</td>
                    <td style="padding:10px; color:var(--text-muted);">${b.company}</td>
                    <td style="padding:10px;"><span class="tier-badge tier-${b.tier.toLowerCase()}" style="font-size:11px; padding:3px 8px;">${b.tier.toUpperCase()}</span></td>
                    <td style="padding:10px; text-align:center;">${b.orderCount}</td>
                    <td style="padding:10px; text-align:right;"><span style="font-family:monospace; color:var(--emerald);">${b.priority.toFixed(1)}</span></td>
                </tr>
            `;
        }).join('');
    }

    /* ══════════════════════════════════════════════
       6. BOOTH MANAGER
       ══════════════════════════════════════════════ */
    let boothEventsBound = false;

    window.initBoothModule = function () {
        function renderBoothList() {
            const booths = getBooths();
            const list = document.querySelector('.view-section.active .booth-list');
            if (!list) return;
            list.innerHTML = '';

            booths.forEach(b => {
                const item = document.createElement('div');
                item.className = 'booth-list-item' + (selectedBoothId === b.id ? ' active' : '');
                item.innerHTML = `
                    <div class="booth-list-item-name">${b.name}</div>
                    <div class="booth-list-item-company">${b.company}</div>
                    <div class="booth-list-item-meta">
                        <span class="tier-badge tier-${b.tier.toLowerCase()}" style="font-size:10px; padding:2px 8px;">${b.tier}</span>
                        <span style="font-size:11px; color:var(--text-dim);">${b.category}</span>
                    </div>
                `;
                item.addEventListener('click', () => selectBooth(b.id));
                list.appendChild(item);
            });

            if (!selectedBoothId && booths.length > 0) selectBooth(booths[0].id);
            else if (selectedBoothId) loadBoothIntoForm(selectedBoothId);
        }

        function selectBooth(id) {
            selectedBoothId = id;
            document.querySelectorAll('.booth-list-item').forEach(el => el.classList.remove('active'));
            renderBoothList();
        }

        function loadBoothIntoForm(id) {
            const b = getBooths().find(x => x.id === id);
            if (!b) return;

            // Helper: find the active element, trying plain ID then -owner suffix
            function getField(base) {
                return getActiveEl(base) || getActiveEl(base + '-owner');
            }

            const fieldMap = {
                'booth-name': 'name',
                'booth-company': 'company',
                'booth-category': 'category',
                'booth-tier': 'tier',
                'input-lat': 'lat',
                'input-lng': 'lng',
                'radius-slider': 'radius'
            };
            Object.entries(fieldMap).forEach(([field, key]) => {
                const el = getField(field);
                if (el) el.value = b[key] || '';
            });

            const scriptEl = getField('booth-script');
            if (scriptEl) scriptEl.value = b.script || '';

            const radiusVal = getField('radius-val');
            if (radiusVal) radiusVal.textContent = (b.radius || 30) + 'm';

            updateWordCount();
            updateMapFromInputs();
        }

        if (!boothEventsBound) {
            // Event Delegation for buttons
            document.addEventListener('click', (e) => {
                const btnSave = e.target.closest('#btn-save-booth');
                if (btnSave) {
                    if (!selectedBoothId) return showToast('Please select or add a booth first.', 'error');

                    const booths = getBooths();
                    const idx = booths.findIndex(b => b.id === selectedBoothId);
                    if (idx < 0) return;

                    booths[idx].name = getActiveEl('booth-name')?.value || booths[idx].name;
                    booths[idx].company = getActiveEl('booth-company')?.value || booths[idx].company;
                    booths[idx].category = getActiveEl('booth-category')?.value || booths[idx].category;
                    booths[idx].tier = getActiveEl('booth-tier')?.value || booths[idx].tier;
                    booths[idx].script = getActiveEl('booth-script')?.value || booths[idx].script;
                    booths[idx].lat = parseFloat(getActiveEl('input-lat')?.value) || booths[idx].lat;
                    booths[idx].lng = parseFloat(getActiveEl('input-lng')?.value) || booths[idx].lng;
                    booths[idx].radius = parseInt(getActiveEl('radius-slider')?.value) || booths[idx].radius;

                    saveBooths(booths);
                    renderBoothList();

                    const lastSaved = getActiveEl('last-saved-time');
                    if (lastSaved) lastSaved.textContent = `Saved at ${new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}`;
                    showToast('Booth saved successfully!', 'success');
                }

                const btnAdd = e.target.closest('#btn-add-booth');
                if (btnAdd) {
                    const booths = getBooths();
                    const newId = 'B' + String(booths.length + 1).padStart(3, '0');
                    booths.push({ id: newId, name: 'New Booth', company: 'Company', category: 'Other', tier: 'Basic', lat: 10.7720, lng: 106.6980, radius: 20, script: '' });
                    saveBooths(booths);
                    selectedBoothId = newId;
                    renderBoothList();
                    showToast(`Booth ${newId} created!`, 'success');
                }
            });

            boothEventsBound = true;
        }

        renderBoothList();
    };

    /* ══════════════════════════════════════════════
       7. WAVESURFER & AI OPTIMIZER
       ══════════════════════════════════════════════ */
    let wavesurfer = null;

    function initWaveSurferInstance() {
        if (wavesurfer) wavesurfer.destroy();
        // Try Admin waveform first, fall back to Owner waveform
        const container = getActiveEl('waveform') || getActiveEl('waveform-owner');
        if (!container) return;

        // Determine suffix based on which waveform container was found
        const sfx = container.id === 'waveform-owner' ? '-owner' : '';
        const ga = id => document.getElementById(id + sfx);

        wavesurfer = WaveSurfer.create({
            container: container,
            waveColor: '#1e3524',
            progressColor: emerald,
            cursorColor: '#059669',
            barWidth: 2, barGap: 1, barRadius: 2, height: 80, normalize: true
        });

        wavesurfer.on('ready', () => {
            if (ga('time-total')) ga('time-total').textContent = formatTime(wavesurfer.getDuration());
            if (ga('btn-play')) ga('btn-play').disabled = false;
            if (ga('btn-stop')) ga('btn-stop').disabled = false;
            if (ga('audio-upload-overlay')) ga('audio-upload-overlay').classList.add('hidden');
            if (ga('btn-clear-audio')) ga('btn-clear-audio').classList.remove('hidden');
            if (ga('audio-status')) {
                ga('audio-status').textContent = 'Ready to play';
                ga('audio-status').style.color = emerald;
            }
        });

        wavesurfer.on('audioprocess', () => {
            if (ga('time-current')) ga('time-current').textContent = formatTime(wavesurfer.getCurrentTime());
        });

        wavesurfer.on('finish', () => {
            if (ga('btn-play')) ga('btn-play').innerHTML = '<i class="fa-solid fa-play"></i>';
        });
    }

    function processAudioFile(file) {
        if (!file.type.startsWith('audio/')) return showToast('Please upload a valid audio file.', 'error');
        if (getActiveEl('audio-status')) {
            getActiveEl('audio-status').textContent = 'Loading waveform...';
            getActiveEl('audio-status').style.color = '';
        }
        initWaveSurferInstance();
        wavesurfer.load(URL.createObjectURL(file));
    }

    // Helper: resolve audio/script element IDs based on active view role
    function getAudioId(base) {
        const activeView = document.querySelector('.view-section.active');
        if (activeView && activeView.id && activeView.id.includes('owner')) return base + '-owner';
        return base;
    }

    // Event Delegation for Audio, AI, and Scripts
    document.addEventListener('click', e => {
        const uploadOverlay = e.target.closest('#audio-upload-overlay, #audio-upload-overlay-owner');
        if (uploadOverlay) {
            const inputId = uploadOverlay.id === 'audio-upload-overlay-owner' ? 'audio-input-owner' : 'audio-input';
            document.getElementById(inputId)?.click();
        }

        const btnPlay = e.target.closest('#btn-play, #btn-play-owner');
        if (btnPlay && wavesurfer) {
            wavesurfer.playPause();
            btnPlay.innerHTML = wavesurfer.isPlaying() ? '<i class="fa-solid fa-pause"></i>' : '<i class="fa-solid fa-play"></i>';
        }

        const btnStop = e.target.closest('#btn-stop, #btn-stop-owner');
        if (btnStop && wavesurfer) {
            wavesurfer.stop();
            const playId = getAudioId('btn-play');
            const currId = getAudioId('time-current');
            if (document.getElementById(playId)) document.getElementById(playId).innerHTML = '<i class="fa-solid fa-play"></i>';
            if (document.getElementById(currId)) document.getElementById(currId).textContent = '0:00';
        }

        const btnClear = e.target.closest('#btn-clear-audio, #btn-clear-audio-owner');
        if (btnClear) {
            if (wavesurfer) { wavesurfer.destroy(); wavesurfer = null; }
            const suffix = btnClear.id.includes('owner') ? '-owner' : '';
            const g = id => document.getElementById(id + suffix);
            if (g('audio-upload-overlay')) g('audio-upload-overlay').classList.remove('hidden');
            if (g('btn-clear-audio')) g('btn-clear-audio').classList.add('hidden');
            if (g('btn-play')) g('btn-play').disabled = true;
            if (g('btn-stop')) g('btn-stop').disabled = true;
            if (g('audio-input')) g('audio-input').value = '';
            if (g('time-current')) g('time-current').textContent = '0:00';
            if (g('time-total')) g('time-total').textContent = '0:00';
            if (g('btn-play')) g('btn-play').innerHTML = '<i class="fa-solid fa-play"></i>';
            if (g('audio-status')) { g('audio-status').textContent = 'No file loaded'; g('audio-status').style.color = ''; }
        }

        const btnAi = e.target.closest('#btn-ai-optimize, #btn-ai-optimize-owner');
        if (btnAi && !btnAi.disabled) {
            const isOwner = btnAi.id.includes('owner');
            const suffix = isOwner ? '-owner' : '';
            const g = id => document.getElementById(id + suffix);
            btnAi.disabled = true;
            btnAi.classList.remove('pulse-glow');
            const scriptArea = g('booth-script');
            if (scriptArea) scriptArea.disabled = true;
            if (g('ai-loading-overlay')) g('ai-loading-overlay').classList.remove('hidden');

            const btnAiIcon = btnAi.querySelector('.ai-icon');
            const btnAiText = btnAi.querySelector('.ai-text');
            if (btnAiIcon) btnAiIcon.className = 'fa-solid fa-circle-notch fa-spin ai-icon';
            if (btnAiText) btnAiText.textContent = 'Optimizing...';

            const currentName = g('booth-name')?.value || getActiveEl('booth-name')?.value || 'this booth';
            const lang = g('lang-select')?.value || 'EN';

            setTimeout(() => {
                const optimizedScripts = {
                    'EN': `🎯 Welcome to ${currentName}!\n\nStep into an extraordinary experience where innovation meets inspiration. Our carefully curated products represent the pinnacle of quality and design in their category.\n\nExplore our interactive displays, engage with our expert team, and discover solutions tailored just for you.\n\nThank you for visiting ${currentName}. Let's create something amazing together! 🌟`,
                    'VI': `🎯 Chào mừng đến với ${currentName}!\n\nBước vào một trải nghiệm phi thường nơi sự đổi mới gặp gỡ cảm hứng. Các sản phẩm được tuyển chọn kỹ lưỡng của chúng tôi đại diện cho đỉnh cao của chất lượng và thiết kế.\n\nCảm ơn quý khách đã ghé thăm ${currentName}! 🌟`
                };

                if (scriptArea) {
                    scriptArea.value = optimizedScripts[lang] || optimizedScripts['EN'];
                    updateWordCount();
                    scriptArea.disabled = false;
                }

                btnAi.disabled = false;
                btnAi.classList.add('pulse-glow');
                if (g('ai-loading-overlay')) g('ai-loading-overlay').classList.add('hidden');
                if (btnAiIcon) btnAiIcon.className = 'fa-solid fa-wand-magic-sparkles ai-icon';
                if (btnAiText) btnAiText.textContent = 'Tối ưu bằng AI';

                showToast(`Script optimized for ${lang} by Emergent AI!`, 'success');
            }, 2000);
        }
    });

    // Drag events for audio upload overlay (both Admin and Owner)
    document.addEventListener('dragover', e => {
        const o = e.target.closest('#audio-upload-overlay, #audio-upload-overlay-owner');
        if (o) { e.preventDefault(); o.classList.add('dragging'); }
    });
    document.addEventListener('dragleave', e => {
        const o = e.target.closest('#audio-upload-overlay, #audio-upload-overlay-owner');
        if (o) o.classList.remove('dragging');
    });
    document.addEventListener('drop', e => {
        const o = e.target.closest('#audio-upload-overlay, #audio-upload-overlay-owner');
        if (o) {
            e.preventDefault(); o.classList.remove('dragging');
            if (e.dataTransfer.files[0]) processAudioFile(e.dataTransfer.files[0]);
        }
    });

    document.addEventListener('change', e => {
        if ((e.target.id === 'audio-input' || e.target.id === 'audio-input-owner') && e.target.files[0]) processAudioFile(e.target.files[0]);
        if (e.target.id === 'lang-select' || e.target.id === 'lang-select-owner') {
            const suffix = e.target.id.includes('owner') ? '-owner' : '';
            const t = document.getElementById('script-title' + suffix);
            if (t) t.textContent = `Lời thoại (${e.target.value})`;
        }
    });

    document.addEventListener('input', e => {
        if ((e.target.id === 'volume-slider' || e.target.id === 'volume-slider-owner') && wavesurfer) wavesurfer.setVolume(Number(e.target.value));
        if (e.target.id === 'booth-script' || e.target.id === 'booth-script-owner') updateWordCount();
    });

    function updateWordCount() {
        const scriptArea = getActiveEl('booth-script') || getActiveEl('booth-script-owner');
        const wordCountEl = getActiveEl('word-count') || getActiveEl('word-count-owner');
        if (!scriptArea || !wordCountEl) return;

        const text = scriptArea.value.trim();
        const words = text ? text.split(/\s+/).length : 0;
        const estSec = Math.round((words / 150) * 60);
        wordCountEl.textContent = `${words} từ · Độ dài ước tính: ${formatTime(estSec)}`;
    }

    function formatTime(sec) {
        const min = Math.floor(sec / 60);
        const s = Math.floor(sec % 60);
        return `${min}:${s < 10 ? '0' : ''}${s}`;
    }

    /* ══════════════════════════════════════════════
       8. LEAFLET MAP & GEOFENCING (FIXED DUPLICATE SCOPE)
       ══════════════════════════════════════════════ */
    function initMap() {
        const isOwnerActive = document.getElementById('view-booths-owner').classList.contains('active');
        const mapContainer = document.getElementById(isOwnerActive ? 'map-owner' : 'map');
        if (!mapContainer) return;

        mapInitialized = true;

        const inputLat = getActiveEl('input-lat');
        const inputLng = getActiveEl('input-lng');
        const radiusSlider = getActiveEl('radius-slider');

        const lat = parseFloat(inputLat?.value) || 10.7680;
        const lng = parseFloat(inputLng?.value) || 106.7050;
        const radius = parseFloat(radiusSlider?.value) || 30;

        const mapId = mapContainer.id || 'map';

        // Check if Leaflet instance already exists for this container
        if (mapContainer._leaflet_id && window.myMaps[mapId]) {
            const m = window.myMaps[mapId];
            m.invalidateSize();
            m.setView([lat, lng], 17);
            if (m._myMarker) m._myMarker.setLatLng([lat, lng]);
            if (m._myCircle) { m._myCircle.setLatLng([lat, lng]); m._myCircle.setRadius(radius); }
            return;
        }

        const map = L.map(mapContainer, { zoomControl: true, attributionControl: true }).setView([lat, lng], 17);
        window.myMaps[mapId] = map;

        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            attribution: '© OpenStreetMap'
        }).addTo(map);

        const marker = L.marker([lat, lng], { draggable: true }).addTo(map);
        const circle = L.circle([lat, lng], {
            color: emerald, fillColor: emerald, fillOpacity: 0.15, weight: 2, radius: radius
        }).addTo(map);

        map._myMarker = marker;
        map._myCircle = circle;

        marker.on('dragend', function () {
            const pos = marker.getLatLng();
            const cLat = getActiveEl('input-lat');
            const cLng = getActiveEl('input-lng');
            if (cLat) cLat.value = pos.lat.toFixed(6);
            if (cLng) cLng.value = pos.lng.toFixed(6);
            circle.setLatLng(pos);
            map.panTo(pos);
        });
    }

    function updateMapFromInputs() {
        const mapContainer = getActiveEl('map') || getActiveEl('map-owner');
        if (!mapContainer || !mapContainer._leaflet_id) return;
        const map = window.myMaps[mapContainer.id || 'map'];
        if (!map) return;

        const lat = parseFloat((getActiveEl('input-lat') || getActiveEl('input-lat-owner'))?.value);
        const lng = parseFloat((getActiveEl('input-lng') || getActiveEl('input-lng-owner'))?.value);
        if (isNaN(lat) || isNaN(lng)) return;

        const pos = [lat, lng];
        if (map._myMarker) map._myMarker.setLatLng(pos);
        if (map._myCircle) map._myCircle.setLatLng(pos);
        map.panTo(pos);
    }

    document.addEventListener('input', e => {
        if (e.target.id === 'input-lat' || e.target.id === 'input-lng' ||
            e.target.id === 'input-lat-owner' || e.target.id === 'input-lng-owner') {
            updateMapFromInputs();
        }
        if (e.target.id === 'radius-slider' || e.target.id === 'radius-slider-owner') {
            const val = e.target.value;
            const rVal = getActiveEl('radius-val') || getActiveEl('radius-val-owner');
            if (rVal) rVal.textContent = `${val}m`;

            const mapContainer = getActiveEl('map') || getActiveEl('map-owner');
            if (mapContainer && window.myMaps[mapContainer.id || 'map']) {
                const map = window.myMaps[mapContainer.id || 'map'];
                if (map._myCircle) map._myCircle.setRadius(Number(val));
            }
        }
    });

    /* ══════════════════════════════════════════════
       13. TOAST NOTIFICATIONS
       ══════════════════════════════════════════════ */
    function showToast(message, type = 'success') {
        if (typeof Toastify === 'undefined') return alert(message);
        const bgs = {
            success: 'linear-gradient(135deg, #059669, #22c55e)',
            error: 'linear-gradient(135deg, #dc2626, #ef4444)'
        };
        Toastify({
            text: message, duration: 3000, gravity: 'bottom', position: 'right',
            style: { background: bgs[type], borderRadius: '12px', fontSize: '13px', padding: '12px 20px' }
        }).showToast();
    }

    /* ══════════════════════════════════════════════
       14. RIPPLE EFFECT
       ══════════════════════════════════════════════ */
    document.addEventListener('click', e => {
        const btn = e.target.closest('.btn-ripple');
        if (btn) {
            const ripple = document.createElement('span');
            ripple.className = 'ripple-effect';
            const rect = btn.getBoundingClientRect();
            const size = Math.max(rect.width, rect.height);
            ripple.style.width = ripple.style.height = size + 'px';
            ripple.style.left = (e.clientX - rect.left - size / 2) + 'px';
            ripple.style.top = (e.clientY - rect.top - size / 2) + 'px';
            btn.appendChild(ripple);
            setTimeout(() => ripple.remove(), 600);
        }
    });

    function renderPlans() {
        const container = document.getElementById('plans-grid');
        const btnAddPlan = document.getElementById('btn-add-plan');
        if (!container) return;

        // Lấy thông tin Role hiện tại
        const session = JSON.parse(localStorage.getItem('exhib_session') || '{}');
        const role = session.role || 'Admin';

        // Chỉ Admin mới thấy nút "Gói mới" ở góc trên cùng
        if (btnAddPlan) {
            btnAddPlan.style.display = role === 'Owner' ? 'none' : 'inline-flex';
        }

        const plans = getPlans();

        if (plans.length === 0) {
            container.innerHTML = '<p style="color:var(--text-dim); text-align:center; grid-column: 1/-1;">Chưa có gói dịch vụ nào. Hãy tạo mới.</p>';
            return;
        }

        // Đổi giao diện nút bấm theo Role
        const buttonText = role === 'Owner' ? '<i class="fa-solid"></i> Đăng ký ngay' : '<i class="fa-solid fa-pen-to-square"></i> Chỉnh sửa gói';
        const buttonStyle = role === 'Owner' ? 'background: #000000; color: #FFFFFF;' : ''; 

        container.innerHTML = plans.map(plan => `
            <div class="glass-card" style="padding: 24px; display: flex; flex-direction: column;">
                <div style="margin-bottom: 20px;">
                    <h4 style="font-size: 18px; font-weight: 700; margin-bottom: 6px; color: #000000;">${plan.name}</h4>
                    <p style="font-size: 13px; color: var(--text-muted); margin-bottom: 12px; min-height: 38px;">
                        ${plan.target}
                    </p>
                    <p style="font-size: 24px; font-weight: 700; color: #000000;">
                        ${plan.price === 0 ? 'Miễn phí' : formatCurrency(plan.price) + ' ₫'} 
                        <span style="font-size: 13px; color: var(--text-dim); font-weight: 400;">/ tháng</span>
                    </p>
                </div>
                
                <ul style="list-style: none; padding: 0; margin-bottom: 24px; flex-grow: 1;">
                    ${plan.features ? plan.features.map(f => `
                        <li style="margin-bottom: 12px; font-size: 13px; line-height: 1.5; color: ${f.active ? 'inherit' : 'var(--text-dim)'};">
                            <i class="fa-solid ${f.active ? 'fa-check text-accent' : 'fa-xmark'} " style="margin-right: 8px;"></i> ${f.text}
                        </li>
                    `).join('') : ''}
                </ul>
                
                <button class="btn-primary w-100 btn-ripple" style="padding: 10px; border-radius: 8px; background: #000000; color: #FFFFFF; ${buttonStyle}">
                    ${buttonText}
                </button>
            </div>
        `).join('');
    }

    // Initial setups
    updateWordCount();
});
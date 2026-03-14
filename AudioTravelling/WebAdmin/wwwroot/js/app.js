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
        { id: 'ORD-1002', boothId: 'B001', product: 'Máy chiếu Holo Mini', qty: 5, price: 890000, status: 'success', date: '2026-03-10' },
        { id: 'ORD-1003', boothId: 'B002', product: 'Giỏ quà hữu cơ', qty: 15, price: 450000, status: 'success', date: '2026-03-09' },
        { id: 'ORD-1004', boothId: 'B002', product: 'Nước ép lạnh (Thùng 12)', qty: 8, price: 650000, status: 'success', date: '2026-03-10' },
        { id: 'ORD-1005', boothId: 'B003', product: 'Áo Dài Lụa', qty: 3, price: 3200000, status: 'success', date: '2026-03-09' },
        { id: 'ORD-1006', boothId: 'B003', product: 'Khăn lụa thêu tay', qty: 10, price: 850000, status: 'success', date: '2026-03-10' },
        { id: 'ORD-1007', boothId: 'B004', product: 'Vé tham gia Thiền', qty: 20, price: 250000, status: 'success', date: '2026-03-10' },
        { id: 'ORD-1008', boothId: 'B005', product: 'Bộ lắp ráp Robot STEM', qty: 12, price: 750000, status: 'success', date: '2026-03-09' },
        { id: 'ORD-1009', boothId: 'B006', product: 'Cọc đặt trước xe EV', qty: 4, price: 25000000, status: 'success', date: '2026-03-10' },
        { id: 'ORD-1010', boothId: 'B006', product: 'Gói quà lưu niệm AutoDrive', qty: 25, price: 550000, status: 'success', date: '2026-03-10' },
        { id: 'ORD-1011', boothId: 'B007', product: 'Bình hoa sơn mài', qty: 6, price: 1800000, status: 'success', date: '2026-03-09' },
        { id: 'ORD-1012', boothId: 'B007', product: 'Tranh khắc gỗ (CÓ CHỮ KÝ)', qty: 8, price: 1200000, status: 'success', date: '2026-03-10' },
        { id: 'ORD-1013', boothId: 'B001', product: 'Hub màn hình thông minh', qty: 3, price: 12000000, status: 'success', date: '2026-03-11' },
        { id: 'ORD-1014', boothId: 'B008', product: 'Bản quyền Cloud Starter', qty: 10, price: 1990000, status: 'success', date: '2026-03-10' },
        { id: 'ORD-1015', boothId: 'B002', product: 'Nước Thanh Long (Thùng)', qty: 20, price: 380000, status: 'success', date: '2026-03-11' },
        { id: 'ORD-1016', boothId: 'B006', product: 'Gói Pin 100kWh', qty: 1, price: 85000000, status: 'success', date: '2026-03-11' }
    ];

    const defaultFairs = [
        { id: 'F001', name: 'Saigon Tech Expo 2026', status: 'active', date: '2026-03-01' },
        { id: 'F002', name: 'Vietnam Food Festival', status: 'active', date: '2026-03-05' },
        { id: 'F003', name: 'ASEAN Fashion Week', status: 'upcoming', date: '2026-04-10' }
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
            name: 'Gói Cơ Bản',
            price: 699000,
            maxOrders: 50,
            features: [
                { text: 'Standard Map Listing', active: true },
                { text: 'Basic Audio Description', active: true },
                { text: '50 Orders/month', active: true },
                { text: 'AI Translation', active: false },
                { text: 'Priority Leaderboard', active: false }
            ]
        },
        {
            id: 'gold',
            name: 'Gói Vàng',
            price: 1999000,
            maxOrders: 200,
            features: [
                { text: 'Featured Map Listing', active: true },
                { text: 'HD Audio & Transcripts', active: true },
                { text: '200 Orders/month', active: true },
                { text: 'AI Translation (3 languages)', active: true },
                { text: 'Priority Leaderboard', active: false }
            ]
        },
        {
            id: 'diamond',
            name: 'Gói Kim Cương',
            price: 4999000,
            maxOrders: null,
            features: [
                { text: 'Premium Map Placement', active: true },
                { text: 'Studio Quality Audio', active: true },
                { text: 'Unlimited Orders', active: true },
                { text: 'AI Translation (All languages)', active: true },
                { text: 'Top Priority Leaderboard', active: true }
            ]
        }
    ];

    function getPlans() {
        const raw = localStorage.getItem('exhib_plans');
        if (raw) try { return JSON.parse(raw); } catch (e) { }
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

    // ── Seed a default admin account if none exist ──
    function getAccounts() {
        const raw = localStorage.getItem('exhib_accounts');
        if (raw) try { return JSON.parse(raw); } catch (e) { }
        const defaults = [
            { name: 'Admin User', email: 'admin@audiotravelling.com', phone: '+84 123 456 789', password: btoa('admin123') }
        ];
        localStorage.setItem('exhib_accounts', JSON.stringify(defaults));
        return defaults;
    }

    function saveAccounts(accounts) {
        localStorage.setItem('exhib_accounts', JSON.stringify(accounts));
    }

    // ── Tab Switcher ──
    const tabLogin = document.getElementById('tab-login');
    const tabRegister = document.getElementById('tab-register');
    const tabIndicator = document.getElementById('auth-tab-indicator');
    const formLogin = document.getElementById('form-login');
    const formRegister = document.getElementById('form-register');

    function switchAuthTab(tab) {
        const isLogin = tab === 'login';

        // Tabs
        tabLogin.classList.toggle('active', isLogin);
        tabRegister.classList.toggle('active', !isLogin);
        tabIndicator.classList.toggle('right', !isLogin);

        // Forms
        formLogin.classList.toggle('active', isLogin);
        formRegister.classList.toggle('active', !isLogin);

        // Clear errors/success
        document.getElementById('login-error').classList.add('hidden');
        document.getElementById('register-error').classList.add('hidden');
        document.getElementById('register-success').classList.add('hidden');
    }

    tabLogin.addEventListener('click', () => switchAuthTab('login'));
    tabRegister.addEventListener('click', () => switchAuthTab('register'));

    // ── Password Visibility Toggle ──
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

    // ── Remember Me — auto-fill email ──
    const savedEmail = localStorage.getItem('exhib_rememberEmail');
    if (savedEmail) {
        document.getElementById('login-email').value = savedEmail;
        document.getElementById('login-remember').checked = true;
    }

    // ── Login Handler ──
    function doLogin(name, email) {
        // Store current session
        localStorage.setItem('exhib_session', JSON.stringify({ name, email, loggedIn: true }));

        // Update sidebar
        document.getElementById('sidebar-name').textContent = name;

        // Transition
        authScreen.style.display = 'none';
        dashboardApp.style.display = 'block';
        switchView('view-dashboard');
        showToast(`Welcome back, ${name.split(' ')[0]}!`, 'success');
    }

    formLogin.addEventListener('submit', e => {
        e.preventDefault();
        const email = document.getElementById('login-email').value.trim();
        const password = document.getElementById('login-password').value;
        const remember = document.getElementById('login-remember').checked;
        const errorEl = document.getElementById('login-error');
        const errorText = document.getElementById('login-error-text');
        const btn = document.getElementById('btn-login');

        errorEl.classList.add('hidden');

        // Validate
        const accounts = getAccounts();
        const account = accounts.find(a => a.email.toLowerCase() === email.toLowerCase());

        if (!account) {
            errorText.textContent = 'No account found with this email address.';
            errorEl.classList.remove('hidden');
            return;
        }

        if (account.password !== btoa(password)) {
            errorText.textContent = 'Incorrect password. Please try again.';
            errorEl.classList.remove('hidden');
            return;
        }

        // Remember me
        if (remember) {
            localStorage.setItem('exhib_rememberEmail', email);
        } else {
            localStorage.removeItem('exhib_rememberEmail');
        }

        // Loading state
        btn.disabled = true;
        btn.querySelector('.auth-btn-text').textContent = 'Signing in...';
        btn.querySelector('.auth-btn-arrow').className = 'fa-solid fa-circle-notch fa-spin auth-btn-arrow';

        setTimeout(() => {
            btn.disabled = false;
            btn.querySelector('.auth-btn-text').textContent = 'Sign In';
            btn.querySelector('.auth-btn-arrow').className = 'fa-solid fa-arrow-right auth-btn-arrow';
            doLogin(account.name, account.email);
        }, 800);
    });

    // ── Google Social Login ──
    document.getElementById('btn-google-login')?.addEventListener('click', () => {
        const btn = document.getElementById('btn-google-login');
        btn.innerHTML = '<i class="fa-solid fa-circle-notch fa-spin" style="margin-right:8px;"></i> Connecting...';
        btn.style.pointerEvents = 'none';

        setTimeout(() => {
            // Create or find Google account
            const accounts = getAccounts();
            let googleAcc = accounts.find(a => a.email === 'admin@audiotravelling.com');
            if (!googleAcc) {
                googleAcc = { name: 'Admin User', email: 'admin@audiotravelling.com', phone: '', password: btoa('google_sso') };
                accounts.push(googleAcc);
                saveAccounts(accounts);
            }

            btn.innerHTML = '<img src="https://www.svgrepo.com/show/475656/google-color.svg" style="width:20px;height:20px;" alt="Google"> Emergent Social Login';
            btn.style.pointerEvents = '';
            doLogin(googleAcc.name, googleAcc.email);
        }, 1000);
    });

    // ── Register Handler ──
    formRegister.addEventListener('submit', e => {
        e.preventDefault();
        const name = document.getElementById('reg-name').value.trim();
        const email = document.getElementById('reg-email').value.trim();
        const phone = document.getElementById('reg-phone').value.trim();
        const password = document.getElementById('reg-password').value;
        const confirm = document.getElementById('reg-confirm').value;
        const terms = document.getElementById('reg-terms').checked;
        const errorEl = document.getElementById('register-error');
        const errorText = document.getElementById('register-error-text');
        const successEl = document.getElementById('register-success');
        const btn = document.getElementById('btn-register');

        errorEl.classList.add('hidden');
        successEl.classList.add('hidden');

        // Validate
        if (password !== confirm) {
            errorText.textContent = 'Passwords do not match. Please try again.';
            errorEl.classList.remove('hidden');
            return;
        }

        if (password.length < 6) {
            errorText.textContent = 'Password must be at least 6 characters.';
            errorEl.classList.remove('hidden');
            return;
        }

        if (!terms) {
            errorText.textContent = 'You must agree to the Terms of Service.';
            errorEl.classList.remove('hidden');
            return;
        }

        const accounts = getAccounts();
        if (accounts.find(a => a.email.toLowerCase() === email.toLowerCase())) {
            errorText.textContent = 'An account with this email already exists.';
            errorEl.classList.remove('hidden');
            return;
        }

        // Loading state
        btn.disabled = true;
        btn.querySelector('.auth-btn-text').textContent = 'Creating account...';
        btn.querySelector('.auth-btn-arrow').className = 'fa-solid fa-circle-notch fa-spin auth-btn-arrow';

        setTimeout(() => {
            // Save account
            accounts.push({
                name,
                email,
                phone,
                password: btoa(password)
            });
            saveAccounts(accounts);

            btn.disabled = false;
            btn.querySelector('.auth-btn-text').textContent = 'Create Account';
            btn.querySelector('.auth-btn-arrow').className = 'fa-solid fa-user-plus auth-btn-arrow';

            // Show success, then switch to login
            successEl.classList.remove('hidden');
            formRegister.reset();

            setTimeout(() => {
                switchAuthTab('login');
                document.getElementById('login-email').value = email;
                successEl.classList.add('hidden');
                showToast('Account created! You can now sign in.', 'success');
            }, 1500);
        }, 1000);
    });

    // ── Logout ──
    btnLogout.addEventListener('click', () => {
        localStorage.removeItem('exhib_session');
        dashboardApp.style.display = 'none';
        authScreen.style.display = '';
        formLogin.reset();
        // Restore remembered email
        const rem = localStorage.getItem('exhib_rememberEmail');
        if (rem) {
            document.getElementById('login-email').value = rem;
            document.getElementById('login-remember').checked = true;
        }
        switchAuthTab('login');
        if (wavesurfer && wavesurfer.isPlaying()) wavesurfer.stop();
    });

    // ── Auto-login from session ──
    const existingSession = localStorage.getItem('exhib_session');
    if (existingSession) {
        try {
            const session = JSON.parse(existingSession);
            if (session.loggedIn) {
                authScreen.style.display = 'none';
                dashboardApp.style.display = 'block';
                document.getElementById('sidebar-name').textContent = session.name;
                setTimeout(() => switchView('view-dashboard'), 50);
            }
        } catch (e) { }
    }

    /* ══════════════════════════════════════════════
       3. SPA NAVIGATION
       ══════════════════════════════════════════════ */
    const navLinks = document.querySelectorAll('#sidebar-nav .nav-link[data-target]');
    const views = document.querySelectorAll('.view-section');
    const headerTitle = document.getElementById('header-title');

    const titles = {
        'view-dashboard': 'Dashboard Overview',
        'view-booths': 'Booth Manager',
        'view-finance': 'Finance & Commission',
        'view-plans': 'Subscription Plans',
        'view-settings': 'Admin Profile & Settings'
    };

    function switchView(targetId) {
        views.forEach(v => v.classList.remove('active'));

        const target = document.getElementById(targetId);
        if (target) target.classList.add('active');

        headerTitle.textContent = titles[targetId] || 'Admin Portal';

        navLinks.forEach(link => {
            link.classList.remove('active');
            if (link.getAttribute('data-target') === targetId) {
                link.classList.add('active');
            }
        });

        // Lazy init components
        if (targetId === 'view-dashboard') {
            renderDashboard();
        }
        if (targetId === 'view-booths') {
            renderBoothList();
            if (!mapInitialized) {
                setTimeout(() => initMap(), 150);
            } else {
                setTimeout(() => map.invalidateSize(), 50);
            }
        }
        if (targetId === 'view-finance') {
            populateFinanceBoothSelect();
        }
        if (targetId === 'view-plans') {
            renderPlans();
        }
        if (targetId === 'view-settings') {
            loadProfileData();
            loadTierConfigUI();
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
    let darkMode = false; // Default is light

    const savedTheme = localStorage.getItem('exhib_theme');
    if (savedTheme === 'dark') {
        darkMode = true;
        document.documentElement.removeAttribute('data-theme');
        themeBtn.querySelector('i').className = 'fa-solid fa-moon';
    } else {
        darkMode = false;
        document.documentElement.setAttribute('data-theme', 'light');
        themeBtn.querySelector('i').className = 'fa-solid fa-sun';
    }

    themeBtn.addEventListener('click', () => {
        darkMode = !darkMode;
        if (darkMode) {
            document.documentElement.removeAttribute('data-theme');
            localStorage.setItem('exhib_theme', 'dark');
            themeBtn.querySelector('i').className = 'fa-solid fa-moon';
            showToast('Dark mode active', 'info');
        } else {
            document.documentElement.setAttribute('data-theme', 'light');
            localStorage.setItem('exhib_theme', 'light');
            themeBtn.querySelector('i').className = 'fa-solid fa-sun';
            showToast('Light mode active', 'info');
        }
    });

    /* ══════════════════════════════════════════════
       5. DASHBOARD — Stats, Chart, Leaderboard
       ══════════════════════════════════════════════ */
    function renderDashboard() {
        const booths = getBooths();
        const orders = getOrders();
        const fairs = getFairs();
        const tierCfg = getTierConfig();

        // Stats
        document.getElementById('stat-fairs').textContent = fairs.length;
        document.getElementById('stat-booths').textContent = booths.length;

        // Total Revenue = sum of subscription revenue + commission
        let totalRevenue = 0;
        orders.filter(o => o.status === 'success').forEach(o => {
            totalRevenue += o.price * o.qty;
        });
        document.getElementById('stat-revenue').textContent = formatCurrency(totalRevenue) + ' ₫';

        // Visitor Engagement
        const totalEngagement = booths.reduce((sum, b) => sum + (b.engagement || 0), 0);
        document.getElementById('stat-visitors').textContent = totalEngagement >= 1000 ? (totalEngagement / 1000).toFixed(1) + 'k' : totalEngagement;

        // Chart
        if (!chartInitialized) initRevenueChart(orders, tierCfg, booths);

        // Activity
        renderActivity(orders);

        // Leaderboard
        renderLeaderboard(booths, orders, tierCfg);
    }

    function initRevenueChart(orders, tierCfg, booths) {
        chartInitialized = true;
        const ctx = document.getElementById('revenueChart').getContext('2d');

        // Compute daily commissions for the last 7 days
        const dayLabels = [];
        const dayData = [];
        for (let i = 6; i >= 0; i--) {
            const d = new Date();
            d.setDate(d.getDate() - i);
            const dateStr = d.toISOString().split('T')[0];
            dayLabels.push(d.toLocaleDateString('en-US', { month: 'short', day: 'numeric' }));

            // Calculate commission for this day
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

        // Pad with simulated data for visual appeal
        if (dayData.every(v => v === 0)) {
            const simulated = [320, 480, 580, 420, 750, 890, 1120];
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
                        bodyFont: { family: 'Inter' },
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
                        grid: { color: 'rgba(255,255,255,0.04)', drawBorder: false },
                        ticks: {
                            color: '#6b8f7a',
                            font: { family: 'Inter', size: 11 },
                            callback: v => formatCurrency(v) + ' ₫'
                        }
                    },
                    x: {
                        grid: { display: false, drawBorder: false },
                        ticks: { color: '#6b8f7a', font: { family: 'Inter', size: 11 } }
                    }
                }
            }
        });
    }

    function renderActivity(orders) {
        const container = document.getElementById('activity-list');
        const recent = [...orders].sort((a, b) => b.date.localeCompare(a.date)).slice(0, 5);
        const colors = ['var(--emerald)', '#60a5fa', '#fbbf24', '#a78bfa', '#f472b6'];

        container.innerHTML = recent.map((o, i) => `
            <div class="activity-item">
                <div class="activity-dot" style="background:${colors[i % colors.length]};"></div>
                <div>
                    <p class="activity-text">Order <strong>${o.id}</strong> — ${o.product} (×${o.qty})</p>
                    <p class="activity-time">${o.date}</p>
                </div>
            </div>
        `).join('');
    }

    function renderLeaderboard(booths, orders, tierCfg) {
        const tbody = document.getElementById('leaderboard-body');

        // Calculate priority score: P = TierWeight × (OrderCount + Engagement/100)
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
            const rankClass = rank <= 3 ? `rank-${rank}` : 'rank-other';
            const tierClass = `tier-${b.tier.toLowerCase()}`;
            return `
                <tr>
                    <td><span class="leaderboard-rank ${rankClass}">${rank}</span></td>
                    <td style="font-weight:600;">${b.name}</td>
                    <td style="color:var(--text-muted);">${b.company}</td>
                    <td><span class="tier-badge ${tierClass}">${b.tier.toUpperCase()}</span></td>
                    <td>${b.orderCount}</td>
                    <td style="text-align:right;"><span class="priority-score">${b.priority.toFixed(1)}</span></td>
                </tr>
            `;
        }).join('');
    }

    /* ══════════════════════════════════════════════
       6. BOOTH MANAGER
       ══════════════════════════════════════════════ */
    const boothName = document.getElementById('booth-name');
    const boothCompany = document.getElementById('booth-company');
    const boothCategory = document.getElementById('booth-category');
    const boothTier = document.getElementById('booth-tier');
    const boothScript = document.getElementById('booth-script');
    const inputLat = document.getElementById('input-lat');
    const inputLng = document.getElementById('input-lng');
    const radiusSlider = document.getElementById('radius-slider');
    const radiusVal = document.getElementById('radius-val');

    function renderBoothList() {
        const booths = getBooths();
        const list = document.getElementById('booth-list');
        list.innerHTML = '';

        booths.forEach(b => {
            const item = document.createElement('div');
            item.className = 'booth-list-item' + (selectedBoothId === b.id ? ' active' : '');
            const tierClass = `tier-${b.tier.toLowerCase()}`;
            item.innerHTML = `
                <div class="booth-list-item-name">${b.name}</div>
                <div class="booth-list-item-company">${b.company}</div>
                <div class="booth-list-item-meta">
                    <span class="tier-badge ${tierClass}" style="font-size:10px; padding:2px 8px;">${b.tier}</span>
                    <span style="font-size:11px; color:var(--text-dim);">${b.category}</span>
                </div>
            `;
            item.addEventListener('click', () => selectBooth(b.id));
            list.appendChild(item);
        });

        // If no booth selected, select first
        if (!selectedBoothId && booths.length > 0) {
            selectBooth(booths[0].id);
        } else if (selectedBoothId) {
            loadBoothIntoForm(selectedBoothId);
        }
    }

    function selectBooth(id) {
        selectedBoothId = id;
        // Update active states
        document.querySelectorAll('.booth-list-item').forEach(el => el.classList.remove('active'));
        const items = document.querySelectorAll('.booth-list-item');
        const booths = getBooths();
        const idx = booths.findIndex(b => b.id === id);
        if (idx >= 0 && items[idx]) items[idx].classList.add('active');

        loadBoothIntoForm(id);
    }

    function loadBoothIntoForm(id) {
        const booths = getBooths();
        const b = booths.find(x => x.id === id);
        if (!b) return;

        boothName.value = b.name;
        boothCompany.value = b.company;
        boothCategory.value = b.category;
        boothTier.value = b.tier;
        boothScript.value = b.script || '';
        inputLat.value = b.lat || 10.7680;
        inputLng.value = b.lng || 106.7050;
        radiusSlider.value = b.radius || 30;
        radiusVal.textContent = (b.radius || 30) + 'm';

        updateWordCount();
        updateMapFromInputs();
    }

    // Save Booth
    document.getElementById('btn-save-booth')?.addEventListener('click', () => {
        if (!selectedBoothId) {
            showToast('Please select or add a booth first.', 'error');
            return;
        }

        const booths = getBooths();
        const idx = booths.findIndex(b => b.id === selectedBoothId);
        if (idx < 0) return;

        booths[idx].name = boothName.value;
        booths[idx].company = boothCompany.value;
        booths[idx].category = boothCategory.value;
        booths[idx].tier = boothTier.value;
        booths[idx].script = boothScript.value;
        booths[idx].lat = parseFloat(inputLat.value);
        booths[idx].lng = parseFloat(inputLng.value);
        booths[idx].radius = parseInt(radiusSlider.value);

        saveBooths(booths);
        renderBoothList();

        const lastSaved = document.getElementById('last-saved-time');
        lastSaved.textContent = `Saved at ${new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}`;
        showToast('Booth saved successfully!', 'success');
    });

    // Add Booth
    document.getElementById('btn-add-booth')?.addEventListener('click', () => {
        const booths = getBooths();
        const newId = 'B' + String(booths.length + 1).padStart(3, '0');
        const newBooth = {
            id: newId,
            name: 'New Booth',
            company: 'Company Name',
            category: 'Other',
            tier: 'Basic',
            lat: 10.7720,
            lng: 106.6980,
            radius: 20,
            script: '',
            engagement: 0
        };
        booths.push(newBooth);
        saveBooths(booths);
        selectedBoothId = newId;
        renderBoothList();
        showToast(`Booth ${newId} created!`, 'success');
    });

    /* ══════════════════════════════════════════════
       7. WAVESURFER — Audio Editor
       ══════════════════════════════════════════════ */
    let wavesurfer = null;
    const uploadOverlay = document.getElementById('audio-upload-overlay');
    const audioInput = document.getElementById('audio-input');
    const btnPlay = document.getElementById('btn-play');
    const btnStop = document.getElementById('btn-stop');
    const btnClearAudio = document.getElementById('btn-clear-audio');
    const audioStatus = document.getElementById('audio-status');
    const volumeSlider = document.getElementById('volume-slider');

    function initWaveSurferInstance() {
        if (wavesurfer) wavesurfer.destroy();

        wavesurfer = WaveSurfer.create({
            container: '#waveform',
            waveColor: '#1e3524',
            progressColor: emerald,
            cursorColor: '#059669',
            barWidth: 2,
            barGap: 1,
            barRadius: 2,
            height: 80,
            normalize: true
        });

        wavesurfer.on('ready', () => {
            document.getElementById('time-total').textContent = formatTime(wavesurfer.getDuration());
            btnPlay.disabled = false;
            btnStop.disabled = false;
            uploadOverlay.classList.add('hidden');
            btnClearAudio.classList.remove('hidden');
            audioStatus.textContent = 'Ready to play';
            audioStatus.style.color = emerald;
        });

        wavesurfer.on('audioprocess', () => {
            document.getElementById('time-current').textContent = formatTime(wavesurfer.getCurrentTime());
        });

        wavesurfer.on('finish', () => {
            btnPlay.innerHTML = '<i class="fa-solid fa-play"></i>';
        });
    }

    uploadOverlay.addEventListener('click', () => audioInput.click());
    uploadOverlay.addEventListener('dragover', e => { e.preventDefault(); uploadOverlay.classList.add('dragging'); });
    uploadOverlay.addEventListener('dragleave', () => { uploadOverlay.classList.remove('dragging'); });
    uploadOverlay.addEventListener('drop', e => {
        e.preventDefault();
        uploadOverlay.classList.remove('dragging');
        if (e.dataTransfer.files[0]) processAudioFile(e.dataTransfer.files[0]);
    });

    audioInput.addEventListener('change', e => {
        if (e.target.files[0]) processAudioFile(e.target.files[0]);
    });

    function processAudioFile(file) {
        if (!file.type.startsWith('audio/')) {
            showToast('Please upload a valid audio file.', 'error');
            return;
        }
        audioStatus.textContent = 'Loading waveform...';
        audioStatus.style.color = '';
        initWaveSurferInstance();
        wavesurfer.load(URL.createObjectURL(file));
    }

    btnPlay.addEventListener('click', () => {
        if (!wavesurfer) return;
        wavesurfer.playPause();
        btnPlay.innerHTML = wavesurfer.isPlaying()
            ? '<i class="fa-solid fa-pause"></i>'
            : '<i class="fa-solid fa-play"></i>';
    });

    btnStop.addEventListener('click', () => {
        if (!wavesurfer) return;
        wavesurfer.stop();
        btnPlay.innerHTML = '<i class="fa-solid fa-play"></i>';
        document.getElementById('time-current').textContent = '0:00';
    });

    volumeSlider?.addEventListener('input', e => {
        if (wavesurfer) wavesurfer.setVolume(Number(e.target.value));
    });

    btnClearAudio.addEventListener('click', () => {
        if (wavesurfer) { wavesurfer.destroy(); wavesurfer = null; }
        uploadOverlay.classList.remove('hidden');
        btnClearAudio.classList.add('hidden');
        btnPlay.disabled = true;
        btnStop.disabled = true;
        audioInput.value = '';
        document.getElementById('time-current').textContent = '0:00';
        document.getElementById('time-total').textContent = '0:00';
        btnPlay.innerHTML = '<i class="fa-solid fa-play"></i>';
        audioStatus.textContent = 'No file loaded';
        audioStatus.style.color = '';
    });

    function formatTime(sec) {
        const min = Math.floor(sec / 60);
        const s = Math.floor(sec % 60);
        return `${min}:${s < 10 ? '0' : ''}${s}`;
    }

    /* ══════════════════════════════════════════════
       8. LEAFLET MAP & GEOFENCING
       ══════════════════════════════════════════════ */
    let map, marker, circle;

    function initMap() {
        mapInitialized = true;
        const lat = parseFloat(inputLat.value) || 10.7680;
        const lng = parseFloat(inputLng.value) || 106.7050;
        const radius = parseFloat(radiusSlider.value) || 30;

        map = L.map('map', { zoomControl: true, attributionControl: true }).setView([lat, lng], 17);

        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            attribution: '© OpenStreetMap'
        }).addTo(map);

        marker = L.marker([lat, lng], { draggable: true }).addTo(map);

        circle = L.circle([lat, lng], {
            color: emerald,
            fillColor: emerald,
            fillOpacity: 0.15,
            weight: 2,
            radius: radius
        }).addTo(map);

        marker.on('dragend', function () {
            const pos = marker.getLatLng();
            inputLat.value = pos.lat.toFixed(6);
            inputLng.value = pos.lng.toFixed(6);
            circle.setLatLng(pos);
            map.panTo(pos);
        });
    }

    function updateMapFromInputs() {
        if (!map) return;
        const lat = parseFloat(inputLat.value);
        const lng = parseFloat(inputLng.value);
        if (isNaN(lat) || isNaN(lng)) return;

        const pos = [lat, lng];
        marker.setLatLng(pos);
        circle.setLatLng(pos);
        map.panTo(pos);
    }

    inputLat.addEventListener('input', updateMapFromInputs);
    inputLng.addEventListener('input', updateMapFromInputs);

    radiusSlider.addEventListener('input', e => {
        const val = e.target.value;
        radiusVal.textContent = `${val}m`;
        if (circle) circle.setRadius(Number(val));
    });

    /* ══════════════════════════════════════════════
       9. AI OPTIMIZER
       ══════════════════════════════════════════════ */
    const btnAi = document.getElementById('btn-ai-optimize');
    const btnAiIcon = btnAi.querySelector('.ai-icon');
    const btnAiText = btnAi.querySelector('.ai-text');
    const scriptArea = document.getElementById('booth-script');
    const aiOverlay = document.getElementById('ai-loading-overlay');
    const wordCountEl = document.getElementById('word-count');
    const langSelect = document.getElementById('lang-select');
    const scriptTitle = document.getElementById('script-title');

    scriptArea.addEventListener('input', updateWordCount);

    function updateWordCount() {
        const text = scriptArea.value.trim();
        const words = text ? text.split(/\s+/).length : 0;
        const estSec = Math.round((words / 150) * 60);
        wordCountEl.textContent = `${words} words · Est. duration: ${formatTime(estSec)}`;
    }

    langSelect.addEventListener('change', e => {
        scriptTitle.textContent = `Audio Script (${e.target.value})`;
    });

    btnAi.addEventListener('click', () => {
        if (btnAi.disabled) return;
        btnAi.disabled = true;
        btnAi.classList.remove('pulse-glow');
        scriptArea.disabled = true;
        aiOverlay.classList.remove('hidden');
        btnAiIcon.className = 'fa-solid fa-circle-notch fa-spin ai-icon';
        btnAiText.textContent = 'Optimizing...';

        const currentName = boothName.value || 'this booth';
        const lang = langSelect.value;

        setTimeout(() => {
            const optimizedScripts = {
                'EN': `🎯 Welcome to ${currentName}!\n\nStep into an extraordinary experience where innovation meets inspiration. Our carefully curated products represent the pinnacle of quality and design in their category.\n\nExplore our interactive displays, engage with our expert team, and discover solutions tailored just for you. Whether you're a first-time visitor or a returning partner, we have something special waiting.\n\nDon't miss our exclusive exhibition-only offers — available for a limited time. Scan the QR code at our booth for instant access to our digital catalog and special pricing.\n\nThank you for visiting ${currentName}. Let's create something amazing together! 🌟`,
                'VI': `🎯 Chào mừng đến với ${currentName}!\n\nBước vào một trải nghiệm phi thường nơi sự đổi mới gặp gỡ cảm hứng. Các sản phẩm được tuyển chọn kỹ lưỡng của chúng tôi đại diện cho đỉnh cao của chất lượng và thiết kế.\n\nKhám phá các màn hình tương tác, trao đổi với đội ngũ chuyên gia, và khám phá giải pháp phù hợp dành riêng cho bạn.\n\nCảm ơn quý khách đã ghé thăm ${currentName}! 🌟`,
                'JA': `🎯 ${currentName}へようこそ！\n\nイノベーションとインスピレーションが出会う特別な体験へ。厳選された製品は、品質とデザインの頂点を代表しています。\n\nインタラクティブなディスプレイを探索し、専門チームと交流しましょう。\n\n${currentName}をご訪問いただきありがとうございます！🌟`,
                'FR': `🎯 Bienvenue chez ${currentName} !\n\nEntrez dans une expérience extraordinaire où l'innovation rencontre l'inspiration. Nos produits soigneusement sélectionnés représentent le summum de la qualité et du design.\n\nExplorez nos présentations interactives et découvrez des solutions sur mesure.\n\nMerci de visiter ${currentName} ! 🌟`,
                'KO': `🎯 ${currentName}에 오신 것을 환영합니다!\n\n혁신과 영감이 만나는 특별한 경험을 시작하세요. 엄선된 제품들로 최고의 품질과 디자인을 경험하실 수 있습니다.\n\n${currentName}을 방문해 주셔서 감사합니다! 🌟`,
                'ZH': `🎯 欢迎来到 ${currentName}！\n\n步入创新与灵感交汇的非凡体验。我们精心策划的产品代表着品质与设计的巅峰。\n\n感谢您访问 ${currentName}！🌟`
            };

            scriptArea.value = optimizedScripts[lang] || optimizedScripts['EN'];
            updateWordCount();

            btnAi.disabled = false;
            btnAi.classList.add('pulse-glow');
            scriptArea.disabled = false;
            aiOverlay.classList.add('hidden');
            btnAiIcon.className = 'fa-solid fa-wand-magic-sparkles ai-icon';
            btnAiText.textContent = 'AI Optimize';

            showToast(`Script optimized for ${lang} by Emergent AI!`, 'success');
        }, 2500);
    });

    /* ══════════════════════════════════════════════
       10. FINANCE & COMMISSION
       ══════════════════════════════════════════════ */
    const financeSearchInput = document.getElementById('finance-booth-search');
    const financeSearchResults = document.getElementById('finance-search-results');
    const financeSearchClear = document.getElementById('finance-search-clear');
    let selectedFinanceBoothId = 'all';

    function populateFinanceBoothSelect() {
        // Show all orders by default when navigating to Finance
        selectedFinanceBoothId = 'all';
        financeSearchInput.value = '';
        financeSearchClear.classList.add('hidden');
        financeSearchResults.classList.add('hidden');
        renderFinanceData('all');
    }

    // ── Search Input Handler ──
    financeSearchInput.addEventListener('input', () => {
        const query = financeSearchInput.value.trim().toLowerCase();

        // Show/hide clear button
        if (query.length > 0) {
            financeSearchClear.classList.remove('hidden');
        } else {
            financeSearchClear.classList.add('hidden');
        }

        showSearchResults(query);
    });

    financeSearchInput.addEventListener('focus', () => {
        const query = financeSearchInput.value.trim().toLowerCase();
        showSearchResults(query);
    });

    // ── Show Search Results Dropdown ──
    function showSearchResults(query) {
        const booths = getBooths();
        const orders = getOrders();

        let html = '';

        // "Show All" option
        html += `
            <div class="booth-search-all" data-booth-id="all">
                <i class="fa-solid fa-layer-group"></i> Show All Booths
            </div>
        `;

        // Filter booths
        const filtered = query
            ? booths.filter(b => b.name.toLowerCase().includes(query) || b.company.toLowerCase().includes(query))
            : booths;

        if (filtered.length === 0) {
            html += `
                <div class="booth-search-empty">
                    <i class="fa-solid fa-magnifying-glass"></i>
                    No booths found for "${escapeHtml(query)}"
                </div>
            `;
        } else {
            filtered.forEach(b => {
                const boothOrders = orders.filter(o => o.boothId === b.id && o.status === 'success');
                const tierClass = 'tier-' + b.tier.toLowerCase();

                // Highlight matching text
                let nameHtml = escapeHtml(b.name);
                if (query) {
                    const regex = new RegExp(`(${escapeRegex(query)})`, 'gi');
                    nameHtml = escapeHtml(b.name).replace(regex, '<mark>$1</mark>');
                }

                const isActive = selectedFinanceBoothId === b.id ? ' active' : '';

                html += `
                    <div class="booth-search-item${isActive}" data-booth-id="${b.id}">
                        <div class="booth-search-item-info">
                            <div class="booth-search-item-name">${nameHtml}</div>
                            <div class="booth-search-item-company">${escapeHtml(b.company)} · ${boothOrders.length} orders</div>
                        </div>
                        <span class="tier-badge ${tierClass}" style="font-size:10px; padding:2px 8px;">${b.tier}</span>
                    </div>
                `;
            });
        }

        financeSearchResults.innerHTML = html;
        financeSearchResults.classList.remove('hidden');

        // Bind click handlers
        financeSearchResults.querySelectorAll('[data-booth-id]').forEach(el => {
            el.addEventListener('click', () => {
                const id = el.getAttribute('data-booth-id');
                selectFinanceBooth(id);
            });
        });
    }

    function selectFinanceBooth(id) {
        selectedFinanceBoothId = id;
        financeSearchResults.classList.add('hidden');

        if (id === 'all') {
            financeSearchInput.value = '';
            financeSearchClear.classList.add('hidden');
        } else {
            const booths = getBooths();
            const booth = booths.find(b => b.id === id);
            if (booth) {
                financeSearchInput.value = booth.name;
                financeSearchClear.classList.remove('hidden');
            }
        }

        renderFinanceData(id);
    }

    // ── Clear Button ──
    financeSearchClear.addEventListener('click', () => {
        financeSearchInput.value = '';
        financeSearchClear.classList.add('hidden');
        selectedFinanceBoothId = 'all';
        renderFinanceData('all');
        financeSearchResults.classList.add('hidden');
    });

    // ── Close dropdown on outside click ──
    document.addEventListener('click', e => {
        if (!e.target.closest('.booth-search-wrapper')) {
            financeSearchResults.classList.add('hidden');
        }
    });

    function escapeHtml(str) {
        return str.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;');
    }

    function escapeRegex(str) {
        return str.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
    }

    function renderFinanceData(boothId) {
        if (!boothId) boothId = selectedFinanceBoothId;
        const orders = getOrders();
        const booths = getBooths();
        const tierCfg = getTierConfig();

        // Filter orders
        const filtered = boothId === 'all'
            ? orders.filter(o => o.status === 'success')
            : orders.filter(o => o.boothId === boothId && o.status === 'success');

        // Render orders table
        const tbody = document.getElementById('orders-body');
        document.getElementById('order-count').textContent = `${filtered.length} orders`;

        tbody.innerHTML = filtered.map(o => {
            const total = o.price * o.qty;
            return `
                <tr>
                    <td style="font-family:var(--font-mono); font-size:12px; color:var(--text-muted);">${o.id}</td>
                    <td style="font-weight:600;">${o.product}</td>
                    <td>${o.qty}</td>
                    <td>${formatCurrency(o.price)} ₫</td>
                    <td style="font-weight:700;">${formatCurrency(total)} ₫</td>
                    <td><span class="order-status success"><i class="fa-solid fa-circle-check" style="font-size:10px;"></i> Success</span></td>
                    <td style="color:var(--text-muted);">${o.date}</td>
                </tr>
            `;
        }).join('');

        // Commission Calculator
        if (boothId === 'all') {
            // Calculate total commission across all booths
            let totalCommission = 0;
            let totalN = filtered.length;
            let sumPriceRc = 0;
            let sumNF = 0;

            filtered.forEach(o => {
                const booth = booths.find(b => b.id === o.boothId);
                if (!booth) return;
                const cfg = tierCfg[booth.tier] || tierCfg.Basic;
                sumPriceRc += o.price * o.qty * cfg.rc;
                sumNF += cfg.fee;
            });

            totalCommission = sumPriceRc + sumNF;

            document.getElementById('calc-tier').textContent = 'Mixed';
            document.getElementById('calc-rc').textContent = 'Varies';
            document.getElementById('calc-fee').textContent = 'Varies';
            document.getElementById('calc-n').textContent = totalN;
            document.getElementById('calc-sum').textContent = formatCurrency(sumPriceRc) + ' ₫';
            document.getElementById('calc-nf').textContent = formatCurrency(sumNF) + ' ₫';
            document.getElementById('calc-total').textContent = formatCurrency(totalCommission) + ' ₫';
        } else {
            const booth = booths.find(b => b.id === boothId);
            if (!booth) return;

            const cfg = tierCfg[booth.tier] || tierCfg.Basic;
            const n = filtered.length;
            let sumPriceRc = 0;
            filtered.forEach(o => {
                sumPriceRc += o.price * o.qty * cfg.rc;
            });
            const nF = n * cfg.fee;
            const totalCommission = sumPriceRc + nF;

            document.getElementById('calc-tier').innerHTML = `<span class="tier-badge tier-${booth.tier.toLowerCase()}">${booth.tier.toUpperCase()}</span>`;
            document.getElementById('calc-rc').textContent = (cfg.rc * 100).toFixed(1) + '%';
            document.getElementById('calc-fee').textContent = formatCurrency(cfg.fee) + ' ₫';
            document.getElementById('calc-n').textContent = n;
            document.getElementById('calc-sum').textContent = formatCurrency(sumPriceRc) + ' ₫';
            document.getElementById('calc-nf').textContent = formatCurrency(nF) + ' ₫';
            document.getElementById('calc-total').textContent = formatCurrency(totalCommission) + ' ₫';
        }
    }

    /* ══════════════════════════════════════════════
       11. PROFILE EDITING
       ══════════════════════════════════════════════ */
    const profileElements = {
        btnEdit: document.getElementById('btn-edit-profile'),
        btnSave: document.getElementById('btn-save-profile'),
        btnCancel: document.getElementById('btn-cancel-profile'),
        editActions: document.getElementById('profile-edit-actions'),
        inputs: document.querySelectorAll('.profile-editable'),
        avatarInput: document.getElementById('profile-avatar-input'),
        avatarPreview: document.getElementById('profile-avatar-preview'),
        avatarOverlay: document.getElementById('profile-avatar-overlay'),
        nameInput: document.getElementById('set-name'),
        phoneInput: document.getElementById('set-phone'),
        bioInput: document.getElementById('set-bio'),
        sidebarName: document.getElementById('sidebar-name'),
        sidebarAvatar: document.getElementById('sidebar-avatar')
    };

    let originalProfile = {};

    function loadProfileData() {
        const raw = localStorage.getItem('exhib_profile');
        if (!raw) return;
        try {
            const p = JSON.parse(raw);
            if (p.name) { profileElements.nameInput.value = p.name; profileElements.sidebarName.textContent = p.name; }
            if (p.phone) profileElements.phoneInput.value = p.phone;
            if (p.bio) profileElements.bioInput.value = p.bio;
            if (p.avatar) { profileElements.avatarPreview.src = p.avatar; profileElements.sidebarAvatar.src = p.avatar; }
        } catch (e) { }
    }

    profileElements.btnEdit?.addEventListener('click', () => {
        originalProfile = {
            name: profileElements.nameInput.value,
            phone: profileElements.phoneInput.value,
            bio: profileElements.bioInput.value,
            avatar: profileElements.avatarPreview.src
        };

        profileElements.btnEdit.classList.add('hidden');
        profileElements.editActions.classList.remove('hidden');
        profileElements.editActions.style.display = 'flex';

        profileElements.inputs.forEach(inp => {
            inp.disabled = false;
            inp.style.borderColor = 'rgba(34, 197, 94, 0.3)';
            inp.style.background = 'rgba(255,255,255,0.05)';
        });

        profileElements.avatarOverlay.classList.add('editable');
        profileElements.avatarOverlay.style.pointerEvents = 'auto';
    });

    profileElements.btnCancel?.addEventListener('click', () => {
        profileElements.nameInput.value = originalProfile.name;
        profileElements.phoneInput.value = originalProfile.phone;
        profileElements.bioInput.value = originalProfile.bio;
        profileElements.avatarPreview.src = originalProfile.avatar;
        disableProfileEdit();
    });

    profileElements.btnSave?.addEventListener('click', () => {
        const data = {
            name: profileElements.nameInput.value,
            phone: profileElements.phoneInput.value,
            bio: profileElements.bioInput.value,
            avatar: profileElements.avatarPreview.src
        };

        localStorage.setItem('exhib_profile', JSON.stringify(data));
        profileElements.sidebarName.textContent = data.name;
        profileElements.sidebarAvatar.src = data.avatar;

        disableProfileEdit();
        showToast('Profile updated successfully!', 'success');
    });

    function disableProfileEdit() {
        profileElements.btnEdit.classList.remove('hidden');
        profileElements.editActions.classList.add('hidden');
        profileElements.editActions.style.display = '';

        profileElements.inputs.forEach(inp => {
            inp.disabled = true;
            inp.style.borderColor = '';
            inp.style.background = '';
        });

        profileElements.avatarOverlay.classList.remove('editable');
        profileElements.avatarOverlay.style.pointerEvents = '';
    }

    profileElements.avatarOverlay?.addEventListener('click', () => {
        if (profileElements.avatarOverlay.classList.contains('editable')) {
            profileElements.avatarInput.click();
        }
    });

    profileElements.avatarInput?.addEventListener('change', e => {
        const file = e.target.files[0];
        if (!file) return;
        if (!file.type.startsWith('image/')) {
            showToast('Please select an image file.', 'error');
            return;
        }
        const reader = new FileReader();
        reader.onload = ev => { profileElements.avatarPreview.src = ev.target.result; };
        reader.readAsDataURL(file);
    });

    /* ══════════════════════════════════════════════
       12. SETTINGS — Tier Config Save
       ══════════════════════════════════════════════ */
    function loadTierConfigUI() {
        const cfg = getTierConfig();
        document.getElementById('rc-basic').value = cfg.Basic?.rc ?? 0.10;
        document.getElementById('fee-basic').value = cfg.Basic?.fee ?? 1.50;
        document.getElementById('rc-gold').value = cfg.Gold?.rc ?? 0.08;
        document.getElementById('fee-gold').value = cfg.Gold?.fee ?? 1.00;
        document.getElementById('rc-diamond').value = cfg.Diamond?.rc ?? 0.05;
        document.getElementById('fee-diamond').value = cfg.Diamond?.fee ?? 0.50;
    }

    document.getElementById('btn-save-settings')?.addEventListener('click', () => {
        const cfg = {
            Basic: {
                rc: parseFloat(document.getElementById('rc-basic').value) || 0.10,
                fee: parseFloat(document.getElementById('fee-basic').value) || 1.50,
                weight: 1
            },
            Gold: {
                rc: parseFloat(document.getElementById('rc-gold').value) || 0.08,
                fee: parseFloat(document.getElementById('fee-gold').value) || 1.00,
                weight: 2
            },
            Diamond: {
                rc: parseFloat(document.getElementById('rc-diamond').value) || 0.05,
                fee: parseFloat(document.getElementById('fee-diamond').value) || 0.50,
                weight: 3
            }
        };

        saveTierConfig(cfg);
        showToast('Configuration saved! Commission rates updated.', 'success');
    });

    /* ══════════════════════════════════════════════
       13. TOAST NOTIFICATIONS
       ══════════════════════════════════════════════ */
    function showToast(message, type = 'success') {
        const bgs = {
            success: 'linear-gradient(135deg, #059669, #22c55e)',
            error: 'linear-gradient(135deg, #dc2626, #ef4444)',
            info: 'linear-gradient(135deg, #0369a1, #0ea5e9)'
        };

        Toastify({
            text: message,
            duration: 3000,
            gravity: 'bottom',
            position: 'right',
            stopOnFocus: true,
            style: {
                background: bgs[type],
                borderRadius: '12px',
                fontFamily: 'Inter, sans-serif',
                fontSize: '13px',
                fontWeight: '500',
                boxShadow: '0 8px 32px rgba(0,0,0,0.3)',
                padding: '12px 20px'
            }
        }).showToast();
    }

    /* ══════════════════════════════════════════════
       14. RIPPLE EFFECT
       ══════════════════════════════════════════════ */
    function createRipple(e, element) {
        const ripple = document.createElement('span');
        ripple.className = 'ripple-effect';
        const rect = element.getBoundingClientRect();
        const size = Math.max(rect.width, rect.height);
        ripple.style.width = ripple.style.height = size + 'px';
        ripple.style.left = (e.clientX - rect.left - size / 2) + 'px';
        ripple.style.top = (e.clientY - rect.top - size / 2) + 'px';
        element.appendChild(ripple);
        setTimeout(() => ripple.remove(), 600);
    }

    document.addEventListener('click', e => {
        const btn = e.target.closest('.btn-ripple');
        if (btn) createRipple(e, btn);
    });

    /* ══════════════════════════════════════════════
       15. UTILITY
       ══════════════════════════════════════════════ */
    function formatCurrency(num) {
        return Math.round(num).toString().replace(/\B(?=(\d{3})+(?!\d))/g, '.');
    }

    /* ══════════════════════════════════════════════
       16. SUBSCRIPTION PLANS
       ══════════════════════════════════════════════ */
    const plansGrid = document.getElementById('plans-grid');

    function renderPlans() {
        if (!plansGrid) return;
        const plans = getPlans();
        
        let html = '';
        plans.forEach(plan => {
            const badgeClass = `tier-${plan.id}`;
            const limitText = plan.maxOrders ? `${plan.maxOrders} orders/mo` : 'Unlimited orders';
            
            html += `
                <div class="glass-card plan-card">
                    <div class="plan-header">
                        <div class="plan-name">
                            ${plan.name}
                        </div>
                        <div class="plan-badge ${badgeClass}">${plan.id}</div>
                    </div>
                    
                    <div class="plan-price">
                        ${formatCurrency(plan.price)} ₫<span>/tháng</span>
                    </div>
                    
                    <ul class="plan-features">
                        ${plan.features.map(f => `
                            <li class="${f.active ? '' : 'disabled'}">
                                <i class="fa-solid fa-${f.active ? 'check' : 'xmark'}"></i>
                                ${f.text}
                            </li>
                        `).join('')}
                    </ul>
                    
                    <div class="plan-actions">
                        <button class="btn-ghost btn-edit-plan" data-id="${plan.id}">
                            <i class="fa-solid fa-pen-to-square"></i> Edit Plan
                        </button>
                    </div>
                </div>
            `;
        });
        
        plansGrid.innerHTML = html;

        // Bind Edit logic
        plansGrid.querySelectorAll('.btn-edit-plan').forEach(btn => {
            btn.addEventListener('click', () => {
                const id = btn.getAttribute('data-id');
                openEditPlanModal(id);
            });
        });
    }

    // Modal logic (created dynamically to keep HTML clean)
    function openEditPlanModal(id) {
        let modalOverlay = document.getElementById('plan-modal-overlay');
        if (!modalOverlay) {
            modalOverlay = document.createElement('div');
            modalOverlay.id = 'plan-modal-overlay';
            modalOverlay.className = 'plan-modal-overlay';
            document.body.appendChild(modalOverlay);
        }

        const plans = getPlans();
        const plan = plans.find(p => p.id === id);
        if (!plan) return;
        
        const isUnlimited = plan.maxOrders === null;

        modalOverlay.innerHTML = `
            <div class="plan-modal" style="max-height:90vh; display:flex; flex-direction:column;">
                <div class="plan-modal-header" style="flex-shrink:0;">
                    <h3 class="plan-modal-title">Plan Details: ${plan.name}</h3>
                    <button class="plan-modal-close"><i class="fa-solid fa-xmark"></i></button>
                </div>
                <div style="display:flex; flex-direction:column; gap:16px; overflow-y:auto; padding-right:8px;">
                    <div style="display:grid; grid-template-columns:1fr 1fr; gap:16px;">
                        <div>
                            <label class="form-label">Tên gói</label>
                            <input type="text" id="edit-plan-name" class="form-input" value="${plan.name}">
                        </div>
                        <div>
                            <label class="form-label">Giá mỗi tháng (VNĐ)</label>
                            <input type="number" id="edit-plan-price" class="form-input mono" value="${plan.price}">
                        </div>
                    </div>
                    
                    <div>
                        <label class="form-label">Max Orders per month</label>
                        <div style="display:flex; align-items:center; gap:12px;">
                            <input type="number" id="edit-plan-orders" class="form-input mono" value="${isUnlimited ? '' : plan.maxOrders}" placeholder="Unlimited" ${isUnlimited ? 'disabled' : ''}>
                            <label style="display:flex; align-items:center; gap:6px; font-size:13px; color:var(--text-secondary); cursor:pointer;">
                                <input type="checkbox" id="edit-plan-unlimited" class="form-checkbox" ${isUnlimited ? 'checked' : ''}>
                                Unlimited Orders
                            </label>
                        </div>
                    </div>

                    <div style="margin-top:8px;">
                        <label class="form-label">Plan Features</label>
                        <div style="background:rgba(0,0,0,0.15); border:1px solid var(--border); border-radius:10px; padding:12px; display:flex; flex-direction:column; gap:10px;">
                            ${plan.features.map((f, i) => `
                                <div style="display:flex; align-items:center; gap:12px;">
                                    <input type="checkbox" id="edit-feat-${i}-active" class="form-checkbox" ${f.active ? 'checked' : ''} style="margin-top:2px;">
                                    <input type="text" id="edit-feat-${i}-text" class="form-input" value="${f.text}" style="padding:6px 10px; font-size:13px;">
                                </div>
                            `).join('')}
                        </div>
                    </div>
                </div>
                
                <div style="margin-top:20px; flex-shrink:0;">
                    <button id="btn-save-plan" class="btn-primary" style="width:100%; justify-content:center;">
                        <i class="fa-solid fa-floppy-disk"></i> Save Changes
                    </button>
                </div>
            </div>
        `;

        modalOverlay.classList.add('active');

        // Logic for "Unlimited" checkbox
        const chkUnlimited = modalOverlay.querySelector('#edit-plan-unlimited');
        const inputOrders = modalOverlay.querySelector('#edit-plan-orders');
        chkUnlimited.addEventListener('change', (e) => {
            if (e.target.checked) {
                inputOrders.disabled = true;
                inputOrders.value = '';
            } else {
                inputOrders.disabled = false;
                inputOrders.value = plan.maxOrders || 50;
                inputOrders.focus();
            }
        });

        modalOverlay.querySelector('.plan-modal-close').addEventListener('click', () => {
            modalOverlay.classList.remove('active');
        });

        // Close on outside click
        modalOverlay.addEventListener('click', e => {
            if (e.target === modalOverlay) modalOverlay.classList.remove('active');
        });

        modalOverlay.querySelector('#btn-save-plan').addEventListener('click', () => {
            // Gather values
            plan.name = document.getElementById('edit-plan-name').value.trim();
            plan.price = parseFloat(document.getElementById('edit-plan-price').value) || 0;
            
            if (chkUnlimited.checked) {
                plan.maxOrders = null;
            } else {
                plan.maxOrders = parseInt(inputOrders.value) || 0;
            }

            // Gather features
            plan.features.forEach((f, i) => {
                f.active = document.getElementById(`edit-feat-${i}-active`).checked;
                f.text = document.getElementById(`edit-feat-${i}-text`).value.trim();
            });

            // If we toggled 'active', we should also re-render features immediately.
            savePlans(plans);
            modalOverlay.classList.remove('active');
            renderPlans();
            showToast(`${plan.name} updated successfully`, 'success');
        });
    }

    /* ══════════════════════════════════════════════
       17. INITIAL LOAD
       ══════════════════════════════════════════════ */
    updateWordCount();
    loadProfileData();

});

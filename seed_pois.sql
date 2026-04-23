-- ============================================================
-- Seed POIs: Phố ẩm thực Vĩnh Khánh, Quận 4, TP.HCM
-- Database: SQL Server
-- PoiStatus: 0=Draft | 1=Pending | 2=Approved | 3=Rejected
-- OwnerId: 00000000-0000-0000-0000-000000000002 (owner@audiotravelling.com)
-- PackageId: 1=Cơ bản(15m,P0) | 2=Nâng cao(30m,P1) | 3=Chuyên nghiệp(50m,P2)
-- ============================================================

-- ─── 1. Ốc Vĩnh Khánh ──────────────────────────────────────
INSERT INTO Pois (Id, OwnerId, Name, Lat, Lng, RadiusMeters, Priority, Status, PackageId, CreatedAt, UpdatedAt)
VALUES (
    'A1000000-0000-0000-0000-000000000001',
    '00000000-0000-0000-0000-000000000002',
    N'Ốc Vĩnh Khánh',
    10.7552, 106.7018,
    50, 2, 2, 3,
    GETUTCDATE(), GETUTCDATE()
);

INSERT INTO PoiLocalizations (Id, PoiId, Language, TextContent, AudioUrl, CreatedAt) VALUES
    ('B1000000-0000-0000-0000-000000000001', 'A1000000-0000-0000-0000-000000000001', 'vi',
     N'Quán Ốc Vĩnh Khánh là điểm đến nổi tiếng nhất trên con phố ẩm thực Vĩnh Khánh. Với hơn 20 năm kinh nghiệm, quán phục vụ các món ốc tươi sống được chế biến theo nhiều cách: hấp sả, rang muối ớt, xào bơ tỏi. Đặc biệt món ốc len xào dừa là thương hiệu riêng không thể bỏ qua.',
     NULL, GETUTCDATE()),
    ('B1000000-0000-0000-0000-000000000002', 'A1000000-0000-0000-0000-000000000001', 'en',
     N'Oc Vinh Khanh is the most famous stop on Vinh Khanh food street. With over 20 years of experience, the restaurant serves fresh snails cooked in various styles: steamed with lemongrass, stir-fried with salt and chili, or sauteed with garlic butter. The signature dish is mangrove whelk stir-fried in coconut milk.',
     NULL, GETUTCDATE());

-- ─── 2. Bạch Tuộc Nướng Muối Ớt ───────────────────────────
INSERT INTO Pois (Id, OwnerId, Name, Lat, Lng, RadiusMeters, Priority, Status, PackageId, CreatedAt, UpdatedAt)
VALUES (
    'A1000000-0000-0000-0000-000000000002',
    '00000000-0000-0000-0000-000000000002',
    N'Bạch Tuộc Nướng Muối Ớt',
    10.7558, 106.7021,
    30, 1, 2, 2,
    GETUTCDATE(), GETUTCDATE()
);

INSERT INTO PoiLocalizations (Id, PoiId, Language, TextContent, AudioUrl, CreatedAt) VALUES
    ('B2000000-0000-0000-0000-000000000001', 'A1000000-0000-0000-0000-000000000002', 'vi',
     N'Xe bạch tuộc nướng muối ớt nằm ngay góc đường, thơm lừng mỗi buổi tối. Bạch tuộc tươi được nướng trên than hoa rồi phủ lên lớp muối ớt xanh đặc trưng của miền Nam. Giòn ngoài, dai trong, chấm cùng tương cà chua pha chanh, không thể cưỡng lại.',
     NULL, GETUTCDATE()),
    ('B2000000-0000-0000-0000-000000000002', 'A1000000-0000-0000-0000-000000000002', 'en',
     N'This octopus grilling cart sits right on the corner, filling the air with irresistible aroma every evening. Fresh octopus is grilled over charcoal then coated with Southern-style green chili salt. Crispy outside, chewy inside — dipped in tomato-lime sauce, it is simply irresistible.',
     NULL, GETUTCDATE());

-- ─── 3. Cua Rang Me ────────────────────────────────────────
INSERT INTO Pois (Id, OwnerId, Name, Lat, Lng, RadiusMeters, Priority, Status, PackageId, CreatedAt, UpdatedAt)
VALUES (
    'A1000000-0000-0000-0000-000000000003',
    '00000000-0000-0000-0000-000000000002',
    N'Cua Rang Me Quận 4',
    10.7562, 106.7016,
    30, 1, 2, 2,
    GETUTCDATE(), GETUTCDATE()
);

INSERT INTO PoiLocalizations (Id, PoiId, Language, TextContent, AudioUrl, CreatedAt) VALUES
    ('B3000000-0000-0000-0000-000000000001', 'A1000000-0000-0000-0000-000000000003', 'vi',
     N'Quán chuyên về cua biển tươi sống chế biến theo kiểu Sài Gòn cổ truyền. Món cua rang me là đặc sản: vỏ cua giòn rụm, phủ sốt me chua ngọt sánh đặc, thêm hành tây và ớt xanh. Ghẹ hấp bia và tôm hùm nướng phô mai cũng là những món được thực khách yêu thích.',
     NULL, GETUTCDATE()),
    ('B3000000-0000-0000-0000-000000000002', 'A1000000-0000-0000-0000-000000000003', 'en',
     N'This restaurant specializes in fresh sea crab cooked in traditional Saigon style. The signature dish is tamarind crab: crispy shell coated in a thick sweet-and-sour tamarind sauce with onion and green chili. Steamed crab with beer and cheese-grilled lobster are also crowd favorites.',
     NULL, GETUTCDATE());

-- ─── 4. Bánh Tráng Nướng Đường Phố ────────────────────────
INSERT INTO Pois (Id, OwnerId, Name, Lat, Lng, RadiusMeters, Priority, Status, PackageId, CreatedAt, UpdatedAt)
VALUES (
    'A1000000-0000-0000-0000-000000000004',
    '00000000-0000-0000-0000-000000000002',
    N'Bánh Tráng Nướng Đường Phố',
    10.7567, 106.7019,
    15, 0, 2, 1,
    GETUTCDATE(), GETUTCDATE()
);

INSERT INTO PoiLocalizations (Id, PoiId, Language, TextContent, AudioUrl, CreatedAt) VALUES
    ('B4000000-0000-0000-0000-000000000001', 'A1000000-0000-0000-0000-000000000004', 'vi',
     N'Xe bánh tráng nướng bình dân nhưng đậm đà hương vị đường phố Sài Gòn. Bánh tráng gạo được nướng trực tiếp trên than, phết trứng cút, mỡ hành, tôm khô, pate và tương ớt. Ăn ngay khi còn nóng, giòn rụm từng miếng. Giá chỉ từ 15.000 đồng một cái.',
     NULL, GETUTCDATE()),
    ('B4000000-0000-0000-0000-000000000002', 'A1000000-0000-0000-0000-000000000004', 'en',
     N'A humble street-food cart that captures the true spirit of Saigon street food. Rice paper is grilled over charcoal then topped with quail eggs, scallion oil, dried shrimp, pate and chili sauce. Best eaten hot off the grill, crispy with every bite. Starting from just 15,000 VND each.',
     NULL, GETUTCDATE());

-- ─── 5. Chè Thái Vĩnh Khánh ────────────────────────────────
INSERT INTO Pois (Id, OwnerId, Name, Lat, Lng, RadiusMeters, Priority, Status, PackageId, CreatedAt, UpdatedAt)
VALUES (
    'A1000000-0000-0000-0000-000000000005',
    '00000000-0000-0000-0000-000000000002',
    N'Chè Thái Vĩnh Khánh',
    10.7573, 106.7013,
    15, 0, 2, 1,
    GETUTCDATE(), GETUTCDATE()
);

INSERT INTO PoiLocalizations (Id, PoiId, Language, TextContent, AudioUrl, CreatedAt) VALUES
    ('B5000000-0000-0000-0000-000000000001', 'A1000000-0000-0000-0000-000000000005', 'vi',
     N'Quán chè cuối phố là điểm dừng chân lý tưởng sau khi thưởng thức các món mặn. Chè Thái ngọt thanh với thạch đủ màu, trân châu, thạch rau câu, lớp nước cốt dừa béo ngậy phủ trên cùng. Ngoài ra còn có chè khúc bạch, chè bưởi, và sinh tố trái cây nhiệt đới tươi mát.',
     NULL, GETUTCDATE()),
    ('B5000000-0000-0000-0000-000000000002', 'A1000000-0000-0000-0000-000000000005', 'en',
     N'The dessert shop at the end of the street is the perfect stop after all the savory dishes. Thai-style sweet soup features colorful jellies, tapioca pearls, agar cubes and a generous drizzle of creamy coconut milk on top. Also available: khuc bach pudding, pomelo dessert soup, and fresh tropical fruit smoothies.',
     NULL, GETUTCDATE());

-- ─── Kiểm tra ──────────────────────────────────────────────
SELECT Id, Name, Lat, Lng, Status FROM Pois WHERE Id LIKE 'A100%';

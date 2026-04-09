-- ================== ROLES ==================
INSERT INTO Roles (RoleName)
VALUES 
('Admin'),
('Customer'),
('StoreOwner');

-- ================== PACKAGES ==================
INSERT INTO Packages (PackageName)
VALUES 
(N'Free'),
(N'Premium'),
(N'VIP');

-- ================== BANK ACCOUNTS ==================
INSERT INTO BankAccounts (BankAccountID, BankBalance, BankName)
VALUES 
('BA001', 1000000, 'Vietcombank'),
('BA002', 2000000, 'ACB'),
('BA003', 1500000, 'Techcombank');

-- ================== USERS ==================
INSERT INTO Users 
(Email, Password, PhoneNumber, FirstName, LastName, PackageID, BankAccountID, SubscriptionDate, UserStatus, RoleID, CreatedDate, UpdatedDate)
VALUES
('admin@gmail.com', '123456', '0900000001', N'Admin', N'User', 1, 'BA001', GETDATE(), 1, 1, GETDATE(), GETDATE()),
('user1@gmail.com', '123456', '0900000002', N'Nguyen', N'An', 2, 'BA002', GETDATE(), 1, 2, GETDATE(), GETDATE()),
('owner1@gmail.com', '123456', '0900000003', N'Tran', N'Binh', 3, 'BA003', GETDATE(), 1, 3, GETDATE(), GETDATE());

-- ================== STORES ==================
INSERT INTO Stores 
(StoreName, StoreLatitude, StoreLongitude, StoreRadius, StoreSummary, StoreStatus, OwnerID)
VALUES
(N'Coffee Đà Lạt', 11.9404, 108.4583, 5.0, N'Quán cafe đẹp', 1, 3),
(N'Bún bò Huế', 16.4637, 107.5909, 3.0, N'Đặc sản Huế', 1, 3);

-- ================== STORE ITEMS ==================
INSERT INTO StoreItems 
(StoreItemName, StoreItemStatus, StoreItemPrice, StoreItemImageLink, StoreItemDiscount, StoreID)
VALUES
(N'Cà phê sữa', 1, 30000, 'img1.jpg', 0, 1),
(N'Cà phê đen', 1, 25000, 'img2.jpg', 0, 1),
(N'Bún bò đặc biệt', 1, 50000, 'img3.jpg', 10, 2);

-- ================== CART ITEMS ==================
INSERT INTO CartItems (UserID, StoreItemID, Quantity)
VALUES
(2, 1, 2),
(2, 3, 1);

-- ================== GALLERY ==================
INSERT INTO Gallery (GalleryLink, StoreID)
VALUES
('store1_img1.jpg', 1),
('store1_img2.jpg', 1),
('store2_img1.jpg', 2);

-- ================== ORDERS ==================
INSERT INTO Orders 
(OrderDate, OrderPaymentMethod, OrderRadius, OrderStatus, OrderDescription, OrderCustomerID)
VALUES
(GETDATE(), 'CASH', 5.0, N'Completed', N'Order 1', 2),
(GETDATE(), 'BANK', 3.0, N'Pending', N'Order 2', 2);

-- ================== ORDER DETAILS ==================
INSERT INTO OrderDetails 
(OrderDetailStoreItemID, OrderDetailQuantity, OrderID)
VALUES
(1, 2, 1),
(3, 1, 2);

IF NOT EXISTS(
	SELECT name
	FROM sys.databases
	WHERE name = 'DbAppDuLich'
)
BEGIN
	CREATE DATABASE [DbAppDuLich]
END
GO

USE [DbAppDuLich]
GO

-- USERS
CREATE TABLE [Users] (
  [UserID] int IDENTITY(1,1) PRIMARY KEY,
  [Email] varchar(50) NOT NULL,
  [Password] varchar(255) NOT NULL,
  [PhoneNumber] varchar(20) NOT NULL,
  [FirstName] nvarchar(100) NOT NULL,
  [LastName] nvarchar(100) NOT NULL,
  [PackageID] int,
  [BankAccountID] varchar(50),
  [SubscriptionDate] datetime,
  [UserStatus] int NOT NULL,
  [RoleID] int NOT NULL,
  [CreatedDate] datetime NOT NULL,
  [UpdatedDate] datetime NOT NULL
)
GO

-- CART ITEMS
CREATE TABLE [CartItems] (
  [CartItemID] int IDENTITY(1,1) PRIMARY KEY,
  [UserID] int NOT NULL,
  [StoreItemID] int NOT NULL,
  [Quantity] int NOT NULL DEFAULT (1)
)
GO

-- BANK ACCOUNTS
CREATE TABLE [BankAccounts] (
  [BankAccountID] varchar(50) PRIMARY KEY,
  [BankBalance] DECIMAL(18,2) NOT NULL DEFAULT 0,
  [BankName] varchar(50) NOT NULL
)
GO

-- ROLES
CREATE TABLE [Roles] (
  [RoleID] int IDENTITY(1,1) PRIMARY KEY,
  [RoleName] varchar(50) NOT NULL
)
GO

-- PACKAGES
CREATE TABLE [Packages] (
  [PackageID] int IDENTITY(1,1) PRIMARY KEY,
  [PackageName] nvarchar(50) NOT NULL
)
GO

-- STORES
CREATE TABLE [Stores] (
  [StoreID] int IDENTITY(1,1) PRIMARY KEY,
  [StoreName] nvarchar(100) NOT NULL,
  [StoreLatitude] DECIMAL(9,6) NOT NULL,
  [StoreLongitude] DECIMAL(9,6) NOT NULL,
  [StoreRadius] DECIMAL(10,2) NOT NULL,
  [StoreSummary] nvarchar(500),
  [StoreStatus] int NOT NULL,
  [OwnerID] int NOT NULL
)
GO

-- GALLERY
CREATE TABLE [Gallery] (
  [GalleryID] int IDENTITY(1,1) PRIMARY KEY,
  [GalleryLink] varchar(200) NOT NULL,
  [StoreID] int NOT NULL
)
GO

-- STORE ITEMS
CREATE TABLE [StoreItems] (
  [StoreItemID] int IDENTITY(1,1) PRIMARY KEY,
  [StoreItemName] nvarchar(100) NOT NULL,
  [StoreItemStatus] int NOT NULL,
  [StoreItemPrice] DECIMAL(18,2) NOT NULL,
  [StoreItemImageLink] varchar(200) NOT NULL,
  [StoreItemDiscount] int,
  [StoreID] int NOT NULL
)
GO

-- ORDERS
CREATE TABLE [Orders] (
  [OrderID] int IDENTITY(1,1) PRIMARY KEY,
  [OrderDate] datetime NOT NULL,
  [OrderPaymentMethod] varchar(20) NOT NULL,
  [OrderRadius] DECIMAL(10,2) NOT NULL,
  [OrderStatus] nvarchar(50) NOT NULL,
  [OrderDescription] nvarchar(500),
  [OrderCustomerID] int NOT NULL
)
GO

-- ORDER DETAILS
CREATE TABLE [OrderDetails] (
  [OrderDetailID] int IDENTITY(1,1) PRIMARY KEY,
  [OrderDetailStoreItemID] int NOT NULL,
  [OrderDetailQuantity] int NOT NULL,
  [OrderID] int NOT NULL
)
GO

-- ================== FOREIGN KEYS ==================

ALTER TABLE [Users] 
ADD CONSTRAINT FK_Users_BankAccounts
FOREIGN KEY ([BankAccountID]) REFERENCES [BankAccounts] ([BankAccountID])
GO

ALTER TABLE [Users] 
ADD CONSTRAINT FK_Users_Roles
FOREIGN KEY ([RoleID]) REFERENCES [Roles] ([RoleID])
GO

ALTER TABLE [Users] 
ADD CONSTRAINT FK_Users_Packages
FOREIGN KEY ([PackageID]) REFERENCES [Packages] ([PackageID])
GO

ALTER TABLE [CartItems] 
ADD CONSTRAINT FK_CartItems_Users
FOREIGN KEY ([UserID]) REFERENCES [Users] ([UserID])
GO

ALTER TABLE [CartItems] 
ADD CONSTRAINT FK_CartItems_StoreItems
FOREIGN KEY ([StoreItemID]) REFERENCES [StoreItems] ([StoreItemID])
GO

ALTER TABLE [Stores] 
ADD CONSTRAINT FK_Stores_Users
FOREIGN KEY ([OwnerID]) REFERENCES [Users] ([UserID])
GO

ALTER TABLE [Gallery] 
ADD CONSTRAINT FK_Gallery_Stores
FOREIGN KEY ([StoreID]) REFERENCES [Stores] ([StoreID])
GO

ALTER TABLE [StoreItems] 
ADD CONSTRAINT FK_StoreItems_Stores
FOREIGN KEY ([StoreID]) REFERENCES [Stores] ([StoreID])
GO

ALTER TABLE [Orders] 
ADD CONSTRAINT FK_Orders_Users
FOREIGN KEY ([OrderCustomerID]) REFERENCES [Users] ([UserID])
GO

ALTER TABLE [OrderDetails] 
ADD CONSTRAINT FK_OrderDetails_Orders
FOREIGN KEY ([OrderID]) REFERENCES [Orders] ([OrderID])
GO

ALTER TABLE [OrderDetails] 
ADD CONSTRAINT FK_OrderDetails_StoreItems
FOREIGN KEY ([OrderDetailStoreItemID]) REFERENCES [StoreItems] ([StoreItemID])
GO
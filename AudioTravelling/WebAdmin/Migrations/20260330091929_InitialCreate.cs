using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAdmin.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BankAccounts",
                columns: table => new
                {
                    BankAccountID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    BankBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BankName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankAccounts", x => x.BankAccountID);
                });

            migrationBuilder.CreateTable(
                name: "Packages",
                columns: table => new
                {
                    PackageID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PackageName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Packages", x => x.PackageID);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    RoleID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.RoleID);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PackageID = table.Column<int>(type: "int", nullable: true),
                    BankAccountID = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    SubscriptionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserStatus = table.Column<int>(type: "int", nullable: false),
                    RoleID = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserID);
                    table.ForeignKey(
                        name: "FK_Users_BankAccounts_BankAccountID",
                        column: x => x.BankAccountID,
                        principalTable: "BankAccounts",
                        principalColumn: "BankAccountID");
                    table.ForeignKey(
                        name: "FK_Users_Packages_PackageID",
                        column: x => x.PackageID,
                        principalTable: "Packages",
                        principalColumn: "PackageID");
                    table.ForeignKey(
                        name: "FK_Users_Roles_RoleID",
                        column: x => x.RoleID,
                        principalTable: "Roles",
                        principalColumn: "RoleID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    OrderID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OrderPaymentMethod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrderRadius = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OrderStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrderDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrderCustomerID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.OrderID);
                    table.ForeignKey(
                        name: "FK_Orders_Users_OrderCustomerID",
                        column: x => x.OrderCustomerID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Stores",
                columns: table => new
                {
                    StoreID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StoreName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StoreLatitude = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StoreLongitude = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StoreRadius = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StoreSummary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StoreStatus = table.Column<int>(type: "int", nullable: false),
                    OwnerID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stores", x => x.StoreID);
                    table.ForeignKey(
                        name: "FK_Stores_Users_OwnerID",
                        column: x => x.OwnerID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Galleries",
                columns: table => new
                {
                    GalleryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GalleryLink = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StoreID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Galleries", x => x.GalleryID);
                    table.ForeignKey(
                        name: "FK_Galleries_Stores_StoreID",
                        column: x => x.StoreID,
                        principalTable: "Stores",
                        principalColumn: "StoreID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StoreItems",
                columns: table => new
                {
                    StoreItemID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StoreItemName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StoreItemStatus = table.Column<int>(type: "int", nullable: false),
                    StoreItemPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StoreItemImageLink = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StoreItemDiscount = table.Column<int>(type: "int", nullable: true),
                    StoreID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoreItems", x => x.StoreItemID);
                    table.ForeignKey(
                        name: "FK_StoreItems_Stores_StoreID",
                        column: x => x.StoreID,
                        principalTable: "Stores",
                        principalColumn: "StoreID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CartItems",
                columns: table => new
                {
                    CartItemID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    StoreItemID = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartItems", x => x.CartItemID);
                    table.ForeignKey(
                        name: "FK_CartItems_StoreItems_StoreItemID",
                        column: x => x.StoreItemID,
                        principalTable: "StoreItems",
                        principalColumn: "StoreItemID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CartItems_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderDetails",
                columns: table => new
                {
                    OrderDetailID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderDetailStoreItemID = table.Column<int>(type: "int", nullable: false),
                    OrderDetailQuantity = table.Column<int>(type: "int", nullable: false),
                    OrderID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderDetails", x => x.OrderDetailID);
                    table.ForeignKey(
                        name: "FK_OrderDetails_Orders_OrderID",
                        column: x => x.OrderID,
                        principalTable: "Orders",
                        principalColumn: "OrderID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderDetails_StoreItems_OrderDetailStoreItemID",
                        column: x => x.OrderDetailStoreItemID,
                        principalTable: "StoreItems",
                        principalColumn: "StoreItemID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_StoreItemID",
                table: "CartItems",
                column: "StoreItemID");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_UserID",
                table: "CartItems",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Galleries_StoreID",
                table: "Galleries",
                column: "StoreID");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_OrderDetailStoreItemID",
                table: "OrderDetails",
                column: "OrderDetailStoreItemID");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_OrderID",
                table: "OrderDetails",
                column: "OrderID");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderCustomerID",
                table: "Orders",
                column: "OrderCustomerID");

            migrationBuilder.CreateIndex(
                name: "IX_StoreItems_StoreID",
                table: "StoreItems",
                column: "StoreID");

            migrationBuilder.CreateIndex(
                name: "IX_Stores_OwnerID",
                table: "Stores",
                column: "OwnerID");

            migrationBuilder.CreateIndex(
                name: "IX_Users_BankAccountID",
                table: "Users",
                column: "BankAccountID");

            migrationBuilder.CreateIndex(
                name: "IX_Users_PackageID",
                table: "Users",
                column: "PackageID");

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleID",
                table: "Users",
                column: "RoleID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CartItems");

            migrationBuilder.DropTable(
                name: "Galleries");

            migrationBuilder.DropTable(
                name: "OrderDetails");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "StoreItems");

            migrationBuilder.DropTable(
                name: "Stores");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "BankAccounts");

            migrationBuilder.DropTable(
                name: "Packages");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AudioTravelling.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccessCodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    QrImageUrl = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessCodes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Packages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    RadiusMeters = table.Column<int>(type: "integer", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Packages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AccessSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AccessCodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionToken = table.Column<string>(type: "text", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeviceInfo = table.Column<string>(type: "text", nullable: true),
                    Lat = table.Column<double>(type: "double precision", nullable: true),
                    Lng = table.Column<double>(type: "double precision", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccessSessions_AccessCodes_AccessCodeId",
                        column: x => x.AccessCodeId,
                        principalTable: "AccessCodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Pois",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Lat = table.Column<double>(type: "double precision", nullable: false),
                    Lng = table.Column<double>(type: "double precision", nullable: false),
                    RadiusMeters = table.Column<int>(type: "integer", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    PackageId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pois", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pois_Packages_PackageId",
                        column: x => x.PackageId,
                        principalTable: "Packages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Pois_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PoiApprovalLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PoiId = table.Column<Guid>(type: "uuid", nullable: false),
                    AdminId = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "text", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PoiApprovalLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PoiApprovalLogs_Pois_PoiId",
                        column: x => x.PoiId,
                        principalTable: "Pois",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PoiApprovalLogs_Users_AdminId",
                        column: x => x.AdminId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PoiImages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PoiId = table.Column<Guid>(type: "uuid", nullable: false),
                    ImageUrl = table.Column<string>(type: "text", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PoiImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PoiImages_Pois_PoiId",
                        column: x => x.PoiId,
                        principalTable: "Pois",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PoiLocalizations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PoiId = table.Column<Guid>(type: "uuid", nullable: false),
                    Language = table.Column<string>(type: "text", nullable: false),
                    TextContent = table.Column<string>(type: "text", nullable: false),
                    AudioUrl = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PoiLocalizations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PoiLocalizations_Pois_PoiId",
                        column: x => x.PoiId,
                        principalTable: "Pois",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Packages",
                columns: new[] { "Id", "Description", "Name", "Price", "Priority", "RadiusMeters" },
                values: new object[,]
                {
                    { 1, "Bán kính 15m, ưu tiên thấp nhất", "Cơ bản", 0m, 0, 15 },
                    { 2, "Bán kính 30m, ưu tiên trung bình", "Nâng cao", 200000m, 1, 30 },
                    { 3, "Bán kính 50m, ưu tiên cao nhất", "Chuyên nghiệp", 500000m, 2, 50 }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Admin" },
                    { 2, "Owner" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccessSessions_AccessCodeId",
                table: "AccessSessions",
                column: "AccessCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_AccessSessions_ExpiresAt",
                table: "AccessSessions",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_AccessSessions_SessionToken",
                table: "AccessSessions",
                column: "SessionToken",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PoiApprovalLogs_AdminId",
                table: "PoiApprovalLogs",
                column: "AdminId");

            migrationBuilder.CreateIndex(
                name: "IX_PoiApprovalLogs_PoiId",
                table: "PoiApprovalLogs",
                column: "PoiId");

            migrationBuilder.CreateIndex(
                name: "IX_PoiImages_PoiId",
                table: "PoiImages",
                column: "PoiId");

            migrationBuilder.CreateIndex(
                name: "IX_PoiLocalizations_PoiId",
                table: "PoiLocalizations",
                column: "PoiId");

            migrationBuilder.CreateIndex(
                name: "IX_Pois_OwnerId",
                table: "Pois",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Pois_PackageId",
                table: "Pois",
                column: "PackageId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleId",
                table: "Users",
                column: "RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccessSessions");

            migrationBuilder.DropTable(
                name: "PoiApprovalLogs");

            migrationBuilder.DropTable(
                name: "PoiImages");

            migrationBuilder.DropTable(
                name: "PoiLocalizations");

            migrationBuilder.DropTable(
                name: "AccessCodes");

            migrationBuilder.DropTable(
                name: "Pois");

            migrationBuilder.DropTable(
                name: "Packages");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}

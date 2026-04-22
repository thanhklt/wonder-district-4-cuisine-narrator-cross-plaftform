using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AudioTravelling.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedDefaultUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "PasswordHash", "RoleId" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000001"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "admin@audiotravelling.com", "$2a$11$WhkWccdt9qW.O1NdwVb76uhsDhhgR0WKytBTU24Ilnk/T.cGjWEKS", 1 },
                    { new Guid("00000000-0000-0000-0000-000000000002"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "owner@audiotravelling.com", "$2a$11$vnHVv7WjT0NxVrxuX6T4ReyB/V9TCUCKGNSp7uBuCn/U5HaJAbQha", 2 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000002"));
        }
    }
}

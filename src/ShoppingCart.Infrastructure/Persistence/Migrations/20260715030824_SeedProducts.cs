using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ShoppingCart.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedProducts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Category", "Code", "Name", "Price", "Stock" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "Technology", "PRD-001", "Mechanical Keyboard", 80m, 15 },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "Technology", "PRD-002", "Wireless Mouse", 35m, 25 },
                    { new Guid("33333333-3333-3333-3333-333333333333"), "Monitors", "PRD-003", "Monitor 24 Inches", 180m, 8 },
                    { new Guid("44444444-4444-4444-4444-444444444444"), "Accessories", "PRD-004", "USB Webcam", 55m, 12 },
                    { new Guid("55555555-5555-5555-5555-555555555555"), "Computers", "PRD-005", "Laptop", 850m, 5 },
                    { new Guid("66666666-6666-6666-6666-666666666666"), "Audio", "PRD-006", "Gaming Headset", 95m, 0 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"));
        }
    }
}

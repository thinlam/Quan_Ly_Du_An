using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class AddThanhLyHopDongTrangThaiPheDuyet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "DmTrangThaiPheDuyet",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "IsDeleted", "Loai", "Ma", "MoTa", "Stt", "Ten", "UpdatedAt", "UpdatedBy", "Used" },
                values: new object[,]
                {
                    { 67, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "ThanhLyHopDong", "DT", null, 1, "Dự thảo", null, "", true },
                    { 68, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "ThanhLyHopDong", "ĐTr", null, 2, "Đã trình", null, "", true },
                    { 69, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "ThanhLyHopDong", "ĐD", null, 3, "Đã duyệt", null, "", true },
                    { 70, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "ThanhLyHopDong", "TL", null, 4, "Trả lại", null, "", true }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "DmTrangThaiPheDuyet",
                keyColumn: "Id",
                keyValue: 67);

            migrationBuilder.DeleteData(
                table: "DmTrangThaiPheDuyet",
                keyColumn: "Id",
                keyValue: 68);

            migrationBuilder.DeleteData(
                table: "DmTrangThaiPheDuyet",
                keyColumn: "Id",
                keyValue: 69);

            migrationBuilder.DeleteData(
                table: "DmTrangThaiPheDuyet",
                keyColumn: "Id",
                keyValue: 70);
        }
    }
}

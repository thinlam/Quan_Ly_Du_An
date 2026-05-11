using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class AddPheDuyetTuChoiAndHoSoSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "DmTrangThaiPheDuyet",
                keyColumn: "Id",
                keyValue: 5,
                column: "Loai",
                value: "Default");

            migrationBuilder.InsertData(
                table: "DmTrangThaiPheDuyet",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "IsDeleted", "Loai", "Ma", "MoTa", "Stt", "Ten", "UpdatedAt", "UpdatedBy", "Used" },
                values: new object[,]
                {
                    { 6, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "PheDuyetDuToan", "TC", null, 5, "Từ chối", null, "", true },
                    { 7, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "HoSoDeXuatCapDoCntt", "DT", null, 1, "Dự thảo", null, "", true },
                    { 8, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "HoSoDeXuatCapDoCntt", "ĐTr", null, 2, "Đã trình", null, "", true },
                    { 9, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "HoSoDeXuatCapDoCntt", "ĐD", null, 3, "Đã duyệt", null, "", true },
                    { 10, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "HoSoDeXuatCapDoCntt", "TL", null, 4, "Trả lại", null, "", true },
                    { 11, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "HoSoDeXuatCapDoCntt", "TC", null, 5, "Từ chối", null, "", true },
                    { 12, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "HoSoMoiThauDienTu", "DT", null, 1, "Dự thảo", null, "", true },
                    { 13, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "HoSoMoiThauDienTu", "ĐTr", null, 2, "Đã trình", null, "", true },
                    { 14, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "HoSoMoiThauDienTu", "ĐD", null, 3, "Đã duyệt", null, "", true },
                    { 15, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "HoSoMoiThauDienTu", "TL", null, 4, "Trả lại", null, "", true },
                    { 16, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "HoSoMoiThauDienTu", "TC", null, 5, "Từ chối", null, "", true }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "DmTrangThaiPheDuyet",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "DmTrangThaiPheDuyet",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "DmTrangThaiPheDuyet",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "DmTrangThaiPheDuyet",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "DmTrangThaiPheDuyet",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "DmTrangThaiPheDuyet",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "DmTrangThaiPheDuyet",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "DmTrangThaiPheDuyet",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "DmTrangThaiPheDuyet",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "DmTrangThaiPheDuyet",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "DmTrangThaiPheDuyet",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.UpdateData(
                table: "DmTrangThaiPheDuyet",
                keyColumn: "Id",
                keyValue: 5,
                column: "Loai",
                value: "PheDuyetDuToan");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class addColNgayTrinhToDeXuatMoiAndChuyenThiep : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ThanhToan_NghiemThuId",
                table: "ThanhToan");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "NgayTrinh",
                table: "DeXuatChuyenTiep",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "NgayTrinh",
                table: "DeXuatChuTruongMoi",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ThanhToan_NghiemThuId",
                table: "ThanhToan",
                column: "NghiemThuId",
                unique: true,
                filter: "[IsDeleted] = 0 AND [NghiemThuId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ThanhToan_NghiemThuId",
                table: "ThanhToan");

            migrationBuilder.DropColumn(
                name: "NgayTrinh",
                table: "DeXuatChuyenTiep");

            migrationBuilder.DropColumn(
                name: "NgayTrinh",
                table: "DeXuatChuTruongMoi");

            migrationBuilder.CreateIndex(
                name: "IX_ThanhToan_NghiemThuId",
                table: "ThanhToan",
                column: "NghiemThuId",
                unique: true,
                filter: "[IsDeleted] = 0");
        }
    }
}

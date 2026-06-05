using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class addColNamDxToDeXuatMoiAndChuyenThiep : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NgayTrinh",
                table: "DeXuatChuyenTiep");

            migrationBuilder.DropColumn(
                name: "NgayTrinh",
                table: "DeXuatChuTruongMoi");

            migrationBuilder.AddColumn<int>(
                name: "NamDeXuat",
                table: "DeXuatChuyenTiep",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NamDeXuat",
                table: "DeXuatChuTruongMoi",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NamDeXuat",
                table: "DeXuatChuyenTiep");

            migrationBuilder.DropColumn(
                name: "NamDeXuat",
                table: "DeXuatChuTruongMoi");

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
        }
    }
}

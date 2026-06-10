using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class alterThoaThuanGiaoViec : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Ngay",
                table: "KeHoachLuaChonNhaThauRutGon");

            migrationBuilder.DropColumn(
                name: "So",
                table: "KeHoachLuaChonNhaThauRutGon");

            migrationBuilder.DropColumn(
                name: "Ten",
                table: "KeHoachLuaChonNhaThauRutGon");

            migrationBuilder.DropColumn(
                name: "TrichYeu",
                table: "KeHoachLuaChonNhaThauRutGon");

            migrationBuilder.AddColumn<string>(
                name: "NoiDung",
                table: "ThoaThuanGiaoViec",
                type: "nvarchar(4000)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NoiDung",
                table: "ThoaThuanGiaoViec");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "Ngay",
                table: "KeHoachLuaChonNhaThauRutGon",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "So",
                table: "KeHoachLuaChonNhaThauRutGon",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Ten",
                table: "KeHoachLuaChonNhaThauRutGon",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TrichYeu",
                table: "KeHoachLuaChonNhaThauRutGon",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}

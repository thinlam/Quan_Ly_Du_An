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
                table: "ThoaThuanGiaoViec");

            migrationBuilder.DropColumn(
                name: "So",
                table: "ThoaThuanGiaoViec");

            migrationBuilder.DropColumn(
                name: "Ten",
                table: "ThoaThuanGiaoViec");

            migrationBuilder.DropColumn(
                name: "TrichYeu",
                table: "ThoaThuanGiaoViec");

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
                table: "ThoaThuanGiaoViec",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "So",
                table: "ThoaThuanGiaoViec",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Ten",
                table: "ThoaThuanGiaoViec",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TrichYeu",
                table: "ThoaThuanGiaoViec",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}

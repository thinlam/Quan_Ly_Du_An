using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class UpdateHopDongDateFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "NgayDuKienKetThuc",
                table: "HopDong",
                newName: "NgayDuKienKetThucHopDong");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "NgayDuKienKetThucGoiThau",
                table: "HopDong",
                type: "datetimeoffset",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NgayDuKienKetThucGoiThau",
                table: "HopDong");

            migrationBuilder.RenameColumn(
                name: "NgayDuKienKetThucHopDong",
                table: "HopDong",
                newName: "NgayDuKienKetThuc");
        }
    }
}

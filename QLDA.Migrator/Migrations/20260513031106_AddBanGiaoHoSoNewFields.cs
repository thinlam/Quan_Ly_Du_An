using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class AddBanGiaoHoSoNewFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BuocId",
                table: "BanGiaoHoSo",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DuAnId",
                table: "BanGiaoHoSo",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GhiChu",
                table: "BanGiaoHoSo",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BanGiaoHoSo_BuocId",
                table: "BanGiaoHoSo",
                column: "BuocId");

            migrationBuilder.CreateIndex(
                name: "IX_BanGiaoHoSo_DuAnId",
                table: "BanGiaoHoSo",
                column: "DuAnId");

            migrationBuilder.AddForeignKey(
                name: "FK_BanGiaoHoSo_DmBuoc_BuocId",
                table: "BanGiaoHoSo",
                column: "BuocId",
                principalTable: "DmBuoc",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_BanGiaoHoSo_DuAn_DuAnId",
                table: "BanGiaoHoSo",
                column: "DuAnId",
                principalTable: "DuAn",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BanGiaoHoSo_DmBuoc_BuocId",
                table: "BanGiaoHoSo");

            migrationBuilder.DropForeignKey(
                name: "FK_BanGiaoHoSo_DuAn_DuAnId",
                table: "BanGiaoHoSo");

            migrationBuilder.DropIndex(
                name: "IX_BanGiaoHoSo_BuocId",
                table: "BanGiaoHoSo");

            migrationBuilder.DropIndex(
                name: "IX_BanGiaoHoSo_DuAnId",
                table: "BanGiaoHoSo");

            migrationBuilder.DropColumn(
                name: "BuocId",
                table: "BanGiaoHoSo");

            migrationBuilder.DropColumn(
                name: "DuAnId",
                table: "BanGiaoHoSo");

            migrationBuilder.DropColumn(
                name: "GhiChu",
                table: "BanGiaoHoSo");
        }
    }
}

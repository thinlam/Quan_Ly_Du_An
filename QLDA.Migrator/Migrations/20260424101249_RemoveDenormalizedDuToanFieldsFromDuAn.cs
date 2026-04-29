using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDenormalizedDuToanFieldsFromDuAn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DuAn_DuToan_DuToanHienTaiId",
                table: "DuAn");

            migrationBuilder.DropIndex(
                name: "IX_DuAn_DuToanHienTaiId",
                table: "DuAn");

            migrationBuilder.DropColumn(
                name: "DuToanHienTaiId",
                table: "DuAn");

            migrationBuilder.DropColumn(
                name: "NamDuToan",
                table: "DuAn");

            migrationBuilder.DropColumn(
                name: "NgayKyDuToan",
                table: "DuAn");

            migrationBuilder.DropColumn(
                name: "NgayQuyetDinhDuToan",
                table: "DuAn");

            migrationBuilder.DropColumn(
                name: "SoDuToan",
                table: "DuAn");

            migrationBuilder.DropColumn(
                name: "SoDuToanCuoiCung",
                table: "DuAn");

            migrationBuilder.DropColumn(
                name: "SoQuyetDinhDuToan",
                table: "DuAn");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DuToanHienTaiId",
                table: "DuAn",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NamDuToan",
                table: "DuAn",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "NgayKyDuToan",
                table: "DuAn",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "NgayQuyetDinhDuToan",
                table: "DuAn",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "SoDuToan",
                table: "DuAn",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "SoDuToanCuoiCung",
                table: "DuAn",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SoQuyetDinhDuToan",
                table: "DuAn",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DuAn_DuToanHienTaiId",
                table: "DuAn",
                column: "DuToanHienTaiId",
                unique: true,
                filter: "[DuToanHienTaiId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_DuAn_DuToan_DuToanHienTaiId",
                table: "DuAn",
                column: "DuToanHienTaiId",
                principalTable: "DuToan",
                principalColumn: "Id");
        }
    }
}

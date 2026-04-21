using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSoNgayThucHienHopDongColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DuAn_DuToan_DuToanBanDauId1",
                table: "DuAn");

            migrationBuilder.DropIndex(
                name: "IX_DuAn_DuToanBanDauId1",
                table: "DuAn");

            migrationBuilder.DropColumn(
                name: "DuToanBanDauId1",
                table: "DuAn");

            migrationBuilder.AddColumn<long>(
                name: "SoNgayThucHienHopDong",
                table: "KetQuaTrungThau",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DuAnId1",
                table: "DuToan",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "DuToanBanDauId",
                table: "DuAn",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "KhaiToanKinhPhi",
                table: "DuAn",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "SoDuToanBanDau",
                table: "DuAn",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SoTienDuToanBanDau",
                table: "DuAn",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Stt",
                table: "DmBuocManHinh",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_DuToan_DuAnId1",
                table: "DuToan",
                column: "DuAnId1",
                unique: true,
                filter: "[DuAnId1] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_DuToan_DuAn_DuAnId1",
                table: "DuToan",
                column: "DuAnId1",
                principalTable: "DuAn",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DuToan_DuAn_DuAnId1",
                table: "DuToan");

            migrationBuilder.DropIndex(
                name: "IX_DuToan_DuAnId1",
                table: "DuToan");

            migrationBuilder.DropColumn(
                name: "SoNgayThucHienHopDong",
                table: "KetQuaTrungThau");

            migrationBuilder.DropColumn(
                name: "DuAnId1",
                table: "DuToan");

            migrationBuilder.DropColumn(
                name: "KhaiToanKinhPhi",
                table: "DuAn");

            migrationBuilder.DropColumn(
                name: "SoDuToanBanDau",
                table: "DuAn");

            migrationBuilder.DropColumn(
                name: "SoTienDuToanBanDau",
                table: "DuAn");

            migrationBuilder.DropColumn(
                name: "Stt",
                table: "DmBuocManHinh");

            migrationBuilder.AlterColumn<Guid>(
                name: "DuToanBanDauId",
                table: "DuAn",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DuToanBanDauId1",
                table: "DuAn",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DuAn_DuToanBanDauId1",
                table: "DuAn",
                column: "DuToanBanDauId1");

            migrationBuilder.AddForeignKey(
                name: "FK_DuAn_DuToan_DuToanBanDauId1",
                table: "DuAn",
                column: "DuToanBanDauId1",
                principalTable: "DuToan",
                principalColumn: "Id");
        }
    }
}

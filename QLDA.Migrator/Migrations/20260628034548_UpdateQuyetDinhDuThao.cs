using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class UpdateQuyetDinhDuThao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SoDuThao",
                table: "QuyetDinhLapBanQLDA",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TrangThaiId",
                table: "QuyetDinhLapBanQLDA",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TrichYeuDuThao",
                table: "QuyetDinhLapBanQLDA",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Ten",
                table: "DanhMucLoaiCongViec",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MoTa",
                table: "DanhMucLoaiCongViec",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Ma",
                table: "DanhMucLoaiCongViec",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "Index",
                table: "DanhMucLoaiCongViec",
                type: "bigint",
                nullable: false,
                defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())",
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "DanhMucLoaiCongViec",
                type: "datetimeoffset",
                nullable: false,
                defaultValueSql: "SYSDATETIMEOFFSET()",
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.CreateIndex(
                name: "IX_QuyetDinhLapBanQLDA_TrangThaiId",
                table: "QuyetDinhLapBanQLDA",
                column: "TrangThaiId");

            migrationBuilder.CreateIndex(
                name: "IX_DanhMucLoaiCongViec_Index",
                table: "DanhMucLoaiCongViec",
                column: "Index")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_DanhMucLoaiCongViec_Ma",
                table: "DanhMucLoaiCongViec",
                column: "Ma",
                unique: true,
                filter: "[Ma] IS NOT NULL AND [Ma] <> ''");

            migrationBuilder.AddForeignKey(
                name: "FK_QuyetDinhLapBanQLDA_DmTrangThaiPheDuyet_TrangThaiId",
                table: "QuyetDinhLapBanQLDA",
                column: "TrangThaiId",
                principalTable: "DmTrangThaiPheDuyet",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuyetDinhLapBanQLDA_DmTrangThaiPheDuyet_TrangThaiId",
                table: "QuyetDinhLapBanQLDA");

            migrationBuilder.DropIndex(
                name: "IX_QuyetDinhLapBanQLDA_TrangThaiId",
                table: "QuyetDinhLapBanQLDA");

            migrationBuilder.DropIndex(
                name: "IX_DanhMucLoaiCongViec_Index",
                table: "DanhMucLoaiCongViec");

            migrationBuilder.DropIndex(
                name: "IX_DanhMucLoaiCongViec_Ma",
                table: "DanhMucLoaiCongViec");

            migrationBuilder.DropColumn(
                name: "SoDuThao",
                table: "QuyetDinhLapBanQLDA");

            migrationBuilder.DropColumn(
                name: "TrangThaiId",
                table: "QuyetDinhLapBanQLDA");

            migrationBuilder.DropColumn(
                name: "TrichYeuDuThao",
                table: "QuyetDinhLapBanQLDA");

            migrationBuilder.AlterColumn<string>(
                name: "Ten",
                table: "DanhMucLoaiCongViec",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MoTa",
                table: "DanhMucLoaiCongViec",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(400)",
                oldMaxLength: 400,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Ma",
                table: "DanhMucLoaiCongViec",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "Index",
                table: "DanhMucLoaiCongViec",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldDefaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "DanhMucLoaiCongViec",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldDefaultValueSql: "SYSDATETIMEOFFSET()");
        }
    }
}

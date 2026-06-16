using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class TableQuyetDinhDuyetDuToanNguonVon_ChiPhi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_QuyetDinhDuyetDuToanNguonVon",
                table: "QuyetDinhDuyetDuToanNguonVon");

            migrationBuilder.DropPrimaryKey(
                name: "PK_QuyetDinhDuyetDuToanChiPhi",
                table: "QuyetDinhDuyetDuToanChiPhi");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "QuyetDinhDuyetDuToanNguonVon",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "QuyetDinhDuyetDuToanNguonVon",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "QuyetDinhDuyetDuToanNguonVon",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "Index",
                table: "QuyetDinhDuyetDuToanNguonVon",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "QuyetDinhDuyetDuToanNguonVon",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "QuyetDinhDuyetDuToanNguonVon",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "QuyetDinhDuyetDuToanNguonVon",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "QuyetDinhDuyetDuToanChiPhi",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "QuyetDinhDuyetDuToanChiPhi",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "QuyetDinhDuyetDuToanChiPhi",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "Index",
                table: "QuyetDinhDuyetDuToanChiPhi",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "QuyetDinhDuyetDuToanChiPhi",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "QuyetDinhDuyetDuToanChiPhi",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "QuyetDinhDuyetDuToanChiPhi",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_QuyetDinhDuyetDuToanNguonVon",
                table: "QuyetDinhDuyetDuToanNguonVon",
                columns: new[] { "QuyetDinhDuToanId", "Id" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_QuyetDinhDuyetDuToanChiPhi",
                table: "QuyetDinhDuyetDuToanChiPhi",
                columns: new[] { "Id", "QuyetDinhDuToanId" });

            migrationBuilder.CreateIndex(
                name: "IX_QuyetDinhDuyetDuToanChiPhi_QuyetDinhDuToanId",
                table: "QuyetDinhDuyetDuToanChiPhi",
                column: "QuyetDinhDuToanId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_QuyetDinhDuyetDuToanNguonVon",
                table: "QuyetDinhDuyetDuToanNguonVon");

            migrationBuilder.DropPrimaryKey(
                name: "PK_QuyetDinhDuyetDuToanChiPhi",
                table: "QuyetDinhDuyetDuToanChiPhi");

            migrationBuilder.DropIndex(
                name: "IX_QuyetDinhDuyetDuToanChiPhi_QuyetDinhDuToanId",
                table: "QuyetDinhDuyetDuToanChiPhi");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "QuyetDinhDuyetDuToanNguonVon");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "QuyetDinhDuyetDuToanNguonVon");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "QuyetDinhDuyetDuToanNguonVon");

            migrationBuilder.DropColumn(
                name: "Index",
                table: "QuyetDinhDuyetDuToanNguonVon");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "QuyetDinhDuyetDuToanNguonVon");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "QuyetDinhDuyetDuToanNguonVon");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "QuyetDinhDuyetDuToanNguonVon");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "QuyetDinhDuyetDuToanChiPhi");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "QuyetDinhDuyetDuToanChiPhi");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "QuyetDinhDuyetDuToanChiPhi");

            migrationBuilder.DropColumn(
                name: "Index",
                table: "QuyetDinhDuyetDuToanChiPhi");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "QuyetDinhDuyetDuToanChiPhi");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "QuyetDinhDuyetDuToanChiPhi");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "QuyetDinhDuyetDuToanChiPhi");

            migrationBuilder.AddPrimaryKey(
                name: "PK_QuyetDinhDuyetDuToanNguonVon",
                table: "QuyetDinhDuyetDuToanNguonVon",
                columns: new[] { "QuyetDinhDuToanId", "NguonVonId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_QuyetDinhDuyetDuToanChiPhi",
                table: "QuyetDinhDuyetDuToanChiPhi",
                column: "QuyetDinhDuToanId");
        }
    }
}

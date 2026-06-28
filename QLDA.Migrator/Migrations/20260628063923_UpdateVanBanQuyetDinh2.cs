using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class UpdateVanBanQuyetDinh2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuyetDinhDuyetKHLCNT_VanBanQuyetDinh_Id",
                table: "QuyetDinhDuyetKHLCNT");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "QuyetDinhDuyetKHLCNT",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldDefaultValueSql: "NEWSEQUENTIALID()");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "QuyetDinhDuyetKHLCNT",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "QuyetDinhDuyetKHLCNT",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "Index",
                table: "QuyetDinhDuyetKHLCNT",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "QuyetDinhDuyetKHLCNT",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "QuyetDinhId",
                table: "QuyetDinhDuyetKHLCNT",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "QuyetDinhDuyetKHLCNT",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "QuyetDinhDuyetKHLCNT",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_QuyetDinhDuyetKHLCNT_QuyetDinhId",
                table: "QuyetDinhDuyetKHLCNT",
                column: "QuyetDinhId");

            migrationBuilder.AddForeignKey(
                name: "FK_QuyetDinhDuyetKHLCNT_VanBanQuyetDinh_QuyetDinhId",
                table: "QuyetDinhDuyetKHLCNT",
                column: "QuyetDinhId",
                principalTable: "VanBanQuyetDinh",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuyetDinhDuyetKHLCNT_VanBanQuyetDinh_QuyetDinhId",
                table: "QuyetDinhDuyetKHLCNT");

            migrationBuilder.DropIndex(
                name: "IX_QuyetDinhDuyetKHLCNT_QuyetDinhId",
                table: "QuyetDinhDuyetKHLCNT");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "QuyetDinhDuyetKHLCNT");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "QuyetDinhDuyetKHLCNT");

            migrationBuilder.DropColumn(
                name: "Index",
                table: "QuyetDinhDuyetKHLCNT");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "QuyetDinhDuyetKHLCNT");

            migrationBuilder.DropColumn(
                name: "QuyetDinhId",
                table: "QuyetDinhDuyetKHLCNT");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "QuyetDinhDuyetKHLCNT");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "QuyetDinhDuyetKHLCNT");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "QuyetDinhDuyetKHLCNT",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWSEQUENTIALID()",
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddForeignKey(
                name: "FK_QuyetDinhDuyetKHLCNT_VanBanQuyetDinh_Id",
                table: "QuyetDinhDuyetKHLCNT",
                column: "Id",
                principalTable: "VanBanQuyetDinh",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

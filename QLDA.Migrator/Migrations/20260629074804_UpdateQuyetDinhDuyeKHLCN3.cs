using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class UpdateQuyetDinhDuyeKHLCN3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuyetDinhDuyetKHLCNT_VanBanQuyetDinh_QuyetDinhId",
                table: "QuyetDinhDuyetKHLCNT");

            migrationBuilder.DropIndex(
                name: "IX_QuyetDinhDuyetKHLCNT_QuyetDinhId",
                table: "QuyetDinhDuyetKHLCNT");

            migrationBuilder.DropColumn(
                name: "QuyetDinhId",
                table: "QuyetDinhDuyetKHLCNT");

            migrationBuilder.AlterColumn<string>(
                name: "CoQuanQuyetDinh",
                table: "VanBanQuyetDinh",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_QuyetDinhDuyetKHLCNT_VanBanQuyetDinh_Id",
                table: "QuyetDinhDuyetKHLCNT",
                column: "Id",
                principalTable: "VanBanQuyetDinh",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuyetDinhDuyetKHLCNT_VanBanQuyetDinh_Id",
                table: "QuyetDinhDuyetKHLCNT");

            migrationBuilder.AlterColumn<string>(
                name: "CoQuanQuyetDinh",
                table: "VanBanQuyetDinh",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "QuyetDinhId",
                table: "QuyetDinhDuyetKHLCNT",
                type: "uniqueidentifier",
                nullable: true);

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
    }
}

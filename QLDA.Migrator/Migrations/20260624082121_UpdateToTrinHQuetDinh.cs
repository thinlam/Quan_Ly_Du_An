using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class UpdateToTrinHQuetDinh : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ToTrinhQuyetDinh_HoSoMoiThauDienTu_HoSoMoiThauId",
                table: "ToTrinhQuyetDinh");

            migrationBuilder.DropIndex(
                name: "IX_ToTrinhQuyetDinh_HoSoMoiThauId",
                table: "ToTrinhQuyetDinh");

            migrationBuilder.DropColumn(
                name: "HoSoMoiThauId",
                table: "ToTrinhQuyetDinh");

            migrationBuilder.AddColumn<Guid>(
                name: "HoSoMoiThauQuyetDinhId",
                table: "ToTrinhQuyetDinh",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "HoSoMoiThauToTrinhId",
                table: "ToTrinhQuyetDinh",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ToTrinhQuyetDinh_HoSoMoiThauQuyetDinhId",
                table: "ToTrinhQuyetDinh",
                column: "HoSoMoiThauQuyetDinhId",
                unique: true,
                filter: "[HoSoMoiThauQuyetDinhId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ToTrinhQuyetDinh_HoSoMoiThauToTrinhId",
                table: "ToTrinhQuyetDinh",
                column: "HoSoMoiThauToTrinhId",
                unique: true,
                filter: "[HoSoMoiThauToTrinhId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_ToTrinhQuyetDinh_HoSoMoiThauDienTu_HoSoMoiThauQuyetDinhId",
                table: "ToTrinhQuyetDinh",
                column: "HoSoMoiThauQuyetDinhId",
                principalTable: "HoSoMoiThauDienTu",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ToTrinhQuyetDinh_HoSoMoiThauDienTu_HoSoMoiThauToTrinhId",
                table: "ToTrinhQuyetDinh",
                column: "HoSoMoiThauToTrinhId",
                principalTable: "HoSoMoiThauDienTu",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ToTrinhQuyetDinh_HoSoMoiThauDienTu_HoSoMoiThauQuyetDinhId",
                table: "ToTrinhQuyetDinh");

            migrationBuilder.DropForeignKey(
                name: "FK_ToTrinhQuyetDinh_HoSoMoiThauDienTu_HoSoMoiThauToTrinhId",
                table: "ToTrinhQuyetDinh");

            migrationBuilder.DropIndex(
                name: "IX_ToTrinhQuyetDinh_HoSoMoiThauQuyetDinhId",
                table: "ToTrinhQuyetDinh");

            migrationBuilder.DropIndex(
                name: "IX_ToTrinhQuyetDinh_HoSoMoiThauToTrinhId",
                table: "ToTrinhQuyetDinh");

            migrationBuilder.DropColumn(
                name: "HoSoMoiThauQuyetDinhId",
                table: "ToTrinhQuyetDinh");

            migrationBuilder.DropColumn(
                name: "HoSoMoiThauToTrinhId",
                table: "ToTrinhQuyetDinh");

            migrationBuilder.AddColumn<Guid>(
                name: "HoSoMoiThauId",
                table: "ToTrinhQuyetDinh",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_ToTrinhQuyetDinh_HoSoMoiThauId",
                table: "ToTrinhQuyetDinh",
                column: "HoSoMoiThauId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ToTrinhQuyetDinh_HoSoMoiThauDienTu_HoSoMoiThauId",
                table: "ToTrinhQuyetDinh",
                column: "HoSoMoiThauId",
                principalTable: "HoSoMoiThauDienTu",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

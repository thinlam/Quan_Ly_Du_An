using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class Check : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BanGiaoHoSo_DmBuoc_BuocId",
                table: "BanGiaoHoSo");

            migrationBuilder.AddForeignKey(
                name: "FK_BanGiaoHoSo_DuAnBuoc_BuocId",
                table: "BanGiaoHoSo",
                column: "BuocId",
                principalTable: "DuAnBuoc",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BanGiaoHoSo_DuAnBuoc_BuocId",
                table: "BanGiaoHoSo");

            migrationBuilder.AddForeignKey(
                name: "FK_BanGiaoHoSo_DmBuoc_BuocId",
                table: "BanGiaoHoSo",
                column: "BuocId",
                principalTable: "DmBuoc",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}

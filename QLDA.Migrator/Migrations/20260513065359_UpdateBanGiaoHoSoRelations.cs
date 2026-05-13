using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBanGiaoHoSoRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BanGiaoHoSo_DM_DONVI_PhongBanChuTriId",
                table: "BanGiaoHoSo");

            migrationBuilder.DropForeignKey(
                name: "FK_BanGiaoHoSo_USER_MASTER_UserId",
                table: "BanGiaoHoSo");

            migrationBuilder.DropIndex(
                name: "IX_BanGiaoHoSo_PhongBanChuTriId",
                table: "BanGiaoHoSo");

            migrationBuilder.DropIndex(
                name: "IX_BanGiaoHoSo_UserId_TrangThai",
                table: "BanGiaoHoSo");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "BanGiaoHoSo");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "BanGiaoHoSo",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_BanGiaoHoSo_CreatedBy_TrangThai",
                table: "BanGiaoHoSo",
                columns: new[] { "CreatedBy", "TrangThai" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BanGiaoHoSo_CreatedBy_TrangThai",
                table: "BanGiaoHoSo");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "BanGiaoHoSo",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "BanGiaoHoSo",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BanGiaoHoSo_PhongBanChuTriId",
                table: "BanGiaoHoSo",
                column: "PhongBanChuTriId");

            migrationBuilder.CreateIndex(
                name: "IX_BanGiaoHoSo_UserId_TrangThai",
                table: "BanGiaoHoSo",
                columns: new[] { "UserId", "TrangThai" });

            migrationBuilder.AddForeignKey(
                name: "FK_BanGiaoHoSo_DM_DONVI_PhongBanChuTriId",
                table: "BanGiaoHoSo",
                column: "PhongBanChuTriId",
                principalTable: "DM_DONVI",
                principalColumn: "DonViID",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_BanGiaoHoSo_USER_MASTER_UserId",
                table: "BanGiaoHoSo",
                column: "UserId",
                principalTable: "USER_MASTER",
                principalColumn: "User_MasterID",
                onDelete: ReferentialAction.SetNull);
        }
    }
}

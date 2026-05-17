using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class ThongTinDieuChinhChiPhiOneToOne : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ThongTinDieuChinhChiPhi_QuyetDinhDieuChinhId",
                table: "ThongTinDieuChinhChiPhi");

            migrationBuilder.CreateIndex(
                name: "IX_ThongTinDieuChinhChiPhi_QuyetDinhDieuChinhId",
                table: "ThongTinDieuChinhChiPhi",
                column: "QuyetDinhDieuChinhId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ThongTinDieuChinhChiPhi_QuyetDinhDieuChinhId",
                table: "ThongTinDieuChinhChiPhi");

            migrationBuilder.CreateIndex(
                name: "IX_ThongTinDieuChinhChiPhi_QuyetDinhDieuChinhId",
                table: "ThongTinDieuChinhChiPhi",
                column: "QuyetDinhDieuChinhId");
        }
    }
}

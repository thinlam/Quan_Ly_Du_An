using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class UpateTableKetQuaThamDinhNhaTha2u : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ToTrinhThamDinhNhaThau_DmTrangThaiPheDuyet_TrangThaiThamDinhId",
                table: "ToTrinhThamDinhNhaThau");

            migrationBuilder.DropIndex(
                name: "IX_ToTrinhThamDinhNhaThau_TrangThaiThamDinhId",
                table: "ToTrinhThamDinhNhaThau");

            migrationBuilder.DropColumn(
                name: "TrangThaiThamDinhId",
                table: "ToTrinhThamDinhNhaThau");

            migrationBuilder.AddColumn<bool>(
                name: "DaThamDinh",
                table: "ToTrinhThamDinhNhaThau",
                type: "bit",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DaThamDinh",
                table: "ToTrinhThamDinhNhaThau");

            migrationBuilder.AddColumn<int>(
                name: "TrangThaiThamDinhId",
                table: "ToTrinhThamDinhNhaThau",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ToTrinhThamDinhNhaThau_TrangThaiThamDinhId",
                table: "ToTrinhThamDinhNhaThau",
                column: "TrangThaiThamDinhId");

            migrationBuilder.AddForeignKey(
                name: "FK_ToTrinhThamDinhNhaThau_DmTrangThaiPheDuyet_TrangThaiThamDinhId",
                table: "ToTrinhThamDinhNhaThau",
                column: "TrangThaiThamDinhId",
                principalTable: "DmTrangThaiPheDuyet",
                principalColumn: "Id");
        }
    }
}

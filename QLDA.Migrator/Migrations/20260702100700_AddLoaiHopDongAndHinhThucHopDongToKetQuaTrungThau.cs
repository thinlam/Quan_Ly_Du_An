using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class AddLoaiHopDongAndHinhThucHopDongToKetQuaTrungThau : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HinhThucHopDong",
                table: "KetQuaTrungThau",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LoaiHopDongId",
                table: "KetQuaTrungThau",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_KetQuaTrungThau_LoaiHopDongId",
                table: "KetQuaTrungThau",
                column: "LoaiHopDongId");

            migrationBuilder.AddForeignKey(
                name: "FK_KetQuaTrungThau_DmLoaiHopDong_LoaiHopDongId",
                table: "KetQuaTrungThau",
                column: "LoaiHopDongId",
                principalTable: "DmLoaiHopDong",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KetQuaTrungThau_DmLoaiHopDong_LoaiHopDongId",
                table: "KetQuaTrungThau");

            migrationBuilder.DropIndex(
                name: "IX_KetQuaTrungThau_LoaiHopDongId",
                table: "KetQuaTrungThau");

            migrationBuilder.DropColumn(
                name: "HinhThucHopDong",
                table: "KetQuaTrungThau");

            migrationBuilder.DropColumn(
                name: "LoaiHopDongId",
                table: "KetQuaTrungThau");
        }
    }
}

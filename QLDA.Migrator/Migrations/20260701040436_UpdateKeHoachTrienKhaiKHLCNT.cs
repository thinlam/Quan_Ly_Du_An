using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class UpdateKeHoachTrienKhaiKHLCNT : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_TrienKhaiKeHoachLCNT_HinhThucLCNT",
                table: "TrienKhaiKeHoachLCNT",
                column: "HinhThucLCNT");

            migrationBuilder.AddForeignKey(
                name: "FK_TrienKhaiKeHoachLCNT_DmHinhThucLuaChonNhaThau_HinhThucLCNT",
                table: "TrienKhaiKeHoachLCNT",
                column: "HinhThucLCNT",
                principalTable: "DmHinhThucLuaChonNhaThau",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrienKhaiKeHoachLCNT_DmHinhThucLuaChonNhaThau_HinhThucLCNT",
                table: "TrienKhaiKeHoachLCNT");

            migrationBuilder.DropIndex(
                name: "IX_TrienKhaiKeHoachLCNT_HinhThucLCNT",
                table: "TrienKhaiKeHoachLCNT");
        }
    }
}

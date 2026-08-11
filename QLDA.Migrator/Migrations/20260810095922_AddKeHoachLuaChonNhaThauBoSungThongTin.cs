using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class AddKeHoachLuaChonNhaThauBoSungThongTin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NguonVonId",
                table: "KeHoachLuaChonNhaThau",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ThoiGianThucHien",
                table: "KeHoachLuaChonNhaThau",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "TongDuToan",
                table: "KeHoachLuaChonNhaThau",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "TongDuToanThamDinhGia",
                table: "KeHoachLuaChonNhaThau",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_KeHoachLuaChonNhaThau_NguonVonId",
                table: "KeHoachLuaChonNhaThau",
                column: "NguonVonId");

            migrationBuilder.AddForeignKey(
                name: "FK_KeHoachLuaChonNhaThau_DmNguonVon_NguonVonId",
                table: "KeHoachLuaChonNhaThau",
                column: "NguonVonId",
                principalTable: "DmNguonVon",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KeHoachLuaChonNhaThau_DmNguonVon_NguonVonId",
                table: "KeHoachLuaChonNhaThau");

            migrationBuilder.DropIndex(
                name: "IX_KeHoachLuaChonNhaThau_NguonVonId",
                table: "KeHoachLuaChonNhaThau");

            migrationBuilder.DropColumn(
                name: "NguonVonId",
                table: "KeHoachLuaChonNhaThau");

            migrationBuilder.DropColumn(
                name: "ThoiGianThucHien",
                table: "KeHoachLuaChonNhaThau");

            migrationBuilder.DropColumn(
                name: "TongDuToan",
                table: "KeHoachLuaChonNhaThau");

            migrationBuilder.DropColumn(
                name: "TongDuToanThamDinhGia",
                table: "KeHoachLuaChonNhaThau");
        }
    }
}

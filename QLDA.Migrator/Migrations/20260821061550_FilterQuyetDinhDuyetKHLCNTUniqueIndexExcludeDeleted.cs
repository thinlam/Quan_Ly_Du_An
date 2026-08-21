using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class FilterQuyetDinhDuyetKHLCNTUniqueIndexExcludeDeleted : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_QuyetDinhDuyetKHLCNT_KeHoachLuaChonNhaThauId",
                table: "QuyetDinhDuyetKHLCNT");

            migrationBuilder.CreateIndex(
                name: "IX_QuyetDinhDuyetKHLCNT_KeHoachLuaChonNhaThauId",
                table: "QuyetDinhDuyetKHLCNT",
                column: "KeHoachLuaChonNhaThauId",
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_QuyetDinhDuyetKHLCNT_KeHoachLuaChonNhaThauId",
                table: "QuyetDinhDuyetKHLCNT");

            migrationBuilder.CreateIndex(
                name: "IX_QuyetDinhDuyetKHLCNT_KeHoachLuaChonNhaThauId",
                table: "QuyetDinhDuyetKHLCNT",
                column: "KeHoachLuaChonNhaThauId",
                unique: true,
                filter: "[KeHoachLuaChonNhaThauId] IS NOT NULL");
        }
    }
}

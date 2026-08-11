using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class FixRenameTongDuToanThamDinhGiaToDuToanThamDinh : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TongDuToanThamDinhGia",
                table: "KeHoachLuaChonNhaThau",
                newName: "DuToanThamDinh");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DuToanThamDinh",
                table: "KeHoachLuaChonNhaThau",
                newName: "TongDuToanThamDinhGia");
        }
    }
}

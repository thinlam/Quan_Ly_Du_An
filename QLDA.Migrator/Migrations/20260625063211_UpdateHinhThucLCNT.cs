using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class UpdateHinhThucLCNT : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ThamDinh",
                table: "HoSoMoiThauDienTu",
                type: "bit",
                nullable: true);
            migrationBuilder.AddColumn<bool>(
               name: "LaChiDinhThau",
               table: "DanhMucHinhThucLuaChonNhaThau",
               type: "bit",
               nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ThamDinh",
                table: "HoSoMoiThauDienTu");
            migrationBuilder.DropColumn(
              name: "LaChiDinhThau",
              table: "DanhMucHinhThucLuaChonNhaThau");
        }
    }
}

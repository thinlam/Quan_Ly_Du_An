using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class updateToTrinhThamDinhNhaThau : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "GiaTriTrungThau",
                table: "ToTrinhThamDinhNhaThau",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SoNgayThucHienHopDong",
                table: "ToTrinhThamDinhNhaThau",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ThoiGianThucHienGoiThau",
                table: "ToTrinhThamDinhNhaThau",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GiaTriTrungThau",
                table: "ToTrinhThamDinhNhaThau");

            migrationBuilder.DropColumn(
                name: "SoNgayThucHienHopDong",
                table: "ToTrinhThamDinhNhaThau");

            migrationBuilder.DropColumn(
                name: "ThoiGianThucHienGoiThau",
                table: "ToTrinhThamDinhNhaThau");
        }
    }
}

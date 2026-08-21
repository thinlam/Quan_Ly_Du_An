using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class MoveSoLuongGoiThauToQuyetDinhDuyetKHLCNT : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SoLuongGoiThau",
                table: "KeHoachLuaChonNhaThau");

            migrationBuilder.AddColumn<int>(
                name: "SoLuongGoiThau",
                table: "QuyetDinhDuyetKHLCNT",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SoLuongGoiThau",
                table: "QuyetDinhDuyetKHLCNT");

            migrationBuilder.AddColumn<int>(
                name: "SoLuongGoiThau",
                table: "KeHoachLuaChonNhaThau",
                type: "int",
                nullable: true);
        }
    }
}

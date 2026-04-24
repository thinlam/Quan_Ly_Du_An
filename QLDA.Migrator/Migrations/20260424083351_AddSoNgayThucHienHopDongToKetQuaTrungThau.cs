using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class AddSoNgayThucHienHopDongToKetQuaTrungThau : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "SoNgayTrienKhai",
                table: "KetQuaTrungThau",
                type: "int",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SoNgayThucHienHopDong",
                table: "KetQuaTrungThau",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SoNgayThucHienHopDong",
                table: "KetQuaTrungThau");

            migrationBuilder.AlterColumn<long>(
                name: "SoNgayTrienKhai",
                table: "KetQuaTrungThau",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}

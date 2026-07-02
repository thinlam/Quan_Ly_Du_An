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
            migrationBuilder.DropColumn(
                name: "LevelId",
                table: "PhanQuyenChucNang");

            migrationBuilder.DropColumn(
                name: "NguoiDungId",
                table: "PhanQuyenChucNang");

            migrationBuilder.DropColumn(
                name: "NguoiDungMacDinh",
                table: "PhanQuyenChucNang");

            migrationBuilder.AddColumn<long>(
                name: "RecipientRoleId",
                table: "DuongDiTrangThaiToTrinh",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RecipientRoleLevel",
                table: "DuongDiTrangThaiToTrinh",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PhanQuyenChucNangCapDo",
                columns: table => new
                {
                    QuyenId = table.Column<int>(type: "int", nullable: false),
                    LevelId = table.Column<long>(type: "bigint", nullable: false),
                    NguoiDungMacDinh = table.Column<bool>(type: "bit", nullable: true),
                    NguoiDungChiDinhs = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhanQuyenChucNangCapDo", x => new { x.QuyenId, x.LevelId });
                    table.ForeignKey(
                        name: "FK_PhanQuyenChucNangCapDo_PhanQuyenChucNang_QuyenId",
                        column: x => x.QuyenId,
                        principalTable: "PhanQuyenChucNang",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PhanQuyenChucNangCapDo");

            migrationBuilder.DropColumn(
                name: "RecipientRoleId",
                table: "DuongDiTrangThaiToTrinh");

            migrationBuilder.DropColumn(
                name: "RecipientRoleLevel",
                table: "DuongDiTrangThaiToTrinh");

            migrationBuilder.AddColumn<long>(
                name: "LevelId",
                table: "PhanQuyenChucNang",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NguoiDungId",
                table: "PhanQuyenChucNang",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "NguoiDungMacDinh",
                table: "PhanQuyenChucNang",
                type: "bit",
                nullable: true);
        }
    }
}

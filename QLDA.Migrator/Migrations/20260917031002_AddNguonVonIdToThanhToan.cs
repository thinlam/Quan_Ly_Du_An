using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class AddNguonVonIdToThanhToan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NguonVonId",
                table: "ThanhToan",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ThanhToan_NguonVonId",
                table: "ThanhToan",
                column: "NguonVonId");

            migrationBuilder.AddForeignKey(
                name: "FK_ThanhToan_DmNguonVon_NguonVonId",
                table: "ThanhToan",
                column: "NguonVonId",
                principalTable: "DmNguonVon",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql(@"
                UPDATE tt
                SET tt.NguonVonId = gt.NguonVonId
                FROM dbo.ThanhToan tt
                JOIN dbo.NghiemThu nt ON nt.Id = tt.NghiemThuId
                JOIN dbo.HopDong hd ON hd.Id = nt.HopDongId
                JOIN dbo.GoiThau gt ON gt.Id = hd.GoiThauId
                WHERE tt.NguonVonId IS NULL AND gt.NguonVonId IS NOT NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ThanhToan_DmNguonVon_NguonVonId",
                table: "ThanhToan");

            migrationBuilder.DropIndex(
                name: "IX_ThanhToan_NguonVonId",
                table: "ThanhToan");

            migrationBuilder.DropColumn(
                name: "NguonVonId",
                table: "ThanhToan");
        }
    }
}

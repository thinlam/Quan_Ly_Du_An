using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class RenameRoleLanhDaoToLDDV : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CauHinhVaiTroQuyen_DmQuyen_QuyenId",
                table: "CauHinhVaiTroQuyen");

            migrationBuilder.UpdateData(
                table: "CauHinhVaiTroQuyen",
                keyColumn: "Id",
                keyValue: 59,
                column: "VaiTro",
                value: "QLDA_LDDV");

            migrationBuilder.UpdateData(
                table: "CauHinhVaiTroQuyen",
                keyColumn: "Id",
                keyValue: 60,
                column: "VaiTro",
                value: "QLDA_LDDV");

            migrationBuilder.UpdateData(
                table: "CauHinhVaiTroQuyen",
                keyColumn: "Id",
                keyValue: 61,
                column: "VaiTro",
                value: "QLDA_LDDV");

            migrationBuilder.UpdateData(
                table: "CauHinhVaiTroQuyen",
                keyColumn: "Id",
                keyValue: 62,
                column: "VaiTro",
                value: "QLDA_LDDV");

            migrationBuilder.UpdateData(
                table: "CauHinhVaiTroQuyen",
                keyColumn: "Id",
                keyValue: 63,
                column: "VaiTro",
                value: "QLDA_LDDV");

            migrationBuilder.UpdateData(
                table: "CauHinhVaiTroQuyen",
                keyColumn: "Id",
                keyValue: 64,
                column: "VaiTro",
                value: "QLDA_LDDV");

            migrationBuilder.UpdateData(
                table: "CauHinhVaiTroQuyen",
                keyColumn: "Id",
                keyValue: 65,
                column: "VaiTro",
                value: "QLDA_LDDV");

            migrationBuilder.UpdateData(
                table: "CauHinhVaiTroQuyen",
                keyColumn: "Id",
                keyValue: 66,
                column: "VaiTro",
                value: "QLDA_LDDV");

            migrationBuilder.UpdateData(
                table: "CauHinhVaiTroQuyen",
                keyColumn: "Id",
                keyValue: 67,
                column: "VaiTro",
                value: "QLDA_LDDV");

            migrationBuilder.AddForeignKey(
                name: "FK_CauHinhVaiTroQuyen_DmQuyen_QuyenId",
                table: "CauHinhVaiTroQuyen",
                column: "QuyenId",
                principalTable: "DmQuyen",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CauHinhVaiTroQuyen_DmQuyen_QuyenId",
                table: "CauHinhVaiTroQuyen");

            migrationBuilder.UpdateData(
                table: "CauHinhVaiTroQuyen",
                keyColumn: "Id",
                keyValue: 59,
                column: "VaiTro",
                value: "QLDA_LD");

            migrationBuilder.UpdateData(
                table: "CauHinhVaiTroQuyen",
                keyColumn: "Id",
                keyValue: 60,
                column: "VaiTro",
                value: "QLDA_LD");

            migrationBuilder.UpdateData(
                table: "CauHinhVaiTroQuyen",
                keyColumn: "Id",
                keyValue: 61,
                column: "VaiTro",
                value: "QLDA_LD");

            migrationBuilder.UpdateData(
                table: "CauHinhVaiTroQuyen",
                keyColumn: "Id",
                keyValue: 62,
                column: "VaiTro",
                value: "QLDA_LD");

            migrationBuilder.UpdateData(
                table: "CauHinhVaiTroQuyen",
                keyColumn: "Id",
                keyValue: 63,
                column: "VaiTro",
                value: "QLDA_LD");

            migrationBuilder.UpdateData(
                table: "CauHinhVaiTroQuyen",
                keyColumn: "Id",
                keyValue: 64,
                column: "VaiTro",
                value: "QLDA_LD");

            migrationBuilder.UpdateData(
                table: "CauHinhVaiTroQuyen",
                keyColumn: "Id",
                keyValue: 65,
                column: "VaiTro",
                value: "QLDA_LD");

            migrationBuilder.UpdateData(
                table: "CauHinhVaiTroQuyen",
                keyColumn: "Id",
                keyValue: 66,
                column: "VaiTro",
                value: "QLDA_LD");

            migrationBuilder.UpdateData(
                table: "CauHinhVaiTroQuyen",
                keyColumn: "Id",
                keyValue: 67,
                column: "VaiTro",
                value: "QLDA_LD");

            migrationBuilder.AddForeignKey(
                name: "FK_CauHinhVaiTroQuyen_DmQuyen_QuyenId",
                table: "CauHinhVaiTroQuyen",
                column: "QuyenId",
                principalTable: "DmQuyen",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class ChangePhanKhaiKinhPhiMoneyToDecimal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "CauHinhVaiTroQuyen",
                keyColumn: "Id",
                keyValue: 81);

            migrationBuilder.AlterColumn<decimal>(
                name: "KinhPhiPhanKhai",
                table: "PhanKhaiKinhPhi",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldPrecision: 18,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "KinhPhiDeXuat",
                table: "PhanKhaiKinhPhi",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldPrecision: 18,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ThuyetMinh",
                table: "PhanKhaiKinhPhi",
                type: "nvarchar(max)",
                nullable: true);

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
                column: "QuyenId",
                value: 25);

            migrationBuilder.UpdateData(
                table: "CauHinhVaiTroQuyen",
                keyColumn: "Id",
                keyValue: 65,
                column: "QuyenId",
                value: 26);

            migrationBuilder.UpdateData(
                table: "CauHinhVaiTroQuyen",
                keyColumn: "Id",
                keyValue: 66,
                column: "QuyenId",
                value: 27);

            migrationBuilder.UpdateData(
                table: "CauHinhVaiTroQuyen",
                keyColumn: "Id",
                keyValue: 67,
                column: "QuyenId",
                value: 29);

            migrationBuilder.UpdateData(
                table: "CauHinhVaiTroQuyen",
                keyColumn: "Id",
                keyValue: 68,
                columns: new[] { "QuyenId", "VaiTro" },
                values: new object[] { 2, "QLDA_ChuyenVien" });

            migrationBuilder.UpdateData(
                table: "CauHinhVaiTroQuyen",
                keyColumn: "Id",
                keyValue: 69,
                column: "QuyenId",
                value: 8);

            migrationBuilder.UpdateData(
                table: "CauHinhVaiTroQuyen",
                keyColumn: "Id",
                keyValue: 70,
                column: "QuyenId",
                value: 13);

            migrationBuilder.UpdateData(
                table: "CauHinhVaiTroQuyen",
                keyColumn: "Id",
                keyValue: 71,
                column: "QuyenId",
                value: 18);

            migrationBuilder.UpdateData(
                table: "CauHinhVaiTroQuyen",
                keyColumn: "Id",
                keyValue: 72,
                column: "QuyenId",
                value: 24);

            migrationBuilder.UpdateData(
                table: "CauHinhVaiTroQuyen",
                keyColumn: "Id",
                keyValue: 73,
                column: "QuyenId",
                value: 3);

            migrationBuilder.UpdateData(
                table: "CauHinhVaiTroQuyen",
                keyColumn: "Id",
                keyValue: 74,
                column: "QuyenId",
                value: 4);

            migrationBuilder.UpdateData(
                table: "CauHinhVaiTroQuyen",
                keyColumn: "Id",
                keyValue: 75,
                column: "QuyenId",
                value: 9);

            migrationBuilder.UpdateData(
                table: "CauHinhVaiTroQuyen",
                keyColumn: "Id",
                keyValue: 76,
                column: "QuyenId",
                value: 10);

            migrationBuilder.UpdateData(
                table: "CauHinhVaiTroQuyen",
                keyColumn: "Id",
                keyValue: 77,
                column: "QuyenId",
                value: 14);

            migrationBuilder.UpdateData(
                table: "CauHinhVaiTroQuyen",
                keyColumn: "Id",
                keyValue: 78,
                column: "QuyenId",
                value: 15);

            migrationBuilder.UpdateData(
                table: "CauHinhVaiTroQuyen",
                keyColumn: "Id",
                keyValue: 79,
                column: "QuyenId",
                value: 19);

            migrationBuilder.UpdateData(
                table: "CauHinhVaiTroQuyen",
                keyColumn: "Id",
                keyValue: 80,
                column: "QuyenId",
                value: 20);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ThuyetMinh",
                table: "PhanKhaiKinhPhi");

            migrationBuilder.AlterColumn<long>(
                name: "KinhPhiPhanKhai",
                table: "PhanKhaiKinhPhi",
                type: "bigint",
                precision: 18,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "KinhPhiDeXuat",
                table: "PhanKhaiKinhPhi",
                type: "bigint",
                precision: 18,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldNullable: true);

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
                column: "QuyenId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "CauHinhVaiTroQuyen",
                keyColumn: "Id",
                keyValue: 65,
                column: "QuyenId",
                value: 7);

            migrationBuilder.UpdateData(
                table: "CauHinhVaiTroQuyen",
                keyColumn: "Id",
                keyValue: 66,
                column: "QuyenId",
                value: 12);

            migrationBuilder.UpdateData(
                table: "CauHinhVaiTroQuyen",
                keyColumn: "Id",
                keyValue: 67,
                column: "QuyenId",
                value: 17);

            migrationBuilder.UpdateData(
                table: "CauHinhVaiTroQuyen",
                keyColumn: "Id",
                keyValue: 68,
                columns: new[] { "QuyenId", "VaiTro" },
                values: new object[] { 23, "QLDA_LD" });

            migrationBuilder.UpdateData(
                table: "CauHinhVaiTroQuyen",
                keyColumn: "Id",
                keyValue: 69,
                column: "QuyenId",
                value: 2);

            migrationBuilder.UpdateData(
                table: "CauHinhVaiTroQuyen",
                keyColumn: "Id",
                keyValue: 70,
                column: "QuyenId",
                value: 8);

            migrationBuilder.UpdateData(
                table: "CauHinhVaiTroQuyen",
                keyColumn: "Id",
                keyValue: 71,
                column: "QuyenId",
                value: 13);

            migrationBuilder.UpdateData(
                table: "CauHinhVaiTroQuyen",
                keyColumn: "Id",
                keyValue: 72,
                column: "QuyenId",
                value: 18);

            migrationBuilder.UpdateData(
                table: "CauHinhVaiTroQuyen",
                keyColumn: "Id",
                keyValue: 73,
                column: "QuyenId",
                value: 24);

            migrationBuilder.UpdateData(
                table: "CauHinhVaiTroQuyen",
                keyColumn: "Id",
                keyValue: 74,
                column: "QuyenId",
                value: 3);

            migrationBuilder.UpdateData(
                table: "CauHinhVaiTroQuyen",
                keyColumn: "Id",
                keyValue: 75,
                column: "QuyenId",
                value: 4);

            migrationBuilder.UpdateData(
                table: "CauHinhVaiTroQuyen",
                keyColumn: "Id",
                keyValue: 76,
                column: "QuyenId",
                value: 9);

            migrationBuilder.UpdateData(
                table: "CauHinhVaiTroQuyen",
                keyColumn: "Id",
                keyValue: 77,
                column: "QuyenId",
                value: 10);

            migrationBuilder.UpdateData(
                table: "CauHinhVaiTroQuyen",
                keyColumn: "Id",
                keyValue: 78,
                column: "QuyenId",
                value: 14);

            migrationBuilder.UpdateData(
                table: "CauHinhVaiTroQuyen",
                keyColumn: "Id",
                keyValue: 79,
                column: "QuyenId",
                value: 15);

            migrationBuilder.UpdateData(
                table: "CauHinhVaiTroQuyen",
                keyColumn: "Id",
                keyValue: 80,
                column: "QuyenId",
                value: 19);

            migrationBuilder.InsertData(
                table: "CauHinhVaiTroQuyen",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "IsDeleted", "KichHoat", "QuyenId", "UpdatedAt", "UpdatedBy", "VaiTro" },
                values: new object[] { 81, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 20, null, "", "QLDA_ChuyenVien" });
        }
    }
}

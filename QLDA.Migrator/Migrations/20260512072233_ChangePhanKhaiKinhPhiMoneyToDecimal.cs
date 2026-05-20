using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Migrator.Migrations {
    /// <inheritdoc />
    public partial class ChangePhanKhaiKinhPhiMoneyToDecimal : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
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

        }
    }
}

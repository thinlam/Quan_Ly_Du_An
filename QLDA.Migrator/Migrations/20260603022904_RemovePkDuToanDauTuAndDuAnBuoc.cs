using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class RemovePkDuToanDauTuAndDuAnBuoc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DuToanDauTu_DuAnBuoc_DuAnBuocId",
                table: "DuToanDauTu");

            migrationBuilder.DropIndex(
                name: "IX_DuToanDauTu_DuAnBuocId",
                table: "DuToanDauTu");

            migrationBuilder.DropColumn(
                name: "DuAnBuocId",
                table: "DuToanDauTu");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DuAnBuocId",
                table: "DuToanDauTu",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DuToanDauTu_DuAnBuocId",
                table: "DuToanDauTu",
                column: "DuAnBuocId");

            migrationBuilder.AddForeignKey(
                name: "FK_DuToanDauTu_DuAnBuoc_DuAnBuocId",
                table: "DuToanDauTu",
                column: "DuAnBuocId",
                principalTable: "DuAnBuoc",
                principalColumn: "Id");
        }
    }
}

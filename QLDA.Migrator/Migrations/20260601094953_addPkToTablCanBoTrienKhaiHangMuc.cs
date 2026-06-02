using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class addPkToTablCanBoTrienKhaiHangMuc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_CanBoTrienKhaiHangMuc_CanBoId",
                table: "CanBoTrienKhaiHangMuc",
                column: "CanBoId");

            migrationBuilder.AddForeignKey(
                name: "FK_CanBoTrienKhaiHangMuc_USER_MASTER_CanBoId",
                table: "CanBoTrienKhaiHangMuc",
                column: "CanBoId",
                principalSchema: "dbo",
                principalTable: "USER_MASTER",
                principalColumn: "User_PortalID",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CanBoTrienKhaiHangMuc_USER_MASTER_CanBoId",
                table: "CanBoTrienKhaiHangMuc");

            migrationBuilder.DropIndex(
                name: "IX_CanBoTrienKhaiHangMuc_CanBoId",
                table: "CanBoTrienKhaiHangMuc");
        }
    }
}

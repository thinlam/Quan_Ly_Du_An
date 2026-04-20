using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class ChangeDuToanBanDauIdToBigint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop index first
            migrationBuilder.DropIndex(
                name: "IX_DuAn_DuToanBanDauId",
                table: "DuAn");

            // Drop FK constraint
            migrationBuilder.DropForeignKey(
                name: "FK_DuAn_DuToan_DuToanBanDauId",
                table: "DuAn");

            // Drop old column
            migrationBuilder.DropColumn(
                name: "DuToanBanDauId",
                table: "DuAn");

            // Create new column with bigint type
            migrationBuilder.AddColumn<long>(
                name: "DuToanBanDauId",
                table: "DuAn",
                type: "bigint",
                nullable: true);

            // Recreate index
            migrationBuilder.CreateIndex(
                name: "IX_DuAn_DuToanBanDauId",
                table: "DuAn",
                column: "DuToanBanDauId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop index
            migrationBuilder.DropIndex(
                name: "IX_DuAn_DuToanBanDauId",
                table: "DuAn");

            // Drop bigint column
            migrationBuilder.DropColumn(
                name: "DuToanBanDauId",
                table: "DuAn");

            // Recreate as uniqueidentifier
            migrationBuilder.AddColumn<Guid>(
                name: "DuToanBanDauId",
                table: "DuAn",
                type: "uniqueidentifier",
                nullable: true);

            // Recreate index
            migrationBuilder.CreateIndex(
                name: "IX_DuAn_DuToanBanDauId",
                table: "DuAn",
                column: "DuToanBanDauId");

            // Recreate FK constraint
            migrationBuilder.AddForeignKey(
                name: "FK_DuAn_DuToan_DuToanBanDauId",
                table: "DuAn",
                column: "DuToanBanDauId",
                principalTable: "DuToan",
                principalColumn: "Id");
        }
    }
}

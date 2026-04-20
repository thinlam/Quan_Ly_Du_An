using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingDuAnColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "SoDuToanBanDau",
                table: "DuAn",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SoTienDuToanBanDau",
                table: "DuAn",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SoDuToanBanDau",
                table: "DuAn");

            migrationBuilder.DropColumn(
                name: "SoTienDuToanBanDau",
                table: "DuAn");
        }
    }
}

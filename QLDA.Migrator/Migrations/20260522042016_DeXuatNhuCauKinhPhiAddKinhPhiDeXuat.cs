using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class DeXuatNhuCauKinhPhiAddKinhPhiDeXuat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "KinhPhiDeXuat",
                table: "DeXuatNhuCauKinhPhi",
                type: "bigint",
                precision: 18,
                scale: 2,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "KinhPhiDeXuat",
                table: "DeXuatNhuCauKinhPhi");
        }
    }
}

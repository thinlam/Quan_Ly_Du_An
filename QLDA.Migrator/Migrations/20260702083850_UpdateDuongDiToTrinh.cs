using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDuongDiToTrinh : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RecipientRoleId",
                table: "DuongDiTrangThaiToTrinh");

            migrationBuilder.DropColumn(
                name: "RecipientRoleLevel",
                table: "DuongDiTrangThaiToTrinh");
        }
    }
}

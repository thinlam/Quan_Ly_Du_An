using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class UpdateToTrinhKetQuaGoiThau : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "NgayToTrinh",
                table: "ToTrinhKetQuaGoiThau",
                newName: "NgayTrinh");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "NgayTrinh",
                table: "ToTrinhKetQuaGoiThau",
                newName: "NgayToTrinh");
        }
    }
}

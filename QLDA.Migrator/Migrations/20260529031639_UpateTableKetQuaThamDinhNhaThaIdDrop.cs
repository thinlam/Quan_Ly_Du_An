using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class UpateTableKetQuaThamDinhNhaThaIdDrop : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Drop the old Id column
            migrationBuilder.DropColumn(
                name: "Id",
                table: "KetQuaThamDinhNhaThau");

            // 2. Add the new Guid column with a default value SQL constraint
            // Notice <Guid> matches "uniqueidentifier"
            migrationBuilder.AddColumn<Guid>(
                name: "Id_temp",
                table: "KetQuaThamDinhNhaThau",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "newid()");
            // 3. Rename Id_temp to Id
            migrationBuilder.RenameColumn(
                name: "Id_temp",
                table: "KetQuaThamDinhNhaThau",
                newName: "Id");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
              name: "Id",
              table: "KetQuaThamDinhNhaThau",
              newName: "Id_temp");

            migrationBuilder.DropColumn(
                name: "Id_temp",
                table: "KetQuaThamDinhNhaThau");

            // Recreate whatever the original Id column was (assuming uniqueidentifier here)
            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "KetQuaThamDinhNhaThau",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "newid()");
        }
    }
}

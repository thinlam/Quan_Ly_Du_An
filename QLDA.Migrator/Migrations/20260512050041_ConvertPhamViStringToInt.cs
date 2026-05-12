using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class ConvertPhamViStringToInt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Convert dữ liệu từ string sang int
            migrationBuilder.Sql(@"
                UPDATE KySo 
                SET PhamVi = CASE 
                    WHEN PhamVi = 'CANHAN' THEN '2'
                    WHEN PhamVi = 'DONVI' THEN '1'
                    ELSE NULL
                END
                WHERE PhamVi IS NOT NULL
            ");

            migrationBuilder.AlterColumn<int>(
                name: "PhamVi",
                table: "KySo",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PhamVi",
                table: "KySo",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}

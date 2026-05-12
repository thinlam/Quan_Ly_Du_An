using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class DropNguoiGiaoViecIdFromPheDuyetDuToan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NguoiGiaoViecId",
                table: "PheDuyetDuToan");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "NguoiGiaoViecId",
                table: "PheDuyetDuToan",
                type: "bigint",
                nullable: true);
        }
    }
}

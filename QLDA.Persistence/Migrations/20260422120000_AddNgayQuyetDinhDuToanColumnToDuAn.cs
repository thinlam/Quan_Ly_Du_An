using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddNgayQuyetDinhDuToanColumnToDuAn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "NgayQuyetDinhDuToan",
                table: "DuAn",
                type: "datetimeoffset",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NgayQuyetDinhDuToan",
                table: "DuAn");
        }
    }
}

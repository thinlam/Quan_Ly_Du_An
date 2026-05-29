using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class updatepkTrienKhaiKeHoach : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TrienKhaiKeHoachLCNT_GoiThauId",
                table: "TrienKhaiKeHoachLCNT");

            migrationBuilder.CreateIndex(
                name: "IX_TrienKhaiKeHoachLCNT_GoiThauId",
                table: "TrienKhaiKeHoachLCNT",
                column: "GoiThauId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TrienKhaiKeHoachLCNT_GoiThauId",
                table: "TrienKhaiKeHoachLCNT");

            migrationBuilder.CreateIndex(
                name: "IX_TrienKhaiKeHoachLCNT_GoiThauId",
                table: "TrienKhaiKeHoachLCNT",
                column: "GoiThauId",
                unique: true);
        }
    }
}

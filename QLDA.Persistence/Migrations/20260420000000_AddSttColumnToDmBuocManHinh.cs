using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSttColumnToDmBuocManHinh : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Stt",
                table: "DmBuocManHinh",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_DmBuocManHinh_BuocId_Stt",
                table: "DmBuocManHinh",
                columns: new[] { "BuocId", "Stt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DmBuocManHinh_BuocId_Stt",
                table: "DmBuocManHinh");

            migrationBuilder.DropColumn(
                name: "Stt",
                table: "DmBuocManHinh");
        }
    }
}

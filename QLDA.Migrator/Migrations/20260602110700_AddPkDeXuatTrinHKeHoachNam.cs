using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class AddPkDeXuatTrinHKeHoachNam : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_DeXuatTrinhKinhPhiNam_DeXuatNhuCauKinhPhiId",
                table: "DeXuatTrinhKinhPhiNam",
                column: "DeXuatNhuCauKinhPhiId");

            migrationBuilder.AddForeignKey(
                name: "FK_DeXuatTrinhKinhPhiNam_DeXuatNhuCauKinhPhi_DeXuatNhuCauKinhPhiId",
                table: "DeXuatTrinhKinhPhiNam",
                column: "DeXuatNhuCauKinhPhiId",
                principalTable: "DeXuatNhuCauKinhPhi",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeXuatTrinhKinhPhiNam_DeXuatNhuCauKinhPhi_DeXuatNhuCauKinhPhiId",
                table: "DeXuatTrinhKinhPhiNam");

            migrationBuilder.DropIndex(
                name: "IX_DeXuatTrinhKinhPhiNam_DeXuatNhuCauKinhPhiId",
                table: "DeXuatTrinhKinhPhiNam");
        }
    }
}

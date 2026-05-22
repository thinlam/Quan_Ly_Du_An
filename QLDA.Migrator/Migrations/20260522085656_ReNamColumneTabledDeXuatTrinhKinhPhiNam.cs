using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class ReNamColumneTabledDeXuatTrinhKinhPhiNam : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeXuatTrinhKinhPhiNam_DeXuatNhuCauKinhPhiNam_DuAnId",
                table: "DeXuatTrinhKinhPhiNam");

            migrationBuilder.RenameColumn(
                name: "DuAnId",
                table: "DeXuatTrinhKinhPhiNam",
                newName: "DeXuatKinhPhiNamId");

            migrationBuilder.AddColumn<long>(
                name: "DonViId1",
                table: "DeXuatDonViXuLy",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DanhMucDonViCbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenDonVi = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DanhMucDonViCbo", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeXuatDonViXuLy_DonViId1",
                table: "DeXuatDonViXuLy",
                column: "DonViId1");

            migrationBuilder.AddForeignKey(
                name: "FK_DeXuatDonViXuLy_DanhMucDonViCbo_DonViId1",
                table: "DeXuatDonViXuLy",
                column: "DonViId1",
                principalTable: "DanhMucDonViCbo",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DeXuatTrinhKinhPhiNam_DeXuatNhuCauKinhPhiNam_DeXuatKinhPhiNamId",
                table: "DeXuatTrinhKinhPhiNam",
                column: "DeXuatKinhPhiNamId",
                principalTable: "DeXuatNhuCauKinhPhiNam",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeXuatDonViXuLy_DanhMucDonViCbo_DonViId1",
                table: "DeXuatDonViXuLy");

            migrationBuilder.DropForeignKey(
                name: "FK_DeXuatTrinhKinhPhiNam_DeXuatNhuCauKinhPhiNam_DeXuatKinhPhiNamId",
                table: "DeXuatTrinhKinhPhiNam");

            migrationBuilder.DropTable(
                name: "DanhMucDonViCbo");

            migrationBuilder.DropIndex(
                name: "IX_DeXuatDonViXuLy_DonViId1",
                table: "DeXuatDonViXuLy");

            migrationBuilder.DropColumn(
                name: "DonViId1",
                table: "DeXuatDonViXuLy");

            migrationBuilder.RenameColumn(
                name: "DeXuatKinhPhiNamId",
                table: "DeXuatTrinhKinhPhiNam",
                newName: "DuAnId");

            migrationBuilder.AddForeignKey(
                name: "FK_DeXuatTrinhKinhPhiNam_DeXuatNhuCauKinhPhiNam_DuAnId",
                table: "DeXuatTrinhKinhPhiNam",
                column: "DuAnId",
                principalTable: "DeXuatNhuCauKinhPhiNam",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

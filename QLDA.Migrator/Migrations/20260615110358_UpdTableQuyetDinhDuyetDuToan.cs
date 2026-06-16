using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class UpdTableQuyetDinhDuyetDuToan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TrangThaiId",
                table: "QuyetDinhDuyetDuToan",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuyetDinhDuyetDuToan_TrangThaiId",
                table: "QuyetDinhDuyetDuToan",
                column: "TrangThaiId");

            migrationBuilder.AddForeignKey(
                name: "FK_QuyetDinhDuyetDuToan_DmTrangThaiPheDuyet_TrangThaiId",
                table: "QuyetDinhDuyetDuToan",
                column: "TrangThaiId",
                principalTable: "DmTrangThaiPheDuyet",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuyetDinhDuyetDuToan_DmTrangThaiPheDuyet_TrangThaiId",
                table: "QuyetDinhDuyetDuToan");

            migrationBuilder.DropIndex(
                name: "IX_QuyetDinhDuyetDuToan_TrangThaiId",
                table: "QuyetDinhDuyetDuToan");

            migrationBuilder.DropColumn(
                name: "TrangThaiId",
                table: "QuyetDinhDuyetDuToan");
        }
    }
}

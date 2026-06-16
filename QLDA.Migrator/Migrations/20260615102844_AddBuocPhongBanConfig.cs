using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class AddBuocPhongBanConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "PhongPhuTrachChinhId",
                table: "DmBuoc",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DmBuocPhongBanPhoiHop",
                columns: table => new
                {
                    BuocId = table.Column<int>(type: "int", nullable: false),
                    PhongBanId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DmBuocPhongBanPhoiHop", x => new { x.BuocId, x.PhongBanId });
                    table.ForeignKey(
                        name: "FK_DmBuocPhongBanPhoiHop_DmBuoc_BuocId",
                        column: x => x.BuocId,
                        principalTable: "DmBuoc",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DmBuocPhongBanPhoiHop");

            migrationBuilder.DropColumn(
                name: "PhongPhuTrachChinhId",
                table: "DmBuoc");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class AHIHI : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "USER_MASTER");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "USER_MASTER",
                columns: table => new
                {
                    User_MasterID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CanBoID = table.Column<long>(type: "bigint", nullable: true),
                    DonViID = table.Column<long>(type: "bigint", nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LaDonViChinh = table.Column<bool>(type: "bit", nullable: true),
                    PhongBanID = table.Column<long>(type: "bigint", nullable: true),
                    Used = table.Column<bool>(type: "bit", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    User_PortalID = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__USER_MAS__CA9BC5E270CE69C2", x => x.User_MasterID);
                });

            migrationBuilder.CreateIndex(
                name: "IDX_USER_MASTER_01",
                table: "USER_MASTER",
                columns: new[] { "DonViID", "User_PortalID", "Used" });

            migrationBuilder.CreateIndex(
                name: "IDX_USER_MASTER_02",
                table: "USER_MASTER",
                column: "User_PortalID");

            migrationBuilder.CreateIndex(
                name: "IDX_USER_MASTER_03",
                table: "USER_MASTER",
                columns: new[] { "PhongBanID", "DonViID", "User_PortalID" });

            migrationBuilder.CreateIndex(
                name: "IDX_USER_MASTER_04",
                table: "USER_MASTER",
                columns: new[] { "DonViID", "User_PortalID" });

            migrationBuilder.CreateIndex(
                name: "IDX_USER_MASTER_05",
                table: "USER_MASTER",
                column: "User_PortalID");

            migrationBuilder.CreateIndex(
                name: "IDX_USER_MASTER_06",
                table: "USER_MASTER",
                column: "DonViID");

            migrationBuilder.CreateIndex(
                name: "IDX_USER_MASTER_07",
                table: "USER_MASTER",
                columns: new[] { "DonViID", "User_PortalID" });

            migrationBuilder.CreateIndex(
                name: "IX_USER_MASTER_122_121",
                table: "USER_MASTER",
                column: "Used");

            migrationBuilder.CreateIndex(
                name: "IX_USER_MASTER_CanBoID_Used",
                table: "USER_MASTER",
                columns: new[] { "CanBoID", "Used" });
        }
    }
}

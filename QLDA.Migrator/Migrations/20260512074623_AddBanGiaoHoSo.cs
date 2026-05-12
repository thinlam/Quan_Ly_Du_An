using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class AddBanGiaoHoSo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BanGiaoHoSo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    Ma = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TenHoSo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    PhongBanChuTriId = table.Column<long>(type: "bigint", nullable: true),
                    TrangThai = table.Column<int>(type: "int", nullable: false),
                    NgayBanGiao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BanGiaoHoSo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BanGiaoHoSo_DM_DONVI_PhongBanChuTriId",
                        column: x => x.PhongBanChuTriId,
                        principalTable: "DM_DONVI",
                        principalColumn: "DonViID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_BanGiaoHoSo_USER_MASTER_UserId",
                        column: x => x.UserId,
                        principalTable: "USER_MASTER",
                        principalColumn: "User_MasterID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BanGiaoHoSo_Index",
                table: "BanGiaoHoSo",
                column: "Index")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_BanGiaoHoSo_PhongBanChuTriId",
                table: "BanGiaoHoSo",
                column: "PhongBanChuTriId");

            migrationBuilder.CreateIndex(
                name: "IX_BanGiaoHoSo_UserId_TrangThai",
                table: "BanGiaoHoSo",
                columns: new[] { "UserId", "TrangThai" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BanGiaoHoSo");
        }
    }
}

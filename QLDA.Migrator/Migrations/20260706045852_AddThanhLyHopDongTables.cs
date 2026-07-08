using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class AddThanhLyHopDongTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ThanhLyHopDong",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    DuAnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BuocId = table.Column<int>(type: "int", nullable: true),
                    So = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ngay = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    TrichYeu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HopDongId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TrangThaiId = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThanhLyHopDong", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ThanhLyHopDong_DmTrangThaiPheDuyet_TrangThaiId",
                        column: x => x.TrangThaiId,
                        principalTable: "DmTrangThaiPheDuyet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ThanhLyHopDong_DuAn_DuAnId",
                        column: x => x.DuAnId,
                        principalTable: "DuAn",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ThanhLyHopDong_HopDong_HopDongId",
                        column: x => x.HopDongId,
                        principalTable: "HopDong",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ThanhLyHopDongNghiemThu",
                columns: table => new
                {
                    ThanhLyHopDongId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NghiemThuId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThanhLyHopDongNghiemThu", x => new { x.ThanhLyHopDongId, x.NghiemThuId });
                    table.ForeignKey(
                        name: "FK_ThanhLyHopDongNghiemThu_NghiemThu_NghiemThuId",
                        column: x => x.NghiemThuId,
                        principalTable: "NghiemThu",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ThanhLyHopDongNghiemThu_ThanhLyHopDong_ThanhLyHopDongId",
                        column: x => x.ThanhLyHopDongId,
                        principalTable: "ThanhLyHopDong",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ThanhLyHopDong_DuAnId",
                table: "ThanhLyHopDong",
                column: "DuAnId");

            migrationBuilder.CreateIndex(
                name: "IX_ThanhLyHopDong_HopDongId",
                table: "ThanhLyHopDong",
                column: "HopDongId");

            migrationBuilder.CreateIndex(
                name: "IX_ThanhLyHopDong_Index",
                table: "ThanhLyHopDong",
                column: "Index")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_ThanhLyHopDong_TrangThaiId",
                table: "ThanhLyHopDong",
                column: "TrangThaiId");

            migrationBuilder.CreateIndex(
                name: "IX_ThanhLyHopDongNghiemThu_NghiemThuId",
                table: "ThanhLyHopDongNghiemThu",
                column: "NghiemThuId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ThanhLyHopDongNghiemThu");

            migrationBuilder.DropTable(
                name: "ThanhLyHopDong");
        }
    }
}

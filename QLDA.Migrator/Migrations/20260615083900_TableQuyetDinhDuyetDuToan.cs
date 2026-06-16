using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class TableQuyetDinhDuyetDuToan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BuocId",
                table: "PhanKhaiKinhPhi",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "QuyetDinhDuyetDuToan",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    DuAnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BuocId = table.Column<int>(type: "int", nullable: true),
                    So = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Ngay = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    TrichYeu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GiaTri = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    HinhThucQuanLyId = table.Column<int>(type: "int", nullable: true),
                    ThoiGianThucHien = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    KeHoachLuaChonNhaThauId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DuAnBuocId = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuyetDinhDuyetDuToan", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuyetDinhDuyetDuToan_DmHinhThucQuanLy_HinhThucQuanLyId",
                        column: x => x.HinhThucQuanLyId,
                        principalTable: "DmHinhThucQuanLy",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_QuyetDinhDuyetDuToan_DuAnBuoc_DuAnBuocId",
                        column: x => x.DuAnBuocId,
                        principalTable: "DuAnBuoc",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_QuyetDinhDuyetDuToan_DuAn_DuAnId",
                        column: x => x.DuAnId,
                        principalTable: "DuAn",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuyetDinhDuyetDuToan_KeHoachLuaChonNhaThau_KeHoachLuaChonNhaThauId",
                        column: x => x.KeHoachLuaChonNhaThauId,
                        principalTable: "KeHoachLuaChonNhaThau",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "QuyetDinhDuyetDuToanChiPhi",
                columns: table => new
                {
                    QuyetDinhDuToanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChiPhi = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    GiaTri = table.Column<long>(type: "bigint", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuyetDinhDuyetDuToanChiPhi", x => x.QuyetDinhDuToanId);
                    table.ForeignKey(
                        name: "FK_QuyetDinhDuyetDuToanChiPhi_QuyetDinhDuyetDuToan_QuyetDinhDuToanId",
                        column: x => x.QuyetDinhDuToanId,
                        principalTable: "QuyetDinhDuyetDuToan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuyetDinhDuyetDuToanNguonVon",
                columns: table => new
                {
                    QuyetDinhDuToanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NguonVonId = table.Column<int>(type: "int", nullable: false),
                    GiaTri = table.Column<long>(type: "bigint", precision: 18, scale: 2, nullable: false),
                    Nam = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuyetDinhDuyetDuToanNguonVon", x => new { x.QuyetDinhDuToanId, x.NguonVonId });
                    table.ForeignKey(
                        name: "FK_QuyetDinhDuyetDuToanNguonVon_QuyetDinhDuyetDuToan_QuyetDinhDuToanId",
                        column: x => x.QuyetDinhDuToanId,
                        principalTable: "QuyetDinhDuyetDuToan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuyetDinhDuyetDuToan_DuAnBuocId",
                table: "QuyetDinhDuyetDuToan",
                column: "DuAnBuocId");

            migrationBuilder.CreateIndex(
                name: "IX_QuyetDinhDuyetDuToan_DuAnId",
                table: "QuyetDinhDuyetDuToan",
                column: "DuAnId");

            migrationBuilder.CreateIndex(
                name: "IX_QuyetDinhDuyetDuToan_HinhThucQuanLyId",
                table: "QuyetDinhDuyetDuToan",
                column: "HinhThucQuanLyId");

            migrationBuilder.CreateIndex(
                name: "IX_QuyetDinhDuyetDuToan_Index",
                table: "QuyetDinhDuyetDuToan",
                column: "Index")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_QuyetDinhDuyetDuToan_KeHoachLuaChonNhaThauId",
                table: "QuyetDinhDuyetDuToan",
                column: "KeHoachLuaChonNhaThauId");

            migrationBuilder.CreateIndex(
                name: "IX_QuyetDinhDuyetDuToan_So",
                table: "QuyetDinhDuyetDuToan",
                column: "So");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuyetDinhDuyetDuToanChiPhi");

            migrationBuilder.DropTable(
                name: "QuyetDinhDuyetDuToanNguonVon");

            migrationBuilder.DropTable(
                name: "QuyetDinhDuyetDuToan");

            migrationBuilder.DropColumn(
                name: "BuocId",
                table: "PhanKhaiKinhPhi");
        }
    }
}

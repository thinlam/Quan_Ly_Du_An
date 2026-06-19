using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class DropTableHoSoMoiThau : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ToTrinhQuyetDinh");

            migrationBuilder.DropTable(
                name: "HoSoMoiThauDienTu");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HoSoMoiThauDienTu",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    BuocId = table.Column<int>(type: "int", nullable: true),
                    DuAnId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    GoiThauId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    HinhThucLuaChonNhaThauId = table.Column<int>(type: "int", nullable: true),
                    TrangThaiId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GiaTri = table.Column<long>(type: "bigint", nullable: true),
                    Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    NhaThauId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ThoiGianThucHien = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TrangThaiDangTai = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoSoMoiThauDienTu", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HoSoMoiThauDienTu_DmHinhThucLuaChonNhaThau_HinhThucLuaChonNhaThauId",
                        column: x => x.HinhThucLuaChonNhaThauId,
                        principalTable: "DmHinhThucLuaChonNhaThau",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_HoSoMoiThauDienTu_DmTrangThaiPheDuyet_TrangThaiId",
                        column: x => x.TrangThaiId,
                        principalTable: "DmTrangThaiPheDuyet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_HoSoMoiThauDienTu_DuAnBuoc_BuocId",
                        column: x => x.BuocId,
                        principalTable: "DuAnBuoc",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_HoSoMoiThauDienTu_DuAn_DuAnId",
                        column: x => x.DuAnId,
                        principalTable: "DuAn",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_HoSoMoiThauDienTu_GoiThau_GoiThauId",
                        column: x => x.GoiThauId,
                        principalTable: "GoiThau",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ToTrinhQuyetDinh",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HoSoMoiThauId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChucVu = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Ngay = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    NgayKy = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    NguoiKy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    So = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TrichYeu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ToTrinhQuyetDinh", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ToTrinhQuyetDinh_HoSoMoiThauDienTu_HoSoMoiThauId",
                        column: x => x.HoSoMoiThauId,
                        principalTable: "HoSoMoiThauDienTu",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HoSoMoiThauDienTu_BuocId",
                table: "HoSoMoiThauDienTu",
                column: "BuocId");

            migrationBuilder.CreateIndex(
                name: "IX_HoSoMoiThauDienTu_DuAnId",
                table: "HoSoMoiThauDienTu",
                column: "DuAnId");

            migrationBuilder.CreateIndex(
                name: "IX_HoSoMoiThauDienTu_GoiThauId",
                table: "HoSoMoiThauDienTu",
                column: "GoiThauId");

            migrationBuilder.CreateIndex(
                name: "IX_HoSoMoiThauDienTu_HinhThucLuaChonNhaThauId",
                table: "HoSoMoiThauDienTu",
                column: "HinhThucLuaChonNhaThauId");

            migrationBuilder.CreateIndex(
                name: "IX_HoSoMoiThauDienTu_Index",
                table: "HoSoMoiThauDienTu",
                column: "Index")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_HoSoMoiThauDienTu_TrangThaiId",
                table: "HoSoMoiThauDienTu",
                column: "TrangThaiId");

            migrationBuilder.CreateIndex(
                name: "IX_ToTrinhQuyetDinh_HoSoMoiThauId",
                table: "ToTrinhQuyetDinh",
                column: "HoSoMoiThauId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ToTrinhQuyetDinh_Index",
                table: "ToTrinhQuyetDinh",
                column: "Index")
                .Annotation("SqlServer:Clustered", false);
        }
    }
}

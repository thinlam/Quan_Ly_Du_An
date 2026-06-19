using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class InsertTableHoSoMoiThau_Lan3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HoSoMoiThauDienTu",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    DuAnId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BuocId = table.Column<int>(type: "int", nullable: true),
                    HinhThucLuaChonNhaThauId = table.Column<int>(type: "int", nullable: true),
                    GoiThauId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    GiaTri = table.Column<long>(type: "bigint", nullable: true),
                    ThoiGianThucHien = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TrangThaiDangTai = table.Column<bool>(type: "bit", nullable: false),
                    TrangThaiId = table.Column<int>(type: "int", nullable: true),
                    NhaThauId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())")
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
                    So = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ngay = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    TrichYeu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NguoiKy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayKy = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ChucVu = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ToTrinhQuyetDinh");

            migrationBuilder.DropTable(
                name: "HoSoMoiThauDienTu");
        }
    }
}

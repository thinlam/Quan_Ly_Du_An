using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class addKeHoachLCNTRutGon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KeHoachLuaChonNhaThauRutGon",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DuAnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BuocId = table.Column<int>(type: "int", nullable: true),
                    Ten = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TrangThaiId = table.Column<int>(type: "int", nullable: true),
                    So = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ngay = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    TrichYeu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GoiThauId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NhaThauId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    KetQuaDanhGia = table.Column<string>(type: "nvarchar(4000)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Index = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KeHoachLuaChonNhaThauRutGon", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KeHoachLuaChonNhaThauRutGon_DmNhaThau_NhaThauId",
                        column: x => x.NhaThauId,
                        principalTable: "DmNhaThau",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_KeHoachLuaChonNhaThauRutGon_DmTrangThaiPheDuyet_TrangThaiId",
                        column: x => x.TrangThaiId,
                        principalTable: "DmTrangThaiPheDuyet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_KeHoachLuaChonNhaThauRutGon_DuAn_DuAnId",
                        column: x => x.DuAnId,
                        principalTable: "DuAn",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_KeHoachLuaChonNhaThauRutGon_GoiThau_GoiThauId",
                        column: x => x.GoiThauId,
                        principalTable: "GoiThau",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ThoaThuanGiaoViec",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DuAnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BuocId = table.Column<int>(type: "int", nullable: true),
                    TrangThaiId = table.Column<int>(type: "int", nullable: true),
                    GoiThauId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PhamVi = table.Column<string>(type: "nvarchar(4000)", nullable: true),
                    ThoiGian = table.Column<int>(type: "int", nullable: true),
                    GiaTri = table.Column<long>(type: "bigint", nullable: true),
                    ChatLuong = table.Column<string>(type: "nvarchar(4000)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Index = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThoaThuanGiaoViec", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ThoaThuanGiaoViec_DmTrangThaiPheDuyet_TrangThaiId",
                        column: x => x.TrangThaiId,
                        principalTable: "DmTrangThaiPheDuyet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ThoaThuanGiaoViec_DuAn_DuAnId",
                        column: x => x.DuAnId,
                        principalTable: "DuAn",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ThoaThuanGiaoViec_GoiThau_GoiThauId",
                        column: x => x.GoiThauId,
                        principalTable: "GoiThau",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_KeHoachLuaChonNhaThauRutGon_DuAnId",
                table: "KeHoachLuaChonNhaThauRutGon",
                column: "DuAnId");

            migrationBuilder.CreateIndex(
                name: "IX_KeHoachLuaChonNhaThauRutGon_GoiThauId",
                table: "KeHoachLuaChonNhaThauRutGon",
                column: "GoiThauId");

            migrationBuilder.CreateIndex(
                name: "IX_KeHoachLuaChonNhaThauRutGon_NhaThauId",
                table: "KeHoachLuaChonNhaThauRutGon",
                column: "NhaThauId");

            migrationBuilder.CreateIndex(
                name: "IX_KeHoachLuaChonNhaThauRutGon_TrangThaiId",
                table: "KeHoachLuaChonNhaThauRutGon",
                column: "TrangThaiId");

            migrationBuilder.CreateIndex(
                name: "IX_ThoaThuanGiaoViec_DuAnId",
                table: "ThoaThuanGiaoViec",
                column: "DuAnId");

            migrationBuilder.CreateIndex(
                name: "IX_ThoaThuanGiaoViec_GoiThauId",
                table: "ThoaThuanGiaoViec",
                column: "GoiThauId");

            migrationBuilder.CreateIndex(
                name: "IX_ThoaThuanGiaoViec_TrangThaiId",
                table: "ThoaThuanGiaoViec",
                column: "TrangThaiId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KeHoachLuaChonNhaThauRutGon");

            migrationBuilder.DropTable(
                name: "ThoaThuanGiaoViec");
        }
    }
}

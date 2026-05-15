using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class SyncQuyetDinhDieuChinhStatusCodes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DanhMucLoaiDieuChinh",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())"),
                    Ma = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Ten = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MoTa = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Used = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DanhMucLoaiDieuChinh", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QuyetDinhDieuChinh",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PheDuyetEntityName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PheDuyetEntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DuAnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SoQuyetDinh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayQuyetDinh = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    TrichYeu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LoaiDieuChinhId = table.Column<int>(type: "int", nullable: false),
                    LyDo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TepDinhKem = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TrangThaiId = table.Column<int>(type: "int", nullable: false),
                    Lan = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Index = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuyetDinhDieuChinh", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuyetDinhDieuChinh_DanhMucLoaiDieuChinh_LoaiDieuChinhId",
                        column: x => x.LoaiDieuChinhId,
                        principalTable: "DanhMucLoaiDieuChinh",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuyetDinhDieuChinh_DmTrangThaiPheDuyet_TrangThaiId",
                        column: x => x.TrangThaiId,
                        principalTable: "DmTrangThaiPheDuyet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuyetDinhDieuChinh_DuAn_DuAnId",
                        column: x => x.DuAnId,
                        principalTable: "DuAn",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ThongTinDieuChinhChiPhi",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuyetDinhDieuChinhId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TongMucDauTu = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    ChiPhiXayLap = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    ChiPhiThietBi = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    ChiPhiKhac = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    ChiPhiDuPhong = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Index = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThongTinDieuChinhChiPhi", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ThongTinDieuChinhChiPhi_QuyetDinhDieuChinh_QuyetDinhDieuChinhId",
                        column: x => x.QuyetDinhDieuChinhId,
                        principalTable: "QuyetDinhDieuChinh",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "DanhMucLoaiDieuChinh",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "IsDeleted", "Ma", "MoTa", "Ten", "UpdatedAt", "UpdatedBy", "Used" },
                values: new object[,]
                {
                    { 1, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "MDQ", null, "Điều chỉnh mục tiêu, quy mô đầu tư", null, "", true },
                    { 2, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "TMDT", null, "Điều chỉnh tổng mức đầu tư", null, "", true },
                    { 3, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "TDO", null, "Điều chỉnh tiến độ đầu tư", null, "", true },
                    { 4, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "CDT", null, "Chuyển đổi chủ đầu tư", null, "", true },
                    { 5, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "TDD", null, "Tạm dừng dự án", null, "", true },
                    { 6, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "NVU", null, "Điều chỉnh nguồn vốn dự án", null, "", true },
                    { 7, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "CTMDT", null, "Điều chỉnh cơ cấu tổng mức đầu tư", null, "", true }
                });

            migrationBuilder.InsertData(
                table: "DmTrangThaiPheDuyet",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "IsDeleted", "Loai", "Ma", "MoTa", "Stt", "Ten", "UpdatedAt", "UpdatedBy", "Used" },
                values: new object[,]
                {
                    { 21, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "QuyetDinhDieuChinh", "DT", null, 1, "Dự thảo", null, "", true },
                    { 22, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "QuyetDinhDieuChinh", "ĐTr", null, 2, "Đã trình", null, "", true },
                    { 23, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "QuyetDinhDieuChinh", "ĐD", null, 3, "Đã duyệt", null, "", true },
                    { 24, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "QuyetDinhDieuChinh", "TL", null, 4, "Trả lại", null, "", true },
                    { 25, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "QuyetDinhDieuChinh", "TC", null, 5, "Từ chối", null, "", true }
                });

            migrationBuilder.CreateIndex(
                name: "IX_DanhMucLoaiDieuChinh_Index",
                table: "DanhMucLoaiDieuChinh",
                column: "Index")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_DanhMucLoaiDieuChinh_Ma",
                table: "DanhMucLoaiDieuChinh",
                column: "Ma",
                unique: true,
                filter: "[Ma] IS NOT NULL AND [Ma] <> ''");

            migrationBuilder.CreateIndex(
                name: "IX_QuyetDinhDieuChinh_DuAnId",
                table: "QuyetDinhDieuChinh",
                column: "DuAnId");

            migrationBuilder.CreateIndex(
                name: "IX_QuyetDinhDieuChinh_LoaiDieuChinhId",
                table: "QuyetDinhDieuChinh",
                column: "LoaiDieuChinhId");

            migrationBuilder.CreateIndex(
                name: "IX_QuyetDinhDieuChinh_PheDuyetEntityName_PheDuyetEntityId",
                table: "QuyetDinhDieuChinh",
                columns: new[] { "PheDuyetEntityName", "PheDuyetEntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_QuyetDinhDieuChinh_TrangThaiId",
                table: "QuyetDinhDieuChinh",
                column: "TrangThaiId");

            migrationBuilder.CreateIndex(
                name: "IX_ThongTinDieuChinhChiPhi_QuyetDinhDieuChinhId",
                table: "ThongTinDieuChinhChiPhi",
                column: "QuyetDinhDieuChinhId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ThongTinDieuChinhChiPhi");

            migrationBuilder.DropTable(
                name: "QuyetDinhDieuChinh");

            migrationBuilder.DropTable(
                name: "DanhMucLoaiDieuChinh");

            migrationBuilder.DeleteData(
                table: "DmTrangThaiPheDuyet",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "DmTrangThaiPheDuyet",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "DmTrangThaiPheDuyet",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "DmTrangThaiPheDuyet",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "DmTrangThaiPheDuyet",
                keyColumn: "Id",
                keyValue: 25);
        }
    }
}

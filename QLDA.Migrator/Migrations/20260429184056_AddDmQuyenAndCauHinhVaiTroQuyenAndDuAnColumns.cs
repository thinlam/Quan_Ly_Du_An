using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class AddDmQuyenAndCauHinhVaiTroQuyenAndDuAnColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PheDuyetDuToan_DmTrangThaiPheDuyetDuToan_TrangThaiPheDuyetDuToanId",
                table: "PheDuyetDuToan");

            migrationBuilder.DropForeignKey(
                name: "FK_PheDuyetDuToanHistory_DmTrangThaiPheDuyetDuToan_TrangThaiActionId",
                table: "PheDuyetDuToanHistory");

            migrationBuilder.RenameColumn(
                name: "TrangThaiActionId",
                table: "PheDuyetDuToanHistory",
                newName: "TrangThaiId");

            migrationBuilder.RenameIndex(
                name: "IX_PheDuyetDuToanHistory_TrangThaiActionId",
                table: "PheDuyetDuToanHistory",
                newName: "IX_PheDuyetDuToanHistory_TrangThaiId");

            migrationBuilder.RenameColumn(
                name: "TrangThaiPheDuyetDuToanId",
                table: "PheDuyetDuToan",
                newName: "TrangThaiId");

            migrationBuilder.RenameIndex(
                name: "IX_PheDuyetDuToan_TrangThaiPheDuyetDuToanId",
                table: "PheDuyetDuToan",
                newName: "IX_PheDuyetDuToan_TrangThaiId");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "NgayQuyetDinhPheDuyet",
                table: "DuAn",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SoQuyetDinhPheDuyet",
                table: "DuAn",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DmQuyen",
                columns: table => new
                {
                    NhomQuyen = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Stt = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_DmQuyen", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CauHinhVaiTroQuyen",
                columns: table => new
                {
                    VaiTro = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    QuyenId = table.Column<int>(type: "int", nullable: false),
                    KichHoat = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CauHinhVaiTroQuyen", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CauHinhVaiTroQuyen_DmQuyen_QuyenId",
                        column: x => x.QuyenId,
                        principalTable: "DmQuyen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "DmQuyen",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "IsDeleted", "Ma", "MoTa", "NhomQuyen", "Stt", "Ten", "UpdatedAt", "UpdatedBy", "Used" },
                values: new object[,]
                {
                    { 1, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "DuAn.XemTatCa", null, "DuAn", 1, "Xem tất cả dự án", null, "", true },
                    { 2, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "DuAn.XemTheoPhong", null, "DuAn", 2, "Xem theo phòng dự án", null, "", true },
                    { 3, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "DuAn.Tao", null, "DuAn", 3, "Tạo dự án", null, "", true },
                    { 4, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "DuAn.Sua", null, "DuAn", 4, "Sửa dự án", null, "", true },
                    { 5, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "DuAn.Xoa", null, "DuAn", 5, "Xóa dự án", null, "", true },
                    { 6, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "DuAn.PheDuyet", null, "DuAn", 6, "Phê duyệt dự án", null, "", true },
                    { 7, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "GoiThau.XemTatCa", null, "GoiThau", 1, "Xem tất cả gói thầu", null, "", true },
                    { 8, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "GoiThau.XemTheoPhong", null, "GoiThau", 2, "Xem theo phòng gói thầu", null, "", true },
                    { 9, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "GoiThau.Tao", null, "GoiThau", 3, "Tạo gói thầu", null, "", true },
                    { 10, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "GoiThau.Sua", null, "GoiThau", 4, "Sửa gói thầu", null, "", true },
                    { 11, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "GoiThau.Xoa", null, "GoiThau", 5, "Xóa gói thầu", null, "", true },
                    { 12, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "HopDong.XemTatCa", null, "HopDong", 1, "Xem tất cả hợp đồng", null, "", true },
                    { 13, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "HopDong.XemTheoPhong", null, "HopDong", 2, "Xem theo phòng hợp đồng", null, "", true },
                    { 14, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "HopDong.Tao", null, "HopDong", 3, "Tạo hợp đồng", null, "", true },
                    { 15, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "HopDong.Sua", null, "HopDong", 4, "Sửa hợp đồng", null, "", true },
                    { 16, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "HopDong.Xoa", null, "HopDong", 5, "Xóa hợp đồng", null, "", true },
                    { 17, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "VanBan.XemTatCa", null, "VanBan", 1, "Xem tất cả văn bản", null, "", true },
                    { 18, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "VanBan.XemTheoPhong", null, "VanBan", 2, "Xem theo phòng văn bản", null, "", true },
                    { 19, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "VanBan.Tao", null, "VanBan", 3, "Tạo văn bản", null, "", true },
                    { 20, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "VanBan.Sua", null, "VanBan", 4, "Sửa văn bản", null, "", true },
                    { 21, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "VanBan.Xoa", null, "VanBan", 5, "Xóa văn bản", null, "", true },
                    { 22, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "ThanhToan.QuanLy", null, "ThanhToan", 1, "Quản lý thanh toán", null, "", true }
                });

            migrationBuilder.UpdateData(
                table: "DmTrangThaiPheDuyetDuToan",
                keyColumn: "Id",
                keyValue: 1,
                column: "Used",
                value: true);

            migrationBuilder.UpdateData(
                table: "DmTrangThaiPheDuyetDuToan",
                keyColumn: "Id",
                keyValue: 2,
                column: "Used",
                value: true);

            migrationBuilder.UpdateData(
                table: "DmTrangThaiPheDuyetDuToan",
                keyColumn: "Id",
                keyValue: 3,
                column: "Used",
                value: true);

            migrationBuilder.UpdateData(
                table: "DmTrangThaiPheDuyetDuToan",
                keyColumn: "Id",
                keyValue: 4,
                column: "Used",
                value: true);

            migrationBuilder.InsertData(
                table: "CauHinhVaiTroQuyen",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "IsDeleted", "KichHoat", "QuyenId", "UpdatedAt", "UpdatedBy", "VaiTro" },
                values: new object[,]
                {
                    { 1, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 1, null, "", "QLDA_TatCa" },
                    { 2, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 2, null, "", "QLDA_TatCa" },
                    { 3, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 3, null, "", "QLDA_TatCa" },
                    { 4, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 4, null, "", "QLDA_TatCa" },
                    { 5, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 5, null, "", "QLDA_TatCa" },
                    { 6, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 6, null, "", "QLDA_TatCa" },
                    { 7, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 7, null, "", "QLDA_TatCa" },
                    { 8, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 8, null, "", "QLDA_TatCa" },
                    { 9, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 9, null, "", "QLDA_TatCa" },
                    { 10, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 10, null, "", "QLDA_TatCa" },
                    { 11, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 11, null, "", "QLDA_TatCa" },
                    { 12, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 12, null, "", "QLDA_TatCa" },
                    { 13, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 13, null, "", "QLDA_TatCa" },
                    { 14, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 14, null, "", "QLDA_TatCa" },
                    { 15, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 15, null, "", "QLDA_TatCa" },
                    { 16, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 16, null, "", "QLDA_TatCa" },
                    { 17, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 17, null, "", "QLDA_TatCa" },
                    { 18, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 18, null, "", "QLDA_TatCa" },
                    { 19, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 19, null, "", "QLDA_TatCa" },
                    { 20, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 20, null, "", "QLDA_TatCa" },
                    { 21, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 21, null, "", "QLDA_TatCa" },
                    { 22, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 22, null, "", "QLDA_TatCa" },
                    { 23, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 1, null, "", "QLDA_QuanTri" },
                    { 24, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 2, null, "", "QLDA_QuanTri" },
                    { 25, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 3, null, "", "QLDA_QuanTri" },
                    { 26, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 4, null, "", "QLDA_QuanTri" },
                    { 27, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 5, null, "", "QLDA_QuanTri" },
                    { 28, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 6, null, "", "QLDA_QuanTri" },
                    { 29, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 7, null, "", "QLDA_QuanTri" },
                    { 30, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 8, null, "", "QLDA_QuanTri" },
                    { 31, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 9, null, "", "QLDA_QuanTri" },
                    { 32, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 10, null, "", "QLDA_QuanTri" },
                    { 33, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 11, null, "", "QLDA_QuanTri" },
                    { 34, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 12, null, "", "QLDA_QuanTri" },
                    { 35, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 13, null, "", "QLDA_QuanTri" },
                    { 36, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 14, null, "", "QLDA_QuanTri" },
                    { 37, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 15, null, "", "QLDA_QuanTri" },
                    { 38, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 16, null, "", "QLDA_QuanTri" },
                    { 39, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 17, null, "", "QLDA_QuanTri" },
                    { 40, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 18, null, "", "QLDA_QuanTri" },
                    { 41, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 19, null, "", "QLDA_QuanTri" },
                    { 42, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 20, null, "", "QLDA_QuanTri" },
                    { 43, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 21, null, "", "QLDA_QuanTri" },
                    { 44, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 22, null, "", "QLDA_QuanTri" },
                    { 45, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 1, null, "", "QLDA_LDDV" },
                    { 46, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 7, null, "", "QLDA_LDDV" },
                    { 47, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 12, null, "", "QLDA_LDDV" },
                    { 48, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 17, null, "", "QLDA_LDDV" },
                    { 49, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 1, null, "", "QLDA_LD" },
                    { 50, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 7, null, "", "QLDA_LD" },
                    { 51, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 12, null, "", "QLDA_LD" },
                    { 52, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 17, null, "", "QLDA_LD" },
                    { 53, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 2, null, "", "QLDA_ChuyenVien" },
                    { 54, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 8, null, "", "QLDA_ChuyenVien" },
                    { 55, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 13, null, "", "QLDA_ChuyenVien" },
                    { 56, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 18, null, "", "QLDA_ChuyenVien" },
                    { 57, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 3, null, "", "QLDA_ChuyenVien" },
                    { 58, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 4, null, "", "QLDA_ChuyenVien" },
                    { 59, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 9, null, "", "QLDA_ChuyenVien" },
                    { 60, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 10, null, "", "QLDA_ChuyenVien" },
                    { 61, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 14, null, "", "QLDA_ChuyenVien" },
                    { 62, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 15, null, "", "QLDA_ChuyenVien" },
                    { 63, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 19, null, "", "QLDA_ChuyenVien" },
                    { 64, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, true, 20, null, "", "QLDA_ChuyenVien" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CauHinhVaiTroQuyen_Index",
                table: "CauHinhVaiTroQuyen",
                column: "Index")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_CauHinhVaiTroQuyen_QuyenId",
                table: "CauHinhVaiTroQuyen",
                column: "QuyenId");

            migrationBuilder.CreateIndex(
                name: "IX_CauHinhVaiTroQuyen_VaiTro_QuyenId",
                table: "CauHinhVaiTroQuyen",
                columns: new[] { "VaiTro", "QuyenId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DmQuyen_Index",
                table: "DmQuyen",
                column: "Index")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_DmQuyen_Ma",
                table: "DmQuyen",
                column: "Ma",
                unique: true,
                filter: "[Ma] IS NOT NULL AND [Ma] <> ''");

            migrationBuilder.CreateIndex(
                name: "IX_DmQuyen_Ma_NhomQuyen",
                table: "DmQuyen",
                columns: new[] { "Ma", "NhomQuyen" },
                unique: true,
                filter: "[Ma] IS NOT NULL AND [Ma] <> ''");

            migrationBuilder.AddForeignKey(
                name: "FK_PheDuyetDuToan_DmTrangThaiPheDuyetDuToan_TrangThaiId",
                table: "PheDuyetDuToan",
                column: "TrangThaiId",
                principalTable: "DmTrangThaiPheDuyetDuToan",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PheDuyetDuToanHistory_DmTrangThaiPheDuyetDuToan_TrangThaiId",
                table: "PheDuyetDuToanHistory",
                column: "TrangThaiId",
                principalTable: "DmTrangThaiPheDuyetDuToan",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PheDuyetDuToan_DmTrangThaiPheDuyetDuToan_TrangThaiId",
                table: "PheDuyetDuToan");

            migrationBuilder.DropForeignKey(
                name: "FK_PheDuyetDuToanHistory_DmTrangThaiPheDuyetDuToan_TrangThaiId",
                table: "PheDuyetDuToanHistory");

            migrationBuilder.DropTable(
                name: "CauHinhVaiTroQuyen");

            migrationBuilder.DropTable(
                name: "DmQuyen");

            migrationBuilder.DropColumn(
                name: "NgayQuyetDinhPheDuyet",
                table: "DuAn");

            migrationBuilder.DropColumn(
                name: "SoQuyetDinhPheDuyet",
                table: "DuAn");

            migrationBuilder.RenameColumn(
                name: "TrangThaiId",
                table: "PheDuyetDuToanHistory",
                newName: "TrangThaiActionId");

            migrationBuilder.RenameIndex(
                name: "IX_PheDuyetDuToanHistory_TrangThaiId",
                table: "PheDuyetDuToanHistory",
                newName: "IX_PheDuyetDuToanHistory_TrangThaiActionId");

            migrationBuilder.RenameColumn(
                name: "TrangThaiId",
                table: "PheDuyetDuToan",
                newName: "TrangThaiPheDuyetDuToanId");

            migrationBuilder.RenameIndex(
                name: "IX_PheDuyetDuToan_TrangThaiId",
                table: "PheDuyetDuToan",
                newName: "IX_PheDuyetDuToan_TrangThaiPheDuyetDuToanId");

            migrationBuilder.UpdateData(
                table: "DmTrangThaiPheDuyetDuToan",
                keyColumn: "Id",
                keyValue: 1,
                column: "Used",
                value: false);

            migrationBuilder.UpdateData(
                table: "DmTrangThaiPheDuyetDuToan",
                keyColumn: "Id",
                keyValue: 2,
                column: "Used",
                value: false);

            migrationBuilder.UpdateData(
                table: "DmTrangThaiPheDuyetDuToan",
                keyColumn: "Id",
                keyValue: 3,
                column: "Used",
                value: false);

            migrationBuilder.UpdateData(
                table: "DmTrangThaiPheDuyetDuToan",
                keyColumn: "Id",
                keyValue: 4,
                column: "Used",
                value: false);

            migrationBuilder.AddForeignKey(
                name: "FK_PheDuyetDuToan_DmTrangThaiPheDuyetDuToan_TrangThaiPheDuyetDuToanId",
                table: "PheDuyetDuToan",
                column: "TrangThaiPheDuyetDuToanId",
                principalTable: "DmTrangThaiPheDuyetDuToan",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PheDuyetDuToanHistory_DmTrangThaiPheDuyetDuToan_TrangThaiActionId",
                table: "PheDuyetDuToanHistory",
                column: "TrangThaiActionId",
                principalTable: "DmTrangThaiPheDuyetDuToan",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

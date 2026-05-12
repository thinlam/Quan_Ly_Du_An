using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class AddPhanKhaiKinhPhi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PhanKhaiKinhPhi",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DuAnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SoToTrinh = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    NgayToTrinh = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    NguonVonId = table.Column<int>(type: "int", nullable: true),
                    KinhPhiDeXuat = table.Column<long>(type: "bigint", precision: 18, scale: 2, nullable: true),
                    KinhPhiPhanKhai = table.Column<long>(type: "bigint", precision: 18, scale: 2, nullable: true),
                    TrangThaiId = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Index = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhanKhaiKinhPhi", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PhanKhaiKinhPhi_DmNguonVon_NguonVonId",
                        column: x => x.NguonVonId,
                        principalTable: "DmNguonVon",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhanKhaiKinhPhi_DmTrangThaiPheDuyet_TrangThaiId",
                        column: x => x.TrangThaiId,
                        principalTable: "DmTrangThaiPheDuyet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhanKhaiKinhPhi_DuAn_DuAnId",
                        column: x => x.DuAnId,
                        principalTable: "DuAn",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "DmTrangThaiPheDuyet",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Loai", "Ma", "Stt", "Ten", "Used" },
                values: new object[] { "PheDuyetDuToan", "TC", 5, "Từ chối", true });

            migrationBuilder.UpdateData(
                table: "DmTrangThaiPheDuyet",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Loai", "Ma", "Stt", "Ten", "Used" },
                values: new object[] { "Default", "LEG", 0, "Migrated", false });

            migrationBuilder.InsertData(
                table: "DmTrangThaiPheDuyet",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "IsDeleted", "Loai", "Ma", "MoTa", "Stt", "Ten", "UpdatedAt", "UpdatedBy", "Used" },
                values: new object[,]
                {
                    { 17, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "PhanKhaiKinhPhi", "DT", null, 1, "Dự thảo", null, "", true },
                    { 18, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "PhanKhaiKinhPhi", "ĐTr", null, 2, "Đã trình", null, "", true },
                    { 19, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "PhanKhaiKinhPhi", "ĐD", null, 3, "Đã duyệt", null, "", true },
                    { 20, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "PhanKhaiKinhPhi", "TL", null, 4, "Trả lại", null, "", true }
                });

            migrationBuilder.CreateIndex(
                name: "IX_PhanKhaiKinhPhi_DuAnId",
                table: "PhanKhaiKinhPhi",
                column: "DuAnId");

            migrationBuilder.CreateIndex(
                name: "IX_PhanKhaiKinhPhi_NguonVonId",
                table: "PhanKhaiKinhPhi",
                column: "NguonVonId");

            migrationBuilder.CreateIndex(
                name: "IX_PhanKhaiKinhPhi_TrangThaiId",
                table: "PhanKhaiKinhPhi",
                column: "TrangThaiId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PhanKhaiKinhPhi");

            migrationBuilder.DeleteData(
                table: "DmTrangThaiPheDuyet",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "DmTrangThaiPheDuyet",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "DmTrangThaiPheDuyet",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "DmTrangThaiPheDuyet",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.UpdateData(
                table: "DmTrangThaiPheDuyet",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Loai", "Ma", "Stt", "Ten", "Used" },
                values: new object[] { "Default", "LEG", 0, "Migrated", false });

            migrationBuilder.UpdateData(
                table: "DmTrangThaiPheDuyet",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Loai", "Ma", "Stt", "Ten", "Used" },
                values: new object[] { "PheDuyetDuToan", "TC", 5, "Từ chối", true });
        }
    }
}

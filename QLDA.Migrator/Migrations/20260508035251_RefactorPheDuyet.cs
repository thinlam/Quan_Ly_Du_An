using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class RefactorPheDuyet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PheDuyetDuToanHistory");

            migrationBuilder.DropTable(
                name: "PheDuyetNoiDungHistory");

            migrationBuilder.DropTable(
                name: "PheDuyetNoiDung");

            migrationBuilder.DeleteData(
                table: "DmTrangThaiPheDuyet",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "DmTrangThaiPheDuyet",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "DmTrangThaiPheDuyet",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "DmTrangThaiPheDuyet",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "DmTrangThaiPheDuyet",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "DmTrangThaiPheDuyet",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "DmTrangThaiPheDuyet",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.AlterColumn<int>(
                name: "TrangThaiId",
                table: "PheDuyetDuToan",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 5);

            migrationBuilder.CreateTable(
                name: "PheDuyetHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    EntityName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DuAnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NguoiXuLyId = table.Column<long>(type: "bigint", nullable: true),
                    TrangThaiId = table.Column<int>(type: "int", nullable: true),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayXuLy = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PheDuyetHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PheDuyetHistory_DmTrangThaiPheDuyet_TrangThaiId",
                        column: x => x.TrangThaiId,
                        principalTable: "DmTrangThaiPheDuyet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PheDuyetHistory_DuAn_DuAnId",
                        column: x => x.DuAnId,
                        principalTable: "DuAn",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "DmTrangThaiPheDuyet",
                keyColumn: "Id",
                keyValue: 1,
                column: "Loai",
                value: "PheDuyetDuToan");

            migrationBuilder.UpdateData(
                table: "DmTrangThaiPheDuyet",
                keyColumn: "Id",
                keyValue: 2,
                column: "Loai",
                value: "PheDuyetDuToan");

            migrationBuilder.UpdateData(
                table: "DmTrangThaiPheDuyet",
                keyColumn: "Id",
                keyValue: 3,
                column: "Loai",
                value: "PheDuyetDuToan");

            migrationBuilder.UpdateData(
                table: "DmTrangThaiPheDuyet",
                keyColumn: "Id",
                keyValue: 4,
                column: "Loai",
                value: "PheDuyetDuToan");

            migrationBuilder.UpdateData(
                table: "DmTrangThaiPheDuyet",
                keyColumn: "Id",
                keyValue: 5,
                column: "Loai",
                value: "PheDuyetDuToan");

            migrationBuilder.CreateIndex(
                name: "IX_PheDuyetHistory_DuAnId",
                table: "PheDuyetHistory",
                column: "DuAnId");

            migrationBuilder.CreateIndex(
                name: "IX_PheDuyetHistory_EntityName_EntityId",
                table: "PheDuyetHistory",
                columns: new[] { "EntityName", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_PheDuyetHistory_Index",
                table: "PheDuyetHistory",
                column: "Index")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_PheDuyetHistory_TrangThaiId",
                table: "PheDuyetHistory",
                column: "TrangThaiId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PheDuyetHistory");

            migrationBuilder.AlterColumn<int>(
                name: "TrangThaiId",
                table: "PheDuyetDuToan",
                type: "int",
                nullable: false,
                defaultValue: 5,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "PheDuyetDuToanHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    DuAnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PheDuyetDuToanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrangThaiId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    NgayXuLy = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    NguoiXuLyId = table.Column<long>(type: "bigint", nullable: true),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PheDuyetDuToanHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PheDuyetDuToanHistory_DmTrangThaiPheDuyet_TrangThaiId",
                        column: x => x.TrangThaiId,
                        principalTable: "DmTrangThaiPheDuyet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PheDuyetDuToanHistory_DuAn_DuAnId",
                        column: x => x.DuAnId,
                        principalTable: "DuAn",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PheDuyetDuToanHistory_PheDuyetDuToan_PheDuyetDuToanId",
                        column: x => x.PheDuyetDuToanId,
                        principalTable: "PheDuyetDuToan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PheDuyetNoiDung",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    BuocId = table.Column<int>(type: "int", nullable: true),
                    DuAnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrangThaiId = table.Column<int>(type: "int", nullable: true),
                    VanBanQuyetDinhId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DaChuyenQLVB = table.Column<bool>(type: "bit", nullable: false),
                    Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    NgayPhatHanh = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    NguoiXuLyId = table.Column<long>(type: "bigint", nullable: true),
                    NoiDungPhanHoi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SoPhatHanh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PheDuyetNoiDung", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PheDuyetNoiDung_DmTrangThaiPheDuyet_TrangThaiId",
                        column: x => x.TrangThaiId,
                        principalTable: "DmTrangThaiPheDuyet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PheDuyetNoiDung_DuAnBuoc_BuocId",
                        column: x => x.BuocId,
                        principalTable: "DuAnBuoc",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PheDuyetNoiDung_DuAn_DuAnId",
                        column: x => x.DuAnId,
                        principalTable: "DuAn",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PheDuyetNoiDung_VanBanQuyetDinh_VanBanQuyetDinhId",
                        column: x => x.VanBanQuyetDinhId,
                        principalTable: "VanBanQuyetDinh",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PheDuyetNoiDungHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    DuAnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PheDuyetNoiDungId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrangThaiId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    NgayXuLy = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    NguoiXuLyId = table.Column<long>(type: "bigint", nullable: true),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PheDuyetNoiDungHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PheDuyetNoiDungHistory_DmTrangThaiPheDuyet_TrangThaiId",
                        column: x => x.TrangThaiId,
                        principalTable: "DmTrangThaiPheDuyet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PheDuyetNoiDungHistory_DuAn_DuAnId",
                        column: x => x.DuAnId,
                        principalTable: "DuAn",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PheDuyetNoiDungHistory_PheDuyetNoiDung_PheDuyetNoiDungId",
                        column: x => x.PheDuyetNoiDungId,
                        principalTable: "PheDuyetNoiDung",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "DmTrangThaiPheDuyet",
                keyColumn: "Id",
                keyValue: 1,
                column: "Loai",
                value: "DuToan");

            migrationBuilder.UpdateData(
                table: "DmTrangThaiPheDuyet",
                keyColumn: "Id",
                keyValue: 2,
                column: "Loai",
                value: "DuToan");

            migrationBuilder.UpdateData(
                table: "DmTrangThaiPheDuyet",
                keyColumn: "Id",
                keyValue: 3,
                column: "Loai",
                value: "DuToan");

            migrationBuilder.UpdateData(
                table: "DmTrangThaiPheDuyet",
                keyColumn: "Id",
                keyValue: 4,
                column: "Loai",
                value: "DuToan");

            migrationBuilder.UpdateData(
                table: "DmTrangThaiPheDuyet",
                keyColumn: "Id",
                keyValue: 5,
                column: "Loai",
                value: "DuToan");

            migrationBuilder.InsertData(
                table: "DmTrangThaiPheDuyet",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "IsDeleted", "Loai", "Ma", "MoTa", "Stt", "Ten", "UpdatedAt", "UpdatedBy", "Used" },
                values: new object[,]
                {
                    { 6, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "NoiDung", "CXL", null, 10, "Chờ xử lý", null, "", true },
                    { 7, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "NoiDung", "TC", null, 11, "Từ chối", null, "", true },
                    { 8, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "NoiDung", "DKS", null, 12, "Đã ký số", null, "", true },
                    { 9, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "NoiDung", "DQLVB", null, 13, "Đã chuyển QLVB", null, "", true },
                    { 10, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "NoiDung", "DPH", null, 14, "Đã phát hành", null, "", true },
                    { 11, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "NoiDung", "DD", null, 15, "Đã duyệt", null, "", true },
                    { 12, new DateTimeOffset(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "", false, "NoiDung", "TL", null, 16, "Trả lại", null, "", true }
                });

            migrationBuilder.CreateIndex(
                name: "IX_PheDuyetDuToanHistory_DuAnId",
                table: "PheDuyetDuToanHistory",
                column: "DuAnId");

            migrationBuilder.CreateIndex(
                name: "IX_PheDuyetDuToanHistory_Index",
                table: "PheDuyetDuToanHistory",
                column: "Index")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_PheDuyetDuToanHistory_PheDuyetDuToanId",
                table: "PheDuyetDuToanHistory",
                column: "PheDuyetDuToanId");

            migrationBuilder.CreateIndex(
                name: "IX_PheDuyetDuToanHistory_TrangThaiId",
                table: "PheDuyetDuToanHistory",
                column: "TrangThaiId");

            migrationBuilder.CreateIndex(
                name: "IX_PheDuyetNoiDung_BuocId",
                table: "PheDuyetNoiDung",
                column: "BuocId");

            migrationBuilder.CreateIndex(
                name: "IX_PheDuyetNoiDung_DuAnId",
                table: "PheDuyetNoiDung",
                column: "DuAnId");

            migrationBuilder.CreateIndex(
                name: "IX_PheDuyetNoiDung_Index",
                table: "PheDuyetNoiDung",
                column: "Index")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_PheDuyetNoiDung_TrangThaiId",
                table: "PheDuyetNoiDung",
                column: "TrangThaiId");

            migrationBuilder.CreateIndex(
                name: "IX_PheDuyetNoiDung_VanBanQuyetDinhId",
                table: "PheDuyetNoiDung",
                column: "VanBanQuyetDinhId");

            migrationBuilder.CreateIndex(
                name: "IX_PheDuyetNoiDungHistory_DuAnId",
                table: "PheDuyetNoiDungHistory",
                column: "DuAnId");

            migrationBuilder.CreateIndex(
                name: "IX_PheDuyetNoiDungHistory_Index",
                table: "PheDuyetNoiDungHistory",
                column: "Index")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_PheDuyetNoiDungHistory_PheDuyetNoiDungId",
                table: "PheDuyetNoiDungHistory",
                column: "PheDuyetNoiDungId");

            migrationBuilder.CreateIndex(
                name: "IX_PheDuyetNoiDungHistory_TrangThaiId",
                table: "PheDuyetNoiDungHistory",
                column: "TrangThaiId");
        }
    }
}

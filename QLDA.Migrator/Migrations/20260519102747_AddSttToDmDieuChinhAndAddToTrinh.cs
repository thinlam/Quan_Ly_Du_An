using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class AddSttToDmDieuChinhAndAddToTrinh : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Stt",
                table: "DanhMucLoaiDieuChinh",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ToTrinhKeHoach",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    DuAnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BuocId = table.Column<int>(type: "int", nullable: true),
                    So = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    NgayToTrinh = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    TrichYeu = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
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
                    table.PrimaryKey("PK_ToTrinhKeHoach", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ToTrinhKeHoach_DmTrangThaiPheDuyet_TrangThaiId",
                        column: x => x.TrangThaiId,
                        principalTable: "DmTrangThaiPheDuyet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ToTrinhKeHoach_DuAn_DuAnId",
                        column: x => x.DuAnId,
                        principalTable: "DuAn",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "DanhMucLoaiDieuChinh",
                keyColumn: "Id",
                keyValue: 1,
                column: "Stt",
                value: 1);

            migrationBuilder.UpdateData(
                table: "DanhMucLoaiDieuChinh",
                keyColumn: "Id",
                keyValue: 2,
                column: "Stt",
                value: 2);

            migrationBuilder.UpdateData(
                table: "DanhMucLoaiDieuChinh",
                keyColumn: "Id",
                keyValue: 3,
                column: "Stt",
                value: 3);

            migrationBuilder.UpdateData(
                table: "DanhMucLoaiDieuChinh",
                keyColumn: "Id",
                keyValue: 4,
                column: "Stt",
                value: 4);

            migrationBuilder.UpdateData(
                table: "DanhMucLoaiDieuChinh",
                keyColumn: "Id",
                keyValue: 5,
                column: "Stt",
                value: 5);

            migrationBuilder.UpdateData(
                table: "DanhMucLoaiDieuChinh",
                keyColumn: "Id",
                keyValue: 6,
                column: "Stt",
                value: 6);

            migrationBuilder.UpdateData(
                table: "DanhMucLoaiDieuChinh",
                keyColumn: "Id",
                keyValue: 7,
                column: "Stt",
                value: 7);

            migrationBuilder.CreateIndex(
                name: "IX_ToTrinhKeHoach_DuAnId",
                table: "ToTrinhKeHoach",
                column: "DuAnId");

            migrationBuilder.CreateIndex(
                name: "IX_ToTrinhKeHoach_Index",
                table: "ToTrinhKeHoach",
                column: "Index")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_ToTrinhKeHoach_TrangThaiId",
                table: "ToTrinhKeHoach",
                column: "TrangThaiId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ToTrinhKeHoach");

            migrationBuilder.DropColumn(
                name: "Stt",
                table: "DanhMucLoaiDieuChinh");
        }
    }
}

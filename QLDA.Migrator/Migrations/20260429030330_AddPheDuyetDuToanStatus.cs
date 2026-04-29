using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class AddPheDuyetDuToanStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PheDuyetDuToan_DmChucVu_ChucVuId",
                table: "PheDuyetDuToan");

            migrationBuilder.AddColumn<int>(
                name: "DanhMucChucVuId",
                table: "PheDuyetDuToan",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "NguoiGiaoViecId",
                table: "PheDuyetDuToan",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "NguoiXuLyId",
                table: "PheDuyetDuToan",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TrangThaiId",
                table: "PheDuyetDuToan",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "DmTrangThaiPheDuyetDuToan",
                columns: table => new
                {
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
                    table.PrimaryKey("PK_DmTrangThaiPheDuyetDuToan", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PheDuyetDuToanHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    PheDuyetDuToanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DuAnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NguoiXuLyId = table.Column<long>(type: "bigint", nullable: true),
                    TrangThaiId = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_PheDuyetDuToanHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PheDuyetDuToanHistory_DmTrangThaiPheDuyetDuToan_TrangThaiId",
                        column: x => x.TrangThaiId,
                        principalTable: "DmTrangThaiPheDuyetDuToan",
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

            migrationBuilder.InsertData(
                table: "DmTrangThaiPheDuyetDuToan",
                columns: new[] { "Id", "CreatedBy", "IsDeleted", "Ma", "MoTa", "Stt", "Ten", "UpdatedAt", "UpdatedBy", "Used" },
                values: new object[,]
                {
                    { 1, "", false, "DT", null, 1, "Dự thảo", null, "", true },
                    { 2, "", false, "ĐTr", null, 2, "Đã trình", null, "", true },
                    { 3, "", false, "ĐD", null, 3, "Đã duyệt", null, "", true },
                    { 4, "", false, "TL", null, 4, "Trả lại", null, "", true }
                });

            migrationBuilder.CreateIndex(
                name: "IX_PheDuyetDuToan_DanhMucChucVuId",
                table: "PheDuyetDuToan",
                column: "DanhMucChucVuId");

            migrationBuilder.CreateIndex(
                name: "IX_PheDuyetDuToan_TrangThaiId",
                table: "PheDuyetDuToan",
                column: "TrangThaiId");

            migrationBuilder.CreateIndex(
                name: "IX_DmTrangThaiPheDuyetDuToan_Index",
                table: "DmTrangThaiPheDuyetDuToan",
                column: "Index")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_DmTrangThaiPheDuyetDuToan_Ma",
                table: "DmTrangThaiPheDuyetDuToan",
                column: "Ma",
                unique: true,
                filter: "[Ma] IS NOT NULL AND [Ma] <> ''");

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

            migrationBuilder.AddForeignKey(
                name: "FK_PheDuyetDuToan_DmChucVu_ChucVuId",
                table: "PheDuyetDuToan",
                column: "ChucVuId",
                principalTable: "DmChucVu",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PheDuyetDuToan_DmChucVu_DanhMucChucVuId",
                table: "PheDuyetDuToan",
                column: "DanhMucChucVuId",
                principalTable: "DmChucVu",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PheDuyetDuToan_DmTrangThaiPheDuyetDuToan_TrangThaiId",
                table: "PheDuyetDuToan",
                column: "TrangThaiId",
                principalTable: "DmTrangThaiPheDuyetDuToan",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PheDuyetDuToan_DmChucVu_ChucVuId",
                table: "PheDuyetDuToan");

            migrationBuilder.DropForeignKey(
                name: "FK_PheDuyetDuToan_DmChucVu_DanhMucChucVuId",
                table: "PheDuyetDuToan");

            migrationBuilder.DropForeignKey(
                name: "FK_PheDuyetDuToan_DmTrangThaiPheDuyetDuToan_TrangThaiId",
                table: "PheDuyetDuToan");

            migrationBuilder.DropTable(
                name: "PheDuyetDuToanHistory");

            migrationBuilder.DropTable(
                name: "DmTrangThaiPheDuyetDuToan");

            migrationBuilder.DropIndex(
                name: "IX_PheDuyetDuToan_DanhMucChucVuId",
                table: "PheDuyetDuToan");

            migrationBuilder.DropIndex(
                name: "IX_PheDuyetDuToan_TrangThaiId",
                table: "PheDuyetDuToan");

            migrationBuilder.DropColumn(
                name: "DanhMucChucVuId",
                table: "PheDuyetDuToan");

            migrationBuilder.DropColumn(
                name: "NguoiGiaoViecId",
                table: "PheDuyetDuToan");

            migrationBuilder.DropColumn(
                name: "NguoiXuLyId",
                table: "PheDuyetDuToan");

            migrationBuilder.DropColumn(
                name: "TrangThaiId",
                table: "PheDuyetDuToan");

            migrationBuilder.AddForeignKey(
                name: "FK_PheDuyetDuToan_DmChucVu_ChucVuId",
                table: "PheDuyetDuToan",
                column: "ChucVuId",
                principalTable: "DmChucVu",
                principalColumn: "Id");
        }
    }
}

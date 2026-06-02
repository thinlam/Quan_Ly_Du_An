using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class tableKeHoachTrienKhaiCacHangMuc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KeHoachTrienKhaiHangMuc",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    DuAnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BuocId = table.Column<int>(type: "int", nullable: true),
                    So = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NgayToTrinh = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    TrichYeu = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    TrangThaiId = table.Column<int>(type: "int", nullable: true),
                    TenHangMuc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GiaiDoanId = table.Column<int>(type: "int", nullable: true),
                    CanBoChuTriId = table.Column<long>(type: "bigint", nullable: true),
                    NgayBatDau = table.Column<DateOnly>(type: "date", nullable: true),
                    NgayKetThuc = table.Column<DateOnly>(type: "date", nullable: true),
                    ThoiHan = table.Column<DateOnly>(type: "date", nullable: true),
                    KinhPhi = table.Column<long>(type: "bigint", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KeHoachTrienKhaiHangMuc", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KeHoachTrienKhaiHangMuc_DmTrangThaiPheDuyet_TrangThaiId",
                        column: x => x.TrangThaiId,
                        principalTable: "DmTrangThaiPheDuyet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_KeHoachTrienKhaiHangMuc_DuAn_DuAnId",
                        column: x => x.DuAnId,
                        principalTable: "DuAn",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CanBoTrienKhaiHangMuc",
                columns: table => new
                {
                    KeHoachId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CanBoId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CanBoTrienKhaiHangMuc", x => new { x.KeHoachId, x.CanBoId });
                    table.ForeignKey(
                        name: "FK_CanBoTrienKhaiHangMuc_KeHoachTrienKhaiHangMuc_KeHoachId",
                        column: x => x.KeHoachId,
                        principalTable: "KeHoachTrienKhaiHangMuc",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_KeHoachTrienKhaiHangMuc_DuAnId",
                table: "KeHoachTrienKhaiHangMuc",
                column: "DuAnId");

            migrationBuilder.CreateIndex(
                name: "IX_KeHoachTrienKhaiHangMuc_Index",
                table: "KeHoachTrienKhaiHangMuc",
                column: "Index")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_KeHoachTrienKhaiHangMuc_TrangThaiId",
                table: "KeHoachTrienKhaiHangMuc",
                column: "TrangThaiId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CanBoTrienKhaiHangMuc");

            migrationBuilder.DropTable(
                name: "KeHoachTrienKhaiHangMuc");
        }
    }
}

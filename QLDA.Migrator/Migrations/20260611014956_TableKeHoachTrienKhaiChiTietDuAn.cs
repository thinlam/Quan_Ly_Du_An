using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class TableKeHoachTrienKhaiChiTietDuAn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.CreateTable(
                name: "DanhMucTinhHinhXuLy",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Stt = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Index = table.Column<long>(type: "bigint", nullable: false),
                    Ma = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ten = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Used = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DanhMucTinhHinhXuLy", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KeHoachTrienKhaiChiTietDuAn",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    DuAnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BuocId = table.Column<int>(type: "int", nullable: true),
                    MaMoc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Ten = table.Column<string>(type: "nvarchar(4000)", nullable: true),
                    GhiChu = table.Column<string>(type: "nvarchar(4000)", nullable: true),
                    DonViChuTriId = table.Column<long>(type: "bigint", nullable: true),
                    NgayBatDauKeHoach = table.Column<DateOnly>(type: "date", nullable: true),
                    NgayKetThucKeHoach = table.Column<DateOnly>(type: "date", nullable: true),
                    NgayBatDauThucTe = table.Column<DateOnly>(type: "date", nullable: true),
                    NgayKetThucThucTe = table.Column<DateOnly>(type: "date", nullable: true),
                    TiLeHoanThanh = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_KeHoachTrienKhaiChiTietDuAn", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KeHoachTrienKhaiChiTietDuAn_DanhMucTinhHinhXuLy_TrangThaiId",
                        column: x => x.TrangThaiId,
                        principalTable: "DanhMucTinhHinhXuLy",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_KeHoachTrienKhaiChiTietDuAn_DuAn_DuAnId",
                        column: x => x.DuAnId,
                        principalTable: "DuAn",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });


            migrationBuilder.CreateIndex(
                name: "IX_KeHoachTrienKhaiChiTietDuAn_DuAnId",
                table: "KeHoachTrienKhaiChiTietDuAn",
                column: "DuAnId");

            migrationBuilder.CreateIndex(
                name: "IX_KeHoachTrienKhaiChiTietDuAn_Index",
                table: "KeHoachTrienKhaiChiTietDuAn",
                column: "Index")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_KeHoachTrienKhaiChiTietDuAn_TrangThaiId",
                table: "KeHoachTrienKhaiChiTietDuAn",
                column: "TrangThaiId");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        

            migrationBuilder.DropTable(
                name: "KeHoachTrienKhaiChiTietDuAn");

            migrationBuilder.DropTable(
                name: "DanhMucTinhHinhXuLy");

     
        }
    }
}

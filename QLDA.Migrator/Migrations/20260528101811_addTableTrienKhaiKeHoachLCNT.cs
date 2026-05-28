using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class addTableTrienKhaiKeHoachLCNT : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DonViTuVanKeHoachKeHoachId",
                table: "TepDinhKem",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TrienKhaiKeHoachLCNT",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    DuAnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BuocId = table.Column<int>(type: "int", nullable: true),
                    GoiThauId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    So = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NgayTrinh = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    TrichYeu = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    HinhThucLCNT = table.Column<int>(type: "int", nullable: true),
                    TrangThaiId = table.Column<int>(type: "int", nullable: true),
                    TrangThaiDangTaiId = table.Column<int>(type: "int", nullable: true),
                    GiaTri = table.Column<long>(type: "bigint", nullable: true),
                    ThoiGianThucHien = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    YeuCau = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Index = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "DATEDIFF(SECOND, '19700101', GETUTCDATE())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrienKhaiKeHoachLCNT", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrienKhaiKeHoachLCNT_DmTrangThaiPheDuyet_TrangThaiId",
                        column: x => x.TrangThaiId,
                        principalTable: "DmTrangThaiPheDuyet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrienKhaiKeHoachLCNT_DuAn_DuAnId",
                        column: x => x.DuAnId,
                        principalTable: "DuAn",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrienKhaiKeHoachLCNT_GoiThau_GoiThauId",
                        column: x => x.GoiThauId,
                        principalTable: "GoiThau",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DonViTuVanKeHoach",
                columns: table => new
                {
                    KeHoachLCNTId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenDonVi = table.Column<string>(type: "nvarchar(400)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DonViTuVanKeHoach", x => x.KeHoachLCNTId);
                    table.ForeignKey(
                        name: "FK_DonViTuVanKeHoach_TrienKhaiKeHoachLCNT_KeHoachLCNTId",
                        column: x => x.KeHoachLCNTId,
                        principalTable: "TrienKhaiKeHoachLCNT",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TepDinhKem_DonViTuVanKeHoachKeHoachId",
                table: "TepDinhKem",
                column: "DonViTuVanKeHoachKeHoachId");

            migrationBuilder.CreateIndex(
                name: "IX_TrienKhaiKeHoachLCNT_DuAnId",
                table: "TrienKhaiKeHoachLCNT",
                column: "DuAnId");

            migrationBuilder.CreateIndex(
                name: "IX_TrienKhaiKeHoachLCNT_GoiThauId",
                table: "TrienKhaiKeHoachLCNT",
                column: "GoiThauId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrienKhaiKeHoachLCNT_Index",
                table: "TrienKhaiKeHoachLCNT",
                column: "Index")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_TrienKhaiKeHoachLCNT_TrangThaiId",
                table: "TrienKhaiKeHoachLCNT",
                column: "TrangThaiId");

            migrationBuilder.AddForeignKey(
                name: "FK_TepDinhKem_DonViTuVanKeHoach_DonViTuVanKeHoachKeHoachId",
                table: "TepDinhKem",
                column: "DonViTuVanKeHoachKeHoachId",
                principalTable: "DonViTuVanKeHoach",
                principalColumn: "KeHoachLCNTId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TepDinhKem_DonViTuVanKeHoach_DonViTuVanKeHoachKeHoachId",
                table: "TepDinhKem");

            migrationBuilder.DropTable(
                name: "DonViTuVanKeHoach");

            migrationBuilder.DropTable(
                name: "TrienKhaiKeHoachLCNT");

            migrationBuilder.DropIndex(
                name: "IX_TepDinhKem_DonViTuVanKeHoachKeHoachId",
                table: "TepDinhKem");

            migrationBuilder.DropColumn(
                name: "DonViTuVanKeHoachKeHoachId",
                table: "TepDinhKem");
        }
    }
}

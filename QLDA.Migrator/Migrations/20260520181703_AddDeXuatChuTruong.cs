using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class AddDeXuatChuTruong : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DeXuatChuTruongMoiId",
                table: "TepDinhKem",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DeXuatChuTruongMoi",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    DuAnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BuocId = table.Column<int>(type: "int", nullable: true),
                    TomTatNoiDung = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    TongMucDauTu = table.Column<long>(type: "bigint", precision: 18, scale: 2, nullable: true),
                    NgayBatDauDuKien = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    HinhThucDauTuId = table.Column<int>(type: "int", nullable: true),
                    LanhDaoPhuTrachId = table.Column<long>(type: "bigint", nullable: true),
                    DonViPhuTrachChinhId = table.Column<long>(type: "bigint", nullable: true),
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
                    table.PrimaryKey("PK_DeXuatChuTruongMoi", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeXuatChuTruongMoi_DmHinhThucDauTu_HinhThucDauTuId",
                        column: x => x.HinhThucDauTuId,
                        principalTable: "DmHinhThucDauTu",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DeXuatChuTruongMoi_DmTrangThaiPheDuyet_TrangThaiId",
                        column: x => x.TrangThaiId,
                        principalTable: "DmTrangThaiPheDuyet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DeXuatChuTruongMoi_DuAn_DuAnId",
                        column: x => x.DuAnId,
                        principalTable: "DuAn",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DeXuatDonViXuLy",
                columns: table => new
                {
                    DuAnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DonViId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeXuatDonViXuLy", x => new { x.DuAnId, x.DonViId });
                    table.ForeignKey(
                        name: "FK_DeXuatDonViXuLy_DeXuatChuTruongMoi_DuAnId",
                        column: x => x.DuAnId,
                        principalTable: "DeXuatChuTruongMoi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TepDinhKem_DeXuatChuTruongMoiId",
                table: "TepDinhKem",
                column: "DeXuatChuTruongMoiId");

            migrationBuilder.CreateIndex(
                name: "IX_DeXuatChuTruongMoi_DuAnId",
                table: "DeXuatChuTruongMoi",
                column: "DuAnId");

            migrationBuilder.CreateIndex(
                name: "IX_DeXuatChuTruongMoi_HinhThucDauTuId",
                table: "DeXuatChuTruongMoi",
                column: "HinhThucDauTuId");

            migrationBuilder.CreateIndex(
                name: "IX_DeXuatChuTruongMoi_Index",
                table: "DeXuatChuTruongMoi",
                column: "Index")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_DeXuatChuTruongMoi_TrangThaiId",
                table: "DeXuatChuTruongMoi",
                column: "TrangThaiId");

            migrationBuilder.AddForeignKey(
                name: "FK_TepDinhKem_DeXuatChuTruongMoi_DeXuatChuTruongMoiId",
                table: "TepDinhKem",
                column: "DeXuatChuTruongMoiId",
                principalTable: "DeXuatChuTruongMoi",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TepDinhKem_DeXuatChuTruongMoi_DeXuatChuTruongMoiId",
                table: "TepDinhKem");

            migrationBuilder.DropTable(
                name: "DeXuatDonViXuLy");

            migrationBuilder.DropTable(
                name: "DeXuatChuTruongMoi");

            migrationBuilder.DropIndex(
                name: "IX_TepDinhKem_DeXuatChuTruongMoiId",
                table: "TepDinhKem");

            migrationBuilder.DropColumn(
                name: "DeXuatChuTruongMoiId",
                table: "TepDinhKem");
        }
    }
}

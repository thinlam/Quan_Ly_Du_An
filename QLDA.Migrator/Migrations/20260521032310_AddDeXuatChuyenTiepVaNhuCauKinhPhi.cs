using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QLDA.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class AddDeXuatChuyenTiepVaNhuCauKinhPhi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DeXuatChuyenTiepId",
                table: "TepDinhKem",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DeXuatChuyenTiep",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    DuAnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BuocId = table.Column<int>(type: "int", nullable: true),
                    SoLieuGiaiNgan = table.Column<long>(type: "bigint", precision: 18, scale: 2, nullable: true),
                    UocGiaiNgan = table.Column<long>(type: "bigint", precision: 18, scale: 2, nullable: true),
                    NhuCauKinhPhi = table.Column<long>(type: "bigint", precision: 18, scale: 2, nullable: true),
                    KhoiLuongThucTe = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    KhoiLuongDuKien = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
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
                    table.PrimaryKey("PK_DeXuatChuyenTiep", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeXuatChuyenTiep_DmTrangThaiPheDuyet_TrangThaiId",
                        column: x => x.TrangThaiId,
                        principalTable: "DmTrangThaiPheDuyet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DeXuatChuyenTiep_DuAn_DuAnId",
                        column: x => x.DuAnId,
                        principalTable: "DuAn",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DeXuatNhuCauKinhPhi",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    DuAnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BuocId = table.Column<int>(type: "int", nullable: true),
                    DonViDeXuatId = table.Column<long>(type: "bigint", nullable: true),
                    SoPhieuChuyen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    NgayPhieuChuyen = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    TrichYeu = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
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
                    table.PrimaryKey("PK_DeXuatNhuCauKinhPhi", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeXuatNhuCauKinhPhi_DmTrangThaiPheDuyet_TrangThaiId",
                        column: x => x.TrangThaiId,
                        principalTable: "DmTrangThaiPheDuyet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DeXuatNhuCauKinhPhi_DuAn_DuAnId",
                        column: x => x.DuAnId,
                        principalTable: "DuAn",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TepDinhKem_DeXuatChuyenTiepId",
                table: "TepDinhKem",
                column: "DeXuatChuyenTiepId");

            migrationBuilder.CreateIndex(
                name: "IX_DeXuatChuyenTiep_DuAnId",
                table: "DeXuatChuyenTiep",
                column: "DuAnId");

            migrationBuilder.CreateIndex(
                name: "IX_DeXuatChuyenTiep_Index",
                table: "DeXuatChuyenTiep",
                column: "Index")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_DeXuatChuyenTiep_TrangThaiId",
                table: "DeXuatChuyenTiep",
                column: "TrangThaiId");

            migrationBuilder.CreateIndex(
                name: "IX_DeXuatNhuCauKinhPhi_DuAnId",
                table: "DeXuatNhuCauKinhPhi",
                column: "DuAnId");

            migrationBuilder.CreateIndex(
                name: "IX_DeXuatNhuCauKinhPhi_Index",
                table: "DeXuatNhuCauKinhPhi",
                column: "Index")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_DeXuatNhuCauKinhPhi_TrangThaiId",
                table: "DeXuatNhuCauKinhPhi",
                column: "TrangThaiId");

            migrationBuilder.AddForeignKey(
                name: "FK_TepDinhKem_DeXuatChuyenTiep_DeXuatChuyenTiepId",
                table: "TepDinhKem",
                column: "DeXuatChuyenTiepId",
                principalTable: "DeXuatChuyenTiep",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TepDinhKem_DeXuatChuyenTiep_DeXuatChuyenTiepId",
                table: "TepDinhKem");

            migrationBuilder.DropTable(
                name: "DeXuatChuyenTiep");

            migrationBuilder.DropTable(
                name: "DeXuatNhuCauKinhPhi");

            migrationBuilder.DropIndex(
                name: "IX_TepDinhKem_DeXuatChuyenTiepId",
                table: "TepDinhKem");

            migrationBuilder.DropColumn(
                name: "DeXuatChuyenTiepId",
                table: "TepDinhKem");
        }
    }
}
